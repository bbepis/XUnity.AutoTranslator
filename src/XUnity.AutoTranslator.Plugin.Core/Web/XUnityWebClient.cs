using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web.Internal;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   /// <summary>
   /// Web client that can be used to send web requests in a unity compatible way.
   /// </summary>
   public class XUnityWebClient : ConnectionTrackingWebClient
   {
      private HttpStatusCode? _responseCode;
      private CookieCollection _responseCookies;

      private CookieContainer _requestCookies;
      private WebHeaderCollection _requestHeaders;

      /// <summary>
      /// Default constructor.
      /// </summary>
      public XUnityWebClient()
      {
         Encoding = Encoding.UTF8;
      }

      private void UnityWebClient_UploadStringCompleted( object sender, XUnityUploadStringCompletedEventArgs ev )
      {
         UploadStringCompleted -= UnityWebClient_UploadStringCompleted;

         var handle = ev.UserState as XUnityWebResponse;

         try
         {
            handle.SetCompleted( _responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, ev.Result, responseHeaders, _responseCookies, ev.Error );
         }
         catch( Exception )
         {
            handle.SetCompleted( _responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, null, responseHeaders, _responseCookies, ev.Error );
         }
      }

      private void UnityWebClient_DownloadStringCompleted( object sender, XUnityDownloadStringCompletedEventArgs ev )
      {
         DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;

         var handle = ev.UserState as XUnityWebResponse;

         try
         {
            handle.SetCompleted( _responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, ev.Result, responseHeaders, _responseCookies, ev.Error );
         }
         catch( Exception )
         {
            handle.SetCompleted( _responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, null, responseHeaders, _responseCookies, ev.Error );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="address"></param>
      /// <returns></returns>
      protected override WebRequest GetWebRequest( Uri address )
      {
         var request = base.GetWebRequest( address );
         SetRequestVariables( request );
         return request;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="request"></param>
      /// <param name="result"></param>
      /// <returns></returns>
      protected override WebResponse GetWebResponse( WebRequest request, IAsyncResult result )
      {
         WebResponse response = base.GetWebResponse( request, result );
         SetResponseVariables( response );
         return response;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      protected override WebResponse GetWebResponse( WebRequest request )
      {
         WebResponse response = base.GetWebResponse( request );
         SetResponseVariables( response );
         return response;
      }

      private void SetRequestVariables( WebRequest r )
      {
         var httpRequest = r as HttpWebRequest;
         if( httpRequest != null )
         {
            if( _requestCookies != null )
            {
               httpRequest.CookieContainer = _requestCookies;
            }
            if( _requestHeaders != null )
            {
               Headers = _requestHeaders;
            }
            httpRequest.ReadWriteTimeout = (int)( Settings.Timeout * 1000 ) - 10000;
            httpRequest.Timeout = (int)( Settings.Timeout * 1000 ) - 5000;
         }
      }

      private void SetResponseVariables( WebResponse r )
      {
         var httpResponse = r as HttpWebResponse;
         if( httpResponse != null )
         {
            _responseCode = httpResponse.StatusCode;
            _responseCookies = httpResponse.Cookies;
         }
      }

      /// <summary>
      /// Sends a web request and received a response object that can be yielded.
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      public XUnityWebResponse Send( XUnityWebRequest request )
      {
         var handle = new XUnityWebResponse();

         _requestCookies = request.Cookies;
         _requestHeaders = request.Headers;

         if( request.Data == null )
         {
            try
            {
               DownloadStringCompleted += UnityWebClient_DownloadStringCompleted;
               DownloadStringAsync( request.Address, handle );
            }
            catch
            {
               DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;
               throw;
            }
         }
         else
         {
            try
            {
               UploadStringCompleted += UnityWebClient_UploadStringCompleted;
               UploadStringAsync( request.Address, request.Method, request.Data, handle );
            }
            catch
            {
               UploadStringCompleted -= UnityWebClient_UploadStringCompleted;
               throw;
            }
         }

         return handle;
      }
   }
}
