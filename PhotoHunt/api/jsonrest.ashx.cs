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
using System.Diagnostics;
using System.IO;
using System.Linq;


// For mapping routes
using System.Net;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;


// For JSON parsing.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Used for ResourceManager
using System.Resources;

// Used for the user model.
using PhotoHunt.model;

// Used for Google+ services
using PhotoHunt.utils;

namespace PhotoHunt.api
{
    /// <summary>
    /// A superclass that implements various functions that are shared across
    /// all of the ashx handlers.
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class jsonrest<T> : PlusHelper, IHttpHandler, IRequiresSessionState, IRouteHandler
    {
        // Endpoint virtual path definitions used for routing API paths. Additional route
        // configuration is in Global.asax.cs.
        protected const string CONNECT_ENDPOINT_PATH = "~/api/connect.ashx";
        protected const string DISCONNECT_ENDPOINT_PATH = "~/api/disconnect.ashx";
        protected const string FRIENDS_ENDPOINT_PATH = "~/api/friends.ashx";
        protected const string IMAGES_ENDPOINT_PATH = "~/api/images.ashx";
        protected const string PHOTOS_ENDPOINT_PATH = "~/api/photos.ashx";
        protected const string THEMES_ENDPOINT_PATH = "~/api/themes.ashx";
        protected const string VOTES_ENDPOINT_PATH = "~/api/votes.ashx";


        /// <summary>
        /// Processes the request based on the path.
        /// </summary>
        /// <param name="context">Contains the request and response.</param>
        public virtual void ProcessRequest(HttpContext context)
        {
        }

        /// <summary>
        /// Send the error in the response headers.
        /// </summary>
        /// <param name="resp">The response to use for transmitting the error.</param>
        /// <param name="code">The HTTP response error code to issue.</param>
        /// <param name="message">The message to attach to the response.</param>
        protected void SendError(HttpResponse resp, int code, String message)
        {
            resp.StatusCode = code;
            resp.StatusDescription = message;
        }

        /// <summary>
        /// The mobile applications forge a session using a specific identifier (JSESSIONID).
        /// This method allows the server to emulate the session by writing the current session
        /// identifier for the hard-coded mobile versions.
        /// </summary>
        protected void SaveToMobileSession()
        {
            HttpCookie serverCookie = HttpContext.Current.Request.Cookies.Get(
                    Properties.Resources.SERVER_SESSION_COOKIEID);
            if (serverCookie != null)
            {
                HttpCookie mobileCookie = new HttpCookie(
                        Properties.Resources.MOBILE_SESSION_COOKIEID);
                mobileCookie.Value = serverCookie.Value;
                HttpContext.Current.Response.Cookies.Add(mobileCookie);
            }
            if (HttpContext.Current.Request.Cookies.Get(
                    Properties.Resources.SERVER_SESSION_COOKIEID) != null)
            {
                HttpContext.Current.Request.Cookies.Remove(
                        Properties.Resources.SERVER_SESSION_COOKIEID);
            }
        }

        /// <summary>
        /// Send an object to the client as JSON.
        /// </summary>
        /// <param name="context">Contains the request and response used to send the object.</param>
        /// <param name="body">The object used to generate a response.</param>
        protected void SendResponse(HttpContext context, Jsonifiable<T> responseObject)
        {
            SaveToMobileSession();

            context.Response.ContentType = "application/json";
            try
            {
                context.Response.Write(responseObject.ToJson());
            }
            catch (IOException e)
            {
                SendError(context.Response, 500, "Handler received an IOException while trying to" +
                        " write the response body." + e.InnerException);
            }
            return;
        }

        /// <summary>
        /// Send a list of objects as JSON to the client in the response.
        /// </summary>
        /// <param name="context">Contains the request and response used to send the object.</param>
        /// <param name="body">The objects used to generate a response.</param>
        protected void SendResponse(HttpContext context, List<T> responseObjects)
        {
            SaveToMobileSession();

            context.Response.ContentType = "application/json";

            try
            {
                // For the iOS client, we must return the items and type separate in the collection.
                if (context.Request.Params["items"] != null &&
                        context.Request.Params["items"].Equals("true"))
                {
                    context.Response.Write(JsonConvert.SerializeObject(
                            new Itemifiable<T>(responseObjects, "photohunt#" +
                            context.Request.Url.Segments[2])
                            ));
                }
                else
                {
                    context.Response.Write(JsonConvert.SerializeObject(responseObjects));
                }
            }
            catch (IOException e)
            {
                SendError(context.Response, 500, "Handler received an IOException while trying to" +
                        " write the response body." + e.InnerException);
            }
            return;
        }

        /// <summary>
        /// Checks to see if the user exists within the context session.
        /// </summary>
        /// <param name="context">The current handler's context.</param>
        protected void CheckAuthorization(HttpContext context)
        {
            User currentUser = GetUser(context);

            if (currentUser == null)
            {
                throw new UserNotAuthorizedException();
            }
        }


        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns></returns>
        public virtual IHttpHandler GetHttpHandler(RequestContext
            requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 ("~/jsonrest.ashx", typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }

        public bool IsReusable { get { return false; } }
    }
}
