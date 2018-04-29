using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class PluginLoader
   {
      private static bool _loaded;
      private static bool _bootstrapped;

      public static void LoadWithConfig( IConfiguration config )
      {
         if( !_loaded )
         {
            _loaded = true;
            Config.Current = config;

            var obj = new GameObject( "Auto Translator" );
            var instance = obj.AddComponent<AutoTranslationPlugin>();
            GameObject.DontDestroyOnLoad( obj );
            instance.Initialize();
         }
      }

      public static void Load()
      {
         LoadWithConfig( new DefaultConfiguration() );
      }

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

      class Bootstrapper : MonoBehaviour
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
}
