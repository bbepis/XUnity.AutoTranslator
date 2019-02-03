using System;
using System.Net;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class XUnityWebRequest
   {
      private WebHeaderCollection _headers;

      public XUnityWebRequest( string method, string address, string data )
      {
         Method = method;
         Address = new Uri( address );
         Data = data;
      }

      public XUnityWebRequest( string address )
      {
         Method = "GET";
         Address = new Uri( address );
      }

      public string Method { get; private set; }
      public Uri Address { get; private set; }
      public string Data { get; private set; }

      public CookieContainer Cookies { get; set; }
      public WebHeaderCollection Headers
      {
         get
         {
            if( _headers == null ) _headers = new WebHeaderCollection();

            return _headers;
         }
         set
         {
            _headers = value;
         }
      }
   }
}
