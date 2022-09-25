using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XUnity.AutoTranslator.Setup.Properties;

namespace XUnity.AutoTranslator.Setup
{
   class Program
   {
      [STAThread]
      static void Main( string[] args )
      {
         var gamePath = Environment.CurrentDirectory;


         List<GameLauncher> launchers = new List<GameLauncher>();

         // find all .exe files
         var executables = Directory.GetFiles( gamePath, "*.exe", SearchOption.TopDirectoryOnly );
         foreach( var executable in executables )
         {
            var dataFolder = new DirectoryInfo( Path.Combine( gamePath, Path.GetFileNameWithoutExtension( executable ) + "_Data" ) );
            if( dataFolder.Exists )
            {
               launchers.Add( new GameLauncher( new FileInfo( executable ), dataFolder ) );
            }
         }

         var reiPath = Path.Combine( gamePath, "ReiPatcher" );
         var patchesPath = Path.Combine( reiPath, "Patches" );
         var autoTranslatorPath = Path.Combine( gamePath, "AutoTranslator" );

         // lets add any missing files
         AddFile( Path.Combine( reiPath, "ExIni.dll" ), Resources.ExIni );
         AddFile( Path.Combine( reiPath, "Mono.Cecil.dll" ), Resources.Mono_Cecil );
         AddFile( Path.Combine( reiPath, "Mono.Cecil.Inject.dll" ), Resources.Mono_Cecil_Inject );
         AddFile( Path.Combine( reiPath, "Mono.Cecil.Mdb.dll" ), Resources.Mono_Cecil_Mdb );
         AddFile( Path.Combine( reiPath, "Mono.Cecil.Pdb.dll" ), Resources.Mono_Cecil_Pdb );
         AddFile( Path.Combine( reiPath, "Mono.Cecil.Rocks.dll" ), Resources.Mono_Cecil_Rocks );
         AddFile( Path.Combine( reiPath, "ReiPatcher.exe" ), Resources.ReiPatcher );
         AddFile( Path.Combine( patchesPath, "XUnity.AutoTranslator.Patcher.dll" ), Resources.XUnity_AutoTranslator_Patcher, true );

         foreach( var launcher in launchers )
         {
            var managedDir = Path.Combine( gamePath, launcher.Data.Name, "Managed" );
            AddFile( Path.Combine( managedDir, "0Harmony.dll" ), Resources._0Harmony, true );
            AddFile( Path.Combine( managedDir, "ExIni.dll" ), Resources.ExIni, true );
            AddFile( Path.Combine( managedDir, "ReiPatcher.exe" ), Resources.ReiPatcher, true );
            AddFile( Path.Combine( managedDir, "Mono.Cecil.dll" ), Resources.Mono_Cecil_0_10_4_0, true );
            AddFile( Path.Combine( managedDir, "MonoMod.RuntimeDetour.dll" ), Resources.MonoMod_RuntimeDetour, true );
            AddFile( Path.Combine( managedDir, "MonoMod.Utils.dll" ), Resources.MonoMod_Utils, true );
            AddFile( Path.Combine( managedDir, "XUnity.Common.dll" ), Resources.XUnity_Common, true );
            AddFile( Path.Combine( managedDir, "XUnity.ResourceRedirector.dll" ), Resources.XUnity_ResourceRedirector, true );
            AddFile( Path.Combine( managedDir, "XUnity.AutoTranslator.Plugin.Core.dll" ), Resources.XUnity_AutoTranslator_Plugin_Core, true );
            AddFile( Path.Combine( managedDir, "XUnity.AutoTranslator.Plugin.ExtProtocol.dll" ), Resources.XUnity_AutoTranslator_Plugin_ExtProtocol, true );

            var translatorsPath = Path.Combine( managedDir, "Translators" );
            var fullNetPath = Path.Combine( translatorsPath, "FullNET" );
            AddFile( Path.Combine( translatorsPath, "BaiduTranslate.dll" ), Resources.BaiduTranslate, true );
            AddFile( Path.Combine( translatorsPath, "BingTranslate.dll" ), Resources.BingTranslate, true );
            AddFile( Path.Combine( translatorsPath, "BingLegitimateTranslate.dll" ), Resources.BingTranslateLegitimate, true );
            AddFile( Path.Combine( translatorsPath, "CustomTranslate.dll" ), Resources.CustomTranslate, true );
            AddFile( Path.Combine( translatorsPath, "GoogleTranslate.dll" ), Resources.GoogleTranslate, true );
            AddFile( Path.Combine( translatorsPath, "GoogleTranslateCompat.dll" ), Resources.GoogleTranslateCompat, true );
            AddFile( Path.Combine( translatorsPath, "DeepLTranslate.dll" ), Resources.DeepLTranslate, true );
            AddFile( Path.Combine( translatorsPath, "GoogleTranslateLegitimate.dll" ), Resources.GoogleTranslateLegitimate, true );
            AddFile( Path.Combine( translatorsPath, "LecPowerTranslator15.dll" ), Resources.LecPowerTranslator15, true );
            AddFile( Path.Combine( translatorsPath, "ezTransXP.dll" ), Resources.ezTransXP, true );
            AddFile( Path.Combine( translatorsPath, "WatsonTranslate.dll" ), Resources.WatsonTranslate, true );
            AddFile( Path.Combine( translatorsPath, "YandexTranslate.dll" ), Resources.YandexTranslate, true );
            AddFile( Path.Combine( translatorsPath, "PapagoTranslate.dll" ), Resources.PapagoTranslate, true );
            AddFile( Path.Combine( translatorsPath, "LingoCloudTranslate.dll" ), Resources.LingoCloudTranslate, true );
            AddFile( Path.Combine( fullNetPath, "XUnity.AutoTranslator.Plugin.ExtProtocol.dll" ), Resources.XUnity_AutoTranslator_Plugin_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "Lec.ExtProtocol.exe" ), Resources.Lec_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "ezTransXP.ExtProtocol.exe" ), Resources.ezTransXP_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "GoogleTranslateCompat.ExtProtocol.dll" ), Resources.GoogleTranslateCompat_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "DeepLTranslate.ExtProtocol.dll" ), Resources.DeepLTranslate_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "Common.ExtProtocol.dll" ), Resources.Common_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "Http.ExtProtocol.dll" ), Resources.Http_ExtProtocol, true );
            AddFile( Path.Combine( fullNetPath, "Common.ExtProtocol.Executor.exe" ), Resources.Common_ExtProtocol_Executor, true );
            AddFile( Path.Combine( fullNetPath, "Newtonsoft.Json.dll" ), Resources.Newtonsoft_Json, true );

            // create an .ini file for each launcher, if it does not already exist
            var iniInfo = new FileInfo( Path.Combine( reiPath, Path.GetFileNameWithoutExtension( launcher.Executable.Name ) + ".ini" ) );
            if( !iniInfo.Exists )
            {
               using( var file = new FileStream( iniInfo.FullName, FileMode.CreateNew ) )
               using( var writer = new StreamWriter( file ) )
               {
                  writer.WriteLine( ";" + launcher.Executable.Name + " - ReiPatcher Configuration File" );
                  writer.WriteLine( ";" );
                  writer.WriteLine( "[ReiPatcher]" );
                  writer.WriteLine( ";Directory to search for Patches" );
                  writer.WriteLine( "PatchesDir=Patches" );
                  writer.WriteLine( ";Directory to Look for Assemblies to Patch" );
                  writer.WriteLine( @"AssembliesDir=..\" + launcher.Data.Name + @"\Managed" );
                  writer.WriteLine( "" );
                  writer.WriteLine( "[Launch]" );
                  writer.WriteLine( @"Executable=..\" + launcher.Executable.Name );
                  writer.WriteLine( "Arguments=" );
                  writer.WriteLine( @"Directory=..\" );
               }

               Console.WriteLine( "Created " + iniInfo.Name );

            }
            else
            {
               Console.WriteLine( iniInfo.Name + " already exists. skipping..." );
            }

            var shortcutPath = Path.GetFileNameWithoutExtension( launcher.Executable.Name ) + " (Patch and Run).lnk";
            var lnkInfo = new FileInfo( shortcutPath );
            if( !lnkInfo.Exists )
            {
               // create shortcuts
               CreateShortcut(
                  launcher.Executable.FullName,
                  shortcutPath,
                  gamePath,
                  Path.Combine( reiPath, "ReiPatcher.exe" ) );

               Console.WriteLine( "Created shortcut for " + launcher.Executable.Name );
            }
            else
            {
               Console.WriteLine( lnkInfo.Name + " already exists. skipping..." );
            }
         }

         Console.WriteLine( "Setup completed. Press any key to exit." );
         Console.ReadKey();
      }

      public static void AddFile( string fileName, byte[] bytes, bool overwrite = false )
      {
         var fi = new FileInfo( fileName );
         if( !fi.Exists )
         {
            if( !fi.Directory.Exists )
            {
               Directory.CreateDirectory( fi.Directory.FullName );
               Console.WriteLine( "Created directory: " + fi.Directory.FullName );
            }
            System.IO.File.WriteAllBytes( fi.FullName, bytes );
            Console.WriteLine( "Created file: " + fi.FullName );
         }
         else if( overwrite )
         {
            System.IO.File.WriteAllBytes( fi.FullName, bytes );
            Console.WriteLine( "Updated file: " + fi.FullName );
         }
      }

      public static void CreateShortcut( string gameExePath, string shortcutName, string shortcutPath, string targetFileLocation )
      {
         string shortcutLocation = Path.Combine( shortcutPath, shortcutName );

         // Create empty .lnk file
         File.WriteAllBytes( shortcutName, new byte[ 0 ] );

         // Create a ShellLinkObject that references the .lnk file
         Shell32.Shell shl = new Shell32.Shell();
         Shell32.Folder dir = shl.NameSpace( Path.GetDirectoryName( shortcutLocation ) );
         Shell32.FolderItem itm = dir.Items().Item( shortcutName );
         Shell32.ShellLinkObject lnk = (Shell32.ShellLinkObject)itm.GetLink;

         // Set the .lnk file properties
         lnk.Path = targetFileLocation;
         lnk.Arguments = "-c \"" + Path.GetFileNameWithoutExtension( gameExePath ) + ".ini\"";
         lnk.WorkingDirectory = Path.GetDirectoryName( targetFileLocation );
         lnk.SetIconLocation( gameExePath.Replace( '\\', '/' ), 0 );

         lnk.Save( shortcutName );
      }
   }
}
