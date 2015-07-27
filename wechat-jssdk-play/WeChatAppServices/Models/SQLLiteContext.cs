using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WeChatAppServices.Models
{
   public class SQLLiteContext : DbContext
   {
      public SQLLiteContext()
         : base("SqliteDbConn")
      {

      }
      public DbSet<WeChatApp> WebChatApps { get; set; }

      public DbSet<UploadedImage> UploadedImages { get; set; }

      public DbSet<Vote> Votes { get; set; }
   }


   public class WeChatApp
   {
      public int ID { get; set; }
      public string AppID { get; set; }
      public string Token { get; set; }
   }

   public class UploadedImage
   {
      public System.Int64 ID { get; set; }

      public string FileName { get; set; }

      public string Description { get; set; }

      public int Height { get; set; }

      public int Width { get; set; }

      public string OpenID { get; set; }

      public DateTime UploadDate { get; set; }
   }

   public class Vote
   {
      public System.Int64 ID { get; set; }

      public string OpenID { get; set; }

      public DateTime VoteDate { get; set; }

      public System.Int64 UploadedImageId { get; set; }

      public virtual UploadedImage TargetImage { get; set; }

   }


   #region DTO Class

   public class UploadedImageData
   {
      public Int64 ImageId { get; set; }

      public string FileName { get; set; }

      public string Description { get; set; }

      public int Height { get; set; }

      public int Width { get; set; }

      public string OpenID { get; set; }

   }

   [XmlRoot("ImageGallery")]
   [XmlInclude(typeof(XMLImageModel))]
   public class XMLImageModelCollection //: IEnumerable<XMLImageModel>
   {
      public List<XMLImageModel> images = new List<XMLImageModel>();

      public void Add(XMLImageModel img)
      {
         images.Add(img);
      }

      //public IEnumerator<XMLImageModel> GetEnumerator()
      //{
      //   return images.GetEnumerator();
      //}

      //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      //{
      //   return images.GetEnumerator();
      //}
   }


   [XmlType("Image")]
   public class XMLImageModel
   {
      [XmlElement("FileName")]
      public string FileName { get; set; }

      [XmlElement("Description")]
      public string Description { get; set; }
   }

   public class VoteData
   {
      public string OpenID { get; set; }

      public DateTime VoteDate { get; set; }

      public Int64 ImageId { get; set; }
   }

   #endregion


}