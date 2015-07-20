using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WeChatAppServices.Models;

namespace WeChatAppServices.Controllers
{
    public class WechatAppController : ApiController
    {
        private SQLLiteContext db = new SQLLiteContext();

        [HttpGet]
        public HttpResponseMessage Signature(string jsTicket, string url)
        {
            var message = new HttpResponseMessage();
            
            var app = new WeChatApp();
            app.AppID = "123";
            app.Token = "abcde";
            if (!db.WebChatApps.Any(item => item.AppID == "123"))
            {
                db.WebChatApps.Add(app);
                db.SaveChanges();
            }
            message.Content = new StringContent(string.Format("jsTicket:{0},url:{1}", jsTicket, url));
            message.StatusCode = HttpStatusCode.OK;
            return message;
        }
    }
}
