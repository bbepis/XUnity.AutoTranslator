using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
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

namespace BingTranslateLegitimate
{
   public class BingTranslateLegitimateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "af","ar","bn","bs","bg","yue","ca","zh-Hans","zh-Hant","hr","cs","da","nl","en","et","fj","fil","fi","fr","de","el","ht","he","hi","mww","hu","is","id","it","ja","sw","tlh","tlh-Qaak","ko","lv","lt","mg","ms","mt","nb","fa","pl","pt","otq","ro","ru","sm","sr-Cyrl","sr-Latn","sk","sl","es","sv","ty","ta","te","th","to","tr","uk","ur","vi","cy","yua"
      };

      private static readonly string HttpsServicePointTemplateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={0}&to={1}";
      private static readonly Random RandomNumbers = new Random();

      private static readonly string[] Accepts = new string[] { "application/json" };
      private static readonly string[] ContentTypes = new string[] { "application/json" };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string ContentType = ContentTypes[ RandomNumbers.Next( ContentTypes.Length ) ];

      private string _key;

      public override string Id => "BingTranslateLegitimate";

      public override string FriendlyName => "Bing Translator (Authenticated)";

      public override int MaxTranslationsPerRequest => 10;

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-CN":
            case "zh":
               return "zh-Hans";
            case "zh-TW":
               return "zh-Hant";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "BingLegitimate", "OcpApimSubscriptionKey", "" );
         if( string.IsNullOrEmpty( _key ) ) throw new EndpointInitializationException( "The BingTranslateLegitimate endpoint requires an API key which has not been provided." );

         // Configure service points / service point manager
         context.DisableCertificateChecksFor( "api.cognitive.microsofttranslator.com" );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         StringBuilder data = new StringBuilder();
         data.Append( "[" );
         for( int i = 0 ; i < context.UntranslatedTexts.Length ; i++ )
         {
            var untranslatedText = JsonHelper.Escape( context.UntranslatedTexts[ i ] );
            data.Append( "{\"Text\":\"" );
            data.Append( untranslatedText );
            data.Append( "\"}" );

            if( context.UntranslatedTexts.Length - 1 != i )
            {
               data.Append( "," );
            }
         }
         data.Append( "]" );


         var request = new XUnityWebRequest(
            "POST",
            string.Format( HttpsServicePointTemplateUrl, FixLanguage( context.SourceLanguage ), FixLanguage( context.DestinationLanguage ) ),
            data.ToString() );

         if( Accept != null )
         {
            request.Headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( ContentType != null )
         {
            request.Headers[ HttpRequestHeader.ContentType ] = ContentType;
         }
         request.Headers[ "Ocp-Apim-Subscription-Key" ] = _key;

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var arr = JSON.Parse( context.Response.Data ).AsArray;

         var translations = new List<string>();
         for( int i = 0 ; i < arr.Count ; i++ )
         {
            var token = arr[ i ].AsObject[ "translations" ]?.AsArray[ 0 ]?.AsObject[ "text" ]?.ToString();
            var translation = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );

            translations.Add( translation );
         }

         context.Complete( translations.ToArray() );
      }
   }
}
