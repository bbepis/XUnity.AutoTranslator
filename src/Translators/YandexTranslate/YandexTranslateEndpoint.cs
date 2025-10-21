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
      private static readonly string HttpsServiceUrl = "https://translate.api.cloud.yandex.net/translate/v2/translate";

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
         context.DisableCertificateChecksFor( "translate.api.cloud.yandex.net" );

         // if the plugin cannot be enabled, simply throw so the user cannot select the plugin
         if( string.IsNullOrEmpty( _key ) ) throw new EndpointInitializationException( "The YandexTranslate endpoint requires an API key which has not been provided." );
         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         string sourceLang = FixLanguage(context.SourceLanguage);
         string targetLang = FixLanguage(context.DestinationLanguage);
         string text = context.UntranslatedText ?? string.Empty;

         string escapedText = JsonHelper.Escape(text);

         string body = $"{{\"sourceLanguageCode\":\"{sourceLang}\",\"targetLanguageCode\":\"{targetLang}\",\"texts\":[\"{escapedText}\"]}}";

         var request = new XUnityWebRequest("POST", HttpsServiceUrl, body);

         request.Headers["Authorization"] = $"Api-Key {_key}";
         request.Headers["Content-Type"] = "application/json; charset=utf-8";
         request.Headers["Accept"] = "application/json";

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;

         if (string.IsNullOrEmpty(data))
         {
            context.Fail("Empty response from Yandex.");
         }

         var obj = JSON.Parse( data );

         if (obj == null)
         {
            context.Fail("Failed to parse JSON from Yandex response.");
         }

         var translations = obj["translations"];
         if (translations == null || translations.Count == 0)
         {
            context.Fail("No translations found in response.");
         }

         var translation = translations[0]["text"]?.Value;

         if( string.IsNullOrEmpty( translation ) ) context.Fail( "Received no translation." );

         context.Complete( translation );
      }
   }
}
