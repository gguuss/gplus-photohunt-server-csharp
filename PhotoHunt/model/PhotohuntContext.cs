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
using System.Data.Entity;

namespace PhotoHunt.model
{
    /// <summary>
    /// The Entity Framework context used for getting the data objects
    //  appropriate to the app model.
    /// </summary>
    public class PhotohuntContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DirectedUserToEdge> Edges { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Vote> Votes { get; set; }
    }
}
