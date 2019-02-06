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
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace GoogleTranslateLegitimate
{
   internal class GoogleTranslateLegitimateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "af","sq","am","ar","hy","az","eu","be","bn","bs","bg","ca","ceb","zh-CN","zh-TW","co","hr","cs","da","nl","en","eo","et","fi","fr","fy","gl","ka","de","el","gu","ht","ha","haw","he","hi","hmn","hu","is","ig","id","ga","it","ja","jw","kn","kk","km","ko","ku","ky","lo","la","lv","lt","lb","mk","mg","ms","ml","mt","mi","mr","mn","my","ne","no","ny","ps","fa","pl","pt","pa","ro","ru","sm","gd","sr","st","sn","sd","si","sk","sl","so","es","su","sw","sv","tl","tg","ta","te","th","tr","uk","ur","uz","vi","cy","xh","yi","yo","zu"
      };

      private static readonly string HttpsServicePointTemplateUrl = "https://translation.googleapis.com/language/translate/v2?key={0}";

      private string _key;

      public override string Id => "GoogleTranslateLegitimate";

      public override string FriendlyName => "Google! Translate (Authenticated)";

      public override void Initialize( IInitializationContext context )
      {
         _key = context.GetOrCreateSetting( "GoogleLegitimate", "GoogleAPIKey", "" );
         if( string.IsNullOrEmpty( _key ) ) throw new Exception( "The GoogleTranslateLegitimate endpoint requires an API key which has not been provided." );

         // Configure service points / service point manager
         context.EnableSslFor( "translation.googleapis.com" );

         if( !SupportedLanguages.Contains( context.SourceLanguage ) ) throw new Exception( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( context.DestinationLanguage ) ) throw new Exception( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var b = new StringBuilder();
         b.Append( "{" );
         b.Append( "\"q\":\"" ).Append( context.UntranslatedText.EscapeJson() ).Append( "\"," );
         b.Append( "\"target\":\"" ).Append( context.DestinationLanguage ).Append( "\"," );
         b.Append( "\"source\":\"" ).Append( context.SourceLanguage ).Append( "\"," );
         b.Append( "\"format\":\"text\"" );
         b.Append( "}" );
         var data = b.ToString();

         var request = new XUnityWebRequest(
            "POST",
            string.Format( HttpsServicePointTemplateUrl, WWW.EscapeURL( _key ) ),
            data );

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;
         var obj = JSON.Parse( data );
         var lineBuilder = new StringBuilder( data.Length );

         foreach( JSONNode entry in obj.AsObject[ "data" ].AsObject[ "translations" ].AsArray )
         {
            var token = entry.AsObject[ "translatedText" ].ToString();
            token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

            lineBuilder.Append( token );
         }

         var translated = lineBuilder.ToString();

         context.Complete( translated );
      }
   }
}
