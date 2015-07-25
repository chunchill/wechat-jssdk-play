using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatUtil.Providers
{
   public interface IWeChatApp
   {
      string ProvideAccessToken(string appId, string appSecret);
      string ProvideJsTicket(string accessToken);
   }
}
