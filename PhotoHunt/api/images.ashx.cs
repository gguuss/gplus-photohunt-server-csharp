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

// For mapping routes.
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;

// For entity framework constants;
using System.Data;

// For data members.
using PhotoHunt.model;

// For JSON parsing.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `POST /api/images`.
    ///
    /// Creates and returns a URL that can be used to upload an image for a
    /// photo. Returned URL, after receiving an upload, will fire a callback
    /// (resend the entire HTTP request) to /api/photos. This is done to
    /// simulate storage solutions where a POST URI is returned from an API.
    ///
    /// Takes no request payload.
    ///
    /// Returns the following JSON response representing an upload URL:
    ///
    /// "http://appid.appspot.com/_ah/upload/upload-key"
    ///
    /// Issues the following errors along with corresponding HTTP response codes:
    /// 401: "Unauthorized request"
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class images : jsonrest<images>
    {
        /// <summary>
        /// The route handler for the request, maps images to photos to simulate
        /// an image upload service.
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

            if (context.Request.Files["image"] == null)
            {
                // Note, this is a NOOP that is used to simulate an upload service.
                // The returned URL is /photos
                context.Response.ContentType = "application/json";
                string redir = BASE_URL + "api/photos";
                context.Response.Write("{\"url\" : \"" + redir +"\"}");
                return;
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
                (IMAGES_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }
}
