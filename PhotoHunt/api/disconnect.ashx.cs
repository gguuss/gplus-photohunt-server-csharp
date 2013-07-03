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

// For mapping routes.
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;

// For entity framework constants;
using System.Data;

// For JSON
using Newtonsoft.Json;

// For the disconnect helper and data access.
using PhotoHunt.model;
using PhotoHunt.utils;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `POST /api/disconnect`.
    ///
    /// As required by the Google+ Platform Terms of Service, this end point
    /// performs the following actions:
    ///
    ///   1. Deletes all data that the app retrieved from Google+ that is
    ///      stored in the app.
    ///   2. Revokes all of the user's tokens that are issued to this app.
    ///
    /// Disconnects the user that is currently identified by their session. Do
    /// not supply a request body with this method.
    ///
    /// If successful, returns "Successfully disconnected."
    ///
    /// Issues the following errors along with corresponding HTTP response codes:
    /// 401: "Unauthorized request".  This indicates that there was not a
    ///      connected User to disconnect.
    /// 500: "Failed to revoke token for given user: " Also includes the error
    ///      from the revoke end point.
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class disconnect : jsonrest<Object>
    {
        /// <summary>
        /// The route handler for the request, which removes the user and
        /// disconnects their Google+ account from this app.
        /// </summary>
        /// <param name="context">The request/response context.</param>
        public override void ProcessRequest(HttpContext context)
        {
            // User is signed in.
            User user = GetUser(context);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Unauthorized request";
            }
            else
            {
                // Remove the cached credentials.
                context.Session[Properties.Resources.CURRENT_USER_SESSION_KEY] = null;
                context.Response.Cookies.Clear();

                // Load the User object that is to be deleted from the model.
                PhotohuntContext db = new PhotohuntContext();
                User toRemove = db.Users.Find(user.id);

                // Clean up the user data by removing votes and friend edges.
                DisconnectHelper.RemoveFriendEdges(toRemove);
                DisconnectHelper.RemoveVotes(toRemove);

                db.SaveChanges();

                // Disconnect the user's account from this app.
                if (DisconnectHelper.DisconnectAccount(context, toRemove))
                {
                    context.Response.StatusCode = 200;
                    context.Response.Write("Successfully disconnected.");
                }
            }
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
                 (DISCONNECT_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }
}
