using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace LingoCloudTranslate
{
   internal class LingoCloudTranslateEndpoint : HttpEndpoint
   {
      private static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
      {
         { "en", "en" },

         { "ja", "ja" },
         { "jp", "ja" },

         { "zh", "zh" },
         { "zh-Hans", "zh" },
         { "zh-CN", "zh" },
         { "zh-Hant", "zh" },
         { "zh-TW", "zh" },
      };

      private string FixLanguage( string lang )
      {
         if( SupportedLanguages.TryGetValue( lang, out var transformed ) )
         {
            return transformed;
         }
         return lang;
      }

      private static readonly string HttpServicePointTemplateUrl = "https://api.interpreter.caiyunai.com/v1/translator";

      public override string Id => "LingoCloudTranslate";

      public override string FriendlyName => "CaiYun Translator";

      public string _token;

      public override void Initialize( IInitializationContext context )
      {
         _token = context.GetOrCreateSetting( "LingoCloud", "LingoCloudToken", "" );
         if( string.IsNullOrEmpty( _token ) ) throw new EndpointInitializationException( "The LingoCloudTranslate endpoint requires an App Id which has not been provided." );

         context.DisableCertificateChecksFor( "api.interpreter.caiyunai.com" );

         if( !SupportedLanguages.ContainsKey( context.SourceLanguage ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.ContainsKey( context.DestinationLanguage ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );

      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var json = new JSONObject();
         foreach( var source in context.UntranslatedTexts )
         {
            json[ "source" ].Add( source );
         }
         json[ "trans_type" ] = $"auto2{FixLanguage( context.DestinationLanguage )}";
         json[ "request_id" ] = "demo";
         json[ "detect" ] = "true";
         var data = json.ToString();

         var request = new XUnityWebRequest( "POST", HttpServicePointTemplateUrl, data );
         request.Headers[ HttpRequestHeader.ContentType ] = "application/json";
         request.Headers[ "X-Authorization" ] = $"token {this._token}";

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var obj = JSON.Parse( data );
         var token = obj.AsObject[ "target" ].ToString();
         var Translated = JsonHelper.Unescape( token.Substring( 2, token.Length - 4 ) );
         context.Complete( Translated );
      }
   }
}
