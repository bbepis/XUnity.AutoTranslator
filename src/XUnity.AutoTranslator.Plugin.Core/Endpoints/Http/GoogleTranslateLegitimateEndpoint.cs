using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   internal class GoogleTranslateLegitimateEndpoint : HttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translation.googleapis.com/language/translate/v2?key={0}";

      private string _key;

      public override string Id => "GoogleTranslateLegitimate";

      public override string FriendlyName => "Google! Translate (Authenticated)";

      public override void Initialize( InitializationContext context )
      {
         _key = context.Config.Preferences[ "GoogleLegitimate" ][ "GoogleAPIKey" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _key ) ) throw new Exception( "The GoogleTranslateLegitimate endpoint requires an API key which has not been provided." );

         // Configure service points / service point manager
         context.HttpSecurity.EnableSslFor( "translation.googleapis.com" );
      }

      public override XUnityWebRequest CreateTranslationRequest( string untranslatedText, string from, string to )
      {
         var b = new StringBuilder();
         b.Append( "{" );
         b.Append( "\"q\":\"" ).Append( untranslatedText.EscapeJson() ).Append( "\"," );
         b.Append( "\"target\":\"" ).Append( to ).Append( "\"," );
         b.Append( "\"source\":\"" ).Append( from ).Append( "\"," );
         b.Append( "\"format\":\"text\"" );
         b.Append( "}" );
         var data = b.ToString();

         var request = new XUnityWebRequest(
            "POST",
            string.Format( HttpsServicePointTemplateUrl, WWW.EscapeURL( _key ) ),
            data );

         return request;
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var obj = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in obj.AsObject[ "data" ].AsObject[ "translations" ].AsArray )
            {
               var token = entry.AsObject[ "translatedText" ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }

            translated = lineBuilder.ToString();

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }
   }
}
