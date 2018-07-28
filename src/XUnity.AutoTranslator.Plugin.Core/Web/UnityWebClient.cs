using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class UnityWebClient : WebClient
   {
      private static UnityWebClient _default;

      public static UnityWebClient Default
      {
         get
         {
            if( _default == null )
            {
               _default = new UnityWebClient();
            }
            return _default;
         }
      }

      private CookieContainer _container = new CookieContainer();

      public UnityWebClient()
      {
         Encoding = Encoding.UTF8;
         DownloadStringCompleted += UnityWebClient_DownloadStringCompleted;
      }

      public void ClearCookies()
      {
         _container = new CookieContainer();
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
            httpRequest.CookieContainer = _container;
         }
         return request;
      }

      public DownloadResult GetDownloadResult( Uri address )
      {
         var handle = new DownloadResult();

         DownloadStringAsync( address, handle );

         Console.WriteLine( "GetDownloadResult: " + address );

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
