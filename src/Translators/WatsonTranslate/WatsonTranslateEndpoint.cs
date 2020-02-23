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
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Www;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace WatsonTranslate
{
   internal class WatsonTranslateEndpoint : WwwEndpoint
   {
      private static readonly HashSet<string> SupportedLanguagePairs = new HashSet<string> { "ar-en", "ca-es", "zh-en", "zh-TW-en", "cs-en", "da-en", "nl-en", "en-ar", "en-cs", "en-da", "en-de", "en-es", "en-fi", "en-fr", "en-hi", "en-it", "en-ja", "en-ko", "en-nb", "en-nl", "en-pl", "en-pt", "en-ru", "en-sv", "en-tr", "en-zh", "en-zh-TW", "fi-en", "fr-de", "fr-en", "fr-es", "de-en", "de-fr", "de-it", "hi-en", "hu-en", "it-de", "it-en", "ja-en", "ko-en", "nb-en", "pl-en", "pt-en", "ru-en", "es-ca", "es-en", "es-fr", "sv-en", "tr-en" };

      private string _fullUrl;
      private string _url;
      private string _key;

      public override string Id => "WatsonTranslate";

      public override string FriendlyName => "Watson Language Translator";

      public override int MaxTranslationsPerRequest => 10;

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-CN":
            case "zh-Hans":
               return "zh";
            case "zh-Hant":
               return "zh-TW";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         _url = context.GetOrCreateSetting( "Watson", "Url", "" );
         _key = context.GetOrCreateSetting( "Watson", "Key", "" );
         if( string.IsNullOrEmpty( _url ) ) throw new EndpointInitializationException( "The WatsonTranslate endpoint requires a url which has not been provided." );
         if( string.IsNullOrEmpty( _key ) ) throw new EndpointInitializationException( "The WatsonTranslate endpoint requires a key which has not been provided." );

         _fullUrl = _url.TrimEnd( '/' ) + "/v3/translate?version=2018-05-01";

         var model = FixLanguage( context.SourceLanguage ) + "-" + FixLanguage( context.DestinationLanguage );
         if( !SupportedLanguagePairs.Contains( model ) ) throw new EndpointInitializationException( $"The language model '{model}' is not supported." );
      }

      public override void OnCreateRequest( IWwwRequestCreationContext context )
      {
         StringBuilder data = new StringBuilder();
         data.Append( "{\"text\":[" );
         for( int i = 0 ; i < context.UntranslatedTexts.Length ; i++ )
         {
            var untranslatedText = JsonHelper.Escape( context.UntranslatedTexts[ i ] );
            data.Append( "\"" ).Append( untranslatedText ).Append( "\"" );

            if( context.UntranslatedTexts.Length - 1 != i )
            {
               data.Append( "," );
            }
         }
         data.Append( "],\"model_id\":\"" )
            .Append( FixLanguage( context.SourceLanguage ) )
            .Append( "-" )
            .Append( FixLanguage( context.DestinationLanguage ) )
            .Append( "\"}" );

         var request = new WwwRequestInfo(
            _fullUrl,
            data.ToString() );
         
         request.Headers[ "Accept" ] = "application/json";
         request.Headers[ "Content-Type" ] = "application/json";
         request.Headers[ "Authorization" ] = "Basic " + Convert.ToBase64String( Encoding.ASCII.GetBytes( "apikey:" + _key ) );

         context.Complete( request );
      }

      public override void OnExtractTranslation( IWwwTranslationExtractionContext context )
      {
         var data = context.ResponseData;
         var arr = JSON.Parse( data ).AsObject[ "translations" ].AsArray;

         var translatedTexts = new List<string>();
         for( int i = 0 ; i < arr.Count ; i++ )
         {
            var token = arr[ i ].AsObject[ "translation" ].ToString();
            var translatedText = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );
            translatedTexts.Add( translatedText );
         }

         context.Complete( translatedTexts.ToArray() );
      }
   }
}
