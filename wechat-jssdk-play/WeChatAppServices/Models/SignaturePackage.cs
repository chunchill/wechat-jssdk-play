using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChatAppServices.Models
{
   public class SignaturePackage
   {
      public string AppId { get; set; }
      public long Timestamp { get; set; }
      public string NonceString { get; set; }
      public string Signature { get; set; }
      public string Url { get; set; }
   }
}