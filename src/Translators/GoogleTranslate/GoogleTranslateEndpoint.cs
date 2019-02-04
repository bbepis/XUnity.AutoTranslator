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
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace GoogleTranslate
{
   internal class GoogleTranslateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "romaji","af","sq","am","ar","hy","az","eu","be","bn","bs","bg","ca","ceb","zh-CN","zh-TW","co","hr","cs","da","nl","en","eo","et","fi","fr","fy","gl","ka","de","el","gu","ht","ha","haw","he","hi","hmn","hu","is","ig","id","ga","it","ja","jw","kn","kk","km","ko","ku","ky","lo","la","lv","lt","lb","mk","mg","ms","ml","mt","mi","mr","mn","my","ne","no","ny","ps","fa","pl","pt","pa","ro","ru","sm","gd","sr","st","sn","sd","si","sk","sl","so","es","su","sw","sv","tl","tg","ta","te","th","tr","uk","ur","uz","vi","cy","xh","yi","yo","zu"
      };

      private static readonly string HttpsServicePointTranslateTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=webapp&sl={0}&tl={1}&dt=t&tk={2}&q={3}";
      private static readonly string HttpsServicePointRomanizeTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=webapp&sl={0}&tl=en&dt=rm&tk={1}&q={2}";
      private static readonly string HttpsTranslateUserSite = "https://translate.google.com";
      private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36";
      private static readonly System.Random RandomNumbers = new System.Random();

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

      public GoogleTranslateEndpoint()
      {
         _cookieContainer = new CookieContainer();
      }

      public override string Id => KnownEndpointNames.GoogleTranslate;

      public override string FriendlyName => "Google! Translate";

      public override void Initialize( InitializationContext context )
      {
         context.HttpSecurity.EnableSslFor( "translate.google.com", "translate.googleapis.com" );

         if( !SupportedLanguages.Contains( context.DestinationLanguage ) ) throw new Exception( $"The destination language {context.DestinationLanguage} is not supported." );
      }

      public override XUnityWebRequest CreateTranslationRequest( HttpTranslationContext context )
      {
         XUnityWebRequest request;
         if( context.DestinationLanguage == "romaji" )
         {
            request = new XUnityWebRequest(
               string.Format(
                  HttpsServicePointRomanizeTemplateUrl,
                  context.SourceLanguage,
                  Tk( context.UntranslatedText ),
                  WWW.EscapeURL( context.UntranslatedText ) ) );
         }
         else
         {
            request = new XUnityWebRequest(
               string.Format(
                  HttpsServicePointTranslateTemplateUrl,
                  context.SourceLanguage,
                  context.DestinationLanguage,
                  Tk( context.UntranslatedText ),
                  WWW.EscapeURL( context.UntranslatedText ) ) );
         }

         request.Cookies = _cookieContainer;
         AddHeaders( request, true );

         return request;
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
         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslationState.UserAgent ) ? DefaultUserAgent : AutoTranslationState.UserAgent;
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

      public override void InspectTranslationResponse( HttpTranslationContext context, XUnityWebResponse response )
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

      public override IEnumerator OnBeforeTranslate( HttpTranslationContext context )
      {
         if( !_hasSetup || AutoTranslationState.TranslationCount % 100 == 0 )
         {
            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupTKK( context );
            while( enumerator.MoveNext() )
            {
               yield return enumerator.Current;
            }
         }
      }

      public IEnumerator SetupTKK( HttpTranslationContext context )
      {
         XUnityWebResponse response = null;

         _cookieContainer = new CookieContainer();

         try
         {
            var client = new XUnityWebClient();
            var request = CreateWebSiteRequest();
            response = client.Send( request );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Warn( e, "An error occurred while setting up GoogleTranslate TKK. Using fallback TKK values instead." );
            yield break;
         }

         if( Features.SupportsCustomYieldInstruction )
         {
            yield return response;
         }
         else
         {
            while( response.keepWaiting )
            {
               yield return new WaitForSeconds( 0.2f );
            }
         }

         InspectTranslationResponse( context, response );

         // failure
         if( response.Error != null )
         {
            XuaLogger.Current.Warn( response.Error, "An error occurred while setting up GoogleTranslate TKK. Using fallback TKK values instead." );
            yield break;
         }

         // failure
         if( response.Result == null )
         {
            XuaLogger.Current.Warn( null, "An error occurred while setting up GoogleTranslate TKK. Using fallback TKK values instead." );
            yield break;
         }

         try
         {
            var html = response.Result;

            bool found = false;
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
                     found = true;
                     break;
                  }
               }
            }

            if( !found )
            {
               XuaLogger.Current.Warn( "An error occurred while setting up GoogleTranslate TKK. Could not locate TKK value. Using fallback TKK values instead." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Warn( e, "An error occurred while setting up GoogleTranslate TKK. Using fallback TKK values instead." );
         }
      }

      // TKK Approach stolen from Translation Aggregator r190, all credits to Sinflower

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

      public override void ExtractTranslatedText( HttpTranslationContext context )
      {
         var dataIndex = context.DestinationLanguage == "romaji" ? 3 : 0;

         var data = context.ResultData;
         var arr = JSON.Parse( data );
         var lineBuilder = new StringBuilder( data.Length );

         foreach( JSONNode entry in arr.AsArray[ 0 ].AsArray )
         {
            var token = entry.AsArray[ dataIndex ].ToString();
            token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

            lineBuilder.Append( token );
         }

         var translated = lineBuilder.ToString();

         context.Complete( translated );
      }
   }
}
