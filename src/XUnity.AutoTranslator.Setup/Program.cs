using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;

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
         var reiInfo = new DirectoryInfo( reiPath );
         if( !reiInfo.Exists )
         {
            Console.WriteLine( "ReiPatcher directory missing!" );
            Console.WriteLine( "Press any key to exit..." );
            return;
         }

         foreach( var launcher in launchers )
         {
            var setupPath = Path.Combine( gamePath, "AutoTranslatorSetupFiles" );
            var setupInfo = new DirectoryInfo( setupPath );
            if( setupInfo.Exists )
            {
               var setupFiles = setupInfo.GetFiles( "*.dll", SearchOption.TopDirectoryOnly ).Concat( setupInfo.GetFiles( "*.exe", SearchOption.TopDirectoryOnly ) );
               foreach( var setupFile in setupFiles )
               {
                  var copyToPath = Path.Combine( gamePath, launcher.Data.Name, "Managed", setupFile.Name );
                  var copyToFile = new FileInfo( copyToPath );
                  setupFile.CopyTo( copyToPath, true );
                  Console.WriteLine( "Copied " + setupFile.Name + " to " + launcher.Data.FullName );
               }
            }
            else
            {
               Console.WriteLine( "AutoTranslatorSetupFiles directory missing. Skipping copying files to managed directory..." );
            }

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
