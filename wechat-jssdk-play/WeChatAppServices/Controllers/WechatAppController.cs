using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WeChatAppServices.Models;
using WebChatUtil.Utils;
using System.Web.Script.Serialization;

namespace WeChatAppServices.Controllers
{
    public class WechatAppController : ApiController
    {
        private SQLLiteContext db = new SQLLiteContext();
        JavaScriptSerializer Jss = new JavaScriptSerializer();

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


        [HttpGet]
        public IHttpActionResult GetCodeUrl(string Appid, string redirect_uri, string scope)
        {
            redirect_uri = System.Web.HttpUtility.UrlEncode(redirect_uri);
            var result = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect", Appid, redirect_uri, scope);
            return Ok(result);
              
        }

        [HttpGet]
        public IHttpActionResult CodeGetOpenid(string Appid, string Appsecret, string Code)
        {
            string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", Appid, Appsecret, Code);
            string ReText = CommonMethod.WebRequestPostOrGet(url, "");//post/get方法获取信息 
            Dictionary<string, object> DicText = (Dictionary<string, object>)Jss.DeserializeObject(ReText);
            if (!DicText.ContainsKey("openid"))
                return Ok("");
            return Ok(DicText["openid"].ToString());
        }

        [HttpGet]
        public IHttpActionResult GetUserInfo(string Appid, string Appsecret, string Code)
        {
            string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", Appid, Appsecret, Code);
            string ReText = CommonMethod.WebRequestPostOrGet(url, "");//post/get方法获取信息
            Dictionary<string, object> DicText = (Dictionary<string, object>)Jss.DeserializeObject(ReText);
            if (!DicText.ContainsKey("openid"))
            {
                CommonMethod.WriteTxt("获取openid失败，错误码：" + DicText["errcode"].ToString());
                return Ok("");
            }
            else
            {
                var result= CommonMethod.WebRequestPostOrGet("https://api.weixin.qq.com/sns/userinfo?access_token=" + DicText["access_token"] + "&openid=" + DicText["openid"] + "&lang=zh_CN", "");
                return Ok(result);
            }
        }
    }
}
