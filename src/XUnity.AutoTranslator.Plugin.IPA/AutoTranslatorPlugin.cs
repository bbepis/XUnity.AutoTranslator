using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExIni;
using IllusionPlugin;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.IPA
{
   public class AutoTranslatorPlugin : IPlugin, IPluginEnvironment
   {
      private IniFile _file;
      private string _configPath;
      private readonly string _dataPath;

      public AutoTranslatorPlugin()
      {
         _dataPath = Path.Combine( Paths.GameRoot, "Plugins" );
         _configPath = Path.Combine( _dataPath, "AutoTranslatorConfig.ini" );
      }

      public IniFile Preferences
      {
         get
         {
            return ( _file ?? ( _file = ReloadConfig() ) ); ;
         }
      }

      public string TranslationPath => _dataPath;

      public string ConfigPath => _dataPath;

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

      public string Name => PluginData.Name;

      public string Version => PluginData.Version;

      public void OnApplicationQuit()
      {
      }

      public void OnApplicationStart()
      {
         PluginLoader.LoadWithConfig( this );
      }

      public void OnFixedUpdate()
      {
      }

      public void OnLevelWasInitialized( int level )
      {
      }

      public void OnLevelWasLoaded( int level )
      {
      }

      public void OnUpdate()
      {
      }
   }
}
