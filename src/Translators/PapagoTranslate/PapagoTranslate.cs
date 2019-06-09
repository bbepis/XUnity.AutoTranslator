using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace PapagoTranslate
{
   public class PapagoTranslate : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string> { "en", "ko", "zh-CN", "zh-TW", "es", "fr", "ru", "vi", "th", "id", "de", "ja" };

      private static readonly string Url = "https://papago.naver.com/apis/n2mt/translate";
      private static readonly string Website = "https://papago.naver.com";
      private static readonly string JsonTemplate = "{{\"deviceId\":\"{0}\",\"dict\":false,\"dictDisplay\":0,\"honorific\":false,\"instant\":false,\"source\":\"{1}\",\"target\":\"{2}\",\"text\":\"{3}\"}}";
      private static readonly string FormUrlEncodedTemplate = "data={0}";

      private CookieContainer _cookies;
      private string _deviceId;
      private int _translationCount = 0;

      public override string Id => "PapagoTranslate";

      public override string FriendlyName => "Papago Translator";

      public override int MaxTranslationsPerRequest => 10;

      public override void Initialize( IInitializationContext context )
      {
         context.DisableCertificateChecksFor( "papago.naver.com" );

         if( !SupportedLanguages.Contains( context.DestinationLanguage ) ) throw new Exception( $"The language '{context.DestinationLanguage}' is not supported by Papago Translate." );
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         if( _translationCount % 133 == 0 )
         {
            _cookies = new CookieContainer();
            _deviceId = Guid.NewGuid().ToString();

            // terminate session?????

            var client = new XUnityWebClient();
            var request = new XUnityWebRequest( Website );
            request.Cookies = _cookies;

            SetupDefaultHeaders( request );

            var response = client.Send( request );
            while( response.MoveNext() ) yield return response.Current;

            // dont actually cared about the response, just the cookies
         }
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var fullTranslationText = string.Join( "\n", context.UntranslatedTexts );
         var jsonString = string.Format( JsonTemplate, _deviceId, context.SourceLanguage, context.DestinationLanguage, JsonHelper.Escape( fullTranslationText ) );
         var base64 = Convert.ToBase64String( Encoding.UTF8.GetBytes( jsonString ) );
         var obfuscatedBase64 = Obfuscate( 16, base64 );
         var data = string.Format( FormUrlEncodedTemplate, Uri.EscapeDataString( obfuscatedBase64 ) );

         var request = new XUnityWebRequest( "POST", Url, data );
         request.Cookies = _cookies;

         SetupDefaultHeaders( request );
         SetupApiRequestHeaders( request );

         context.Complete( request );

         _translationCount++;
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var obj = JSON.Parse( context.Response.Data ).AsObject;
         var token = obj[ "translatedText" ].ToString();
         var fullTranslatedText = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );

         if( context.UntranslatedTexts.Length == 1 )
         {
            context.Complete( fullTranslatedText );
         }
         else
         {
            var splittedTranslations = fullTranslatedText.Split( '\n' );
            var allTranslations = new string[ context.UntranslatedTexts.Length ];
            int idx = 0;
            for( int i = 0; i < context.UntranslatedTexts.Length; i++ )
            {
               var untranslatedLines = context.UntranslatedTexts[ i ].Split( '\n' );

               StringBuilder builder = new StringBuilder();
               for( int j = 0; j < untranslatedLines.Length; j++ )
               {
                  var translatedLine = splittedTranslations[ idx++ ];
                  if( untranslatedLines.Length - 1 == j )
                  {
                     builder.Append( translatedLine );
                  }
                  else
                  {
                     builder.AppendLine( translatedLine );
                  }
               }

               allTranslations[ i ] = builder.ToString();
            }

            if( idx != splittedTranslations.Length ) context.Fail( "Received invalid number of translations in batch." );

            context.Complete( allTranslations );
         }
      }

      private static void SetupDefaultHeaders( XUnityWebRequest request )
      {
         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslatorSettings.UserAgent ) ? UserAgents.Chrome_Win10_Latest : AutoTranslatorSettings.UserAgent;
         request.Headers[ "Accept-Language" ] = "en-US";
      }

      private static void SetupApiRequestHeaders( XUnityWebRequest request )
      {
         request.Headers[ "device-type" ] = "pc";
         request.Headers[ "Accept" ] = "application/json";
         request.Headers[ "x-apigw-partnerid" ] = "papago";
         request.Headers[ "Content-Type" ] = "application/x-www-form-urlencoded; charset=UTF-8";
         request.Headers[ "Origin" ] = "https://papago.naver.com";
         request.Headers[ "Referer" ] = "https://papago.naver.com/";
      }

      private static string Obfuscate( int count, string str )
      {
         var builder = new StringBuilder();
         for( int i = 0; i < count; i++ )
         {
            var c = str[ i ];
            if( ( 'a' <= c && c <= 'm' ) || ( 'A' <= c && c <= 'M' ) )
            {
               c += (char)13;

            }
            else if( ( 'n' <= c && c <= 'z' ) || 'N' <= c && c <= 'Z' )
            {
               c -= (char)13;
            }

            builder.Append( c );
         }

         for( int i = count; i < str.Length; i++ )
         {
            builder.Append( str[ i ] );
         }

         return builder.ToString();
      }
   }
}
