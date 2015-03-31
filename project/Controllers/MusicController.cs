using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project.Controllers
{
    [Authorize]
    public class MusicController : Controller
    {
        //
        // GET: /Music/
        [Authorize(Roles = "Members")]
        public ActionResult Upload()
        {
            return View();
        }
        [Authorize(Roles = "Members")]
        public ActionResult Listen()
        {
            return View();
        }
        [Authorize(Roles = "Members")]
        public ActionResult CreatePlaylist()
        {
            return View();
        }

    }
}
