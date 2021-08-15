using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.ExtProtocol.Utilities;
using Http.ExtProtocol;
using SimpleJSON;

namespace GoogleTranslateCompat.ExtProtocol
{
   internal class GoogleTranslateCompatTranslate : ExtHttpEndpoint
   {
      public static readonly string UserAgent_Chrome_Win10_Latest = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

      private static readonly string HttpsServicePointTranslateTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=webapp&sl={0}&tl={1}&dt=t&tk={2}&q={3}";
      private static readonly string HttpsTranslateUserSite = "https://translate.google.com";
      private static readonly Random RandomNumbers = new Random();

      private static readonly string[] Accepts = new string[] { null, "*/*", "application/json" };
      private static readonly string[] AcceptLanguages = new string[] { null, "en-US,en;q=0.9", "en-US", "en" };
      private static readonly string[] Referers = new string[] { null, "https://translate.google.com/" };
      private static readonly string[] AcceptCharsets = new string[] { null, Encoding.UTF8.WebName };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string AcceptLanguage = AcceptLanguages[ RandomNumbers.Next( AcceptLanguages.Length ) ];
      private static readonly string Referer = Referers[ RandomNumbers.Next( Referers.Length ) ];
      private static readonly string AcceptCharset = AcceptCharsets[ RandomNumbers.Next( AcceptCharsets.Length ) ];

      private CookieContainer _cookieContainer;
      private bool _hasSetup = false;
      private long m = 427761;
      private long s = 1179739010;
      private int _translationCount = 0;
      private int _resetAfter = RandomNumbers.Next( 75, 125 );

      public GoogleTranslateCompatTranslate()
      {
         _cookieContainer = new CookieContainer();
      }

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

      public override async Task OnBeforeTranslate( IHttpTranslationContext context )
      {
         if( !_hasSetup || _translationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _translationCount = 1;

            _hasSetup = true;

            // Setup TKK and cookies
            await SetupTKK();
         }
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         _translationCount++;

         var allUntranslatedText = string.Join( "\n", context.UntranslatedTexts );

         XUnityWebRequest request;
         request = new XUnityWebRequest(
            string.Format(
               HttpsServicePointTranslateTemplateUrl,
               FixLanguage( context.SourceLanguage ),
               FixLanguage( context.DestinationLanguage ),
               Tk( allUntranslatedText ),
               Uri.EscapeDataString( allUntranslatedText ) ) );

         request.Cookies = _cookieContainer;
         AddHeaders( request, true );

         context.Complete( request );
      }

      public override void OnInspectResponse( IHttpResponseInspectionContext context )
      {
         InspectResponse( context.Response );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var arr = JSON.Parse( data );
         var lineBuilder = new StringBuilder( data.Length );

         arr = arr.AsArray[ 0 ];
         if( arr.IsNull )
         {
            context.Complete( context.UntranslatedText );
            return;
         }

         foreach( JSONNode entry in arr.AsArray )
         {
            var token = entry.AsArray[ 0 ].ToString();
            token = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );

            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( '\n' );

            lineBuilder.Append( token );
         }

         var allTranslation = lineBuilder.ToString();
         if( context.UntranslatedTexts.Length == 1 )
         {
            context.Complete( allTranslation );
         }
         else
         {
            var translatedLines = allTranslation.Split( '\n' );
            var translatedTexts = new List<string>();

            int current = 0;
            foreach( var untranslatedText in context.UntranslatedTexts )
            {
               var untranslatedLines = untranslatedText.Split( '\n' );
               var untranslatedLinesCount = untranslatedLines.Length;
               var translatedText = string.Empty;

               for( int i = 0 ; i < untranslatedLinesCount ; i++ )
               {
                  if( current >= translatedLines.Length ) context.Fail( "Batch operation received incorrect number of translations." );

                  var translatedLine = translatedLines[ current++ ];
                  translatedText += translatedLine;

                  if( i != untranslatedLinesCount - 1 ) translatedText += '\n';
               }

               translatedTexts.Add( translatedText );
            }

            if( current != translatedLines.Length ) context.Fail( "Batch operation received incorrect number of translations." );

            context.Complete( translatedTexts.ToArray() );
         }
      }

      private XUnityWebRequest CreateWebSiteRequest()
      {
         var request = new XUnityWebRequest( HttpsTranslateUserSite );

         request.Cookies = _cookieContainer;
         AddHeaders( request, false );

         return request;
      }

      private void AddHeaders( XUnityWebRequest request, bool isTranslationRequest )
      {
         request.Headers[ HttpRequestHeader.UserAgent ] = UserAgent_Chrome_Win10_Latest;
         if( AcceptLanguage != null )
         {
            request.Headers[ HttpRequestHeader.AcceptLanguage ] = AcceptLanguage;
         }
         if( Accept != null )
         {
            request.Headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( Referer != null && isTranslationRequest )
         {
            request.Headers[ HttpRequestHeader.Referer ] = Referer;
         }
         if( AcceptCharset != null )
         {
            request.Headers[ HttpRequestHeader.AcceptCharset ] = AcceptCharset;
         }
      }

      private void InspectResponse( XUnityWebResponse response )
      {
         CookieCollection cookies = response.NewCookies;
         foreach( Cookie cookie in cookies )
         {
            // redirect cookie to correct domain
            cookie.Domain = ".googleapis.com";
         }

         // FIXME: Is this needed? Should already be added
         _cookieContainer.Add( cookies );
      }

      public async Task SetupTKK()
      {
         XUnityWebResponse response = null;

         _cookieContainer = new CookieContainer();

         try
         {
            var client = new XUnityWebClient();
            var request = CreateWebSiteRequest();
            response = await client.SendAsync( request );
         }
         catch( Exception )
         {
            return;
         }

         // failure
         if( response.Error != null )
         {
            return;
         }

         // failure
         if( response.Data == null )
         {
            return;
         }

         InspectResponse( response );

         try
         {
            var html = response.Data;

            string[] lookups = new[] { "tkk:'", "TKK='" };
            foreach( var lookup in lookups )
            {
               var index = html.IndexOf( lookup );
               if( index > -1 ) // simple string approach
               {
                  var startIndex = index + lookup.Length;
                  var endIndex = html.IndexOf( "'", startIndex );
                  var result = html.Substring( startIndex, endIndex - startIndex );

                  var parts = result.Split( '.' );
                  if( parts.Length == 2 )
                  {
                     m = long.Parse( parts[ 0 ] );
                     s = long.Parse( parts[ 1 ] );
                     break;
                  }
               }
            }
         }
         catch( Exception )
         {
            // hard to warn...
         }
      }

      private long Vi( long r, string o )
      {
         for( var t = 0 ; t < o.Length ; t += 3 )
         {
            long a = o[ t + 2 ];
            a = a >= 'a' ? a - 87 : a - '0';
            a = '+' == o[ t + 1 ] ? r >> (int)a : r << (int)a;
            r = '+' == o[ t ] ? r + a & 4294967295 : r ^ a;
         }

         return r;
      }

      private string Tk( string r )
      {
         List<long> S = new List<long>();

         for( var v = 0 ; v < r.Length ; v++ )
         {
            long A = r[ v ];
            if( 128 > A )
               S.Add( A );
            else
            {
               if( 2048 > A )
                  S.Add( A >> 6 | 192 );
               else if( 55296 == ( 64512 & A ) && v + 1 < r.Length && 56320 == ( 64512 & r[ v + 1 ] ) )
               {
                  A = 65536 + ( ( 1023 & A ) << 10 ) + ( 1023 & r[ ++v ] );
                  S.Add( A >> 18 | 240 );
                  S.Add( A >> 12 & 63 | 128 );
               }
               else
               {
                  S.Add( A >> 12 | 224 );
                  S.Add( A >> 6 & 63 | 128 );
               }

               S.Add( 63 & A | 128 );
            }
         }

         const string F = "+-a^+6";
         const string D = "+-3^+b+-f";
         long p = m;

         for( var b = 0 ; b < S.Count ; b++ )
         {
            p += S[ b ];
            p = Vi( p, F );
         }

         p = Vi( p, D );
         p ^= s;
         if( 0 > p )
            p = ( 2147483647 & p ) + 2147483648;

         p %= (long)1e6;

         return p.ToString( CultureInfo.InvariantCulture ) + "." + ( p ^ m ).ToString( CultureInfo.InvariantCulture );
      }
   }
}
