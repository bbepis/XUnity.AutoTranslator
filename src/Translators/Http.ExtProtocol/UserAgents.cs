using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Http.ExtProtocol
{
   /// <summary>
   /// Class defining user agents for various browsers.
   ///
   /// Updated 2019-02-09.
   /// </summary>
   public static class UserAgents
   {
      /// <summary>
      /// Latest Chrome Win10 user-agent as of 2019-02-09.
      /// </summary>
      public static readonly string Chrome_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36";

      /// <summary>
      /// Latest Chrome Win7 user-agent as of 2019-02-09.
      /// </summary>
      public static readonly string Chrome_Win7_Latest = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36";

      /// <summary>
      /// Latest Firefox Win10 user-agent as of 2019-02-09.
      /// </summary>
      public static readonly string Firefox_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0";

      /// <summary>
      /// Latest Edge Win10 user-agent as of 2019-02-09.
      /// </summary>
      public static readonly string Edge_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763";
   }
}
