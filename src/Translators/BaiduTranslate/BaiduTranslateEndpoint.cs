using System;
using System.Collections;
using System.Collections.Generic;
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
using XUnity.Common.Utilities;

namespace BaiduTranslate
{
   internal class BaiduTranslateEndpoint : HttpEndpoint
   {
      private static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
      {
         { "en", "en" },

         { "ja", "jp" },
         { "jp", "jp" },

         { "zh", "zh" },
         { "zh-Hans", "zh" },
         { "zh-CN", "zh" },
         { "zh-Hant", "cht" },
         { "zh-TW", "cht" },

         { "ko", "kor" },
         { "kor", "kor" },

         { "fra", "fra" },
         { "fr", "fra" },

         { "spa", "spa" },
         { "es", "spa" },

         { "ara", "ara" },
         { "ar", "ara" },

         { "bg", "bul" },
         { "bul", "bul" },

         { "et", "est" },
         { "est", "est" },

         { "da", "dan" },
         { "dan", "dan" },

         { "fi", "fin" },
         { "fin", "fin" },

         { "ro", "rom" },
         { "rom", "rom" },

         { "sl", "slo" },
         { "slo", "slo" },

         { "vi", "vie" },
         { "vie", "vie" },

         { "sv", "swe" },
         { "swe", "swe" },

         { "th", "th" },
         { "ru", "ru" },
         { "pt", "pt" },
         { "de", "de" },
         { "it", "it" },
         { "el", "el" },
         { "nl", "nl" },
         { "pl", "pl" },
         { "cs", "cs" },
         { "hu", "hu" },
      };

      private static readonly string HttpServicePointTemplateUrl = "https://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}";
      private static readonly MD5 HashMD5 = MD5.Create();

      private string _appId;
      private string _appSecret;
      private float _delay;
      private float _lastRequestTimestamp;

      public override string Id => "BaiduTranslate";

      public override string FriendlyName => "Baidu Translator";

      private string FixLanguage( string lang )
      {
         if( SupportedLanguages.TryGetValue( lang, out var transformed ) )
         {
            return transformed;
         }
         return lang;
      }

      public override void Initialize( IInitializationContext context )
      {
         _appId = context.GetOrCreateSetting( "Baidu", "BaiduAppId", "" );
         _appSecret = context.GetOrCreateSetting( "Baidu", "BaiduAppSecret", "" );
         _delay = context.GetOrCreateSetting( "Baidu", "DelaySeconds", 1.0f );
         if( string.IsNullOrEmpty( _appId ) ) throw new EndpointInitializationException( "The BaiduTranslate endpoint requires an App Id which has not been provided." );
         if( string.IsNullOrEmpty( _appSecret ) ) throw new EndpointInitializationException( "The BaiduTranslate endpoint requires an App Secret which has not been provided." );

         context.DisableCertificateChecksFor( "api.fanyi.baidu.com" );

         if( !SupportedLanguages.ContainsKey( context.SourceLanguage ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.ContainsKey( context.DestinationLanguage ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         var realtimeSinceStartup = TimeHelper.realtimeSinceStartup;

         var timeSinceLast = realtimeSinceStartup - _lastRequestTimestamp;
         if( timeSinceLast < _delay )
         {
            var delay = _delay - timeSinceLast;

            var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( delay );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               float start = realtimeSinceStartup;
               var end = start + delay;
               while( realtimeSinceStartup < end )
               {
                  yield return null;
               }
            }
         }

         _lastRequestTimestamp = TimeHelper.realtimeSinceStartup;
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         string salt = DateTime.UtcNow.Millisecond.ToString();
         var md5 = CreateMD5( _appId + context.UntranslatedText + salt + _appSecret );

         var request = new XUnityWebRequest(
            string.Format(
               HttpServicePointTemplateUrl,
               Uri.EscapeDataString( context.UntranslatedText ),
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
