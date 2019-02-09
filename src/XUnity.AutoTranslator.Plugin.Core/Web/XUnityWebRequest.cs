using System;
using System.Net;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   /// <summary>
   /// Class that represents a web request.
   /// </summary>
   public class XUnityWebRequest
   {
      private WebHeaderCollection _headers;

      /// <summary>
      /// Constructs a web request.
      /// </summary>
      /// <param name="method"></param>
      /// <param name="address"></param>
      /// <param name="data"></param>
      public XUnityWebRequest( string method, string address, string data )
      {
         Method = method;
         Address = new Uri( address );
         Data = data;
      }

      /// <summary>
      /// Constructs a web request.
      /// </summary>
      /// <param name="method"></param>
      /// <param name="address"></param>
      public XUnityWebRequest( string method, string address )
      {
         Method = method;
         Address = new Uri( address );
         Data = string.Empty;
      }

      /// <summary>
      /// Constructs a GET web request.
      /// </summary>
      /// <param name="address"></param>
      public XUnityWebRequest( string address )
      {
         Method = "GET";
         Address = new Uri( address );
      }

      /// <summary>
      /// Gets the HTTP method.
      /// </summary>
      public string Method { get; private set; }

      /// <summary>
      /// Gets the address that the request is issued against.
      /// </summary>
      public Uri Address { get; private set; }

      /// <summary>
      /// Gets the request data that is sent to the web service.
      /// </summary>
      public string Data { get; private set; }

      /// <summary>
      /// Gets or sets the CookieContainer to manage cookies.
      /// </summary>
      public CookieContainer Cookies { get; set; }

      /// <summary>
      /// Gets or sets the request headers.
      /// </summary>
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
