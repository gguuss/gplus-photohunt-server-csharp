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

// For stream classes and methods
using System.IO;

// For JSON parsing.
using Newtonsoft.Json;

namespace PhotoHunt.model
{
    /// <summary>
    /// The Jsonifiable object is the superclass for data models so that they
    /// can easily be serialized to JSON.
    /// </summary>
    public abstract class Jsonifiable <T>
    {
        /// <summary>
        /// Instantiates this Jsonifiable object from its primary key in
        /// the database.
        /// </summary>
        /// <param name="id"></param>
        public virtual T FromId(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an object given a JSON string.
        /// </summary>
        /// <param name="json">The string to deserialize into an object.</param>
        /// <returns></returns>
        public T FromJson(string json)
        {
            return (T)JsonConvert.DeserializeObject(json, typeof(T));
        }

        /// <summary>
        /// Converts the object to JSON.
        /// </summary>
        /// <returns>The current object serialized to JSON.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
        }

        /// <summary>
        /// Convert a DateTime to UNIX time for clients.
        /// </summary>
        /// <param name="date">The DateTime object to be converted.</param>
        /// <returns>A double representing the time as milliseconds.</returns>
        public double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }
    }

    /// <summary>
    /// The Itemifiable object is used for iOS clients which expect "items" and "kind"
    /// in responses.
    /// </summary>
    public class Itemifiable <T> : Jsonifiable <T>
    {
        /// <summary>
        /// Constructs the Itemifiable type given a list of resources and type.
        /// </summary>
        /// <param name="collection">The list of resources to be returned in JSON as items.</param>
        /// <param name="type">The type of resource to be returned in JSON as kind.</param>
        public Itemifiable(ArrayList collection, string type)
        {
            items = collection;
            kind = type;
        }

        /// <summary>
        /// A list of Jsonifiable objects that represent this resource collection.
        /// </summary>
        public ArrayList items { get; private set; }

        /// <summary>   
        /// The type of collection, for example "photohunt#themes".
        /// </summary>
        public string kind { get; set; }
    }
}
