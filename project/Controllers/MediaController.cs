using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using project.Models;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Globalization;
using System.IO;
using System.Text;
using project.Filters;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Web.Security;

namespace project.Controllers
{
    [InitializeSimpleMembership]
    [Authorize]
    public class MediaController : Controller
    {
        private projectContext db = new projectContext();

        //
        // GET: /Media/
        public ActionResult Index()
        {
            return View(db.MediaElements.ToList());

        }


    
        public ActionResult Details(int id = 0)
        {
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement == null)
            {
                return HttpNotFound();
            }
            return View(mediaelement);
        }

        //
        // GET: /Media/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Media/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MediaElement mediaelement)
        {
            if (ModelState.IsValid)
            {
                db.MediaElements.Add(mediaelement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(mediaelement);
        }

        //
        // GET: /Media/Edit/5
        public ActionResult Edit(int id = 0)
        {
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement == null)
            {
                return HttpNotFound();
            }
            return View(mediaelement);
        }

        //
        // POST: /Media/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MediaElement mediaelement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mediaelement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mediaelement);
        }

        //
        // GET: /Media/Delete/5
        public ActionResult Delete(int id = 0)
        {
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement == null)
            {
                return HttpNotFound();
            }
            return View(mediaelement);
        }

        //
        // POST: /Media/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MediaElement mediaelement = db.MediaElements.Find(id);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("music");
            string fileToDelete = mediaelement.FullBlobName;
            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileToDelete);

            // Delete the blob.
            blockBlob.Delete(); 
            db.MediaElements.Remove(mediaelement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Listen()
        {
            
            return View();
        }

        [HttpGet]
        public ActionResult CreatePlaylist()
        {
            return View();
        }



        [HttpPost]
        public ActionResult UploadFile(MediaElement mediaelement)
        {
            
            if (ModelState.IsValid)
            {
                
            var audio = Request.Files["myFile"];
            if (audio == null)
            {
                ViewBag.UploadMessage = "Failed to upload audio";
            }
            else
            {
                ViewBag.UploadMessage = String.Format("Got image {0} of type {1} and size {2}",
                    audio.FileName, audio.ContentType, audio.ContentLength);
                // TODO: actually save the image to Azure blob storage
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString")); 
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("music");
                container.CreateIfNotExists();
                // configure container for public access
                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);
                string uniqueBlobName = string.Format("music/audioFile{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(audio.FileName));
                CloudBlockBlob blob = container.GetBlockBlobReference(uniqueBlobName);
                blob.Properties.ContentType = audio.ContentType;
                blob.UploadFromStream(audio.InputStream);
                var blobUrl1 = blob.Uri.ToString();
                mediaelement.FileUrl = blobUrl1;
                mediaelement.FullBlobName = uniqueBlobName;
                string currentUserId = Convert.ToString(Membership.GetUser().ProviderUserKey);
                mediaelement.UserId = currentUserId;
                db.MediaElements.Add(mediaelement);
                db.SaveChanges();
            }
            
               
            }
            return RedirectToAction("Upload", "Media");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}