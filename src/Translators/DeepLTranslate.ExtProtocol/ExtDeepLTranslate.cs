using Common.ExtProtocol;
using Common.ExtProtocol.Utilities;
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
            throw new Exception( "Too many requests!" );
         }
      }
   }

   public class ExtDeepLTranslate : IExtTranslateEndpoint
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

      static ExtDeepLTranslate()
      {
         ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Ssl3
            | SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;
      }

      private static readonly Regex NewlineSplitter = new Regex( @"([\s]*[\r\n]+[\s]*)" );
      private static readonly DateTime Epoch = new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc );

      private static readonly string HttpsServicePointTemplateUrl = "https://www2.deepl.com/jsonrpc";
      private static readonly string HttpsTranslateUserSite = "https://www.deepl.com/translator";
      private static readonly string HttpsTranslateStateSetup = "https://www.deepl.com/PHP/backend/clientState.php?request_type=jsonrpc&il=EN";
      private static readonly Random RandomNumbers = new Random();

      private static readonly string[] Accepts = new string[] { "*/*" };
      private static readonly string[] AcceptLanguages = new string[] { null, "en-US,en;q=0.9", "en-US", "en" };
      private static readonly string[] Referers = new string[] { "https://www.deepl.com/translator" };
      private static readonly string[] Origins = new string[] { "https://www.deepl.com" };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string AcceptLanguage = AcceptLanguages[ RandomNumbers.Next( AcceptLanguages.Length ) ];
      private static readonly string Referer = Referers[ RandomNumbers.Next( Referers.Length ) ];
      private static readonly string Origin = Origins[ RandomNumbers.Next( Origins.Length ) ];

      private SemaphoreSlim _sem;

      private HttpClient _client;
      private HttpClientHandler _handler;
      private bool _hasSetup = false;
      private int _translationCount = 0;
      private int _resetAfter = RandomNumbers.Next( 75, 125 );
      private long _id;

      public ExtDeepLTranslate()
      {
         _sem = new SemaphoreSlim( 1, 1 );
      }

      public void Initialize( string config )
      {

      }

      public void Reset()
      {
         _hasSetup = false;
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
         _client.DefaultRequestHeaders.TryAddWithoutValidation( "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36" );
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

      public async Task EnsureSetupState()
      {
         if( !_hasSetup || _translationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _hasSetup = true;
            _id = 10000 * (long)( 10000 * RandomNumbers.NextDouble() );

            CreateClientAndHandler();

            // Setup TKK and cookies
            await SetupState();
         }
      }

      public async Task SetupState()
      {
         _translationCount = 0;

         await RequestWebsite();

         await GetClientState();
      }

      private void AddHeaders( HttpRequestMessage request, HttpContent content, bool isTranslationRequest )
      {
         if( AcceptLanguage != null )
         {
            request.Headers.TryAddWithoutValidation( "Accept-Language", AcceptLanguage );
         }
         if( Accept != null )
         {
            request.Headers.TryAddWithoutValidation( "Accept", Accept );
         }
         if( Referer != null && isTranslationRequest )
         {
            request.Headers.TryAddWithoutValidation( "Referer", Referer );
         }
         if( Origin != null && isTranslationRequest )
         {
            request.Headers.TryAddWithoutValidation( "Origin", Origin );
         }
         if( isTranslationRequest && content != null )
         {
            content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
         }
         request.Headers.TryAddWithoutValidation( "DNT", "1" );
      }

      public async Task Translate( ITranslationContext context )
      {
         try
         {
            await _sem.WaitAsync();

            await EnsureSetupState();

            _translationCount++;
            _id++;

            // construct json content
            long r = (long)( DateTime.UtcNow - Epoch ).TotalMilliseconds;
            long n = 1;

            var builder = new StringBuìlder();
            builder.Append( "{\"jsonrpc\":\"2.0\",\"method\":\"LMT_handle_jobs\",\"params\":{\"jobs\":[" );

            List<UntranslatedTextInfo> untranslatedTextInfos = new List<UntranslatedTextInfo>();
            foreach( var untranslatedTextInfo in context.UntranslatedTextInfos )
            {
               List<TranslationPart> parts = NewlineSplitter
                  .Split( untranslatedTextInfo.UntranslatedText )
                  .Select( x => new TranslationPart { Value = x, IsTranslatable = !NewlineSplitter.IsMatch( x ) } )
                  .ToList();

               var usableParts = parts
                   .Where( x => x.IsTranslatable )
                   .Select( x => x.Value )
                   .ToArray();

               for( int i = 0; i < usableParts.Length; i++ )
               {
                  var usablePart = usableParts[ i ];

                  builder.Append( "{\"kind\":\"default\",\"preferred_num_beams\":1,\"raw_en_sentence\":\"" ); // yes.. "en" no matter what source language is used
                  builder.Append( JsonHelper.Escape( usablePart ) );

                  var addedContext = new HashSet<string>();
                  builder.Append( "\",\"raw_en_context_before\":[" );
                  bool addedAnyBefore = false;
                  foreach( var contextBefore in untranslatedTextInfo.ContextBefore )
                  {
                     if( !addedContext.Contains( contextBefore ) )
                     {
                        builder.Append( "\"" );
                        builder.Append( JsonHelper.Escape( contextBefore ) );
                        builder.Append( "\"" );
                        builder.Append( "," );
                        addedAnyBefore = true;
                     }
                  }
                  for( int j = 0; j < i; j++ )
                  {
                     if( !addedContext.Contains( usableParts[ j ] ) )
                     {
                        builder.Append( "\"" );
                        builder.Append( JsonHelper.Escape( usableParts[ j ] ) );
                        builder.Append( "\"" );
                        builder.Append( "," );
                        addedAnyBefore = true;
                     }
                  }
                  if( addedAnyBefore )
                  {
                     builder.Remove( builder.Length - 1, 1 );
                  }

                  builder.Append( "],\"raw_en_context_after\":[" );
                  bool addedAnyAfter = false;
                  for( int j = i + 1; j < usableParts.Length; j++ )
                  {
                     if( !addedContext.Contains( usableParts[ j ] ) )
                     {
                        builder.Append( "\"" );
                        builder.Append( JsonHelper.Escape( usableParts[ j ] ) );
                        builder.Append( "\"" );
                        builder.Append( "," );
                        addedAnyAfter = true;
                     }
                  }
                  foreach( var contextAfter in untranslatedTextInfo.ContextAfter )
                  {
                     if( !addedContext.Contains( contextAfter ) )
                     {
                        builder.Append( "\"" );
                        builder.Append( JsonHelper.Escape( contextAfter ) );
                        builder.Append( "\"" );
                        builder.Append( "," );
                        addedAnyAfter = true;
                     }
                  }
                  if( addedAnyAfter )
                  {
                     builder.Remove( builder.Length - 1, 1 );
                  }
                  //builder.Append("],\"quality\":\"fast\"},");
                  builder.Append( "]}," );

                  n += usablePart.Count( c => c == 'i' );
               }

               untranslatedTextInfos.Add( new UntranslatedTextInfo { TranslationParts = parts, UntranslatedText = untranslatedTextInfo.UntranslatedText } );
            }
            builder.Remove( builder.Length - 1, 1 ); // remove final ","

            var timestamp = r + ( n - r % n );

            builder.Append( "],\"lang\":{\"user_preferred_langs\":[\"" );
            builder.Append( FixLanguage( context.DestinationLanguage ).ToUpperInvariant() );
            builder.Append( "\",\"" );
            builder.Append( FixLanguage( context.SourceLanguage ).ToUpperInvariant() );
            builder.Append( "\"],\"source_lang_user_selected\":\"" );
            builder.Append( FixLanguage( context.SourceLanguage ).ToUpperInvariant() );
            builder.Append( "\",\"target_lang\":\"" );
            builder.Append( FixLanguage( context.DestinationLanguage ).ToUpperInvariant() );
            builder.Append( "\"},\"priority\":-1,\"timestamp\":" );
            builder.Append( timestamp.ToString( CultureInfo.InvariantCulture ) );
            builder.Append( "},\"id\":" );
            builder.Append( _id );
            builder.Append( "}" );
            var content = builder.ToString();

            var stringContent = new StringContent( content );

            using var request = new HttpRequestMessage( HttpMethod.Post, HttpsServicePointTemplateUrl );
            AddHeaders( request, stringContent, true );


            // create request
            using var response = await _client.PostAsync( HttpsServicePointTemplateUrl, stringContent );
            response.ThrowIfBlocked();
            response.EnsureSuccessStatusCode();

            var str = await response.Content.ReadAsStringAsync();

            ExtractTranslation( str, untranslatedTextInfos, context );
         }
         catch( BlockedException )
         {
            Reset();

            throw;
         }
         finally
         {
            _sem.Release();
         }
      }

      private void ExtractTranslation( string data, List<UntranslatedTextInfo> untranslatedTextInfos, ITranslationContext context )
      {
         var obj = JSON.Parse( data );

         var translations = obj[ "result" ][ "translations" ].AsArray;

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
                  var translation = translations[ transIdx++ ];
                  var beams = translation[ "beams" ].AsArray;
                  if( beams.Count > 0 )
                  {
                     var beam = beams[ 0 ];
                     var sentence = beam[ "postprocessed_sentence" ].ToString();
                     var translatedText = JsonHelper.Unescape( sentence.Substring( 1, sentence.Length - 2 ) );
                     fullTranslatedText.Append( translatedText );
                  }
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

      public async Task RequestWebsite()
      {
         using var request = new HttpRequestMessage( HttpMethod.Get, HttpsTranslateUserSite );
         AddHeaders( request, null, false );

         using var response = await _client.SendAsync( request );
         response.ThrowIfBlocked();
         response.EnsureSuccessStatusCode();

         await response.Content.ReadAsStringAsync();
      }

      public async Task GetClientState()
      {
         _id++;

         // construct json content
         var builder = new StringBuìlder();
         builder.Append( "{\"jsonrpc\":\"2.0\",\"method\":\"getClientState\",\"params\":{\"v\":\"20180814\"},\"id\":" );
         builder.Append( _id );
         builder.Append( "}" );
         var content = builder.ToString();

         var stringContent = new StringContent( content );

         using var request = new HttpRequestMessage( HttpMethod.Get, HttpsTranslateStateSetup );
         AddHeaders( request, stringContent, false );

         using var response = await _client.SendAsync( request );
         response.ThrowIfBlocked();
         response.EnsureSuccessStatusCode();

         await response.Content.ReadAsStringAsync();
      }

      public void Dispose()
      {
         _client?.Dispose();
      }
   }
}
