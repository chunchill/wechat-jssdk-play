<%@ WebHandler Language="C#" Class="OAuth" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using WebChatUtil.Utils;
using System.Collections.Generic;

public class OAuth : IHttpHandler
{
    JavaScriptSerializer Jss = new System.Web.Script.Serialization.JavaScriptSerializer();

    public void ProcessRequest(HttpContext context)
    {
        if (string.IsNullOrEmpty(context.Request.QueryString["code"]))
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("没有授权成功");
        }
        else
        {
            var Appid = context.Request.QueryString["Appid"];
            var url = context.Request.QueryString["redirect_uri"];
            var scope = context.Request.QueryString["scope"];

            var code = context.Request.QueryString["code"];
            var appId = string.Empty;
            var appSecret = string.Empty;
            var openId = CodeGetOpenid(appId, appSecret, code);
            if (!string.IsNullOrEmpty(openId))
            {
                context.Response.SetCookie(new HttpCookie("wechat-app-user-openId") { Value = openId });
                context.Response.Redirect("index.html");
            }
          
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    public string CodeGetOpenid(string Appid, string Appsecret, string Code)
    {
        string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", Appid, Appsecret, Code);
        string ReText = CommonMethod.WebRequestPostOrGet(url, "");//post/get方法获取信息 
        Dictionary<string, object> DicText = (Dictionary<string, object>)Jss.DeserializeObject(ReText);
        if (!DicText.ContainsKey("openid"))
            return string.Empty;
        return DicText["openid"].ToString();
    }

    public string GetUserInfo(string Appid, string Appsecret, string Code)
    {
        string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", Appid, Appsecret, Code);
        string ReText = CommonMethod.WebRequestPostOrGet(url, "");//post/get方法获取信息
        Dictionary<string, object> DicText = (Dictionary<string, object>)Jss.DeserializeObject(ReText);
        if (!DicText.ContainsKey("openid"))
        {
            CommonMethod.WriteTxt("获取openid失败，错误码：" + DicText["errcode"].ToString());
            return string.Empty;
        }
        else
        {
            var result = CommonMethod.WebRequestPostOrGet("https://api.weixin.qq.com/sns/userinfo?access_token=" + DicText["access_token"] + "&openid=" + DicText["openid"] + "&lang=zh_CN", "");
            return result;
        }
    }
}