using System.Collections.Generic;
using System.Web.Mvc;
namespace project.Models
{
    public class PlayList
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PlaylistName { get; set; }
        public string SongsID { get; set; }
    }

}
