using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class WatsonTranslateEndpoint : KnownWwwEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = Settings.WatsonAPIUrl.TrimEnd( '/' ) + "/v2/translate?model_id={0}-{1}&text={2}";

      private string _url;
      private string _username;
      private string _password;

      public WatsonTranslateEndpoint( string url, string username, string password )
      {
         if( string.IsNullOrEmpty( url ) ) throw new ArgumentException( "The WatsonTranslate endpoint requires a url which has not been provided.", nameof( url ) );
         if( string.IsNullOrEmpty( username ) ) throw new ArgumentException( "The WatsonTranslate endpoint requires a username which has not been provided.", nameof( username ) );
         if( string.IsNullOrEmpty( password ) ) throw new ArgumentException( "The WatsonTranslate endpoint requires a password which has not been provided.", nameof( password ) );

         _url = url;
         _username = username;
         _password = password;

         //ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( new Uri( _url ).Host );
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = Settings.GetUserAgent( "curl/7.55.1" );
         headers[ "Accept" ] = "application/json";
         headers[ "Accept-Charset" ] = "UTF-8";
         headers[ "Authorization" ] = "Basic " + System.Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes( _username + ":" + _password ) );
      }

      //public override void ApplyHeaders( WebHeaderCollection headers )
      //{
      //   headers[ HttpRequestHeader.UserAgent ] = "curl/7.55.1";
      //   headers[ HttpRequestHeader.Accept ] = "application/json";
      //   headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      //   headers[ HttpRequestHeader.Authorization ] = "Basic " + System.Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes( _username + ":" + _password ) );
      //}

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var obj = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in obj.AsObject[ "translations" ].AsArray )
            {
               var token = entry.AsObject[ "translation" ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }
            translated = lineBuilder.ToString();

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch( Exception )
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
