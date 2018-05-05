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

namespace XUnity.AutoTranslator.Plugin.IPA
{
   public class AutoTranslatorPlugin : IPlugin, IConfiguration
   {
      private IniFile _file;
      private string _configPath;
      private string _dataFolder;

      public AutoTranslatorPlugin()
      {
         _dataFolder = "Plugins";
         _configPath = Path.Combine( _dataFolder, "AutoTranslatorConfig.ini" );
      }

      public IniFile Preferences
      {
         get
         {
            return ( _file ?? ( _file = ReloadConfig() ) ); ;
         }
      }

      public string DataPath
      {
         get
         {
            return _dataFolder;
         }
      }

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
