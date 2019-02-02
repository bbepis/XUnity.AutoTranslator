using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Shim;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class UnityWebClient : ConnectionTrackingWebClient
   {
      private HttpEndpoint _httpEndpoint;

      public UnityWebClient( HttpEndpoint endpoint )
      {
         _httpEndpoint = endpoint;
         Encoding = Encoding.UTF8;
      }

      private void UnityWebClient_UploadStringCompleted( object sender, UnityUploadStringCompletedEventArgs ev )
      {
         UploadStringCompleted -= UnityWebClient_UploadStringCompleted;

         var handle = ev.UserState as UnityWebResponse;

         // obtain result, error, etc.
         string text = null;
         string error = null;

         try
         {
            if( ev.Error == null )
            {
               text = ev.Result ?? string.Empty;
            }
            else
            {
               error = ev.Error.ToString();
            }
         }
         catch( Exception e )
         {
            error = e.ToString();
         }

         handle.SetCompleted( text, error );
      }

      private void UnityWebClient_DownloadStringCompleted( object sender, UnityDownloadStringCompletedEventArgs ev )
      {
         DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;

         var handle = ev.UserState as UnityWebResponse;

         // obtain result, error, etc.
         string text = null;
         string error = null;

         try
         {
            if( ev.Error == null )
            {
               text = ev.Result ?? string.Empty;
            }
            else
            {
               error = ev.Error.ToString();
            }
         }
         catch( Exception e )
         {
            error = e.ToString();
         }

         handle.SetCompleted( text, error );
      }

      protected override WebRequest GetWebRequest( Uri address )
      {
         var request = base.GetWebRequest( address );
         var httpRequest = request as HttpWebRequest;
         if( httpRequest != null )
         {
            var cookies = _httpEndpoint.GetCookiesForNewRequest();
            httpRequest.CookieContainer = cookies;
         }
         return request;
      }

      protected override WebResponse GetWebResponse( WebRequest request, IAsyncResult result )
      {
         WebResponse response = base.GetWebResponse( request, result );
         WriteCookies( response );
         return response;
      }

      protected override WebResponse GetWebResponse( WebRequest request )
      {
         WebResponse response = base.GetWebResponse( request );
         WriteCookies( response );
         return response;
      }

      private void WriteCookies( WebResponse r )
      {
         var response = r as HttpWebResponse;
         if( response != null )
         {
            _httpEndpoint.StoreCookiesFromResponse( response );
         }
      }

      public UnityWebResponse DownloadStringByUnityInstruction( Uri address )
      {
         var handle = new UnityWebResponse();

         try
         {
            DownloadStringCompleted += UnityWebClient_DownloadStringCompleted;
            DownloadStringAsync( address, handle );
         }
         catch
         {
            DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;
            throw;
         }

         return handle;
      }

      public UnityWebResponse UploadStringByUnityInstruction( Uri address, string request )
      {
         var handle = new UnityWebResponse();

         try
         {
            UploadStringCompleted += UnityWebClient_UploadStringCompleted;
            UploadStringAsync( address, "POST", request, handle );
         }
         catch
         {
            UploadStringCompleted -= UnityWebClient_UploadStringCompleted;
            throw;
         }

         return handle;
      }
   }

   public class UnityWebResponse : CustomYieldInstructionShim
   {
      public void SetCompleted( string result, string error )
      {
         IsCompleted = true;

         Result = result;
         Error = error;
      }

      public override bool keepWaiting => !IsCompleted;

      public string Result { get; set; }

      public string Error { get; set; }

      public bool IsCompleted { get; private set; } = false;

      public bool Succeeded => Error == null;
   }
}
