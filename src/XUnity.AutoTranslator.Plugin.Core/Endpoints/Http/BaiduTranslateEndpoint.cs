using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   internal class BaiduTranslateEndpoint : HttpEndpoint
   {
      private static readonly string HttpServicePointTemplateUrl = "http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}";
      private static readonly MD5 HashMD5 = MD5.Create();

      private string _appId;
      private string _appSecret;

      public BaiduTranslateEndpoint()
      {

      }

      public override string Id => "BaiduTranslate";

      public override string FriendlyName => "Baidu Translator";

      public override void Initialize( IConfiguration configuration, ServiceEndpointConfiguration servicePoints )
      {
         _appId = configuration.Preferences[ "Baidu" ][ "BaiduAppId" ].GetOrDefault( "" );
         _appSecret = configuration.Preferences[ "Baidu" ][ "BaiduAppSecret" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _appId ) ) throw new ArgumentException( "The BaiduTranslate endpoint requires an App Id which has not been provided." );
         if( string.IsNullOrEmpty( _appSecret ) ) throw new ArgumentException( "The BaiduTranslate endpoint requires an App Secret which has not been provided." );

         servicePoints.EnableHttps( "api.fanyi.baidu.com" );
      }

      private static string CreateMD5( string input )
      {
         byte[] inputBytes = Encoding.UTF8.GetBytes( input );
         byte[] hashBytes = HashMD5.ComputeHash( inputBytes );

         StringBuilder sb = new StringBuilder();
         for( int i = 0 ; i < hashBytes.Length ; i++ )
         {
            sb.Append( hashBytes[ i ].ToString( "X2" ) );
         }
         return sb.ToString().ToLower();
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = Settings.GetUserAgent( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36" );
         headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            if( result.StartsWith( "{\"error" ) )
            {
               translated = null;
               return false;
            }


            var obj = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in obj.AsObject[ "trans_result" ].AsArray )
            {
               var token = entry.AsObject[ "dst" ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }

            translated = lineBuilder.ToString();

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch( Exception )
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         string salt = DateTime.UtcNow.Millisecond.ToString();
         var md5 = CreateMD5( _appId + untranslatedText + salt + _appSecret );

         return string.Format( HttpServicePointTemplateUrl, WWW.EscapeURL( untranslatedText ), from, to, _appId, salt, md5 );
      }
   }
}
