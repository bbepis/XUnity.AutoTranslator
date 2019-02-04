using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{
   internal class LecPowerTranslator15Endpoint : ExtProtocolEndpoint
   {
      public override string Id => "LecPowerTranslator15";

      public override string FriendlyName => "LEC Power Translator 15";

      public override void Initialize( InitializationContext context )
      {
         var pathToLec = context.Config.Preferences[ "LecPowerTranslator15" ][ "InstallationPath" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( pathToLec ) ) throw new Exception( "The LecPowerTranslator15 requires the path to the installation folder." );

         var path1 = context.Config.DataPath;
         var exePath1 = Path.Combine( path1, @"Translators\Lec.ExtProtocol.exe" );
         var file1Exists = File.Exists( exePath1 );
         if( !file1Exists )
         {
            throw new Exception( $"Could not find any executable at '{exePath1}'" );
         }

         _arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( pathToLec ) );

         if( file1Exists )
         {
            _exePath = exePath1;
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
