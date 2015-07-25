using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeChatUtil.Utils
{
   public static class ImagesNetHelper
   {
      /// <summary>
      /// Download a image from a specified path, and then save to a new path
      /// </summary>
      /// <param name="fromPath">a path of the orignal image</param>
      /// <param name="toPath">a new path to save the image</param>
      public static void DownloadImageAndSave(string fromPath, string toPath)
      {
         WebClient my = new WebClient();
         byte[] mybyte;
         mybyte = my.DownloadData(fromPath);
         MemoryStream ms = new MemoryStream(mybyte);
         System.Drawing.Image img;
         img = System.Drawing.Image.FromStream(ms);
         img.Save(toPath);
      }
   }
}
