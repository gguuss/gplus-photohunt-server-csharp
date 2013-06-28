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
using System.ComponentModel.DataAnnotations;

namespace PhotoHunt.model
{
    /// <summary>
    /// The vote class abstracts votes for photos.
    /// </summary>
    public class Vote : Jsonifiable<Vote>
    {
        /// <summary>
        /// The vote identifier.
        /// </summary>
        [Key]
        public int id { get; set; }

        /// <summary>
        /// The Google+ user ID associated with this vote.
        /// </summary>
        public int ownerUserId { get; set; }

        /// <summary>
        /// The photo associated with this vote.
        /// </summary>
        public int photoId { get; set; }
    }
}
