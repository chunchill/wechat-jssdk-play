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
    }


    public class WeChatApp
    {
        public int ID { get; set; }
        public string AppID { get; set; }
        public string Token { get; set; }
    }

    public class UploadedImages
    {
        public int ID { get; set; }
        public int OpenID { get; set; }
        public string ImageServerPath { get; set; }
        public DateTime UploadDateTime { get; set; }
    }
}