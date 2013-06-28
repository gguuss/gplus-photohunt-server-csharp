using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// For disconnect HTTP POST request.
using System.Net;

// For entity framework constants.
using System.Data;

// For data members.
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    /// <summary>
    /// Provides the methods to cleanly remove a user account when they
    /// disconnect from the app such as removing friend edges, removing photos
    /// and votes, and sending the token revocation request to Google.
    /// </summary>
    public class DisconnectHelper
    {
        /// <summary>
        /// Remove all of the friend edges associated with a PhotoHunt user.
        /// </summary>
        /// <param name="toRemove">The user whose friend edges will be removed.</param>
        static public void RemoveFriendEdges(User toRemove)
        {
            PhotohuntContext db = new PhotohuntContext();

            // Remove this user's edges.
            var query = from b in db.Edges
                        where b.photohuntUserId.Equals(toRemove.id)
                        select b;
            foreach (var edge in query)
            {
                db.Entry(edge).State = EntityState.Deleted;
            }

            // Remove this user from friend edges.
            var friendEdgeQuery = from b in db.Edges
                                  where b.friendUserId.Equals(toRemove.id)
                                  select b;
            foreach (var edge in friendEdgeQuery)
            {
                db.Entry(edge).State = EntityState.Deleted;
            }

            db.Entry(toRemove).State = EntityState.Deleted;

            // Remove this user's photos
            var photosQuery = from b in db.Photos
                              where b.ownerUserId.Equals(toRemove.id)
                              select b;
            foreach (var photo in photosQuery)
            {
                // Remove votes for this photo.
                var photoVotesQuery = from b in db.Votes
                                      where b.photoId.Equals(photo.id)
                                      select b;
                foreach (var vote in photoVotesQuery)
                {
                    db.Entry(vote).State = EntityState.Deleted;
                }

                // Remove the photo from the DB.
                db.Entry(photo).State = EntityState.Deleted;
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Remove all of a user's votes from the PhotoHunt database.
        /// </summary>
        /// <param name="toRemove">The user whose votes will be removed.</param>
        static public void RemoveVotes(User toRemove)
        {
            PhotohuntContext db = new PhotohuntContext();

            // Remove this user's votes and decrement vote count.
            var votesQuery = from b in db.Votes
                             where b.ownerUserId == toRemove.id
                             select b;
            foreach (var vote in votesQuery)
            {
                PhotohuntContext dbInner = new PhotohuntContext();

                Photo voteTarget = dbInner.Photos.First(p => p.id == vote.photoId);
                voteTarget.numVotes -= 1;
                dbInner.SaveChanges();

                db.Entry(vote).State = EntityState.Deleted;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Disconnects the user's account and writes the response data to the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context to write the response to.</param>
        /// <param name="toDisconnect">The PhotoHunt user to disconnect.</param>
        /// <returns>True on success; otherwise, returns false.</returns>
        static public bool DisconnectAccount(HttpContext context, User toDisconnect)
        {
            string token = toDisconnect.googleRefreshToken != null &&
                toDisconnect.googleRefreshToken.Length > 0 ?
                toDisconnect.googleRefreshToken : toDisconnect.googleAccessToken;
            bool succeeded = false;

            // Disconnect by performing a GET request to the OAuth v2 endpoint.
            WebRequest request = WebRequest.Create(
                "https://accounts.google.com/o/oauth2/revoke?token=" +
                token);

            try
            {
                WebResponse response = request.GetResponse();
                System.Diagnostics.Debug.WriteLine("Disconnect success, response: " +
                    response.GetResponseStream().ToString());
                succeeded = true;
            }
            catch (WebException wee)
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Error returned from revoke endpoint:" +
                        wee.Message;
            }
            return succeeded;
        }
    }
}
