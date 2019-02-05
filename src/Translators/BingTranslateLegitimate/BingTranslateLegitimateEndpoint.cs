using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace BingTranslateLegitimate
{
   internal class BingTranslateLegitimateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "af","ar","bn","bs","bg","yue","ca","zh-Hans","zh-Hant","hr","cs","da","nl","en","et","fj","fil","fi","fr","de","el","ht","he","hi","mww","hu","is","id","it","ja","sw","tlh","tlh-Qaak","ko","lv","lt","mg","ms","mt","nb","fa","pl","pt","otq","ro","ru","sm","sr-Cyrl","sr-Latn","sk","sl","es","sv","ty","ta","te","th","to","tr","uk","ur","vi","cy","yua"
      };

      private static readonly string HttpsServicePointTemplateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={0}&to={1}";
      private static readonly string RequestTemplate = "[{{\"Text\":\"{0}\"}}]";
      private static readonly System.Random RandomNumbers = new System.Random();

      private static readonly string[] Accepts = new string[] { "application/json" };
      private static readonly string[] ContentTypes = new string[] { "application/json" };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string ContentType = ContentTypes[ RandomNumbers.Next( ContentTypes.Length ) ];

      private string _key;

      public override string Id => "BingTranslateLegitimate";

      public override string FriendlyName => "Bing Translator (Authenticated)";

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "BingLegitimate", "OcpApimSubscriptionKey", "" );
         if( string.IsNullOrEmpty( _key ) ) throw new Exception( "The BingTranslateLegitimate endpoint requires an API key which has not been provided." );

         // Configure service points / service point manager
         context.EnableSslFor( "api.cognitive.microsofttranslator.com" );

         if( !SupportedLanguages.Contains( context.DestinationLanguage ) ) throw new Exception( $"The destination language {context.DestinationLanguage} is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var request = new XUnityWebRequest(
            "POST",
            string.Format( HttpsServicePointTemplateUrl, context.SourceLanguage, context.DestinationLanguage ),
            string.Format( RequestTemplate, context.UntranslatedText.EscapeJson() ) );

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
         var arr = JSON.Parse( context.Response.Data );

         var token = arr.AsArray[ 0 ]?.AsObject[ "translations" ]?.AsArray[ 0 ]?.AsObject[ "text" ]?.ToString();
         token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

         var translated = token;

         context.Complete( translated );
      }
   }
}
