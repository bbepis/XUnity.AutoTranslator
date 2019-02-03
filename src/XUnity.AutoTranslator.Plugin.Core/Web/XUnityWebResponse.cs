using System;
using System.Net;
using XUnity.AutoTranslator.Plugin.Core.Shim;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class XUnityWebResponse : CustomYieldInstructionShim
   {
      public void SetCompleted( HttpStatusCode code, string result, WebHeaderCollection headers, CookieCollection newCookies, Exception error )
      {
         IsCompleted = true;

         Code = code;
         Result = result;
         Headers = headers;
         NewCookies = newCookies;
         Error = error;
      }

      public HttpStatusCode Code { get; set; }
      public string Result { get; private set; }
      public WebHeaderCollection Headers { get; private set; }
      public CookieCollection NewCookies { get; private set; }
      public Exception Error { get; private set; }

      public override bool keepWaiting => !IsCompleted;
      internal bool IsCompleted { get; private set; } = false;
   }
}
