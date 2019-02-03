using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   internal class WatsonTranslateEndpoint : WwwEndpoint
   {
      private static readonly string RequestTemplate = "{{\"text\":[\"{2}\"],\"model_id\":\"{0}-{1}\"}}";

      private string _template;
      private string _url;
      private string _key;

      public WatsonTranslateEndpoint()
      {
      }

      public override string Id => "WatsonTranslate";

      public override string FriendlyName => "Watson Language Translator";

      public override void Initialize( InitializationContext context )
      {
         _url = context.Config.Preferences[ "Watson" ][ "Url" ].GetOrDefault( "" );
         _key = context.Config.Preferences[ "Watson" ][ "Key" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _url ) ) throw new Exception( "The WatsonTranslate endpoint requires a url which has not been provided." );
         if( string.IsNullOrEmpty( _key ) ) throw new Exception( "The WatsonTranslate endpoint requires a key which has not been provided." );

         _template = _url.TrimEnd( '/' ) + "/v3/translate?version=2018-05-01";
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return _template;
      }

      public override string GetRequestObject( string untranslatedText, string from, string to )
      {
         return string.Format( RequestTemplate, from, to, TextHelper.EscapeJson( untranslatedText ) );
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = string.IsNullOrEmpty( AutoTranslationState.UserAgent ) ? "curl/7.55.1" : AutoTranslationState.UserAgent;
         headers[ "Accept" ] = "application/json";
         headers[ "Content-Type" ] = "application/json";
         headers[ "Authorization" ] = "Basic " + Convert.ToBase64String( Encoding.ASCII.GetBytes( "apikey:" + _key ) );
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
   }
}
