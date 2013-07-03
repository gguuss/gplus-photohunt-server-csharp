using System;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

// For jsonrest class
using PhotoHunt.api;

namespace PhotoHunt.utils
{
    /// <summary>
    /// This route is used to catch all unmatched and redirect to index.html for the
    /// AngularJS front-end.
    /// </summary>
    public class _default : IHttpHandler, IRouteHandler
    {
        /// <summary>
        /// Handler to redirect to the AngularJS front-end.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Redirect(PhotoHunt.api.jsonrest<Object>.BASE_URL + "index.html");
        }

        /// <summary>
        /// Implements IRouteHandler interface for mapping routes to this IHttpHandler.
        /// </summary>
        /// <param name="requestContext">Information about the request.
        /// </param>
        /// <returns>The handler for this route.</returns>
        public IHttpHandler GetHttpHandler(RequestContext
            requestContext)
        {
            var page = BuildManager.CreateInstanceFromVirtualPath
                 ("~/utils/default.ashx", typeof(IHttpHandler)) as IHttpHandler;
            return page;
        }

        public bool IsReusable { get { return false; } }
    }
}
