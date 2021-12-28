using System;
using System.Collections;
using System.Net;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Shims;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   /// <summary>
   /// Class represent a web response.
   /// </summary>
   public class XUnityWebResponse : CustomYieldInstructionShim
   {
      /// <summary>
      /// Default construcutor.
      /// </summary>
      public XUnityWebResponse()
      {
         InGameTimeout = Settings.Timeout;
      }

      internal void SetCompleted( HttpStatusCode code, string data, WebHeaderCollection headers, CookieCollection newCookies, Exception error )
      {
         IsCompleted = true;

         Code = code;
         Data = data;
         Headers = headers;
         NewCookies = newCookies;
         Error = error;
      }

      /// <summary>
      /// Gets the returned HTTP status code.
      /// </summary>
      public HttpStatusCode Code { get; private set; }

      /// <summary>
      /// Gets the returned data.
      /// </summary>
      public string Data { get; private set; }

      /// <summary>
      /// Gets the returned response headers.
      /// </summary>
      public WebHeaderCollection Headers { get; private set; }

      /// <summary>
      /// Gets the new cookies returned from the response by the
      /// Set-Cookie header.
      /// </summary>
      public CookieCollection NewCookies { get; private set; }

      /// <summary>
      /// Gets the error, if an error occurred.
      /// </summary>
      public Exception Error { get; private set; }

      /// <summary>
      /// Gets a bool indicating if Unity should keep waiting for the response to complete.
      /// </summary>
      public override bool keepWaiting => !IsCompleted;

      internal bool IsCompleted { get; private set; } = false;
   }
}
