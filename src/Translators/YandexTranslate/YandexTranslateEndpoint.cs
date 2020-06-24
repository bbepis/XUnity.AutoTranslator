using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
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
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string> { "az", "sq", "am", "en", "ar", "hy", "af", "eu", "ba", "be", "bn", "my", "bg", "bs", "cy", "hu", "vi", "ht", "gl", "nl", "mrj", "el", "ka", "gu", "da", "he", "yi", "id", "ga", "it", "is", "es", "kk", "kn", "ca", "ky", "zh", "ko", "xh", "km", "lo", "la", "lv", "lt", "lb", "mg", "ms", "ml", "mt", "mk", "mi", "mr", "mhr", "mn", "de", "ne", "no", "pa", "pap", "fa", "pl", "pt", "ro", "ru", "ceb", "sr", "si", "sk", "sl", "sw", "su", "tg", "th", "tl", "ta", "tt", "te", "tr", "udm", "uz", "uk", "ur", "fi", "fr", "hi", "hr", "cs", "sv", "gd", "et", "eo", "jv", "ja" };
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={3}&text={2}&lang={0}-{1}&format=plain";

      private string _key;

      public override string Id => "YandexTranslate";

      public override string FriendlyName => "Yandex Translate";

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-CN":
            case "zh-Hans":
               return "zh";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "Yandex", "YandexAPIKey", "" );
         context.DisableCertificateChecksFor( "translate.yandex.net" );

         // if the plugin cannot be enabled, simply throw so the user cannot select the plugin
         if( string.IsNullOrEmpty( _key ) ) throw new EndpointInitializationException( "The YandexTranslate endpoint requires an API key which has not been provided." );
         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var request = new XUnityWebRequest(
            string.Format(
               HttpsServicePointTemplateUrl,
               FixLanguage( context.SourceLanguage ),
               FixLanguage( context.DestinationLanguage ),
               Uri.EscapeDataString( context.UntranslatedText ),
               _key ) );
         
         request.Headers[ HttpRequestHeader.Accept ] = "*/*";
         request.Headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var obj = JSON.Parse( data );

         var code = obj.AsObject[ "code" ].ToString();
         if( code != "200" ) context.Fail( "Received bad response code: " + code );

         var token = obj.AsObject[ "text" ].ToString();
         var translation = JsonHelper.Unescape( token.Substring( 2, token.Length - 4 ) );

         if( string.IsNullOrEmpty( translation ) ) context.Fail( "Received no translation." );

         context.Complete( translation );
      }
   }
}
