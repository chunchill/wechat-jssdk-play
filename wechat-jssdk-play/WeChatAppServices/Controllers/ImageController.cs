using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;
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

        [HttpPost]
        public IHttpActionResult Vote(VoteData vote)
        {
            var targetImg = db.UploadedImages.Where(img => img.ID == vote.ImageId).FirstOrDefault();
            if (targetImg != null)
            {
                var oneRecord = db.Votes.OrderByDescending(v => v.VoteDate).FirstOrDefault(v => v.OpenID == vote.OpenID
                   && v.UploadedImageId == targetImg.ID);
                if (oneRecord != null)
                {
                    var alreadyVoteToday = DateTime.Now.Subtract(oneRecord.VoteDate).Days == 0;
                    if (alreadyVoteToday)
                        return InternalServerError(new InvalidOperationException("Error-02"));//Already vote
                }
                db.Votes.Add(new Vote()
                {
                    OpenID = vote.OpenID,
                    UploadedImageId = targetImg.ID,
                    VoteDate = DateTime.Now
                });
                db.SaveChanges();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult HelloWorld()
        {
            return Ok("Hello World");
        }



        [HttpPost]
        public IHttpActionResult Submit(UploadedImageData imageData)
        {
            if (!ImageFileExist(imageData.FileName, "tempFiles"))
            {
                return InternalServerError(new FileNotFoundException("Error-03"));//File missing
            }
            var dateFolder = DateTime.Today.ToString("yyyy-MM-dd");
            var currentImagePath = GetImageFile(imageData.FileName, "tempFiles");
            System.Drawing.Image img = System.Drawing.Image.FromFile(currentImagePath);
            var width = img.Width;
            var height = img.Height;
            img.Dispose();
            var imageModel = new UploadedImage()
            {
                OpenID = imageData.OpenID,
                FileName = dateFolder + "/" + imageData.FileName,
                Description = imageData.Description,
                UploadDate = DateTime.Now,
                Width = width,
                Height = height
            };
            try
            {
                db.UploadedImages.Add(imageModel);
                db.SaveChanges();

                var tempFilepath = GetImageFile(imageData.FileName, "tempFiles");

                //create distination file folder path if not exist
                if (!ImageFolderExist("images"))
                    System.IO.Directory.CreateDirectory(GetImageFolder("images"));

                var destFolder = GetImageFolder("images");
                var combinedFolder = destFolder + "\\" + dateFolder;
                if (!System.IO.Directory.Exists(combinedFolder))
                    System.IO.Directory.CreateDirectory(combinedFolder);
                System.IO.File.Move(tempFilepath, combinedFolder + "\\" + imageData.FileName);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class VoteCount
        {
            public string FileName { get; set; }
            public Int64 UploadedImageId { get; set; }
            public int Count { get; set; }
        }

        [HttpGet]
        public List<VoteCount> GetAllVoteData()
        {
            var result = new List<VoteCount>();
            IQueryable<IGrouping<Int64, Vote>> groupData;
            groupData = db.Votes.GroupBy(v => v.UploadedImageId).AsQueryable();
            var groupList = groupData.ToList();
            foreach (var item in groupList)
            {
                result.Add(new VoteCount()
                {
                    FileName = item.First().TargetImage.FileName,
                    UploadedImageId = item.First().UploadedImageId,
                    Count = item.Count()
                });
            }
            return result;
        }

        [HttpGet]
        public VoteCount GetVoteDataByFileName(string fileName)
        {
            var result = new List<VoteCount>();
            IQueryable<IGrouping<Int64, Vote>> groupData;
            groupData = db.Votes.GroupBy(v => v.UploadedImageId).AsQueryable();
            var groupList = groupData.ToList();
            foreach (var item in groupList)
            {
                result.Add(new VoteCount()
                {
                    FileName = item.First().TargetImage.FileName,
                    UploadedImageId = item.First().UploadedImageId,
                    Count = item.Count()
                });
            }
            return result.FirstOrDefault(item => item.FileName == fileName);
        }

        [HttpGet]
        public VoteCount GetVoteDataByImageID(Int64 imageId)
        {
            var result = new List<VoteCount>();
            IQueryable<IGrouping<Int64, Vote>> groupData;
            groupData = db.Votes.GroupBy(v => v.UploadedImageId).AsQueryable();
            var groupList = groupData.ToList();
            foreach (var item in groupList)
            {
                result.Add(new VoteCount()
                {
                    FileName = item.First().TargetImage.FileName,
                    UploadedImageId = item.First().UploadedImageId,
                    Count = item.Count()
                });
            }
            return result.FirstOrDefault(item => item.UploadedImageId == imageId);
        }

        [HttpGet]
        public IQueryable<UploadedImageData> GetUploadedImages(string openId)
        {
            IQueryable<UploadedImageData> images;

            images = db.UploadedImages.Where(item => item.OpenID == openId)
                .OrderByDescending(img => img.UploadDate)
                .Select(img => new UploadedImageData()
           {
               ImageId = img.ID,
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

            images = db.UploadedImages
                .OrderByDescending(img => img.UploadDate)
                .Select(img => new UploadedImageData()
            {
                ImageId = img.ID,
                FileName = img.FileName,
                OpenID = img.OpenID,
                Description = img.Description,
                Height = img.Height,
                Width = img.Width
            });
            return images;
        }

        [HttpGet]
        public List<UploadedImageData> GetAllGreatImages()
        {
            var result = new List<UploadedImageData>();

            var showFolder = GetImageFolder("Show");
            if (!System.IO.Directory.Exists(showFolder))
            {
                System.IO.Directory.CreateDirectory(showFolder);
            }
            var descriptionXMLPath = showFolder + "\\Description.xml";

            var imagesFiles = System.IO.Directory.GetFiles(showFolder + "\\images");

            // Serialize 
            //Type[] imgTypes = { typeof(XMLImageModel) };
            //XmlSerializer serializer = new XmlSerializer(typeof(XMLImageModelCollection), imgTypes); 
            //FileStream fs = new FileStream(descriptionXMLPath, FileMode.Create);
            //serializer.Serialize(fs, personen);
            //fs.Close(); 

            // Deserialize 
            using (var fs = new FileStream(descriptionXMLPath, FileMode.Open))
            {
                Type[] imgTypes = { typeof(XMLImageModel) };
                XmlSerializer serializer = new XmlSerializer(typeof(XMLImageModelCollection), imgTypes);
                XMLImageModelCollection imgs = (XMLImageModelCollection)serializer.Deserialize(fs);
                foreach (var item in imagesFiles)
                {
                    var fileName = System.IO.Path.GetFileName(item);

                    using (var imgItem = System.Drawing.Image.FromFile(item))
                    {

                        result.Add(new UploadedImageData()
                        {
                            FileName = fileName,
                            Height = imgItem.Height,
                            Width = imgItem.Width,
                            Description = GetDescriptionByFileName(imgs, fileName)
                        });
                    }
                }
            }
            return result;
        }

        [HttpGet]
        public IHttpActionResult GenerateDescription()
        {
            var showFolder = GetImageFolder("Show");
            if (!System.IO.Directory.Exists(showFolder))
            {
                System.IO.Directory.CreateDirectory(showFolder);
            }
            var descriptionXMLPath = showFolder + "\\Description.xml";
            if (!System.IO.File.Exists(descriptionXMLPath))
            {
                System.IO.File.Create(descriptionXMLPath);
            }
            var imagesFiles = System.IO.Directory.GetFiles(showFolder + "\\images");
            XMLImageModelCollection collection = new XMLImageModelCollection();
            foreach (var item in imagesFiles)
            {
                collection.images.Add(new XMLImageModel()
                {
                    Description = System.IO.Path.GetFileNameWithoutExtension(item),
                    FileName = System.IO.Path.GetFileName(item)
                });
            }
            // Serialize 
            Type[] imgTypes = { typeof(XMLImageModel) };
            XmlSerializer serializer = new XmlSerializer(typeof(XMLImageModelCollection), imgTypes);
            FileStream fs = new FileStream(descriptionXMLPath, FileMode.Create);
            serializer.Serialize(fs, collection);
            fs.Close();
            return Ok();
        }

        private static string GetDescriptionByFileName(XMLImageModelCollection collection, string fileName)
        {
            var img = collection.images.FirstOrDefault(item => item.FileName == fileName);
            if (img != null)
            {
                return img.Description;
            }
            return string.Empty;
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
