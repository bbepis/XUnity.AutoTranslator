using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Logging;

namespace PapagoTranslate
{
   public class PapagoTranslateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string> { "en", "ko", "zh-CN", "zh-TW", "es", "fr", "ru", "vi", "th", "id", "de", "ja", "hi", "pt" };
      private static readonly HashSet<string> SMTLanguages = new HashSet<string> { "hi", "pt" };

      private static readonly string UrlBase = "https://papago.naver.com";
      private static readonly string UrlN2MT = "/apis/n2mt/translate"; // Neural Machine Translation
      private static readonly string UrlNSMT = "/apis/nsmt/translate"; // Statistical Machine Translation
      private static readonly string FormUrlEncodedTemplate = "deviceId={0}&locale=en&dict=false&honorific=false&instant=true&source={1}&target={2}&text={3}";
      private static readonly Random RandomNumbers = new Random();
      private static readonly Guid UUID = Guid.NewGuid();
      private static readonly Regex PatternSource = new Regex( @"/vendors~main[^""]+", RegexOptions.Singleline );
      private static readonly Regex PatternVersion = new Regex( @"v\d\.\d\.\d_[^""]+", RegexOptions.Singleline );

      private string _version; // for hmac key
      private bool _isSMT;
      private int _translationCount = 0;
      private int _resetAfter = 0;

      public override string Id => "PapagoTranslate";

      public override string FriendlyName => "Papago Translator";

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
         context.DisableCertificateChecksFor( "papago.naver.com" );

         var fixedSourceLanguage = FixLanguage( context.SourceLanguage );
         var fixedDestinationLanguage = FixLanguage( context.DestinationLanguage );

         _isSMT = SMTLanguages.Contains( fixedSourceLanguage ) || SMTLanguages.Contains( fixedDestinationLanguage );

         if( !SupportedLanguages.Contains( fixedDestinationLanguage ) )
            throw new EndpointInitializationException( $"The language '{context.DestinationLanguage}' is not supported by Papago Translate." );

         if( _isSMT )
         {
            // SMT can only be translated into English
            if( fixedSourceLanguage != "en" && fixedDestinationLanguage != "en" )
               throw new EndpointInitializationException( $"Translation from '{context.SourceLanguage}' to '{context.DestinationLanguage}' is not supported by Papago Translate." );
         }
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         if( _resetAfter == 0 || _translationCount % _resetAfter == 0 )
         {
            _translationCount = 1;
            _resetAfter = RandomNumbers.Next( 150, 200 );

            var enumerator = SetupVersion();
            while( enumerator.MoveNext() ) yield return enumerator.Current;
         }
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var text = Uri.EscapeDataString( string.Join( "\n", context.UntranslatedTexts ) );
         var request = new XUnityWebRequest(
            "POST",
            UrlBase + ( _isSMT ? UrlNSMT : UrlN2MT ),
            string.Format(
               FormUrlEncodedTemplate,
               UUID,
               FixLanguage( context.SourceLanguage ),
               FixLanguage( context.DestinationLanguage ),
               text ) );

         // create token
         var timestamp = Math.Truncate( DateTime.UtcNow.Subtract( DateTime.MinValue.AddYears( 1969 ) ).TotalMilliseconds );
         var key = Encoding.UTF8.GetBytes( _version );
         var data = Encoding.UTF8.GetBytes( $"{UUID}\n{request.Address}\n{timestamp}" );
         var token = Convert.ToBase64String( new HMACMD5( key ).ComputeHash( data ) );

         // set required headers
         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslatorSettings.UserAgent ) ? UserAgents.Chrome_Win10_Latest : AutoTranslatorSettings.UserAgent;
         request.Headers[ "Authorization" ] = $"PPG {UUID}:{token}";
         request.Headers[ "Content-Type" ] = "application/x-www-form-urlencoded; charset=UTF-8";
         request.Headers[ "Timestamp" ] = timestamp.ToString();

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

      private IEnumerator SetupVersion()
      {
         var client = new XUnityWebClient();

         string urlSource;

         // parse javascript url from main page
         {
            var response = client.Send( new XUnityWebRequest( UrlBase ) );

            var iterator = response.GetSupportedEnumerator();
            while( iterator.MoveNext() ) yield return iterator.Current;

            var match = PatternSource.Match( response.Data );
            if( !match.Success )
            {
               XuaLogger.AutoTranslator.Warn( "Could not parse papago page" );
               yield break;
            }

            urlSource = match.Value;
         }

         // parse version from javascript file
         {
            var response = client.Send( new XUnityWebRequest( UrlBase + urlSource ) );

            var iterator = response.GetSupportedEnumerator();
            while( iterator.MoveNext() ) yield return iterator.Current;

            var match = PatternVersion.Match( response.Data );
            if( !match.Success )
            {
               XuaLogger.AutoTranslator.Warn( "Could not parse papago version" );
               yield break;
            }

            XuaLogger.AutoTranslator.Debug( $"Current papago version is {match.Value}" );

            _version = match.Value;
         }
      }
   }
}
