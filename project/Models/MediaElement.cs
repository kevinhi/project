using System.Collections.Generic;
using System.Web.Mvc;
namespace project.Models
{
    public class MediaElement
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public bool IsPublic { get; set; }
        public string FullBlobName { get; set; }
        public string FileUrl { get; set; }
        public string Genre { get; set; }
        
    }
}
