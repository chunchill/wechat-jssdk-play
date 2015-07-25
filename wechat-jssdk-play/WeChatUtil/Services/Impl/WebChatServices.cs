using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WeChatUtil.Models;
using WeChatUtil.Providers;
using WeChatUtil.Utils;

namespace WeChatUtil.Services.Impl
{
   public class WeChatServices : IWeChatServices
   {
      private TimeSpan _cacheDuration;
      private IWeChatApp _wechat;

      public void Setup(IWeChatApp wechat, TimeSpan cacheDuration)
      {
         if (wechat == null)
         {
            throw new ArgumentNullException("wechat");
         }

         _wechat = wechat;
         _cacheDuration = cacheDuration;
      }

      public string GetTicket(string accessToken)
      {
         Parameters.RequireNotEmpty("accessToken", accessToken);

         Tuple<string, DateTime> cachedTicketItem;
         if (_cachedAccessTokens.TryGetValue(accessToken, out cachedTicketItem) && CacheNotExpired(cachedTicketItem.Item2))
         {
            return cachedTicketItem.Item1;
         }

         var newTicket = _wechat.ProvideJsTicket(accessToken);
         var newItem = Tuple.Create(newTicket, DateTime.UtcNow);
         _cachedAccessTokens.AddOrUpdate(accessToken, newItem, (key, old) => newItem);
         return newTicket;
      }

      public string GetAccessToken(string appId, string appSecret)
      {
         Parameters.RequireNotEmpty("appId", appId);
         Parameters.RequireNotEmpty("appSecret", appSecret);

         var cachedKey = string.Format("appId={0}&appSecret={1}", appId, appSecret);
         Tuple<string, DateTime> cachedAccessTokenItem;
         if (_cachedAccessTokens.TryGetValue(cachedKey, out cachedAccessTokenItem) && CacheNotExpired(cachedAccessTokenItem.Item2))
         {
            return cachedAccessTokenItem.Item1;
         }

         var newAccessToken = _wechat.ProvideAccessToken(appId, appSecret);
         var newItem = Tuple.Create(newAccessToken, DateTime.UtcNow);
         _cachedAccessTokens.AddOrUpdate(cachedKey, newItem, (key, old) => newItem);

         return newAccessToken;
      }

      public SignaturePackage Signature(string jsTicket, string url)
      {
         var jsConfig = new SignaturePackage
         {
            NonceString = NonceString(),
            Timestamp = TimeStamp()
         };

         var dataList = new Dictionary<string, string> {
                {"noncestr", jsConfig.NonceString},
                {"timestamp", jsConfig.Timestamp.ToString() },
                {"jsapi_ticket", jsTicket},
                {"url", url}
            }.ToList();

         dataList.Sort(ParameterKeyComparison);
         var queryString = dataList.Aggregate(string.Empty, (query, item) => string.Concat(query, "&", item.Key, "=", item.Value)).TrimStart('&');

         using (var sha1 = new SHA1CryptoServiceProvider())
         {
            var hashed = sha1.ComputeHash(Encoding.Default.GetBytes(queryString));
            jsConfig.Signature = HexStringFromBytes(hashed);
            return jsConfig;
         }
      }

      private bool CacheNotExpired(DateTime timeCached)
      {
         var dueTime = DateTime.UtcNow - timeCached;
         return dueTime < _cacheDuration;
      }

      static long TimeStamp()
      {
         return (long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
      }

      static string NonceString()
      {
         return Guid.NewGuid().ToString("N").Substring(0, 16);
      }

      static string HexStringFromBytes(byte[] bytes)
      {
         var sb = new StringBuilder();
         foreach (byte b in bytes)
         {
            var hex = b.ToString("x2");
            sb.Append(hex);
         }
         return sb.ToString();
      }

      static int ParameterKeyComparison(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
      {
         return x.Key.CompareTo(y.Key);
      }


      private ConcurrentDictionary<string, Tuple<string, DateTime>> _cachedAccessTokens = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
      private ConcurrentDictionary<string, Tuple<string, DateTime>> _cachedJsTickets = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
   }
}
