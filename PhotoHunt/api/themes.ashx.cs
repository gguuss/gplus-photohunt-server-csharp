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
using PhotoHunt.utils;

namespace PhotoHunt.api
{
    /// <summary>
    /// Exposed as `GET /api/themes`.  Retrieves the current theme. If a theme
    /// does not exist, the default theme of "Beautiful" is created and returned
    /// to make the app easier to get up and running.
    ///
    /// Returns the following JSON response representing a list of Themes before
    /// today.
    ///
    /// [
    ///   {
    ///     "id":0,
    ///     "displayName":"",
    ///     "created":0,
    ///     "start":0
    ///   },
    ///   ...
    /// ]
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class themes : jsonrest<JsonTheme>
    {

        /// <summary>
        /// The route handler for the request, which selects the current themes
        /// and returns them as JSON. If the current theme for today does not
        /// exist, the app creates a new theme for today.
        /// </summary>
        /// <param name="context">The context containing the request and
        /// response.</param>
        public override void ProcessRequest(HttpContext context)
        {
            Theme currentTheme = null;
            ArrayList themes = new ArrayList();

            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime tomorrow = today.AddDays(1);

            // Select the historical list of themes
            PhotoHunt.model.PhotohuntContext db = new PhotoHunt.model.PhotohuntContext();
            var themeQuery = from b in db.Themes
                             where b.start < DateTime.Now
                             orderby b.start descending
                             select b;
            foreach (Theme t in themeQuery)
            {
                themes.Add(new JsonTheme(t));
                if (t.start < tomorrow && t.start >= today)
                {
                    currentTheme = t;
                }
            }


            // If no theme exists for today, create the "beautiful" theme and add it.
            if (currentTheme == null)
            {
                themes.Add(new JsonTheme(ThemesHelper.GenerateDefaultTheme()));
            }

            SendResponse(context, themes);
        }

        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns>The handler for rendering this route.</returns>
        public override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 (THEMES_ENDPOINT_PATH, typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }
    }
}
