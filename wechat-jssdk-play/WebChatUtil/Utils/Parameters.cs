using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebChatUtil.Utils
{
   static class Parameters
   {
      public static void RequireNotEmpty(string parameterName, object @value)
      {
         if (value is string && string.IsNullOrWhiteSpace(value as string))
         {
            throw new ArgumentNullException(parameterName);
         }

         if (value == null)
         {
            throw new ArgumentNullException(parameterName);
         }
      }
   }
}
