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
using System.IO;
using System.Linq;

// For revocation and REST queries using HTTPRequest
using System.Net;
using System.Web;
using System.Web.Compilation;

// For string manipulations used in the template and string building
using System.Text;
using System.Text.RegularExpressions;

// For mapping routes
using System.Web.Routing;
using System.Web.SessionState;

// For JSON parsing
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Used for ResourceManager
using System.Resources;

// Used for jsonifiable
using PhotoHunt.model;

// Used for manually performing code exchange with the Google+ Sign-In button
using PhotoHunt.utils;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `POST /api/connect`.
    ///
    /// This endpoint receives the following JSON data in the request body.  The
    /// JSON data contains all of the information required to authorize or
    /// connect the current user.
    /// {
    ///   "state":"",
    ///   "access_token":"",
    ///   "token_type":"",
    ///   "expires_in":"",
    ///   "code":"",
    ///   "id_token":"",
    ///   "authuser":"",
    ///   "session_state":"",
    ///   "prompt":"",
    ///   "client_id":"",
    ///   "scope":"",
    ///   "g_user_cookie_policy":"",
    ///   "cookie_policy":"",
    ///   "issued_at":"",
    ///   "expires_at":"",
    ///   "g-oauth-window":""
    /// }
    ///
    /// Returns the following JSON response representing the PhotoHunt User that
    /// was connected:
    /// {
    ///   "id":0,
    ///   "googleUserId":"",
    ///   "googleDisplayName":"",
    ///   "googlePublicProfileUrl":"",
    ///   "googlePublicProfilePhotoUrl":"",
    ///   "googleExpiresAt":0
    /// }
    ///
    /// Issues the following errors along with corresponding HTTP response codes:
    /// 401: The error from the Google token verification end point.
    /// 500: "Failed to upgrade the authorization code." This can happen during
    ///      OAuth v2 code exchange flows.
    /// 500: "Failed to read token data from Google."
    ///      This response also sends the error from the token verification
    ///      response concatenated to the error message.
    /// 500: "Failed to query the Google+ API: "
    ///      This error also includes the error from the client library
    ///      concatenated to the error response.
    /// 500: "IOException occurred." The IOException could happen when any
    ///      IO-related errors occur such as network connectivity loss or local
    ///      file-related errors.
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class connect : jsonrest<JsonUser>
    {
        /// <summary>
        /// The route handler for the request which connects the PhotoHunt
        /// user to Google+.
        /// </summary>
        /// <param name="context">The request/response context.</param>
        public override void ProcessRequest(HttpContext context)
        {
            User user = GetUser(context);
            ConnectHelper connectHelper = new ConnectHelper();

            if (user == null)
            {
                if (context.Request.Cookies[Properties.Resources.MOBILE_SESSION_COOKIEID] != null)
                {
                    context.Request.Cookies.Remove(Properties.Resources.MOBILE_SESSION_COOKIEID);
                }

                // Get the authorization code from the request POST body.
                StreamReader sr = new StreamReader(
                    context.Request.InputStream);
                string input = sr.ReadToEnd();

                TokenData td = (TokenData)(new TokenData().FromJson(input));

                // Manually perform the OAuth2 flow for now.
                // TODO(class) Use the library for code exchange once
                // "postmessage" no longer throws exceptions in URI.
                if (td.code != null)
                {
                    var authObject = ManualCodeExchanger.ExchangeCode(td.code);

                    // Create an authorization state from the returned token.
                    _authState = CreateState(
                        authObject.access_token, authObject.refresh_token,
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddSeconds(authObject.expires_in));
                }
                else
                {
                    // Create an authorization state from the returned token.
                    _authState = CreateState(
                        td.access_token, td.refresh_token,
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddSeconds(td.expires_in));
                }

                PhotoHunt.utils.ConnectHelper.VerifyToken(_authState);

                user = connectHelper.SaveTokenForUser(_authState);
                context.Session[Properties.Resources.CURRENT_USER_SESSION_KEY] = user.ToJson();
            }

            SendResponse(context, new JsonUser(user));
        }


        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this
        /// IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns>The handler for this route.</returns>
        public override IHttpHandler GetHttpHandler(RequestContext
            requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 (CONNECT_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }

    /// <summary>
    /// A simple class for JSON data that is sent and received from
    /// PhotoHunt clients.
    /// </summary>
    public class TokenData: Jsonifiable<TokenData> {
        public String access_token { get; set; }
        public String refresh_token { get; set; }
        public String code { get; set; }
        public String id_token { get; set; }
        public long issued_at { get; set; }
        public long expires_in { get; set; }
    }

}
