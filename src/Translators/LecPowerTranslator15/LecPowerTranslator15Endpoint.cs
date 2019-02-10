using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace LecPowerTranslator15
{
   internal class LecPowerTranslator15Endpoint : ExtProtocolEndpoint
   {
      public override string Id => "LecPowerTranslator15";

      public override string FriendlyName => "LEC Power Translator 15";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 50;

      public override void Initialize( IInitializationContext context )
      {
         var pathToLec = context.GetOrCreateSetting( "LecPowerTranslator15", "InstallationPath", "" );
         if( string.IsNullOrEmpty( pathToLec ) ) throw new Exception( "The LecPowerTranslator15 requires the path to the installation folder." );

         var exePath = Path.Combine( context.PluginDirectory, @"Translators\Lec.ExtProtocol.exe" );
         var fileExists = File.Exists( exePath );
         if( !fileExists )
         {
            throw new Exception( $"Could not find any executable at '{exePath}'" );
         }

         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( pathToLec ) );

         if( fileExists )
         {
            ExecutablePath = exePath;
         }
         else
         {
            throw new Exception( "Unexpected error occurred." );
         }

         if( context.SourceLanguage != "ja" ) throw new Exception( "Current implementation only supports japanese-to-english." );
         if( context.DestinationLanguage != "en" ) throw new Exception( "Current implementation only supports japanese-to-english." );
      }
   }
}
