﻿using System;
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
using Microsoft.WindowsAzure.ServiceRuntime;
using WebMatrix.WebData;
using System.Linq.Dynamic;


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



        public ActionResult UpVote(int id = 0)
        {
            int mu1 = (int)WebSecurity.CurrentUserId;
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement.UserVotedForSong != null)
            {
                string[] userThatVoted = mediaelement.UserVotedForSong.Split(',');
                List<string> tempUsersID = userThatVoted.ToList();
                foreach (string word in userThatVoted)
                {
                    if (Convert.ToString(mu1) == word)
                    {
                        TempData["UserVoted"] = "You have already voted for this song";
                        return RedirectToAction("Listen", new { isRating = true, hasVoted = true });
                    }
                    else if (Convert.ToString(mu1) != word)
                    {
                        // user can vote
                        tempUsersID.Remove(word);
                        if (tempUsersID.Count == 0)
                        {
                            // Users can vote
                            mediaelement.UserVotedForSong = mediaelement.UserVotedForSong + "," + mu1;
                            db.SaveChanges();
                            if (mediaelement.Votes == null)
                            {
                                mediaelement.Votes = "+1";
                                db.SaveChanges();

                                return RedirectToAction("Listen", new { isRating = true, hasVoted = false });
                            }
                            else
                            {
                                string vote = mediaelement.Votes;
                                int i = Int16.Parse(vote);
                                i++;
                                vote = Convert.ToString(i);
                                mediaelement.Votes = vote;
                                db.SaveChanges();
                                return RedirectToAction("Listen", new { isRating = true, hasVoted = false });
                            }
                        }
                    }


                }
            }
            return null;

        }

        public ActionResult FindGenre(FormCollection form)
        {

            string genre = form["genre"].ToString();
            System.Diagnostics.Debug.WriteLine(genre);
            //ViewBag.selectedGenre = genre;
            TempData["userGenre"] = genre;
            return RedirectToAction("BrowseByGenre", new { tempDataSet = true });
        }

        public ActionResult BrowseByGenre(bool tempDataSet = false)
        {
            if (tempDataSet)
            {
                ViewBag.selectedGenre = TempData["UserGenre"].ToString();
            }
            List<string> genresList = new List<string>();
            genresList.Add("Rock");
            genresList.Add("Pop");
            genresList.Add("Indie");
            genresList.Add("Classical");
            ViewBag.ListOfGenres = new SelectList(genresList);

            return View(db.MediaElements.ToList());
        }

        public ActionResult SearchForSong(FormCollection form)
        {

            string titleToSearchFor = form["titleToSearchFor"].ToString();
            System.Diagnostics.Debug.WriteLine(titleToSearchFor);
            //ViewBag.selectedGenre = genre;
            TempData["searchResults"] = titleToSearchFor;
            return RedirectToAction("SearchTitle", new { hasSearched = true });
        }

        public ActionResult SearchTitle(bool hasSearched = false)
        {
            if (hasSearched)
            {
                ViewBag.searchResults = TempData["searchResults"].ToString();
            }
            return View(db.MediaElements.ToList());
        }

        public ActionResult selectAmountOfSongsToShow(FormCollection form)
        {
            int amount = Convert.ToInt32(form["amountToShow"]);
            return RedirectToAction("TopRated", new { numberOfSongsToShow = amount });
        }

        public ActionResult TopRated(int numberOfSongsToShow = 0)
        {
            ViewBag.songsToShow = numberOfSongsToShow;
            ViewBag.canShow = true;
            if (numberOfSongsToShow == 0)
            {
                ViewBag.canShow = false;
            }
            List<int> amountToShow = new List<int>();
            amountToShow.Add(1);
            amountToShow.Add(2);
            amountToShow.Add(3);
            amountToShow.Add(4);
            ViewBag.ListOfAmounts = new SelectList(amountToShow);

            
            var media = db.MediaElements;
            var orderedMedia = media.OrderByDescending(a => a.Votes).Take(numberOfSongsToShow);

            return View(orderedMedia);
        }

        public ActionResult GetSongs()
        {
            var media = db.MediaElements;
            var orderedMedia = media.OrderBy(a => a.Id).ToList();

            List<MediaElement> mediaList = orderedMedia;

            return Json(mediaList, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DownVote(int id = 0)
        {
            int mu1 = (int)WebSecurity.CurrentUserId;
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement.UserVotedForSong != null)
            {
                string[] userThatVoted = mediaelement.UserVotedForSong.Split(',');
                List<string> tempUsersID = userThatVoted.ToList();
                foreach (string word in userThatVoted)
                {
                    if (Convert.ToString(mu1) == word)
                    {
                        TempData["UserVoted"] = "You have already voted for this song";
                        return RedirectToAction("Listen", new { isRating = true, hasVoted = true });
                    }
                    else if (Convert.ToString(mu1) != word)
                    {
                        // user can vote
                        tempUsersID.Remove(word);
                        if (tempUsersID.Count == 0)
                        {
                            // Users can vote
                            mediaelement.UserVotedForSong = mediaelement.UserVotedForSong + "," + mu1;
                            db.SaveChanges();
                            if (mediaelement.Votes == null)
                            {
                                mediaelement.Votes = "-1";
                                db.SaveChanges();

                                return RedirectToAction("Listen", new { isRating = true, hasVoted = false });
                            }
                            else
                            {
                                string vote = mediaelement.Votes;
                                int i = Int16.Parse(vote);
                                i--;
                                vote = Convert.ToString(i);
                                mediaelement.Votes = vote;
                                db.SaveChanges();
                                return RedirectToAction("Listen", new { isRating = true, hasVoted = false });
                            }
                        }
                    }


                }
            }
            return null;

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
            var model = new MediaElement();


            // the list of available values
            model.Values = new[]
            {
                new SelectListItem { Value = "Rock", Text = "Rock" },
                new SelectListItem { Value = "Pop", Text = "Pop" },
                new SelectListItem { Value = "Indie", Text = "Indie" },
                new SelectListItem { Value = "Classical", Text = "Classical" },
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Listen(bool isRating = false, bool hasVoted = false)
        {
            if (isRating)
            {
                if (hasVoted)
                {
                    ViewBag.VoteMessage = TempData["UserVoted"].ToString();
                    return View(db.MediaElements.ToList());
                }
                else
                {
                    return View(db.MediaElements.ToList());
                }
            }
            else
            {
                return View(db.MediaElements.ToList());
            }

        }

        [HttpGet]
        public ActionResult BrowseMusic()
        {
            return View(db.MediaElements.ToList());

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
                    RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
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
                    mediaelement.UserVotedForSong = "0";
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

        [HttpGet]
        public ActionResult PlayAllMusic()
        {
            return View(db.MediaElements.ToList());
        }
    }
}