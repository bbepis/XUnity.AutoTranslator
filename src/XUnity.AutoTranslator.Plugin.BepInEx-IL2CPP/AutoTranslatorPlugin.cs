using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using ExIni;
using UnhollowerRuntimeLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Support;

namespace XUnity.AutoTranslator.Plugin.BepInEx_Il2Cpp
{
   [BepInPlugin( GUID: PluginData.Identifier, Name: PluginData.Name, Version: PluginData.Version )]
   public class AutoTranslatorPlugin : BasePlugin, IPluginEnvironment
   {
      public static IMonoBehaviour _monoBehaviour;

      private IniFile _file;
      private string _configPath;

      public AutoTranslatorPlugin()
      {
         ConfigPath = BepInEx.Paths.ConfigPath;
         TranslationPath = BepInEx.Paths.BepInExRootPath;

         _configPath = Path.Combine( ConfigPath, "AutoTranslatorConfig.ini" );

         Il2CppProxyAssemblies.Location = Path.Combine( BepInEx.Paths.BepInExRootPath, "unhollowed" );
      }

      public override void Load()
      {
         _monoBehaviour = PluginLoader.LoadWithConfig( this );

         AutoTranslatorBehaviour.Create();
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
   }

   internal class AutoTranslatorBehaviour : MonoBehaviour
   {
      static AutoTranslatorBehaviour()
      {
         ClassInjector.RegisterTypeInIl2Cpp<AutoTranslatorBehaviour>();
      }

      private static GameObject _obj;
      private static AutoTranslatorBehaviour _comp;
      private static bool _destroying;

      internal static void Create()
      {
         _obj = new GameObject();
         GameObject.DontDestroyOnLoad( _obj );
         _comp = _obj.AddComponent<AutoTranslatorBehaviour>();
      }

      public AutoTranslatorBehaviour( IntPtr value ) : base( value )
      {

      }

      void Update()
      {
         AutoTranslatorPlugin._monoBehaviour.Update();
      }

      void Start()
      {
         AutoTranslatorPlugin._monoBehaviour.Start();
      }

      void OnDestroy()
      {
         if( !_destroying )
         {
            XuaLogger.AutoTranslator.Warn( "Recreating plugin behaviour because it was destroyed..." );
            Create();
         }
      }

      void OnApplicationQuit()
      {
         _destroying = true;
      }
   }
}
