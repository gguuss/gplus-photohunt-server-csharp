/*
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Web;

// For routing
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;

// For saving to DB
using PhotoHunt.model;
using System.Data;

// For utility methods
using PhotoHunt.utils;

// For JSON parsing.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `POST|GET|DELETE /api/photos`.
    /// Provides an API for working with photos. This servlet provides the
    /// /api/photos end point, which exposes the following operations:
    ///
    ///   GET /api/photos
    ///   GET /api/photos?photoId=1234
    ///   GET /api/photos?themeId=1234
    ///   GET /api/photos?userId=me
    ///   GET /api/photos?themeId=1234&userId=me
    ///   GET /api/photos?themeId=1234&userId=me&friends=true
    ///   POST /api/photos
    ///   DELETE /api/photos?photoId=1234
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class photos : jsonrest<Photo> {

        /// <summary>
        /// The route handler for the request, which retrieves and stores photos.
        /// </summary>
        /// <param name="context">The request and response context.</param>
        public override void ProcessRequest(HttpContext context)
        {
            User user = GetUser(context);

            PhotosHelper photosUtility = new PhotosHelper();

            bool hasThemeIdParam = (context.Request["themeId"] != null);
            bool hasUserIdParam = (context.Request["userId"] != null);
            bool hasPhotoIdParam = (context.Request["photoId"] != null);
            bool isFriends = (context.Request["friends"] != null &&
                context.Request["friends"].Equals("true"));

            int userId = 0, photoId = 0;

            Theme selectedTheme = ThemesHelper.GetSelectedTheme(context.Request["themeId"]);

            if (hasPhotoIdParam)
            {
                photoId = int.Parse(context.Request["photoId"]);
            }

            if (hasUserIdParam)
            {
                if (context.Request["userId"].Equals("me"))
                {
                    if (user != null)
                    {
                        userId = user.id;
                    }
                }
                else
                {
                    userId = int.Parse(context.Request["userId"]);
                }
            }


            // Handle file upload.
            if (context.Request.Files["image"] != null)
            {
                Photo dbPhoto = PhotoHunt.utils.PhotosHelper.UploadPhoto(context, user, 
                    selectedTheme);

                // Now that the photo has been uploaded to the application, write an app activity to
                // Google for the photo upload using the Google+ API.
                photosUtility.WriteGooglePlusPhotoAppActivity(user, dbPhoto);

                SendResponse(context, dbPhoto);
                return;
            }


            // Delete or return a single photo.
            if (hasPhotoIdParam)
            {
                // Retrieve the photo from the id specific to the PhotoHunt photos data.
                PhotohuntContext db = new PhotohuntContext();
                Photo selectedPhoto = db.Photos.First(p => p.id == photoId);

                // Remove the photo if this is a DELETE request.
                if (context.Request.RequestType.Equals("DELETE"))
                {
                    PhotoHunt.utils.PhotosHelper.DeletePhoto(context, user, selectedPhoto);
                }

                SendResponse(context, selectedPhoto);
                return;
            }


            // Return a list of photos based on the current request.
            SendResponse(context, PhotoHunt.utils.PhotosHelper.GetPhotos(hasThemeIdParam, 
                hasUserIdParam, isFriends, userId, selectedTheme));
            return;
        }

        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        public override IHttpHandler GetHttpHandler(RequestContext
            requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 (PHOTOS_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }
}
