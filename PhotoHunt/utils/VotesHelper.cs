using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

// Libraries for Google APIs
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google;

// For OAuth2
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

// For data members.
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    /// <summary>
    /// A utility class for encapsulating some of the vote logic such as voting on a photo.
    /// </summary>
    public class VotesHelper: PlusHelper
    {

        /// <summary>
        /// Upvotes a photo and writes an app activity for the vote action.
        /// </summary>
        /// <param name="context">The request and response object with the user in session.
        /// </param>
        /// <returns>On success, returns the Photo object for the image voted on. On failure,
        /// returns null.</returns>
        public Photo DoVote(HttpContext context, int photoId)
        {
            User user = GetUser(context);

            // User will be NULL if there isn't someone signed in.
            if (user == null)
            {
                return null;
            }

            if (!CanVote(user.id, photoId)){
                return null;
            }

            // Create the vote and increment the vote count for this photo.
            PhotohuntContext db = new PhotohuntContext();
            Vote v = new Vote();
            v.photoId = photoId;
            v.ownerUserId = user.id;
            db.Votes.Add(v);
            db.SaveChanges();

            var photoQuery = from b in db.Photos
                             where b.id == photoId
                             select b;
            Photo voteTarget = null;
            foreach (Photo currPhoto in photoQuery)
            {
                voteTarget = currPhoto;
            }
            voteTarget.numVotes += 1;
            db.SaveChanges();

            WriteGooglePlusVoteAppActivity(user, voteTarget);

            return voteTarget;
        }

        /// <summary>
        /// Tests whether a user can vote on a photo.
        /// </summary>
        /// <param name="user">The PhotoHunt user who is voting.</param>
        /// <param name="photoId">The id of the PhotoHunt photo being voted on.</param>
        /// <returns>True if the user can vote on the photo; otherwise, returns false.</returns>
        static public bool CanVote(int userId, int photoId)
        {
            PhotohuntContext db = new PhotohuntContext();

            // Protect against self-votes.
            if (db.Photos.Any(p => p.ownerUserId == userId && p.id == photoId))
            {
                return false;
            }

            // Protect against duplicate votes.
            if (db.Votes.Any(v => v.ownerUserId == userId && v.photoId == photoId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes a Google+ App Activity to Google logging that the user has voted on a PhotoHunt
        /// photo.
        /// </summary>
        /// <param name="user">The user who has voted.</param>
        /// <param name="voteTarget">The photo the user has voted on.</param>
        public void WriteGooglePlusVoteAppActivity(User user, Photo voteTarget)
        {
            // Write an app activity for the vote.
            // Set the auth state in a the superclass for the authorization call.
            _authState = CreateState(user.googleAccessToken, user.googleRefreshToken,
                user.googleExpiresAt.AddSeconds(user.googleExpiresIn * -1), user.googleExpiresAt);

            AuthorizationServerDescription description =
                GoogleAuthenticationServer.Description;
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
            ItemScope result = new ItemScope();

            // The target (an image) will be parsed from this URL containing microdata.
            target.Url = BASE_URL + "photo.aspx?photoId=" + voteTarget.id;

            // Just use a static review result.
            result.Type = SCHEMA_REVIEW_TYPE;
            result.Name = "A vote for this PhotoHunt photo.";
            result.Url = target.Url;
            result.Text = "This photo embodies " + voteTarget.themeDisplayName;

            body.Target = target;
            body.Result = result;
            body.Type = REVIEW_ACTIVITY_TYPE;
            MomentsResource.InsertRequest insert =
                new MomentsResource.InsertRequest(
                    ps,
                    body,
                    "me",
                    MomentsResource.Collection.Vault);
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

    }
}
