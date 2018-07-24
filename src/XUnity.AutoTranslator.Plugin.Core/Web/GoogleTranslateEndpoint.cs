using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class GoogleTranslateEndpoint : KnownEndpoint
   {
      //private static readonly string CertificateIssuer = "CN=Google Internet Authority G3, O=Google Trust Services, C=US";
      private static ServicePoint ServicePoint;
      private static readonly string HttpServicePointTemplateUrl = "http://translate.googleapis.com/translate_a/single?client=t&dt=t&sl={0}&tl={1}&ie=UTF-8&oe=UTF-8&tk={2}&q={3}";
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=t&dt=t&sl={0}&tl={1}&ie=UTF-8&oe=UTF-8&tk={2}&q={3}";
      private static readonly string FallbackHttpServicePointTemplateUrl = "http://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";
      private static readonly string FallbackHttpsServicePointTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";
      private static bool _hasFallenBack = false;

      public GoogleTranslateEndpoint()
         : base( KnownEndpointNames.GoogleTranslate )
      {

      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";
         headers[ "Accept" ] = "*/*";
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";
         headers[ HttpRequestHeader.Accept ] = "*/*";
         headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      }

      // TKK Approach stolen from Translation Aggregator r190.

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
         long m = 425586;
         long s = 2342038670;
         List<long> S = new List<long>();

         for( var v = 0 ; v < r.Length ; v++ )
         {
            long A = r[ v ];
            if( 128 > A )
               S.Add( A );
            else if( 2048 > A )
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

      public override void ConfigureServicePointManager()
      {
         try
         {
            //ServicePointManager.ServerCertificateValidationCallback += ( sender, certificate, chain, sslPolicyErrors ) =>
            //{
            //   return certificate.Issuer == CertificateIssuer;
            //};

            ServicePoint = ServicePointManager.FindServicePoint( new Uri( "http://translate.googleapis.com" ) );
            ServicePoint.ConnectionLimit = GetMaxConcurrency();

         }
         catch
         {
         }
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
            return true;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         if( _hasFallenBack )
         {
            return string.Format( Settings.EnableSSL ? FallbackHttpsServicePointTemplateUrl : FallbackHttpServicePointTemplateUrl, from, to, WWW.EscapeURL( untranslatedText ) );
         }
         else
         {
            return string.Format( Settings.EnableSSL ? HttpsServicePointTemplateUrl : HttpServicePointTemplateUrl, from, to, Tk( untranslatedText ), WWW.EscapeURL( untranslatedText ) );
         }
      }

      public override bool Fallback()
      {
         if( !_hasFallenBack )
         {
            _hasFallenBack = true;
            return true;
         }

         return false;
      }
   }
}
