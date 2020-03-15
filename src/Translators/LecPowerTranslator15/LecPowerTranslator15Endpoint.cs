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
         var defaultPath = GetDefaultInstallationPath();
         var pathToLec = context.GetOrCreateSetting( "LecPowerTranslator15", "InstallationPath", defaultPath );

         if( string.IsNullOrEmpty( pathToLec ) && !string.IsNullOrEmpty( defaultPath ) )
         {
            context.SetSetting( "LecPowerTranslator15", "InstallationPath", defaultPath );
            pathToLec = defaultPath;
         }

         if( string.IsNullOrEmpty( pathToLec ) ) throw new EndpointInitializationException( "The LecPowerTranslator15 requires the path to the installation folder." );

         var exePath = Path.Combine( context.TranslatorDirectory, Path.Combine( "FullNET", "Lec.ExtProtocol.exe" ) );

         var fileExists = File.Exists( exePath );
         if( !fileExists ) throw new EndpointInitializationException( $"Could not find any executable at '{exePath}'" );

         ExecutablePath = exePath;
         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( pathToLec ) );

         if( context.SourceLanguage != "ja" ) throw new EndpointInitializationException( "Current implementation only supports japanese-to-english." );
         if( context.DestinationLanguage != "en" ) throw new EndpointInitializationException( "Current implementation only supports japanese-to-english." );
      }

      public static string GetDefaultInstallationPath()
      {
         try
         {
            var path = GetInstallationPathFromRegistry();

            if( !string.IsNullOrEmpty( path ) )
            {
               var di = new DirectoryInfo( path );
               path = di.Parent.FullName;
            }

            return path ?? string.Empty;
         }
         catch
         {
            return string.Empty;
         }
      }

      public static string GetInstallationPathFromRegistry()
      {
         try
         {
            if( IntPtr.Size == 8 ) // 64-bit
            {
               return (string)Microsoft.Win32.Registry.GetValue( @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LogoMedia\LEC Power Translator 15\Configuration", "ApplicationPath", null );
            }
            else // 32-bit
            {
               return (string)Microsoft.Win32.Registry.GetValue( @"HKEY_LOCAL_MACHINE\SOFTWARE\LogoMedia\LEC Power Translator 15\Configuration", "ApplicationPath", null );
            }
         }
         catch
         {
            return null;
         }
      }
   }
}
