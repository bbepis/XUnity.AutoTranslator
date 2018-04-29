using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class WebClientReference
   {
      public WebClientReference( WebClient client )
      {
         Client = client;
         LastTimestamp = DateTime.UtcNow;
      }

      public WebClient Client { get; }

      public DateTime LastTimestamp { get; set; }
   }
}
