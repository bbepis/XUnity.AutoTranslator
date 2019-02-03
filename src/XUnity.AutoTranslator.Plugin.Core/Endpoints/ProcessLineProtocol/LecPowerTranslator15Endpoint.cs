using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ProcessLineProtocol
{
   internal class LecPowerTranslator15Endpoint : ProcessLineProtocolEndpoint
   {
      public override string Id => "LecPowerTranslator15";

      public override string FriendlyName => "LEC Power Translator 15";

      public override void Initialize( InitializationContext context )
      {
         var to = context.Config.Preferences[ "General" ][ "Language" ].Value;
         var from = context.Config.Preferences[ "General" ][ "FromLanguage" ].Value;
         var pathToLec = context.Config.Preferences[ "LecPowerTranslator15" ][ "InstallationPath" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( pathToLec ) ) throw new Exception( "The LecPowerTranslator15 requires the path to the installation folder." );
         if( !from.Equals( "ja", StringComparison.OrdinalIgnoreCase ) ) throw new Exception( "Only japanese to english is supported." );
         if( !to.Equals( "en", StringComparison.OrdinalIgnoreCase ) ) throw new Exception( "Only japanese to english is supported." );

         var path1 = context.Config.DataPath;
         var exePath1 = Path.Combine( path1, @"Translators\Lec.exe" );
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
      }
   }
}
