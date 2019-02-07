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
      /// Gets the path the plugin is located at.
      /// </summary>
      string DataPath { get; }

      /// <summary>
      /// Gets the preferences file.
      /// </summary>
      IniFile Preferences { get; }

      /// <summary>
      /// Saves the preferences file.
      /// </summary>
      void SaveConfig();
   }
}
