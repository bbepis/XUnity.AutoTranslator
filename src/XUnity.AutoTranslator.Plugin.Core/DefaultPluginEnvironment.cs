using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExIni;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class DefaultPluginEnvironment : IPluginEnvironment
   {
      private IniFile _file;
      private string _configPath;
      private string _dataFolder;

      public DefaultPluginEnvironment( bool allowDefaultInitializeHarmonyDetourBridge )
      {
         _dataFolder = Path.Combine( Paths.GameRoot, "AutoTranslator" );
         _configPath = Path.Combine( _dataFolder, "Config.ini" );
         AllowDefaultInitializeHarmonyDetourBridge = allowDefaultInitializeHarmonyDetourBridge;
      }

      public IniFile Preferences
      {
         get
         {
            return ( _file ?? ( _file = ReloadConfig() ) ); ;
         }
      }
      
      public string TranslationPath => _dataFolder;

      public string ConfigPath => _dataFolder;

      public bool AllowDefaultInitializeHarmonyDetourBridge { get; }

      public void SaveConfig()
      {
         _file.Save( _configPath );
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
   }
}
