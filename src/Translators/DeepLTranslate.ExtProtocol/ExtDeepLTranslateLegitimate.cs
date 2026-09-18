using Common.ExtProtocol;
using Common.ExtProtocol.Utilities;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeepLTranslate.ExtProtocol
{
   public class ExtDeepLTranslateLegitimate : IExtTranslateEndpoint
   {
      private class UntranslatedTextInfo
      {
         public string UntranslatedText { get; set; }

         public List<TranslationPart> TranslationParts { get; set; }
      }

      public class TranslationPart
      {
         public bool IsTranslatable { get; set; }

         public string Value { get; set; }
      }

      private class TranslationResponse
      {
         public List<Translation> translations { get; set; }
      }

      private class Translation
      {
         public string detected_source_language { get; set; }

         public string text { get; set; }
      }

      private class LanguageInfo
      {
         public string lang { get; set; }

         public bool usable_as_source { get; set; }

         public bool usable_as_target { get; set; }
      }

      static ExtDeepLTranslateLegitimate()
      {
         ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Ssl3
            | SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;
      }

      private string _httpsServicePointTemplateUrl = "https://api.deepl.com/v2/translate";
      private string _httpsLanguagesUrl = "https://api.deepl.com/v3/languages?resource=translate_text";

      private readonly SemaphoreSlim _languagesSemaphore = new SemaphoreSlim( 1, 1 );
      private HashSet<string> _sourceLanguages;
      private HashSet<string> _targetLanguages;
      private DateTime _languagesExpiryUtc;

      private HttpClient _client;
      private HttpClientHandler _handler;
      private string _apiKey;

      public ExtDeepLTranslateLegitimate()
      {
         CreateClientAndHandler();
      }

      public void Initialize( string config )
      {
         var parts = config.Split( new[] { '\n' }, StringSplitOptions.None );

         _apiKey = parts[0];

         var free = parts[ 1 ];
         if(string.Equals(free, "true", StringComparison.OrdinalIgnoreCase))
         {
            _httpsServicePointTemplateUrl = "https://api-free.deepl.com/v2/translate";
            _httpsLanguagesUrl = "https://api-free.deepl.com/v3/languages?resource=translate_text";
         }
         else
         {
            _httpsServicePointTemplateUrl = "https://api.deepl.com/v2/translate";
            _httpsLanguagesUrl = "https://api.deepl.com/v3/languages?resource=translate_text";
         }
      }

      private void CreateClientAndHandler()
      {
         if( _client != null )
         {
            _client.Dispose();
         }

         _handler = new HttpClientHandler();
         _handler.CookieContainer = new CookieContainer();
         _handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

         _client = new HttpClient( _handler, true );
         _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue( "XUnity", GeneratedInfo.PROJECT_VERSION ) );
         _client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
      }

      private static string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-Hans":
            case "zh-CN":
               return "zh-hans";
            default:
               return lang;
         }
      }

      private async Task EnsureLanguagesAsync( ITranslationContext context )
      {
         await _languagesSemaphore.WaitAsync();
         try
         {
            if( _sourceLanguages == null || DateTime.UtcNow >= _languagesExpiryUtc )
            {
               using( var request = new HttpRequestMessage( HttpMethod.Get, _httpsLanguagesUrl ) )
               {
                  request.Headers.Add( "Authorization", "DeepL-Auth-Key " + _apiKey );
                  using( var response = await _client.SendAsync( request ) )
                  {
                     response.ThrowIfBlocked();
                     response.EnsureSuccessStatusCode();

                     var languages = JsonConvert.DeserializeObject<List<LanguageInfo>>( await response.Content.ReadAsStringAsync() );
                     if( languages == null ) throw new Exception( "DeepL returned no language information." );

                     _sourceLanguages = new HashSet<string>( languages.Where( x => x.usable_as_source ).Select( x => x.lang ), StringComparer.OrdinalIgnoreCase );
                     _targetLanguages = new HashSet<string>( languages.Where( x => x.usable_as_target ).Select( x => x.lang ), StringComparer.OrdinalIgnoreCase );
                     _languagesExpiryUtc = DateTime.UtcNow.AddHours( 24 );
                  }
               }
            }
         }
         finally
         {
            _languagesSemaphore.Release();
         }

         var sourceLanguage = FixLanguage( context.SourceLanguage );
         var targetLanguage = FixLanguage( context.DestinationLanguage );
         if( !string.Equals( sourceLanguage, "auto", StringComparison.OrdinalIgnoreCase ) && !_sourceLanguages.Contains( sourceLanguage ) )
            throw new Exception( $"DeepL does not support '{context.SourceLanguage}' as a source language." );
         if( !_targetLanguages.Contains( targetLanguage ) )
            throw new Exception( $"DeepL does not support '{context.DestinationLanguage}' as a target language." );
      }

      public async Task Translate( ITranslationContext context )
      {
         await EnsureLanguagesAsync( context );

         List<UntranslatedTextInfo> untranslatedTextInfos = new List<UntranslatedTextInfo>();

         var parameters = new List<KeyValuePair<string, string>>();
         if( !string.Equals(context.SourceLanguage, "auto", StringComparison.OrdinalIgnoreCase) ) {
            parameters.Add( new KeyValuePair<string, string>( "source_lang", FixLanguage( context.SourceLanguage ).ToUpperInvariant() ) );
         }
         parameters.Add( new KeyValuePair<string, string>( "target_lang", FixLanguage( context.DestinationLanguage ).ToUpperInvariant() ) );
         parameters.Add( new KeyValuePair<string, string>( "split_sentences", "1" ) );

         foreach( var untranslatedTextInfo in context.UntranslatedTextInfos )
         {
            parameters.Add( new KeyValuePair<string, string>( "text", untranslatedTextInfo.UntranslatedText ) );
            untranslatedTextInfos.Add( new UntranslatedTextInfo { TranslationParts = new List<TranslationPart> { new TranslationPart { IsTranslatable = true, Value = untranslatedTextInfo.UntranslatedText } }, UntranslatedText = untranslatedTextInfo.UntranslatedText } );
         }

         var form = new FormUrlEncodedContent( parameters );

         using var request = new HttpRequestMessage( HttpMethod.Post, _httpsServicePointTemplateUrl );
         request.Headers.Add( "Authorization", "DeepL-Auth-Key " + _apiKey );
         request.Content = form;

         using var response = await _client.SendAsync( request );
         response.ThrowIfBlocked();
         response.EnsureSuccessStatusCode();

         var str = await response.Content.ReadAsStringAsync();

         ExtractTranslation( str, untranslatedTextInfos, context );
      }

      private void ExtractTranslation( string data, List<UntranslatedTextInfo> untranslatedTextInfos, ITranslationContext context )
      {
         var obj = JsonConvert.DeserializeObject<TranslationResponse>( data );

         var translatedTexts = new List<string>();
         int transIdx = 0;
         for( int i = 0; i < untranslatedTextInfos.Count; i++ )
         {
            var parts = untranslatedTextInfos[ i ].TranslationParts;

            var fullTranslatedText = new StringBuilder();
            foreach( var part in parts )
            {
               if( part.IsTranslatable )
               {
                  var translation = obj.translations[ transIdx++ ].text;
                  fullTranslatedText.Append( translation );
               }
               else
               {
                  fullTranslatedText.Append( part.Value );
               }
            }

            var t = fullTranslatedText.ToString();
            if( string.IsNullOrWhiteSpace( t ) )
            {
               throw new Exception( "Found no valid translations in beam!" );
            }

            translatedTexts.Add( t );
         }

         context.Complete( translatedTexts.ToArray() );
      }

      public void Dispose()
      {
         _client?.Dispose();
         _languagesSemaphore.Dispose();
      }
   }
}
