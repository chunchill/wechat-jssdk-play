using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatUtil.Models
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
