using System;
using System.Collections;
using System.Linq;
using System.Web;

// For data model access
using PhotoHunt.model;

// Libraries for Google APIs
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Services;


// For OAuth2
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

namespace PhotoHunt.utils
{
    public class FriendsHelper: PlusHelper
    {
        public FriendsHelper(HttpRequest req): base(req){ }

        /// <summary>
        /// Get a user's PhotoHunt friends.
        /// </summary>
        /// <param name="user">The user whose friend's are retrieved.</param>
        /// <returns>A list of the JsonUsers representing the selected user's friends.</returns>
        static public ArrayList GetFriends(User user)
        {
            // Find the edges, convert them into JsonUsers, and return as JSON.
            PhotoHunt.model.PhotohuntContext db = new PhotoHunt.model.PhotohuntContext();
            var edgeQuery = from b in db.Edges
                            where b.photohuntUserId.Equals(user.id)
                            select b;
            ArrayList people = new ArrayList();
            foreach (var edge in edgeQuery)
            {
                PhotoHunt.model.PhotohuntContext dbInner = new PhotoHunt.model.PhotohuntContext();
                var friendQuery = from b in dbInner.Users
                                  where b.id.Equals(edge.friendUserId)
                                  select b;
                foreach (var friend in friendQuery)
                {
                    people.Add(new JsonUser(friend));
                }
            }

            return people;
        }

        /// <summary>
        /// Creates friend edges within the model for each match between this user's visible people
        /// and others who already exist in the database. This method is here for cases where the
        /// method call is used asynchronously.
        /// </summary>
        /// <param name="user">The user object to create friend edges for.</param>
        /// <param name="authState">Contains the IAuthorizationState object which is used to
        /// authorize the client to generate the signed in user's visible people.</param>
        /// <returns>None.</returns>
        public void GenerateFriends(User user,IAuthorizationState authState)
        {
            // Set the authorization state in the base class for the authorization call.
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

            GenerateFriends(user, ps);
        }

        /// <summary>
        /// Creates friend edges within the model for each match between this user's visible people
        /// and others who already exist in the database.
        /// </summary>
        /// <param name="user">The user object to create friend edges for.</param>
        /// <param name="ps">The Google+ API client service.</param>
        /// <returns>None.</returns>
        static public void GenerateFriends(User user, PlusService ps)
        {
            // Get the PeopleFeed for the currently authenticated user using the Google+ API.
            PeopleResource.ListRequest lr = ps.People.List("me",
                    PeopleResource.CollectionEnum.Visible);

            PeopleFeed pf = lr.Fetch();
            PhotohuntContext db = new PhotohuntContext();

            do
            {
                foreach (Person p in pf.Items)
                {
                    // Check whether the friend has an account on PhotoHunt
                    bool userExists = db.Users.Any(u => u.googleUserId.Equals(p.Id));

                    if (userExists)
                    {
                        // Check whether friend edge already exists.
                        User friend = db.Users.First(f => f.googleUserId.Equals(p.Id));
                        bool edgeExists = db.Edges.Any(e => e.photohuntUserId == user.id &&
                                    e.friendUserId == friend.id);

                        // Only add new edges when the user exists on PhotoHunt and the edge doesn't
                        // already exist
                        if (!edgeExists && userExists && friend.id != user.id)
                        {
                            // Save the friend edges.
                            DirectedUserToEdge fromFriendEdge = new DirectedUserToEdge();
                            fromFriendEdge.friendUserId = friend.id;
                            fromFriendEdge.photohuntUserId = user.id;
                            db.Edges.Add(fromFriendEdge);

                            DirectedUserToEdge toFriendEdge = new DirectedUserToEdge();
                            toFriendEdge.friendUserId = user.id;
                            toFriendEdge.photohuntUserId = friend.id;
                            db.Edges.Add(toFriendEdge);
                            db.SaveChanges();
                        }
                    }
                }

                lr.PageToken = pf.NextPageToken;
                pf = lr.Fetch();
            } while (pf.NextPageToken != null);
        }
    }
}
