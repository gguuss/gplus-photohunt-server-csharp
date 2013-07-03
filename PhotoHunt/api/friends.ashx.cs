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
using System.Linq;
using System.Web;

// For routing
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;

// For JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For getting the friends list.
using PhotoHunt.model;
using PhotoHunt.utils;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `GET /api/friends`.
    ///
    /// Identifies the incoming user using their session.
    ///
    /// Returns the following JSON response that represents the people connected
    /// to the authenticated user:
    /// [
    ///   {
    ///     "id":0,
    ///     "googleUserId":"",
    ///     "googleDisplayName":"",
    ///     "googlePublicProfileUrl":"",
    ///     "googlePublicProfilePhotoUrl":"",
    ///     "googleExpiresAt":0
    ///   },
    ///   ...
    /// ]
    ///
    /// Issues the following errors along with corresponding HTTP response codes:
    /// 401: "Unauthorized request"
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class friends : jsonrest<JsonUser>
    {
        /// <summary>
        /// The route handler for the request, which lists friends for this user to
        /// enable a social graph within the app.
        /// </summary>
        /// <param name="context">The request/response context.</param>
        public override void ProcessRequest(HttpContext context)
        {
            User user = GetUser(context);

            // User will be NULL if there isn't someone signed in.
            if (user == null)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Unauthorized request.";
                return;
            }

            ArrayList friends = FriendsHelper.GetFriends(user);

            context.Response.Write(JsonConvert.SerializeObject(friends));
        }

        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this
        /// IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns></returns>
        public override IHttpHandler GetHttpHandler(RequestContext
            requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 (FRIENDS_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }
}
