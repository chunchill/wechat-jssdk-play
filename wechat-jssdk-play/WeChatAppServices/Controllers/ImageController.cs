using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WeChatAppServices.Models;

namespace WeChatAppServices.Controllers
{
   public class ImageController : ApiController
   {
      private SQLLiteContext db = new SQLLiteContext();

      [HttpPost]
      public IHttpActionResult Upload()
      {
         bool isSavedSuccessfully = true;
         string fName = "";
         try
         {
            foreach (string fileName in HttpContext.Current.Request.Files)
            {
               var file = HttpContext.Current.Request.Files[fileName];
               //Save file content goes here
               var extension = System.IO.Path.GetExtension(file.FileName);
               var newName = Guid.NewGuid().ToString();
               fName = string.Format("{0}{1}", newName, extension);

               if (file != null && file.ContentLength > 0)
               {
                  var originalDirectory = new DirectoryInfo(string.Format("{0}uploadedFiles", HttpContext.Current.Server.MapPath(@"\")));
                  string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "tempFiles");
                  var fileName1 = Path.GetFileName(file.FileName);
                  bool isExists = System.IO.Directory.Exists(pathString);
                  if (!isExists)
                     System.IO.Directory.CreateDirectory(pathString);
                  var path = string.Format("{0}\\{1}", pathString, fName);
                  file.SaveAs(path);
               }
            }
         }
         catch (Exception)
         {
            isSavedSuccessfully = false;
         }
         if (isSavedSuccessfully)
         {
            return Json(new { Message = fName });
         }
         else
         {
            return Json(new { Message = "Error01" });//Error in saving file
         }
      }

      public IHttpActionResult Vote(VoteData vote)
      {
         var targetImg= db.UploadedImages.Where(img => img.FileName == vote.FileName).FirstOrDefault();
         if (targetImg != null)
         {
            var alreadyVoteToday= db.Votes.Any(v=>v.OpenID == vote.OpenID
               && v.VoteDate.ToShortDateString() == DateTime.Now.ToShortDateString()
               && v.TargetImage.FileName == vote.FileName);

            if(alreadyVoteToday) 
               return InternalServerError(new InvalidOperationException("Error-02"));//Already vote
            db.Votes.Add(new Vote()
            {
                OpenID = vote.OpenID,
                TargetImage =targetImg,
                VoteDate = DateTime.Now
            });
             db.SaveChanges();
         }

         return Ok();
      }

      [HttpPost]
      public IHttpActionResult Submit(UploadedImageData imageData)
      {
         if (!ImageFileExist(imageData.FileName, "tempFiles"))
         {
            return InternalServerError(new FileNotFoundException("Error-03"));//File missing
         }
         var currentImagePath = GetImageFile(imageData.FileName, "tempFiles");
         System.Drawing.Image img = System.Drawing.Image.FromFile(currentImagePath);
         var width = img.Width;
         var height = img.Height;
         img.Dispose();
         var imageModel = new UploadedImage()
         {
            OpenID = imageData.OpenID,
            FileName = imageData.FileName,
            Description = imageData.Description,
            UploadDate = DateTime.Now,
            Width = width,
            Height = height
         };
         try
         {
            db.UploadedImages.Add(imageModel);
            db.SaveChanges();

            var tempFilepath = GetImageFile(imageModel.FileName, "tempFiles");
            //create distination file folder path if not exist
            var distFilePath = GetImageFile(imageModel.FileName, "images");
            if (!ImageFolderExist("images"))
               System.IO.Directory.CreateDirectory(GetImageFolder("images"));
            System.IO.File.Move(tempFilepath, distFilePath);

            return Ok();
         }
         catch (Exception ex)
         {
            return InternalServerError(ex);
         }
      }

      [HttpGet]
      public IQueryable<UploadedImageData> GetUploadedImages(string openId)
      {
         IQueryable<UploadedImageData> images;

         images = db.UploadedImages.Where(item => item.OpenID == openId).Select(img => new UploadedImageData()
        {
           FileName = img.FileName,
           OpenID = img.OpenID,
           Description = img.Description,
           Height = img.Height,
           Width = img.Width
        });


         return images;
      }

      [HttpGet]
      public IQueryable<UploadedImageData> GetAllUploadedImages()
      {
         IQueryable<UploadedImageData> images;

         images = db.UploadedImages.Select(img => new UploadedImageData()
         {
            FileName = img.FileName,
            OpenID = img.OpenID,
            Description = img.Description,
            Height = img.Height,
            Width = img.Width
         });
         return images;
      }

      #region Private functions

      private static bool ImageFileExist(string fileName, string folderName)
      {
         var originalDirectory = new DirectoryInfo(string.Format("{0}uploadedFiles", HttpContext.Current.Server.MapPath(@"\")));
         string tempFileFolderPath = System.IO.Path.Combine(originalDirectory.ToString(), folderName);
         var tempFilepath = string.Format("{0}\\{1}", tempFileFolderPath, fileName);
         return System.IO.File.Exists(tempFilepath);
      }

      private static bool ImageFolderExist(string folderName)
      {
         var originalDirectory = new DirectoryInfo(string.Format("{0}uploadedFiles", HttpContext.Current.Server.MapPath(@"\")));
         string tempFileFolderPath = System.IO.Path.Combine(originalDirectory.ToString(), folderName);
         return System.IO.Directory.Exists(tempFileFolderPath);
      }

      private static string GetImageFolder(string folderName)
      {
         var originalDirectory = new DirectoryInfo(string.Format("{0}uploadedFiles", HttpContext.Current.Server.MapPath(@"\")));
         string tempFileFolderPath = System.IO.Path.Combine(originalDirectory.ToString(), folderName);
         return tempFileFolderPath;
      }

      private static string GetImageFile(string fileName, string folderName)
      {
         var originalDirectory = new DirectoryInfo(string.Format("{0}uploadedFiles", HttpContext.Current.Server.MapPath(@"\")));
         string fileFolderPath = System.IO.Path.Combine(originalDirectory.ToString(), folderName);
         var filepath = string.Format("{0}\\{1}", fileFolderPath, fileName);
         return filepath;
      }

      #endregion
   }
}
