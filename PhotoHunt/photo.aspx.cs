using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

// For the votes utility class.
using PhotoHunt.utils;

namespace PhotoHunt
{
    public partial class photo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int photoId = int.Parse(Request["photoId"]);
                VotesHelper voteObject = new VotesHelper();
                voteObject.DoVote(Context, photoId);
            }
            catch (NullReferenceException nre) {
                System.Diagnostics.Debug.WriteLine("An error occurred: There was a request for a " +
                    "photo that is not in the datastore." + nre.StackTrace);
            }
            catch (ArgumentNullException ane) {
                System.Diagnostics.Debug.WriteLine("An error occurred: There was a request for a " +
                    "photo that is not in the datastore." + ane.StackTrace);
            }
        }
    }
}
