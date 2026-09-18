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
   public class DeepLTranslate : ExtProtocolEndpoint
   {
      private const float MinimumMinDelaySeconds = 1;
      private const float MinimumMaxDelaySeconds = 3;

      public override string Id => "DeepLTranslate";

      public override string FriendlyName => "DeepL Translator";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 25;

      protected override string ConfigurationSectionName => "DeepL";

      public override void Initialize( IInitializationContext context )
      {
         base.Initialize( context );

         MinDelay = context.GetOrCreateSetting( "DeepL", "MinDelaySeconds", 2.0f );
         if( MinDelay < MinimumMinDelaySeconds )
         {
            XuaLogger.AutoTranslator.Warn( $"[DeepL] Cannot set MinDelaySeconds below {MinimumMinDelaySeconds} second(s). Setting MinDelaySeconds={MinimumMinDelaySeconds}" );
            context.SetSetting( "DeepL", "MinDelaySeconds", MinimumMinDelaySeconds );
         }

         MaxDelay = context.GetOrCreateSetting( "DeepL", "MaxDelaySeconds", 6.0f );
         if( MaxDelay < MinimumMaxDelaySeconds )
         {
            XuaLogger.AutoTranslator.Warn( $"[DeepL] Cannot set MaxDelaySeconds below {MinimumMaxDelaySeconds} second(s). Setting MaxDelaySeconds={MinimumMaxDelaySeconds}" );
            context.SetSetting( "DeepL", "MaxDelaySeconds", MinimumMaxDelaySeconds );
         }

         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( "DeepLTranslate.ExtProtocol.ExtDeepLTranslate, DeepLTranslate.ExtProtocol" ), Base64FormattingOptions.None );
      }
   }
}
