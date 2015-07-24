using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

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
      public int ID { get; set; }

      public string FileName { get; set; }

      public string Description { get; set; }

      public int Height { get; set; }

      public int Width { get; set; }

      public string OpenID { get; set; }

      public DateTime UploadDate { get; set; }
   }

   public class Vote
   {
      public int ID { get; set; }

      public string OpenID { get; set; }

      public DateTime VoteDate { get; set; }

      public virtual UploadedImage TargetImage { get; set; }

   }


   #region DTO Class

   public class UploadedImageData
   {
      public string FileName { get; set; }

      public string Description { get; set; }

      public int Height { get; set; }

      public int Width { get; set; }

      public string OpenID { get; set; }

   }

   public class VoteData
   {
      public string OpenID { get; set; }

      public DateTime VoteDate { get; set; }

      public string FileName { get; set; }
   }

   #endregion


}