using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Constants
{
   /// <summary>
   /// Class defining user agents for various browsers.
   /// </summary>
   public static class UserAgents
   {
      /// <summary>
      /// Latest Chrome Win10 user-agent as of 2021-01-05.
      /// </summary>
      public static readonly string Chrome_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";

      /// <summary>
      /// Latest Chrome Win7 user-agent as of 2020-01-04.
      /// </summary>
      public static readonly string Chrome_Win7_Latest = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

      /// <summary>
      /// Latest Firefox Win10 user-agent as of 2021-01-05.
      /// </summary>
      public static readonly string Firefox_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0";

      /// <summary>
      /// Latest Edge Win10 user-agent as of 2021-01-05.
      /// </summary>
      public static readonly string Edge_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.66";
   }
}
