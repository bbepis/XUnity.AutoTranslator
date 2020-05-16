using System;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace BaiduTranslate
{
   internal class BaiduTranslateEndpoint : HttpEndpoint
   {
      private static readonly string HttpServicePointTemplateUrl = "http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}";
      private static readonly MD5 HashMD5 = MD5.Create();

      private string _appId;
      private string _appSecret;
      private float _delay;
      private float _lastRequestTimestamp;

      public override string Id => "BaiduTranslate";

      public override string FriendlyName => "Baidu Translator";

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-Hans":
            case "zh-CN":
               return "zh";
            //case "ja":
            //   return "jp";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         _appId = context.GetOrCreateSetting( "Baidu", "BaiduAppId", "" );
         _appSecret = context.GetOrCreateSetting( "Baidu", "BaiduAppSecret", "" );
         _delay = context.GetOrCreateSetting( "Baidu", "DelaySeconds", 1.0f );
         if( string.IsNullOrEmpty( _appId ) ) throw new EndpointInitializationException( "The BaiduTranslate endpoint requires an App Id which has not been provided." );
         if( string.IsNullOrEmpty( _appSecret ) ) throw new EndpointInitializationException( "The BaiduTranslate endpoint requires an App Secret which has not been provided." );

         context.DisableCertificateChecksFor( "api.fanyi.baidu.com" );

         // frankly, I have no idea what languages this does, or does not support...
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         var timeSinceLast = Time.realtimeSinceStartup - _lastRequestTimestamp;
         if( timeSinceLast < 1.0f )
         {
            var delay = 1.0f - timeSinceLast;

            var instruction = Features.GetWaitForSecondsRealtime( delay );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               float start = Time.realtimeSinceStartup;
               var end = start + delay;
               while( Time.realtimeSinceStartup < end )
               {
                  yield return null;
               }
            }
         }

         _lastRequestTimestamp = Time.realtimeSinceStartup;
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         string salt = DateTime.UtcNow.Millisecond.ToString();
         var md5 = CreateMD5( _appId + context.UntranslatedText + salt + _appSecret );

         var request = new XUnityWebRequest(
            string.Format(
               HttpServicePointTemplateUrl,
               WwwHelper.EscapeUrl( context.UntranslatedText ),
               FixLanguage( context.SourceLanguage ),
               FixLanguage( context.DestinationLanguage ),
               _appId,
               salt,
               md5 ) );

         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslatorSettings.UserAgent ) ? UserAgents.Chrome_Win10_Latest : AutoTranslatorSettings.UserAgent;
         request.Headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         if( data.StartsWith( "{\"error" ) )
         {
            return;
         }

         var obj = JSON.Parse( data );
         var lineBuilder = new StringBuilder( data.Length );

         foreach( JSONNode entry in obj.AsObject[ "trans_result" ].AsArray )
         {
            var token = entry.AsObject[ "dst" ].ToString();
            token = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );

            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

            lineBuilder.Append( token );
         }

         var translated = lineBuilder.ToString();

         context.Complete( translated );
      }

      private static string CreateMD5( string input )
      {
         byte[] inputBytes = Encoding.UTF8.GetBytes( input );
         byte[] hashBytes = HashMD5.ComputeHash( inputBytes );

         StringBuilder sb = new StringBuilder();
         for( int i = 0; i < hashBytes.Length; i++ )
         {
            sb.Append( hashBytes[ i ].ToString( "X2" ) );
         }
         return sb.ToString().ToLower();
      }
   }
}
