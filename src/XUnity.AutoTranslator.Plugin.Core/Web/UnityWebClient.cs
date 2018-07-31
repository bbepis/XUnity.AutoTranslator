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

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class UnityWebClient : WebClient
   {
      private KnownHttpEndpoint _httpEndpoint;

      public UnityWebClient( KnownHttpEndpoint endpoint )
      {
         _httpEndpoint = endpoint;
         Encoding = Encoding.UTF8;
         DownloadStringCompleted += UnityWebClient_DownloadStringCompleted;
      }

      private void UnityWebClient_DownloadStringCompleted( object sender, DownloadStringCompletedEventArgs ev )
      {
         var handle = ev.UserState as DownloadResult;

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
            var cookies = _httpEndpoint.ReadCookies();
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
            _httpEndpoint.WriteCookies( response );
         }
      }

      public DownloadResult GetDownloadResult( Uri address )
      {
         var handle = new DownloadResult();
         DownloadStringAsync( address, handle );
         return handle;
      }
   }

   public class DownloadResult : CustomYieldInstruction
   {
      private bool _isCompleted = false;

      public void SetCompleted( string result, string error )
      {
         _isCompleted = true;
         Result = result;
         Error = error;
      }

      public override bool keepWaiting => !_isCompleted;

      public string Result { get; set; }

      public string Error { get; set; }

      public bool Succeeded => Error == null;
   }
}
