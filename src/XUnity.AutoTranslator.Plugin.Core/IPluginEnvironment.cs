using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExIni;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Interface representing the environment in which
   /// the plugin runs.
   ///
   /// This interface is not meant to be used by a translator.
   /// </summary>
   public interface IPluginEnvironment
   {
      /// <summary>
      /// Gets or sets the path representing the root of the translations.
      /// </summary>
      string TranslationPath { get; }

      /// <summary>
      /// Gets or sets the path representing the config directory.
      /// </summary>
      string ConfigPath { get; }

      /// <summary>
      /// Gets the preferences file.
      /// </summary>
      IniFile Preferences { get; }

      /// <summary>
      /// Saves the preferences file.
      /// </summary>
      void SaveConfig();

      /// <summary>
      /// Gets a bool indicating whether or not to allow the default value of InitializeHarmonyDetourBridge to be true.
      /// </summary>
      bool AllowDefaultInitializeHarmonyDetourBridge { get; }
   }
}
