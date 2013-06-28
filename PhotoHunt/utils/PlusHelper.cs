using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

// Libraries for Google APIs
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;

// For JSON parsing.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For OAuth2
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

// For accessing database model
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    public class PlusHelper
    {

        // Requires the trailing "/"
        public static string BASE_URL = "http://localhost:8080/";

        // Get this from https://code.google.com/apis/console
        public static string CLIENT_ID = "YOUR_CLIENT_ID";
        public static string CLIENT_SECRET = "YOUR_CLIENT_SECRET";

        // Constants for schema types.
        public static string SCHEMA_REVIEW_TYPE = "http://schema.org/Review";

        // Constants for app activity types.
        public static string REVIEW_ACTIVITY_TYPE = "http://schemas.google.com/ReviewActivity";
        public static string ADD_ACTIVITY_TYPE = "http://schemas.google.com/AddActivity";


        // Used internally by the OAuth client
        protected IAuthorizationState _authState;

        // Used to perform API calls against Google+.
        protected PlusService ps = null;

        /// <summary>
        /// Generates the credentials needed for the library given the current user stored in
        /// the session.
        /// </summary>
        /// <param name="context">The current handler's context.</param>
        /// <returns></returns>
        protected IAuthorizationState GetCredentialFromLoggedInUser(
                HttpContext context)
        {
            User user = (User)context.Session[Properties.Resources.CURRENT_USER_SESSION_KEY];
            return CreateState(user.googleAccessToken, user.googleRefreshToken,
                    user.googleExpiresAt.Subtract(
                            new TimeSpan(user.googleExpiresIn * TimeSpan.TicksPerSecond)),
                    new DateTime(user.googleExpiresIn));
        }

        /// <summary>
        /// Gets the authorization object for the client-side flow.
        /// </summary>
        /// <param name="client">The client used for authorization.
        /// </param>
        /// <returns>An authorization state that can be used for API queries.
        /// </returns>
        protected IAuthorizationState GetAuthorization(WebServerClient client)
        {
            // If we don't yet have user, use the client to perform
            // authorization.
            if (_authState != null)
            {
                HttpRequestInfo reqinfo =
                    new HttpRequestInfo(HttpContext.Current.Request);
                client.ProcessUserAuthorization(reqinfo);
            }

            // Check for a cached session state.
            if (_authState == null)
            {
                _authState = (IAuthorizationState)HttpContext.Current.
                        Session["AUTH_STATE"];
            }

            // Check if we need to refresh the authorization state and refresh
            // it if necessary.
            if (_authState != null)
            {
                if (_authState.RefreshToken.IsNotNullOrEmpty() && (_authState.AccessToken == null ||
                    DateTime.UtcNow > _authState.AccessTokenExpirationUtc))
                {
                    client.RefreshToken(_authState);
                }
                return _authState;
            }

            // If we fall through to here, perform an authorization request.
            OutgoingWebResponse response =
                client.PrepareRequestUserAuthorization();

            response.Send();
            // Note: response.send will throw a ThreadAbortException to
            // prevent sending another response.
            return null;
        }

        /// <summary>
        /// The CreateState function will generate a state that can be
        /// used to initialize the PlusWrapper.
        /// </summary>
        /// <param name="accessToken">An access token string from an
        /// OAuth2 flow.</param>
        /// <param name="refreshToken">A refresh token string from an
        /// OAuth2 flow.</param>
        /// <param name="issued">A DateTime object representing the time
        /// that the token was issued.</param>
        /// <param name="expires">A DateTime object indicating when the
        /// token expires.</param>
        /// <returns></returns>
        static public IAuthorizationState CreateState(
            string accessToken, string refreshToken, DateTime issued,
            DateTime expires)
        {
            IAuthorizationState state = new AuthorizationState();
            state.AccessToken = accessToken;
            state.RefreshToken = refreshToken;
            state.AccessTokenIssueDateUtc = issued;
            state.AccessTokenExpirationUtc = expires;
            return state;
        }

        /// <summary>
        /// Gets the currently logged in user.
        /// </summary>
        /// <param name="context">The context containing the session to get the user from.</param>
        /// <returns>A PhotoHunt User object representing the currently logged in user on success;
        /// otherwise returns null.</returns>
        static public User GetUser(HttpContext context)
        {
            User user = null;
            if (context.Session[Properties.Resources.CURRENT_USER_SESSION_KEY] != null)
            {
                user = new User((JObject)JsonConvert.DeserializeObject(
                    context.Session[Properties.Resources.CURRENT_USER_SESSION_KEY].ToString()
                    ));
            }

            return user;
        }

        /// <summary>
        /// Thrown when the current user is not authorized.
        /// </summary>
        public class UserNotAuthorizedException : Exception { }

        /// <summary>
        /// Thrown if the current user's access token is expired and the user has
        /// no refresh token stored.
        /// </summary>
        public class GoogleTokenExpirationException : Exception { }


        /// <summary>
        /// An exception from the Google API.
        /// </summary>
        public class GoogleApiException : Exception
        {
            public GoogleApiException(String message) : base(message) { }
        }

        /// <summary>
        /// An exception related to TokenData that is unrelated to verification.
        /// </summary>
        public class TokenDataException : Exception
        {
            public TokenDataException(String message) : base(message) { }
        }

        /// <summary>
        /// Exceptions where verification from the ID token fails such as
        /// client ID not matching this projects client ID.
        /// </summary>
        public class TokenVerificationException : Exception
        {
            public TokenVerificationException(String message) : base(message) { }
        }
    }
}
