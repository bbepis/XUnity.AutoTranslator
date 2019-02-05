using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace YandexTranslate
{
   internal class YandexTranslateEndpoint : HttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={3}&text={2}&lang={0}-{1}&format=plain";

      private string _key;

      public override string Id => "YandexTranslate";

      public override string FriendlyName => "Yandex Translate";

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "Yandex", "YandexAPIKey", "" );
         context.EnableSslFor( "translate.yandex.net" );

         // if the plugin cannot be enabled, simply throw so the user cannot select the plugin
         if( string.IsNullOrEmpty( _key ) ) throw new Exception( "The YandexTranslate endpoint requires an API key which has not been provided." );
         if( context.SourceLanguage != "ja" ) throw new Exception( "Current implementation only supports japanese-to-english." );
         if( context.DestinationLanguage != "en" ) throw new Exception( "Current implementation only supports japanese-to-english." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var request = new XUnityWebRequest(
            string.Format(
               HttpsServicePointTemplateUrl,
               context.SourceLanguage,
               context.DestinationLanguage,
               WWW.EscapeURL( context.UntranslatedText ),
               _key ) );

         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslatorSettings.UserAgent ) ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.183 Safari/537.36 Vivaldi/1.96.1147.55" : AutoTranslatorSettings.UserAgent;
         request.Headers[ HttpRequestHeader.Accept ] = "*/*";
         request.Headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var obj = JSON.Parse( data );
         var lineBuilder = new StringBuilder( data.Length );

         var code = obj.AsObject[ "code" ].ToString();

         if( code == "200" )
         {
            var token = obj.AsObject[ "text" ].ToString();
            token = token.Substring( 2, token.Length - 4 ).UnescapeJson();

            if( string.IsNullOrEmpty( token ) ) return; 

            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );
            lineBuilder.Append( token );

            var translated = lineBuilder.ToString();

            context.Complete( translated );
         }
      }
   }
}
