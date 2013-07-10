using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHunt.model
{
    /// <summary>
    /// A container for the upload URL.
    /// </summary>
    public class UploadUrl: Jsonifiable<UploadUrl>
    {
       
        /// <summary>
        /// The URL returned from the api/photos endpoint that will be returned in JSON.
        /// </summary>
        public string url { get; set; }
    }
}