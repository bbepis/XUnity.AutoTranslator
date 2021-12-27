using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExIni;
using UnityInjector;
using UnityInjector.Attributes;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.UnityInjector
{
   [PluginName( PluginData.Name ), PluginVersion( PluginData.Version )]
   public class AutoTranslatorPlugin : PluginBase, IPluginEnvironment
   {
      public string TranslationPath => DataPath;

      public string ConfigPath => DataPath;

      public bool AllowDefaultInitializeHarmonyDetourBridge => false;

      void IPluginEnvironment.SaveConfig()
      {
         SaveConfig();
      }

      public void Awake()
      {
         PluginLoader.LoadWithConfig( this );
      }
   }
}
