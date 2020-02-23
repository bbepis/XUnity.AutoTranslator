using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace GoogleTranslateLegitimate
{
   internal class GoogleTranslateLegitimateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string> { "af","sq","am","ar","hy","az","eu","be","bn","bs","bg","ca","ceb","zh","zh-Hans","zh-Hant","zh-CN","zh-TW","co","hr","cs","da","nl","en","eo","et","fi","fr","fy","gl","ka","de","el","gu","ht","ha","haw","he","hi","hmn","hu","is","ig","id","ga","it","ja","jw","kn","kk","km","ko","ku","ky","lo","la","lv","lt","lb","mk","mg","ms","ml","mt","mi","mr","mn","my","ne","no","ny","ps","fa","pl","pt","pa","ro","ru","sm","gd","sr","st","sn","sd","si","sk","sl","so","es","su","sw","sv","tl","tg","ta","te","th","tr","uk","ur","uz","vi","cy","xh","yi","yo","zu" };
      private static readonly string HttpsServiceUrl = "https://translation.googleapis.com/language/translate/v2";

      private string _key;

      public override string Id => "GoogleTranslateLegitimate";

      public override string FriendlyName => "Google! Translate (Authenticated)";

      public override int MaxTranslationsPerRequest => 10;

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-Hans":
            case "zh":
               return "zh-CN";
            case "zh-Hant":
               return "zh-TW";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "GoogleLegitimate", "GoogleAPIKey", "" );
         if( string.IsNullOrEmpty( _key ) ) throw new EndpointInitializationException( "The GoogleTranslateLegitimate endpoint requires an API key which has not been provided." );

         // Configure service points / service point manager
         context.DisableCertificateChecksFor( "translation.googleapis.com" );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var urlBuilder = new StringBuilder( HttpsServiceUrl );
         urlBuilder.Append( "?key=" ).Append( Uri.EscapeDataString( _key ) );
         urlBuilder.Append( "&source=" ).Append( FixLanguage( context.SourceLanguage ) );
         urlBuilder.Append( "&target=" ).Append( FixLanguage( context.DestinationLanguage ) );
         for( int i = 0 ; i < context.UntranslatedTexts.Length ; i++ )
         {
            var untranslatedText = context.UntranslatedTexts[ i ];
            urlBuilder.Append( "&q=" ).Append( Uri.EscapeDataString( untranslatedText ) );
         }

         var request = new XUnityWebRequest(
            "POST",
            urlBuilder.ToString() );

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var arr = JSON.Parse( data ).AsObject[ "data" ].AsObject[ "translations" ].AsArray;

         var translatedTexts = new List<string>();
         for( int i = 0 ; i < arr.Count ; i++ )
         {
            var obj = arr[ i ];
            var token = obj.AsObject[ "translatedText" ].ToString();
            var translatedText = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );
            translatedTexts.Add( translatedText );
         }

         context.Complete( translatedTexts.ToArray() );
      }
   }
}
