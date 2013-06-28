using System;
using System.Collections.Generic;
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
            catch (Exception) { }

        }
    }
}
