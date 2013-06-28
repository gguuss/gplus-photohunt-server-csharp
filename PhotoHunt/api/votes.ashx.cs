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
using System.Collections.Generic;
using System.Linq;
using System.Web;

// For mapping routes.
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;

// For entity framework constants;
using System.Data;

// For reading the PUT data
using System.IO;

// For data members.
using PhotoHunt.model;
using PhotoHunt.utils;

// For JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `PUT /api/votes`.
    ///
    /// Expects a request payload to contain a JSON object with the photo ID that the
    /// authenticated user is voting on.
    ///
    /// {
    ///   "photoId":0
    /// }
    ///
    /// Returns the following JSON response representing the Photo for which the user voted.
    ///
    /// {
    ///   "id":0,
    ///   "ownerUserId":0,
    ///   "ownerDisplayName":"",
    ///   "ownerProfileUrl":"",
    ///   "ownerProfilePhoto":"",
    ///   "themeId":0,
    ///   "themeDisplayName":"",
    ///   "numVotes":1,
    ///   "voted":true,
    ///   "created":0,
    ///   "fullsizeUrl":"",
    ///   "thumbnailUrl":"",
    ///   "voteCtaUrl":"",
    ///   "photoContentUrl":""
    /// }
    ///
    /// Issues the following errors along with corresponding HTTP response codes:
    /// 401: "Unauthorized request" indicates there is not a user logged in.
    /// 401: "Access token expired" indicates there is a logged in user, but
    ///      the user doesn’t have a refresh token and their access token is
    ///      expiring in less than 100 secs. To resolve this, get a new token
    ///      and retry the API call.
    /// 500: "Error writing app activity: " indicates there was an unexpected
    ///      while writing the app activity and the error from the Google+ API
    ///      call is returned.
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class votes : jsonrest<Photo>
    {
        /// <summary>
        /// The route handler for the request, increments vote counts.
        /// </summary>
        /// <param name="context">The request and response context.</param>
        public override void ProcessRequest(HttpContext context)
        {
            Photo voteTarget = null;
            VotesHelper votesHelper = new VotesHelper();

            if (context.Request.RequestType.Equals("PUT"))
            {
                StreamReader stream = new StreamReader(context.Request.InputStream);
                string voteJson = stream.ReadToEnd();
                PhotoRequest photoRequest = (PhotoRequest)new PhotoRequest().FromJson(voteJson);
                voteTarget = votesHelper.DoVote(context, photoRequest.photoId);
            }

            if (voteTarget == null)
            {
                SendError(context.Response, 401, "Unauthorized request.");
            }
            else
            {
                SendResponse(context, voteTarget);
            }
        }

        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns></returns>
        public override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 (VOTES_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }

        /// <summary>
        /// A simple class for the JSON data passed as PUT data.
        /// </summary>
        public class PhotoRequest : Jsonifiable<PhotoRequest>
        {
            public int photoId;
        }
    }
}
