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
      public override string Id => "DeepLTranslateLegitimate";

      public override string FriendlyName => "DeepL Translator (Authenticated)";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 25;

      protected override string ConfigurationSectionName => "DeepLLegitimate";

      public override void Initialize( IInitializationContext context )
      {
         base.Initialize( context );

         var apiKey = context.GetOrCreateSetting( ConfigurationSectionName, "ApiKey", "" );
         var isFree = context.GetOrCreateSetting( ConfigurationSectionName, "Free", false );

         if( string.IsNullOrEmpty( apiKey ) ) throw new EndpointInitializationException( $"The endpoint requires an API key which has not been provided." );

         ConfigForExternalProcess = string.Join( "\n", new[] { apiKey, isFree.ToString() } );

         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( "DeepLTranslate.ExtProtocol.ExtDeepLTranslateLegitimate, DeepLTranslate.ExtProtocol" ), Base64FormattingOptions.None );
      }
   }
}
