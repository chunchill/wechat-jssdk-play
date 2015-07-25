using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChatUtil.Models;
using WeChatUtil.Providers;

namespace WeChatUtil.Services
{
   interface IWeChatServices
   {
      void Setup(IWeChatApp wechat, TimeSpan cacheDuration);
      string GetTicket(string accessToken);
      string GetAccessToken(string appId, string appSecret);
      SignaturePackage Signature(string jsTicket, string url);
   }
}
