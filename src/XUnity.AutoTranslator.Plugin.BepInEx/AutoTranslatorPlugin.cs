using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using ExIni;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.BepInEx
{
   [BepInPlugin( GUID: PluginData.Identifier, Name: PluginData.Name, Version: PluginData.Version )]
   public class AutoTranslatorPlugin : BaseUnityPlugin, IPluginEnvironment
   {
      private IniFile _file;
      private string _configPath;

      public AutoTranslatorPlugin()
      {
         ConfigPath = Paths.ConfigPath;
         TranslationPath = Paths.BepInExRootPath;

         _configPath = Path.Combine( ConfigPath, "AutoTranslatorConfig.ini" );
         //XuaLogger.Current = new BepInLogger();
      }

      public IniFile Preferences
      {
         get
         {
            return ( _file ?? ( _file = ReloadConfig() ) );
         }
      }

      public string ConfigPath { get; }

      public string TranslationPath { get; }

      public bool AllowDefaultInitializeHarmonyDetourBridge => false;

      public IniFile ReloadConfig()
      {
         if( !File.Exists( _configPath ) )
         {
            return ( _file ?? new IniFile() );
         }
         IniFile ini = IniFile.FromFile( _configPath );
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

      void Awake()
      {
         PluginLoader.LoadWithConfig( this );
      }
   }
}
