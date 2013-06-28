using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

// For file uploads
using System.IO;

// For resizing images
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;

// Libraries for Google APIs
using Google;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;

// For OAuth2
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

// For entity framework constants.
using System.Data;

// For data members.
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    /// <summary>
    /// A helper class for encapsulating logic and operations for photos.
    /// </summary>
    public class PhotosHelper: PlusHelper
    {

        /// <summary>
        /// Processes and uploads a photo from the currently signed-in user to PhotoHunt.
        /// </summary>
        /// <param name="context">The HttpContext containing the request with an image in it.
        /// </param>
        /// <param name="user">The currently sign-in user.</param>
        /// <param name="themeId">The id for the theme that the photo will be added to.</param>
        /// <param name="themeDisplayName">The display name for the theme that the photo will be
        /// added to.</param>
        /// <returns>The Photo object representing the uploaded photo.</returns>
        public Photo UploadPhoto(HttpContext context, User user, Theme selectedTheme)
        {
            // User will be NULL if there isn't someone signed in.
            if (user == null)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Unauthorized request.";
                return null;
            }

            // path is uploads/theme/userid/filename
            HttpPostedFile upload = context.Request.Files["image"];
            string path = context.Server.MapPath("..") + "\\uploads\\";
            Directory.CreateDirectory(path);
            path += selectedTheme.displayName + "\\";
            Directory.CreateDirectory(path);
            path += user.id + "\\";
            Directory.CreateDirectory(path);

            path += upload.FileName;
            upload.SaveAs(path);

            string urlpath = "uploads/" + selectedTheme.displayName + "/" + user.id + "/" +
                    upload.FileName;
            string thumbPath = ResizePhoto(path, urlpath, user, selectedTheme.displayName, upload);

            // Save the photo using EF
            Photo dbPhoto = new Photo();

            dbPhoto.ownerUserId = user.id;
            dbPhoto.ownerDisplayName = user.googleDisplayName;
            dbPhoto.ownerProfileUrl = user.googlePublicProfileUrl;
            dbPhoto.ownerProfilePhoto = user.googlePublicProfilePhotoUrl;
            dbPhoto.themeId = selectedTheme.id;
            dbPhoto.themeDisplayName = selectedTheme.displayName;
            dbPhoto.numVotes = 0;
            dbPhoto.voted = false;
            dbPhoto.created = (long)Jsonifiable<Object>.ConvertToUnixTimestamp(DateTime.Now);
            dbPhoto.fullsizeUrl = BASE_URL + urlpath;
            dbPhoto.thumbnailUrl = thumbPath;

            // Save to set the ID for the remaining members.
            PhotohuntContext db = new PhotohuntContext();
            db.Photos.Add(dbPhoto);
            db.SaveChanges();

            dbPhoto.photoContentUrl = BASE_URL + "photo.aspx?photoId=" + dbPhoto.id;

            // Set the default theme photo id to this one
            dbPhoto.voteCtaUrl = BASE_URL +
                    "photo.aspx" + "?photoId=" + dbPhoto.id + "&action=vote";
            db.SaveChanges();


            // Set the uploaded photo to the preview for the theme.
            Theme dbTheme = db.Themes.First(theme => theme.id == selectedTheme.id);
            dbTheme.previewPhotoId = dbPhoto.id;
            db.SaveChanges();

            return dbPhoto;
        }

        /// <summary>
        /// Utility function for resizing an uploaded photo.
        /// </summary>
        /// <param name="path">The path to the photo.</param>
        /// <param name="urlpath">The path to the URL of the original photo.</param>
        /// <param name="user">The PhotoHunt user who uploaded the thumbnail.</param>
        /// <param name="themeDisplayName">The theme's name.</param>
        /// <param name="upload">The file uploaded.</param>
        /// <returns></returns>
        public string ResizePhoto(string path, string urlpath, User user, string themeDisplayName,
                HttpPostedFile upload)
        {
            Image image = new Bitmap(path);

            string thumbPath = BASE_URL + urlpath;

            // Calculate thumbnail dimensions and resize the uploaded image.
            int maxWidth = 400;
            if (image.Width > maxWidth)
            {
                float widthRatio = (float)maxWidth / (float)image.Width;
                int height = (int)(widthRatio * image.Height);

                Image newImage = new Bitmap(maxWidth, height);
                using (Graphics gr = Graphics.FromImage(newImage))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(image, new Rectangle(0, 0, maxWidth, height));
                }
                Regex regex = new Regex(@"(\.)(\S+$)");
                thumbPath = BASE_URL + "uploads/" + themeDisplayName + "/" + user.id + "/" +
                        regex.Replace(upload.FileName, "-thumb.$2");

                string thumbFilePath = regex.Replace(path, "-thumb.$2");

                newImage.Save(thumbFilePath);
            }
            return thumbPath;
        }

        /// <summary>
        /// Delete a user's photo.
        /// </summary>
        /// <param name="context">The context containing the request, response, and so on.</param>
        /// <param name="user">The PhotoHunt user deleting the photo.</param>
        /// <param name="photo">The Photo object to be deleted.</param>
        public void DeletePhoto(HttpContext context, User user, Photo toDelete)
        {
            // User will be NULL if there isn't someone signed in.
            if (user == null || user.id != toDelete.ownerUserId)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Unauthorized request.";
                return;
            }
            PhotohuntContext dbRemove = new PhotohuntContext();
            dbRemove.Entry(toDelete).State = EntityState.Deleted;
            dbRemove.SaveChanges();
        }

        /// <summary>
        /// Write an app activity to Google using the Google+ API logging that the user
        /// has uploaded a Photo.
        /// </summary>
        /// <param name="user">The PhotoHunt user who uploaded the Photo.</param>
        /// <param name="dbPhoto">The Photo that was just uploaded.</param>
        public void WriteGooglePlusPhotoAppActivity(User user, Photo dbPhoto)
        {
            _authState = CreateState(user.googleAccessToken, user.googleRefreshToken,
                user.googleExpiresAt.AddSeconds(user.googleExpiresIn * -1), user.googleExpiresAt);

            AuthorizationServerDescription description = GoogleAuthenticationServer.Description;
            var provider = new WebServerClient(description);
            provider.ClientIdentifier = CLIENT_ID;
            provider.ClientSecret = CLIENT_SECRET;
            var authenticator =
                new OAuth2Authenticator<WebServerClient>(
                    provider,
                    GetAuthorization)
                {
                    NoCaching = true
                };
            ps = new PlusService(authenticator);

            Moment body = new Moment();
            ItemScope target = new ItemScope();

            target.Url = BASE_URL + "photo.aspx?photoId=" + dbPhoto.id;

            body.Target = target;
            body.Type = ADD_ACTIVITY_TYPE;

            MomentsResource.InsertRequest insert =
                new MomentsResource.InsertRequest(ps, body, "me", MomentsResource.Collection.Vault);
            try
            {
                Moment m = insert.Fetch();
            }
            catch (GoogleApiRequestException gare)
            {
                Debug.WriteLine("Error while writing app activity: " + gare.InnerException.Message +
                    "\nThis could happen if the Google+ proxy can't access your server.");
            }
        }

        /// <summary>
        /// Retrieves photos for the selected theme, user, or user's friends.
        /// </summary>
        /// <param name="hasThemeIdParam">Indicates photos should be scoped to a theme.</param>
        /// <param name="hasUserIdParam">Indicates photos should be scoped to a user.</param>
        /// <param name="isFriends">Indicates photos are to be retrieved for a user's friends.
        /// </param>
        /// <param name="userId">The PhotoHunt user id to retrieve photos or the user's friend's
        /// photos.</param>
        /// <param name="selectedTheme">The selected PhotoHunt theme.</param>
        /// <returns>A list of objects that can be returned as JSON.</returns>
        public List<Photo> GetPhotos(bool hasThemeIdParam, bool hasUserIdParam,
                bool isFriends, int userId, Theme selectedTheme)
        {
            // This will store JSONified representations of photo objects.
            List<Photo> photos = new List<Photo>();

            if (hasThemeIdParam && hasUserIdParam)
            {
                // Return the photos for this user for this theme.
                PhotohuntContext db = new PhotohuntContext();
                var query = from b in db.Photos
                            where b.themeId.Equals(selectedTheme.id)
                            && b.ownerUserId.Equals(userId)
                            select b;
                if (isFriends)
                {
                    query = from b in db.Photos
                            where b.themeId.Equals(selectedTheme.id)
                            && b.ownerUserId.Equals(userId)
                            select b;

                }
                foreach (Photo photo in query)
                {
                    photo.voted = !VotesHelper.CanVote(userId, photo.id);
                    photos.Add(photo);
                }
            }
            else if (hasUserIdParam)
            {
                // Return this user's photos.
                PhotohuntContext db = new PhotohuntContext();
                var query = from b in db.Photos
                            where b.ownerUserId.Equals(userId)
                            select b;
                foreach (Photo photo in query)
                {
                    photos.Add(photo);
                }
            }
            else if (hasThemeIdParam)
            {
                // Return the photos for the current theme
                PhotohuntContext db = new PhotohuntContext();
                var query = from b in db.Photos
                            where b.themeId.Equals(selectedTheme.id)
                            select b;
                foreach (Photo photo in query)
                {
                    photos.Add(photo);
                }
            }

            return photos;
        }
    }
}
