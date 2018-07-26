using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using XUnity.AutoTranslator.Setup.Properties;

namespace XUnity.AutoTranslator.Setup
{
   class Program
   {
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

         // lets add any missing files
         AddFileIfNotExists( Path.Combine( reiPath, "ExIni.dll" ), Resources.ExIni );
         AddFileIfNotExists( Path.Combine( reiPath, "Mono.Cecil.dll" ), Resources.Mono_Cecil );
         AddFileIfNotExists( Path.Combine( reiPath, "Mono.Cecil.Inject.dll" ), Resources.Mono_Cecil_Inject );
         AddFileIfNotExists( Path.Combine( reiPath, "Mono.Cecil.Mdb.dll" ), Resources.Mono_Cecil_Mdb );
         AddFileIfNotExists( Path.Combine( reiPath, "Mono.Cecil.Pdb.dll" ), Resources.Mono_Cecil_Pdb );
         AddFileIfNotExists( Path.Combine( reiPath, "Mono.Cecil.Rocks.dll" ), Resources.Mono_Cecil_Rocks );
         AddFileIfNotExists( Path.Combine( reiPath, "ReiPatcher.exe" ), Resources.ReiPatcher );
         AddFileIfNotExists( Path.Combine( patchesPath, "XUnity.AutoTranslator.Patcher.dll" ), Resources.XUnity_AutoTranslator_Patcher );


         foreach( var launcher in launchers )
         {
            var managedDir = Path.Combine( gamePath, launcher.Data.Name, "Managed" );
            AddFileIfNotExists( Path.Combine( managedDir, "0Harmony.dll" ), Resources._0Harmony );
            AddFileIfNotExists( Path.Combine( managedDir, "ExIni.dll" ), Resources.ExIni );
            AddFileIfNotExists( Path.Combine( managedDir, "ReiPatcher.exe" ), Resources.ReiPatcher ); // needed because file is modified by attribute in ReiPatcher... QQ
            AddFileIfNotExists( Path.Combine( managedDir, "XUnity.AutoTranslator.Plugin.Core.dll" ), Resources.XUnity_AutoTranslator_Plugin_Core );

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

            var lnkInfo = new FileInfo( Path.GetFileNameWithoutExtension( launcher.Executable.Name ) + ".lnk" );
            if( !lnkInfo.Exists )
            {
               // create shortcuts
               CreateShortcut(
                  Path.GetFileNameWithoutExtension( launcher.Executable.Name ) + ".lnk",
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

      public static void AddFileIfNotExists( string fileName, byte[] bytes )
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
      }

      public static void CreateShortcut( string shortcutName, string shortcutPath, string targetFileLocation )
      {
         string shortcutLocation = Path.Combine( shortcutPath, shortcutName );
         WshShell shell = new WshShell();
         IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut( shortcutLocation );

         shortcut.WorkingDirectory = Path.GetDirectoryName( targetFileLocation );
         shortcut.TargetPath = targetFileLocation;
         shortcut.Arguments = "-c " + Path.GetFileNameWithoutExtension( shortcutName ) + ".ini";
         shortcut.Save();
      }
   }
}
