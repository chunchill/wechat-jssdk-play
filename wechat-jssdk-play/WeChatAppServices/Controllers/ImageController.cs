using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
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
            var mimeType = string.Empty;
            try
            {
                foreach (string fileName in HttpContext.Current.Request.Files)
                {
                    var file = HttpContext.Current.Request.Files[fileName];
                    //Save file content goes here
                    var extension = System.IO.Path.GetExtension(file.FileName);

                    if (string.IsNullOrEmpty(extension))
                    {
                        mimeType = file.ContentType;
                    }
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
                        if (!ValidateImage(path))
                        {
                            return InternalServerError(new InvalidDataException("Not a image"));
                        }
                    }
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }
            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName, MimeType = mimeType });
            }
            else
            {
                return Json(new { Message = "Error01" });//Error in saving file
            }
        }

        //This is not a good practice, but as the Andriod could upload any binary file as an image,have to use such a method to verify an image
        bool ValidateImage(string path)
        {
            System.Drawing.Image img = null;
            bool isImg = true;
            try
            {
                img = System.Drawing.Image.FromFile(path);
            }
            catch (OutOfMemoryException)
            {
                isImg = false;
            }
            finally
            {
                if (img != null) img.Dispose();
                if (!isImg) File.Delete(path);
            }

            return isImg;
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
                if (!db.Members.Any(m => m.OpenID == vote.OpenID))
                {
                    db.Members.Add(new Member() { OpenID = vote.OpenID, NickName = vote.NickName });
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

        [HttpGet]
        public IHttpActionResult Test()
        {
            var m = db.Members.FirstOrDefault();
            var result = "It's a test";
            if (m != null)
            {
                result = m.NickName;
            }
            return Ok(result);
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
                if (!db.Members.Any(m => m.OpenID == imageData.OpenID))
                {
                    db.Members.Add(new Member() { OpenID = imageData.OpenID, NickName = imageData.NickName });
                }
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
        public IList<UploadedImageData> ExportAllData()
        {
            IList<UploadedImageData> images;
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
                }).ToList();
            var members = db.Members.ToList();
            foreach (var item in images)
            {
                item.NickName = members.First(m => m.OpenID == item.OpenID).NickName;
            }
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

        [HttpGet]
        public List<UploadedImageData> GetSessionImages(int sessionId)
        {
            //all of the images in the databases
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

            var sessionFolder = GetImageFolder(string.Format("session{0}", sessionId));
            var imageDateFolders = System.IO.Directory.GetDirectories(sessionFolder);
            List<string> fileNames = new List<string>();
            foreach (var folder in imageDateFolders)
            {
                var dateFolderName = System.IO.Path.GetFileName(folder);
                var imageFiles = Directory.GetFiles(folder);
                foreach (var item in imageFiles)
                {
                    var imgFileName = System.IO.Path.GetFileName(item);
                    var imageFileNameWithDateFolder = string.Format("{0}/{1}", dateFolderName, imgFileName);
                    fileNames.Add(imageFileNameWithDateFolder);
                }
            }
            var imgs= images.ToList();
            return imgs.Where(item => fileNames.Any(f => f == item.FileName)).ToList();
         }

        [HttpGet]
        public List<UploadedImageData> GetSeletectedVoteImages()
        {
            //all of the images in the databases
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

            var sessionFolder = GetImageFolder("imagesForVote");
            var imageDateFolders = System.IO.Directory.GetDirectories(sessionFolder);
            List<string> fileNames = new List<string>();
            foreach (var folder in imageDateFolders)
            {
                var dateFolderName = System.IO.Path.GetFileName(folder);
                var imageFiles = Directory.GetFiles(folder);
                foreach (var item in imageFiles)
                {
                    var imgFileName = System.IO.Path.GetFileName(item);
                    var imageFileNameWithDateFolder = string.Format("{0}/{1}", dateFolderName, imgFileName);
                    fileNames.Add(imageFileNameWithDateFolder);
                }
            }
            return images.Where(item => fileNames.Any(f => f == item.FileName)).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
