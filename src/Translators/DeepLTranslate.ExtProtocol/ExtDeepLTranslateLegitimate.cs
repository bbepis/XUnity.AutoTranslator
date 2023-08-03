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

      static ExtDeepLTranslateLegitimate()
      {
         ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Ssl3
            | SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;
      }

      private string _httpsServicePointTemplateUrl = "https://api.deepl.com/v2/translate?auth_key={0}";

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
            _httpsServicePointTemplateUrl = "https://api-free.deepl.com/v2/translate?auth_key={0}";
         }
         else
         {
            _httpsServicePointTemplateUrl = "https://api.deepl.com/v2/translate?auth_key={0}";
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
         _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue( "XUnity", "5.3.0" ) );
         _client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
      }

      private static string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-Hans":
            case "zh-CN":
               return "zh";
            default:
               return lang;
         }
      }

      public async Task Translate( ITranslationContext context )
      {
         List<UntranslatedTextInfo> untranslatedTextInfos = new List<UntranslatedTextInfo>();

         var parameters = new List<KeyValuePair<string, string>>();
         parameters.Add( new KeyValuePair<string, string>( "auth_key", _apiKey ) );
         parameters.Add( new KeyValuePair<string, string>( "source_lang", FixLanguage( context.SourceLanguage ).ToUpperInvariant() ) );
         parameters.Add( new KeyValuePair<string, string>( "target_lang", FixLanguage( context.DestinationLanguage ).ToUpperInvariant() ) );
         parameters.Add( new KeyValuePair<string, string>( "split_sentences", "1" ) );

         foreach( var untranslatedTextInfo in context.UntranslatedTextInfos )
         {
            parameters.Add( new KeyValuePair<string, string>( "text", untranslatedTextInfo.UntranslatedText ) );
            untranslatedTextInfos.Add( new UntranslatedTextInfo { TranslationParts = new List<TranslationPart> { new TranslationPart { IsTranslatable = true, Value = untranslatedTextInfo.UntranslatedText } }, UntranslatedText = untranslatedTextInfo.UntranslatedText } );
         }

         var form = new FormUrlEncodedContent( parameters );

         using var request = new HttpRequestMessage( HttpMethod.Post, _httpsServicePointTemplateUrl );

         // create request
         using var response = await _client.PostAsync( string.Format( _httpsServicePointTemplateUrl, _apiKey ), form );
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
      }
   }
}
