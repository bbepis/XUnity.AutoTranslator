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
      private static bool _bootstrapped;

      /// <summary>
      /// Loads the plugin with the specified environment.
      /// </summary>
      /// <param name="config"></param>
      public static void LoadWithConfig( IPluginEnvironment config )
      {
         if( !_loaded )
         {
            _loaded = true;
            PluginEnvironment.Current = config;

            var obj = new GameObject( "___XUnityAutoTranslator" );
            var instance = obj.AddComponent<AutoTranslationPlugin>();
            GameObject.DontDestroyOnLoad( obj );
         }
      }

      /// <summary>
      /// Loads the plugin with default environment.
      /// </summary>
      public static void Load()
      {
         LoadWithConfig( new DefaultPluginEnvironment() );
      }

      /// <summary>
      /// Loads the plugin in a delayed fashion.
      /// </summary>
      public static void LoadThroughBootstrapper()
      {
         if( !_bootstrapped )
         {
            _bootstrapped = true;
            var bootstrapper = new GameObject( "Bootstrapper" ).AddComponent<Bootstrapper>();
            bootstrapper.Destroyed += Bootstrapper_Destroyed;
         }
      }

      private static void Bootstrapper_Destroyed()
      {
         Load();
      }
   }

   internal class Bootstrapper : MonoBehaviour
   {
      public event Action Destroyed = delegate { };

      void Start()
      {
         Destroy( gameObject );
      }
      void OnDestroy()
      {
         Destroyed?.Invoke();
      }
   }
}
