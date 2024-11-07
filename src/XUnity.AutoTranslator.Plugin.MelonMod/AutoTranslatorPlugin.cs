using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExIni;
using MelonLoader;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.MelonMod;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

[assembly: MelonInfo( typeof( AutoTranslatorPlugin ), PluginData.Name, PluginData.Version, PluginData.Author )]
[assembly: MelonGame( null, null )]

namespace XUnity.AutoTranslator.Plugin.MelonMod
{
   public class AutoTranslatorPlugin : MelonLoader.MelonMod, IPluginEnvironment
   {
      private ExIni.IniFile _file;
      private string _configPath;
      private string _dataFolder;
      public override void OnApplicationLateStart()
      {
#if IL2CPP
         var modFi = new FileInfo( Location );
         var gameDir = modFi.Directory.Parent;
         var unhollowedPath = Path.Combine( gameDir.FullName, "MelonLoader", "Il2CppAssemblies" );
         Il2CppProxyAssemblies.Location = unhollowedPath;
#endif
         var gameDirectory = MelonUtils.GameDirectory;
         LoggerInstance.Msg( "Mod Directory : "+ gameDirectory );
         _dataFolder = Path.Combine( gameDirectory, "AutoTranslator" );
         _configPath = Path.Combine( _dataFolder, "Config.ini" );
         TranslationPath = _dataFolder;

         PluginLoader.LoadWithConfig( this );
      }

      public ExIni.IniFile Preferences
      {
         get
         {
            return ( _file ?? ( _file = ReloadConfig() ) );
         }
      }

      public string ConfigPath { get; }

      public string TranslationPath { get; set; }

      public bool AllowDefaultInitializeHarmonyDetourBridge => false;

      public ExIni.IniFile ReloadConfig()
      {
         if( !File.Exists( _configPath ) )
         {
            return ( _file ?? new ExIni.IniFile() );
         }
         ExIni.IniFile ini = ExIni.IniFile.FromFile( _configPath );
         if( _file == null )
         {
            return ( _file = ini );
         }
         _file.Merge( ini );
         return _file;
      }

      public void SaveConfig()
      {
         _file.Save( _configPath );
      }
   }
}
