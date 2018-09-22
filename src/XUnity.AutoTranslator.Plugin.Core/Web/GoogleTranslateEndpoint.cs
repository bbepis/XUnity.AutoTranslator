using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using Jurassic;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class GoogleTranslateEndpoint : KnownHttpEndpoint
   {
      //protected static readonly ConstructorInfo WwwConstructor = Constants.Types.WWW?.GetConstructor( new[] { typeof( string ), typeof( byte[] ), typeof( Dictionary<string, string> ) } );
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=t&dt=t&sl={0}&tl={1}&ie=UTF-8&oe=UTF-8&tk={2}&q={3}";
      private static readonly string HttpsTranslateUserSite = "https://translate.google.com";
      private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
      //private static readonly string UserAgentRepository = "https://techblog.willshouse.com/2012/01/03/most-common-user-agents/";
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
      //private bool _hasSetupCustomUserAgent = false;
      //private string _popularUserAgent;
      private long m = 425635;
      private long s = 1953544246;

      public GoogleTranslateEndpoint()
      {
         _cookieContainer = new CookieContainer();

         // Configure service points / service point manager
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "translate.google.com", "translate.googleapis.com" );
         SetupServicePoints( "https://translate.googleapis.com", "https://translate.google.com" );
      }

      public override bool SupportsLineSplitting => true;

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = Settings.GetUserAgent( DefaultUserAgent );

         if( AcceptLanguage != null )
         {
            headers[ HttpRequestHeader.AcceptLanguage ] = AcceptLanguage;
         }
         if( Accept != null )
         {
            headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( Referer != null )
         {
            headers[ HttpRequestHeader.Referer ] = Referer;
         }
         if( AcceptCharset != null )
         {
            headers[ HttpRequestHeader.AcceptCharset ] = AcceptCharset;
         }
      }

      public override IEnumerator OnBeforeTranslate( int translationCount )
      {
         //if( !_hasSetupCustomUserAgent )
         //{
         //   _hasSetupCustomUserAgent = true;

         //   // setup dynamic user agent
         //   var enumerator = SetupDynamicUserAgent();
         //   while( enumerator.MoveNext() )
         //   {
         //      yield return enumerator.Current;
         //   }
         //}

         if( !_hasSetup || translationCount % 100 == 0 )
         {
            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupTKK();
            while( enumerator.MoveNext() )
            {
               yield return enumerator.Current;
            }

         }
      }

      //public IEnumerator SetupDynamicUserAgent()
      //{
      //   // have to use WWW for this because unity mono is broken

      //   if( WwwConstructor != null )
      //   {
      //      object www;
      //      try
      //      {
      //         var headers = new Dictionary<string, string>();
      //         www = WwwConstructor.Invoke( new object[] { UserAgentRepository, null, headers } );
      //      }
      //      catch( Exception e )
      //      {
      //         Logger.Current.Warn( e, "An error occurred while retrieving dynamic user agent." );
      //         yield break;
      //      }

      //      yield return www;

      //      try
      //      {
      //         var error = (string)AccessTools.Property( Constants.Types.WWW, "error" ).GetValue( www, null );
      //         if( error == null )
      //         {
      //            var text = (string)AccessTools.Property( Constants.Types.WWW, "text" ).GetValue( www, null );
      //            var userAgents = text.GetBetween( "<textarea rows=\"10\" class=\"get-the-list\" onclick=\"this.select()\" readonly=\"readonly\">", "</textarea>" );
      //            if( userAgents.Length > 42 )
      //            {
      //               var reader = new StringReader( userAgents );
      //               var popularUserAgent = reader.ReadLine();
      //               if( popularUserAgent.Length > 30 && popularUserAgent.Length < 300 && popularUserAgent.StartsWith( "Mozilla/" ) )
      //               {
      //                  _popularUserAgent = popularUserAgent;
      //               }
      //               else
      //               {
      //                  Logger.Current.Warn( "An error occurred while retrieving dynamic user agent. Could not find a user agent in returned html." );
      //               }
      //            }
      //            else
      //            {
      //               Logger.Current.Warn( "An error occurred while retrieving dynamic user agent. Could not find a user agent in returned html." );
      //            }
      //         }
      //         else
      //         {
      //            Logger.Current.Warn( "An error occurred while retrieving dynamic user agent. Request failed: " + Environment.NewLine + error );
      //         }
      //      }
      //      catch( Exception e )
      //      {
      //         Logger.Current.Warn( e, "An error occurred while retrieving dynamic user agent." );
      //      }
      //   }
      //}

      public IEnumerator SetupTKK()
      {
         string error = null;
         DownloadResult downloadResult = null;

         _cookieContainer = new CookieContainer();

         var client = GetClient();
         try
         {
            ApplyHeaders( client.Headers );
            client.Headers.Remove( HttpRequestHeader.Referer );
            downloadResult = client.GetDownloadResult( new Uri( HttpsTranslateUserSite ) );
         }
         catch( Exception e )
         {
            error = e.ToString();
         }

         if( downloadResult != null )
         {
            if( Features.SupportsCustomYieldInstruction )
            {
               yield return downloadResult;
            }
            else
            {
               while( !downloadResult.IsCompleted )
               {
                  yield return new WaitForSeconds( 0.2f );
               }
            }

            error = downloadResult.Error;
            if( downloadResult.Succeeded && downloadResult.Result != null )
            {
               try
               {
                  var html = downloadResult.Result;

                  const string lookup = "TKK=eval('";
                  var index = html.IndexOf( lookup );
                  if( index > -1 ) // jurassic approach
                  {
                     var lookupIndex = index + lookup.Length;
                     var openClamIndex = html.IndexOf( '{', lookupIndex );
                     var closeClamIndex = html.IndexOf( '}', openClamIndex );
                     var functionIndex = html.IndexOf( "function", lookupIndex );
                     var script = html.Substring( functionIndex, closeClamIndex - functionIndex + 1 );
                     var decodedScript = script.Replace( "\\x3d", "=" ).Replace( "\\x27", "'" ).Replace( "function", "function FuncName" );

                     // https://github.com/paulbartrum/jurassic/wiki/Safely-executing-user-provided-scripts
                     ScriptEngine engine = new ScriptEngine();
                     engine.Evaluate( decodedScript );
                     var result = engine.CallGlobalFunction<string>( "FuncName" );

                     var parts = result.Split( '.' );
                     if( parts.Length == 2 )
                     {
                        m = long.Parse( parts[ 0 ] );
                        s = long.Parse( parts[ 1 ] );
                     }
                     else
                     {
                        Logger.Current.Warn( "An error occurred while setting up GoogleTranslate Cookie/TKK. Could not locate TKK value. Using fallback TKK values instead." );
                     }
                  }
                  else
                  {
                     const string lookup2 = "TKK='";
                     index = html.IndexOf( lookup2 );
                     if( index > -1 ) // simple string approach
                     {
                        var startIndex = index + lookup2.Length;
                        var endIndex = html.IndexOf( "'", startIndex );
                        var result = html.Substring( startIndex, endIndex - startIndex );

                        var parts = result.Split( '.' );
                        if( parts.Length == 2 )
                        {
                           m = long.Parse( parts[ 0 ] );
                           s = long.Parse( parts[ 1 ] );
                        }
                        else
                        {
                           Logger.Current.Warn( "An error occurred while setting up GoogleTranslate Cookie/TKK. Could not locate TKK value. Using fallback TKK values instead." );
                        }
                     }
                     else
                     {
                        Logger.Current.Warn( "An error occurred while setting up GoogleTranslate Cookie/TKK. Could not locate TKK value. Using fallback TKK values instead." );
                     }
                  }
               }
               catch( Exception e )
               {
                  error = e.ToString();
               }
            }
         }

         if( error != null )
         {
            Logger.Current.Warn( "An error occurred while setting up GoogleTranslate Cookie/TKK. Using fallback TKK values instead." + Environment.NewLine + error );
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

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var arr = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in arr.AsArray[ 0 ].AsArray )
            {
               var token = entry.AsArray[ 0 ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }

            translated = lineBuilder.ToString();

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, from, to, Tk( untranslatedText ), WWW.EscapeURL( untranslatedText ) );
      }

      public override void WriteCookies( HttpWebResponse response )
      {
         CookieCollection cookies = response.Cookies;
         foreach( Cookie cookie in cookies )
         {
            // redirect cookie to correct domain
            cookie.Domain = ".googleapis.com";
         }

         _cookieContainer.Add( cookies );
      }

      public override CookieContainer ReadCookies()
      {
         return _cookieContainer;
      }
   }
}
