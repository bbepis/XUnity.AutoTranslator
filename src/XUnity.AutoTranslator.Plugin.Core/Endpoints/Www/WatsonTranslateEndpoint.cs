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
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   internal class WatsonTranslateEndpoint : WwwEndpoint
   {
      private string _template;
      private string _url;
      private string _username;
      private string _password;

      public WatsonTranslateEndpoint()
      {
      }

      public override string Id => "WatsonTranslate";

      public override string FriendlyName => "Watson Language Translator";

      public override void Initialize( InitializationContext context )
      {
         _url = context.Config.Preferences[ "Watson" ][ "WatsonAPIUrl" ].GetOrDefault( "" );
         _username = context.Config.Preferences[ "Watson" ][ "WatsonAPIUsername" ].GetOrDefault( "" );
         _password = context.Config.Preferences[ "Watson" ][ "WatsonAPIPassword" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _url ) ) throw new Exception( "The WatsonTranslate endpoint requires a url which has not been provided." );
         if( string.IsNullOrEmpty( _username ) ) throw new Exception( "The WatsonTranslate endpoint requires a username which has not been provided." );
         if( string.IsNullOrEmpty( _password ) ) throw new Exception( "The WatsonTranslate endpoint requires a password which has not been provided." );

         _template = _url.TrimEnd( '/' ) + "/v2/translate?model_id={0}-{1}&text={2}";
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = string.IsNullOrEmpty( AutoTranslationState.UserAgent ) ? "curl/7.55.1" : AutoTranslationState.UserAgent;
         headers[ "Accept" ] = "application/json";
         headers[ "Accept-Charset" ] = "UTF-8";
         headers[ "Authorization" ] = "Basic " + System.Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes( _username + ":" + _password ) );
      }

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
         return string.Format( _template, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
