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

namespace PhotoHunt.model
{
    /// <summary>
    /// The Photo class encapsulates the data for photos.
    /// that are voted on. Note that the user is denormalized into
    /// this object.
    ///
    /// @author class@google.com (Gus Class)
    /// </summary>
    public class Photo: Jsonifiable<Photo>
    {
        /// <summary>
        /// The unique identifier for this photo.
        /// </summary>
        [Key]
        public int id { get; set; }

        /// <summary>
        /// The PhotoHunt user ID for the photo owner.
        /// </summary>
        public int ownerUserId { get; set; }


        /// <summary>
        /// Denormalized user data (name) for rendering the UI.
        /// </summary>
        public string ownerDisplayName { get; set; }

        /// <summary>
        /// Denormalized user data (profile URL) for rendering the UI.
        /// </summary>
        public string ownerProfileUrl { get; set; }

        /// <summary>
        /// Denormalized user data (profile photo) for rendering the UI.
        /// </summary>
        public string ownerProfilePhoto { get; set; }

        /// <summary>
        /// The identifier for the theme that is associated with this photo,
        /// which is 0 for the default theme.
        /// </summary>
        public int themeId { get; set; }

        /// <summary>
        /// Denormalized theme data for rendering UI.
        /// </summary>
        public string themeDisplayName { get; set; }

        /// <summary>
        /// A count of votes for this photo.
        /// </summary>
        public int numVotes { get; set; }

        /// <summary>
        /// Indicates whether the current user has voted for this photo.
        /// </summary>
        public bool voted { get; set; }

        /// <summary>
        /// The created time in milliseconds since the epoch.
        /// </summary>
        public long created { get; set; }

        /// <summary>
        /// The full photo URL, which can be any size.
        /// </summary>
        public string fullsizeUrl { get; set; }

        /// <summary>
        /// The thumbnail for the Photo (<500px wide).
        /// </summary>
        public string thumbnailUrl { get; set; }

        /// <summary>
        /// The call-to-action URI for the vote button on interactive posts for
        /// this photo.
        /// </summary>
        public string voteCtaUrl { get; set; }

        /// <summary>
        /// The URL for linking to this photo.
        /// </summary>
        public string photoContentUrl { get; set; }
    }

}
