using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// PluginLoader class used to load the plugin during startup.
   ///
   /// This is not meant to be used by Translators plugins!
   /// </summary>
   public static class PluginLoader
   {
      private static bool _loaded;
      private static AutoTranslationPlugin _plugin;

      /// <summary>
      /// Loads the plugin with the specified environment.
      /// </summary>
      /// <param name="config"></param>
      public static IMonoBehaviour LoadWithConfig( IPluginEnvironment config )
      {
         if( !_loaded )
         {
            _loaded = true;
            PluginEnvironment.Current = config;

            _plugin = new AutoTranslationPlugin();
         }

         return _plugin;
      }

      /// <summary>
      /// Loads the plugin with default environment.
      /// </summary>
      public static IMonoBehaviour Load()
      {
         return LoadWithConfig( new DefaultPluginEnvironment() );
      }
   }
}
