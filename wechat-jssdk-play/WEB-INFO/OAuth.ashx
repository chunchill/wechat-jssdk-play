<%@ WebHandler Language="C#" Class="OAuth" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using WeChatUtil.Utils;
using System.Collections.Generic;

public class OAuth : IHttpHandler
{
    JavaScriptSerializer Jss = new System.Web.Script.Serialization.JavaScriptSerializer();
    //appid:wxcf7ccfe5c27e0dd1
    //appsecret:3e389b75d7d4545423f9f75c420f6bd1
    //redirect_url:http://139.129.15.91/photography/OAuth.ashx
    //encode_url:http%3A%2F%2F139.129.15.91%2Fphotography%2FOAuth.ashx
    //https://open.weixin.qq.com/connect/oauth2/authorize?appid=wxcf7ccfe5c27e0dd1&redirect_uri=http%3A%2F%2F139.129.15.91%2Fphotography%2FOAuth.ashx&response_type=code&scope=snsapi_base&state=123#wechat_redirect
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
                //context.Response.SetCookie(new HttpCookie("wechat-app-user-openId") { Value = openId });
                //context.Response.Redirect("index.html");
                context.Response.Write(string.Format("OpenId:{0}", openId));
                context.Response.Write(string.Format("Code:{0}", code));
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