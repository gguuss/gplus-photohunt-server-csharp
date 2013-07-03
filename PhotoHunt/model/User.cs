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
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace PhotoHunt.model
{
    /// <summary>
    /// The user class abstracts users, which are specific to PhotoHunt.
    /// </summary>
    public class User: Jsonifiable<User>
    {
        public User()
        {
        }
        public User(JObject JSONUser)
        {
            this.id = int.Parse(JSONUser["id"].ToString());
            this.googleUserId=JSONUser["googleUserId"].ToString();
            this.googleRefreshToken=JSONUser["googleRefreshToken"].ToString();
            this.googlePublicProfileUrl=JSONUser["googlePublicProfileUrl"].ToString();
            this.googleExpiresIn=int.Parse(JSONUser["googleExpiresIn"].ToString());
            this.googleExpiresAt=DateTime.Parse(JSONUser["googleExpiresAt"].ToString());
            this.googleDisplayName=JSONUser["googleDisplayName"].ToString();
            this.googleAccessToken=JSONUser["googleAccessToken"].ToString();
            this.email=JSONUser["email"].ToString();
            this.googlePublicProfilePhotoUrl=JSONUser["googlePublicProfilePhotoUrl"].ToString();
        }

        /// <summary>
        /// The primary identifier for this user, specific to PhotoHunt.
        /// </summary>
        [Key]
        public int id { get; set; }

        /// <summary>
        /// Primary email address of this user.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// The UUID identifier of this user within Google products.
        /// </summary>
        public String googleUserId { get; set; }

        /// <summary>
        /// The display name for this user has chosen for Google products.
        /// </summary>
        public string googleDisplayName { get; set; }

        /// <summary>
        /// Public Google+ profile URL for this user.
        /// </summary>
        public string googlePublicProfileUrl { get; set; }

        /// <summary>
        /// Public Google+ profile image for this user.
        /// </summary>
        public string googlePublicProfilePhotoUrl { get; set; }

        /// <summary>
        /// The access token for accessing Google APIs.
        /// </summary>
        public string googleAccessToken { get; set; }

        /// <summary>
        /// The refresh token for accessing Google APIs.
        /// </summary>
        public string googleRefreshToken { get; set; }

        /// <summary>
        /// The validity of this user's googleAccessToken in seconds.
        /// </summary>
        public int googleExpiresIn { get; set; }

        /// <summary>
        /// The expiration time in milliseconds since Epoch for this user's
        /// googleAccessToken.
        /// </summary>
        public DateTime googleExpiresAt { get; set; }
    }

    /// <summary>
    /// The user class abstracts users, which are specific to PhotoHunt.
    /// </summary>
    public class JsonUser : Jsonifiable<JsonUser>
    {
        public JsonUser(User u)
        {
            id = u.id;
            googleUserId = u.googleUserId;
            googleDisplayName = u.googleDisplayName;
            googlePublicProfileUrl = u.googlePublicProfileUrl;
            googlePublicProfilePhotoUrl = u.googlePublicProfilePhotoUrl;
            googleExpiresAt = (long)ConvertToUnixTimestamp(u.googleExpiresAt);
        }

        /// <summary>
        /// The primary identifier for this user, specific to PhotoHunt.
        /// </summary>
        [Key]
        public int id { get; set; }

        /// <summary>
        /// The UUID identifier of this user within Google products.
        /// </summary>
        public String googleUserId { get; set; }

        /// <summary>
        /// The display name for this user has chosen for Google products.
        /// </summary>
        public string googleDisplayName { get; set; }

        /// <summary>
        /// Public Google+ profile URL for this user.
        /// </summary>
        public string googlePublicProfileUrl { get; set; }

        /// <summary>
        /// Public Google+ profile image for this user.
        /// </summary>
        public string googlePublicProfilePhotoUrl { get; set; }

        /// <summary>
        /// The expiration time in milliseconds since Epoch for this user's
        /// googleAccessToken.
        /// </summary>
        public long googleExpiresAt { get; set; }
    }
}
