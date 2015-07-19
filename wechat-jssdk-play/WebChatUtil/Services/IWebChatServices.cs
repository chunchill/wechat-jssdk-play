using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebChatUtil.Models;
using WebChatUtil.Providers;

namespace WebChatUtil.Services
{
   interface IWebChatServices
   {
      void Setup(IWeChatApp wechat, TimeSpan cacheDuration);
      string GetTicket(string accessToken);
      string GetAccessToken(string appId, string appSecret);
      SignaturePackage Signature(string jsTicket, string url);
   }
}
