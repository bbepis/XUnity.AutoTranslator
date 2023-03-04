using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol;
using XUnity.Common.Logging;

namespace DeepLTranslate
{
   public class DeepLTranslateLegitimate : ExtProtocolEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "bg", "cs", "da", "de", "el", "en", "es", "et", "fi", "fr", "hu", "it", "ja", "lt", "lv", "nl", "pl", "pt", "ro", "ru", "sk", "sl", "sv", "zh", "ko"
      };

      public override string Id => "DeepLTranslateLegitimate";

      public override string FriendlyName => "DeepL Translator (Authenticated)";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 25;

      protected override string ConfigurationSectionName => "DeepLLegitimate";

      private string FixLanguage( string lang )
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

      public override void Initialize( IInitializationContext context )
      {
         base.Initialize( context );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );

         var apiKey = context.GetOrCreateSetting( ConfigurationSectionName, "ApiKey", "" );
         var isFree = context.GetOrCreateSetting( ConfigurationSectionName, "Free", false );

         if( string.IsNullOrEmpty( apiKey ) ) throw new EndpointInitializationException( $"The endpoint requires an API key which has not been provided." );

         ConfigForExternalProcess = string.Join( "\n", new[] { apiKey, isFree.ToString() } );

         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( "DeepLTranslate.ExtProtocol.ExtDeepLTranslateLegitimate, DeepLTranslate.ExtProtocol" ), Base64FormattingOptions.None );
      }
   }
}
