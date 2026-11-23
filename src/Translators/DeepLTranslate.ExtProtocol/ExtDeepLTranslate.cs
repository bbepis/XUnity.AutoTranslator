using Common.ExtProtocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
   [Serializable]
   public class BlockedException : Exception
   {
      public BlockedException() { }
      public BlockedException( string message ) : base( message ) { }
      public BlockedException( string message, Exception inner ) : base( message, inner ) { }
      protected BlockedException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
   }

   public static class HttpResponseMessageExtensions
   {
      public static void ThrowIfBlocked( this HttpResponseMessage msg )
      {
         if( msg.StatusCode == (HttpStatusCode)429 )
         {
            throw new BlockedException( "Too many requests!" );
         }
      }
   }

   public class ExtDeepLTranslate : IExtTranslateEndpoint
   {
      private class TranslationPart
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
         public string text { get; set; }
      }

      static ExtDeepLTranslate()
      {
         ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Ssl3
            | SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;
      }

      private static readonly Regex NewlineSplitter = new Regex( @"([\s]*[\r\n]+[\s]*)");

      private const string OneshotTranslateUrl = "https://oneshot-free.www.deepl.com/v1/translate";
      private const int MaxAnonymousTextLength = 1500;
      private const string OutdatedClientNotice = "older version of DeepL";

      private readonly SemaphoreSlim _sem;
      private HttpClient _client;
      private HttpClientHandler _handler;

      public ExtDeepLTranslate()
      {
         _sem = new SemaphoreSlim( 1, 1 );
         CreateClientAndHandler();
      }

      public void Initialize( string config )
      {

      }

      private void CreateClientAndHandler()
      {
         _handler = new HttpClientHandler();
         _handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

         _client = new HttpClient( _handler, true );
         _client.DefaultRequestHeaders.TryAddWithoutValidation( "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36" );
         _client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
      }

      private static string FixSourceLanguage( string lang )
      {
         return lang.Split( '-' )[ 0 ].ToLowerInvariant();
      }

      private static string FixTargetLanguage( string lang )
      {
         switch( lang.ToLowerInvariant() )
         {
            case "zh-hans":
            case "zh-cn":
               return "zh-Hans";
            case "zh-hant":
            case "zh-tw":
               return "zh-Hant";
            case "en":
            case "en-us":
               return "en-US";
            case "en-gb":
               return "en-GB";
            case "pt":
            case "pt-pt":
               return "pt-PT";
            case "pt-br":
               return "pt-BR";
            default:
               return lang.ToLowerInvariant();
         }
      }

      public async Task Translate( ITranslationContext context )
      {
         try
         {
            await _sem.WaitAsync();

            var translatedTexts = new List<string>();
            foreach( var untranslatedTextInfo in context.UntranslatedTextInfos )
            {
               var parts = NewlineSplitter
                  .Split( untranslatedTextInfo.UntranslatedText )
                  .Select( x => new TranslationPart { Value = x, IsTranslatable = !NewlineSplitter.IsMatch( x ) && !string.IsNullOrWhiteSpace( x ) } )
                  .ToList();

               var fullTranslatedText = new StringBuilder();
               foreach( var part in parts )
               {
                  if( part.IsTranslatable )
                  {
                     fullTranslatedText.Append( await TranslatePartAsync( part.Value, context ) );
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
         catch( BlockedException e )
         {
            throw new Exception( "Request was blocked, and reset used", e );
         }
         finally
         {
            _sem.Release();
         }
      }

      private async Task<string> TranslatePartAsync( string text, ITranslationContext context )
      {
         if( text.Length > MaxAnonymousTextLength )
         {
            throw new Exception( $"DeepL does not accept texts longer than {MaxAnonymousTextLength} characters without an API key." );
         }

         var body = new Dictionary<string, object>
         {
            { "text", new[] { text } },
            { "target_lang", FixTargetLanguage( context.DestinationLanguage ) },
         };
         if( !string.Equals( context.SourceLanguage, "auto", StringComparison.OrdinalIgnoreCase ) )
         {
            body.Add( "source_lang", FixSourceLanguage( context.SourceLanguage ) );
         }

         string str;
         using( var request = new HttpRequestMessage( HttpMethod.Post, OneshotTranslateUrl ) )
         {
            request.Headers.TryAddWithoutValidation( "Authorization", "None" );
            request.Content = new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" );

            using( var response = await _client.SendAsync( request ) )
            {
               response.ThrowIfBlocked();
               response.EnsureSuccessStatusCode();

               str = await response.Content.ReadAsStringAsync();
            }
         }

         var obj = JsonConvert.DeserializeObject<TranslationResponse>( str );
         var translation = obj?.translations?.FirstOrDefault()?.text;

         if( string.IsNullOrWhiteSpace( translation ) || translation.IndexOf( OutdatedClientNotice, StringComparison.OrdinalIgnoreCase ) >= 0 )
         {
            throw new Exception( "DeepL did not return a valid translation." );
         }

         return translation;
      }

      public void Dispose()
      {
         _client?.Dispose();
      }
   }
}
