using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;


// Libraries for Google APIs
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;

// For OAuth2
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

// For entity framework constants.
using System.Data;

// For data members.
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    public class ConnectHelper: PlusHelper
    {
        /// <summary>
        /// Verifies the token against the client ID.
        /// </summary>
        /// <param name="authState">The credential to verify.</param>
        /// <returns>The user ID that is associated with this token.</returns>
        static public String VerifyToken(IAuthorizationState authState)
        {
            // Use Tokeninfo to validate the user and the client.
            var tokeninfo_request = new Oauth2Service().Tokeninfo();
            tokeninfo_request.Access_token = authState.AccessToken;
            var tokeninfo = tokeninfo_request.Fetch();

            if (tokeninfo == null)
            {
                throw new TokenVerificationException("Error while fetching tokeninfo");
            }

            // Verify the first part of the token for authorization. On mobile clients,
            // the full string might differ, but the first part is consistent.
            Regex matcher = new Regex("(\\d+)(.*).apps.googleusercontent.com$");
            if (matcher.Match(tokeninfo.Issued_to).Groups[1].Value !=
                matcher.Match(CLIENT_ID).Groups[1].Value)
            {
                throw new TokenVerificationException("Issuer other than current client.");
            }

            return tokeninfo.User_id;
        }

        /// <summary>
        /// Either:
        /// 1. Create a user for the given ID and credential
        /// 2. Or, update the existing user with the existing credential
        ///
        /// If 2, then ask Google for the user's public profile information to store.
        /// </summary>
        /// <param name="authState">The OAuth v2 state for authorizing the user.</param>
        /// <returns>A User object that represents the created user.</returns>
        public User SaveTokenForUser(IAuthorizationState authState)
        {
            // Set the auth state in a the superclass for the authorization call.
            _authState = authState;

            var provider = new WebServerClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = CLIENT_ID;
            provider.ClientSecret = CLIENT_SECRET;
            var authenticator =
                new OAuth2Authenticator<WebServerClient>(
                    provider,
                    GetAuthorization)
                {
                    NoCaching = true
                };

            ps = new PlusService(new BaseClientService.Initializer()
            {
                Authenticator = authenticator
            });

            Person me = ps.People.Get("me").Fetch();

            // Load the user model from the DB if the user already exists.
            bool userExists = false;
            User user = new User();
            PhotohuntContext db = new PhotohuntContext();
            User existing = db.Users.FirstOrDefault(u => u.googleUserId.Equals(me.Id));
            if (existing != null)
            {
                user = existing;
                userExists = true;
            }

            if (!userExists)
            {
                // Save the new user.
                user.googleAccessToken = authState.AccessToken;
                user.googleRefreshToken =
                        authState.RefreshToken == null ? "" : authState.RefreshToken;
                user.googleExpiresIn = (int)(authState.AccessTokenExpirationUtc.Value -
                        authState.AccessTokenIssueDateUtc.Value).TotalSeconds;
                user.googleExpiresAt = authState.AccessTokenExpirationUtc.Value;
                user.googleUserId = me.Id;
                user.googleDisplayName = me.DisplayName;
                user.googlePublicProfilePhotoUrl = me.Image.Url;
                user.googlePublicProfileUrl = me.Url;
                user.email = me.Emails == null ? "" : me.Emails[0].Value;

                db.Users.Add(user);
                db.SaveChanges();
                db.Entry(user);

                // Use the FriendsHelper to generate this user's list of friends
                // who also use this app.
                PhotoHunt.utils.FriendsHelper.GenerateFriends(user, ps);
            }
            else
            {
                // Update the existing user's authorization state. Note that we aren't updating the
                // refresh token because it is only returned the first time the user authorizes the
                // app.
                user.googleAccessToken = authState.AccessToken;
                user.googleExpiresIn = (int)(authState.AccessTokenExpirationUtc.Value -
                        authState.AccessTokenIssueDateUtc.Value).TotalSeconds;
                user.googleExpiresAt = authState.AccessTokenExpirationUtc.Value;
                db.SaveChanges();
            }

            return user;
        }

    }
}
