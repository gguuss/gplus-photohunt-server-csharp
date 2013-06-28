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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PhotoHunt.model;

// For JSON parsing.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PhotoHunt
{
    /// <summary>
    /// This page is used for testing the DB.  You can wipe out the stored data
    /// by loading the page with the clear variable set. For example:
    ///  http://localhost:8080/admin/showDb.aspx?clear=yes
    /// </summary>
    public partial class showDb : System.Web.UI.Page
    {
        public bool showEdges = false;
        public User user;
        public Theme latestTheme = new Theme();
        public List<Theme> themes = new List<Theme>();
        public List<Photo> photos = new List<Photo>();
        public List<User> usersList = new List<User>();
        public int userCount = 0;
        public List<DirectedUserToEdge> edges = new List<DirectedUserToEdge>();

        /// <summary>
        /// This is the code behind for the database inspector.
        /// All of the data used in the view is retrieved here.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            User user = null;
            try
            {
                user = new User((JObject)JsonConvert.DeserializeObject(
                    Session[Properties.Resources.CURRENT_USER_SESSION_KEY].ToString())
                );
            }
            catch (Exception nre)
            {
                // This is fine because the interface could be accessed by someone not logged in.
                // If you wanted to add the notion of an administrator, you could add it to the
                // User model and enforce it here.
                Debug.WriteLine("Could not get user: " + nre.StackTrace.ToString());
            };

            PhotohuntContext db = new PhotohuntContext();
            if (Request.Params["clear"] != null)
            {
                db.Database.Delete();
                Session[Properties.Resources.CURRENT_USER_SESSION_KEY] = null;
            }

            var themeQuery = from b in db.Themes
                             orderby b.start descending
                             select b;
            foreach (Theme t in themeQuery)
            {
                latestTheme = t;
                themes.Add(t);
            }

            var photoQuery = from p in db.Photos
                             select p;
            foreach (Photo photo in photoQuery)
            {
                photos.Add(photo);
            }

            var userQuery = from u in db.Users
                            orderby u.googleDisplayName
                            select u;
            foreach (User dbUser in userQuery)
            {
                usersList.Add(dbUser);
                userCount++;
            }

            bool showEdges = Request.Params["edges"] != null;
            if (showEdges)
            {
                var edgeQuery = from edge in db.Edges
                                select edge;
                foreach (DirectedUserToEdge du in edgeQuery)
                {
                    edges.Add(du);
                }
            }
        }
    }
}
