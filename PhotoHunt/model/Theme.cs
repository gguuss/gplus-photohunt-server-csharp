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
    /// The model for Themes.
    /// </summary>
    public class Theme: Jsonifiable<Theme>
    {
        /// <summary>
        /// The unique ID for this theme.
        /// </summary>
        [Key]
        public int id { get; set; }

        /// <summary>
        /// The name shown for this theme.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// The created date for the theme.
        /// </summary>
        public DateTime createdTime { get; set; }

        /// <summary>
        /// A string representing the time.
        /// </summary>
        public string created { get; set; }

        /// <summary>
        /// The date that this theme will be starting for
        /// PhotoHunt.
        /// </summary>
        public DateTime start { get; set; }

        /// <summary>
        /// The preview id associated with this theme.
        /// </summary>
        public int previewPhotoId { get; set; }
    }

    /// <summary>
    /// A convenience class to avoid conversions between Dates used
    /// by EF and the millisecond dates used by clients.
    /// </summary>
    public class JsonTheme : Jsonifiable<JsonTheme>
    {
        public JsonTheme(Theme t)
        {
            id = t.id;
            displayName = t.displayName;

            start = (long)ConvertToUnixTimestamp(t.start);

            // PhotoHunt is using "created" as a selector on themes. For now,
            // just rewrite start to created.
            //created = (long)ConvertToUnixTimestamp(t.createdTime);

            created = (long)ConvertToUnixTimestamp(t.start);
        }

        /// <summary>
        /// The unique ID for this theme.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// The name shown for this theme.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// A string representing the time.
        /// </summary>
        public long created { get; set; }

        /// <summary>
        /// The date that this theme will be starting for
        /// PhotoHunt.
        /// </summary>
        public long start { get; set; }

    }
}
