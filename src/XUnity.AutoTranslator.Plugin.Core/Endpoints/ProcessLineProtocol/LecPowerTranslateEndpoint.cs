using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ProcessLineProtocol
{
   internal class LecPowerTranslateEndpoint : ProcessLineProtocolEndpoint
   {
      public override string Id => "LecPowerTranslator15";

      public override string FriendlyName => "LEC Power Translator 15";

      public override void Initialize( InitializationContext context )
      {
         var to = context.Config.Preferences[ "General" ][ "Language" ].Value;
         var from = context.Config.Preferences[ "General" ][ "FromLanguage" ].Value;
         var pathToLec = context.Config.Preferences[ "LecPowerTranslator15" ][ "Path" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( pathToLec ) ) throw new Exception( "The LecPowerTranslator15 requires the path to the installation folder." );
         if( !from.Equals( "ja", StringComparison.OrdinalIgnoreCase ) ) throw new Exception( "Only japanese to english is supported." );
         if( !to.Equals( "en", StringComparison.OrdinalIgnoreCase ) ) throw new Exception( "Only japanese to english is supported." );

         var path1 = context.Config.DataPath;
         var exePath1 = Path.Combine( path1, "XUnity.AutoTranslator.Plugin.Lec.exe" );
         var file1Exists = File.Exists( exePath1 );
         var path2 = new FileInfo( typeof( LecPowerTranslateEndpoint ).Assembly.Location ).Directory.FullName;
         var exePath2 = Path.Combine( path2, "XUnity.AutoTranslator.Plugin.Lec.exe" );
         var file2Exists = File.Exists( exePath2 );
         if( !file1Exists && !file2Exists )
         {
            if( path1 != path2 )
            {
               throw new Exception( $"Could not find any executable at '{exePath1}' or at '{exePath2}'" );
            }
            else
            {
               throw new Exception( $"Could not find any executable at '{exePath1}'" );
            }
         }

         _arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( pathToLec ) );

         if( file1Exists )
         {
            _exePath = exePath1;
         }
         else if( file2Exists )
         {
            _exePath = exePath2;
         }
         else
         {
            throw new Exception( "Unexpected error occurred." );
         }
      }
   }
}
