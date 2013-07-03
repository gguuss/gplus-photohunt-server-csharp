/*
 * Copyright (c) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions and limitations under
 * the License.
 */
using System;

using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace PhotoHunt
{
    public class Global : System.Web.HttpApplication
    {
        public void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// At the beginning of each request, set any required headers for CORS and copy cookies for
        /// mobile sessions.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Debugging
            System.Diagnostics.Debug.WriteLine("Starting request: " +
                    HttpContext.Current.Request.RawUrl.ToString());

            // For CORS on the troublesome JavaScript uploader
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

            // Inject the session ID for mobile clients.
            if (HttpContext.Current.Request.Cookies[Properties.Resources.MOBILE_SESSION_COOKIEID] !=
                    null)
            {
                HttpCookie mobileCookie = HttpContext.Current.Request.Cookies.Get(
                        Properties.Resources.MOBILE_SESSION_COOKIEID);
                HttpCookie serverCookie = new HttpCookie(
                        Properties.Resources.SERVER_SESSION_COOKIEID);
                serverCookie.Value = mobileCookie.Value;
                HttpContext.Current.Request.Cookies.Add(serverCookie);
            }
        }

        /// <summary>
        /// Routes all of the apis to restful endpoints, forwards HTML requests to ASPs, and
        /// reroutes all other requests to the AngularJS front-end.
        /// </summary>
        static protected void RegisterRoutes(RouteCollection routes)
        {
            // API routes
            routes.Add("api/connect", new Route("api/connect", new api.connect()) );
            routes.Add("api/disconnect", new Route("api/disconnect", new api.disconnect()));
            routes.Add("api/friends", new Route("api/friends", new api.friends()));
            routes.Add("api/images", new Route("api/images", new api.images()));
            routes.Add("api/photos", new Route("api/photos", new api.photos()));
            routes.Add("api/themes", new Route("api/themes", new api.themes()));
            routes.Add("api/votes", new Route("api/votes", new api.votes()));

            // HTML pages to be handled by ASP.
            routes.MapPageRoute("photo", "photo.html", "~/photo.aspx");
            routes.MapPageRoute("invite", "invite.html", "~/invite.aspx");

            // Catch-all
            routes.Add("*", new Route("{*url}", new utils._default()));
        }

    }
}
