using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// PluginLoader class used to load the plugin during startup.
   ///
   /// This is not meant to be used by Translators plugins!
   /// </summary>
   public static class PluginLoader
   {
      internal static AutoTranslationPlugin Plugin;
      internal static MonoBehaviour MonoBehaviour;

      private static bool _loaded;
      private static bool _bootstrapped;

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

#if MANAGED
            var obj = new GameObject( "___XUnityAutoTranslator" );
            obj.hideFlags = HideFlags.HideAndDontSave;
            Plugin = obj.AddComponent<AutoTranslationPlugin>();
            MonoBehaviour = Plugin;
            GameObject.DontDestroyOnLoad( obj );
#else
            Plugin = new AutoTranslationPlugin();
            var obj = new GameObject( "___XUnityAutoTranslator" );
            obj.hideFlags = HideFlags.HideAndDontSave;
            MonoBehaviour = obj.AddComponent<AutoTranslatorProxyBehaviour>();
            GameObject.DontDestroyOnLoad( obj );
#endif
         }
         return Plugin;
      }

      /// <summary>
      /// Loads the plugin with default environment.
      /// </summary>
      public static IMonoBehaviour Load()
      {
         return LoadWithConfig( new DefaultPluginEnvironment( true ) );
      }

      /// <summary>
      /// Loads the plugin with default environment.
      /// </summary>
      public static IMonoBehaviour Load( bool allowDefaultInitializeHarmonyDetourBridge )
      {
         return LoadWithConfig( new DefaultPluginEnvironment( allowDefaultInitializeHarmonyDetourBridge ) );
      }

#if MANAGED
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
#endif

#if IL2CPP
      internal class AutoTranslatorProxyBehaviour : MonoBehaviour
      {
         static AutoTranslatorProxyBehaviour()
         {
            Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<AutoTranslatorProxyBehaviour>();
         }

         public AutoTranslatorProxyBehaviour( IntPtr value ) : base( value )
         {

         }

         void Update()
         {
            Plugin.Update();
         }

         void OnGUI()
         {
            Plugin.OnGUI();
         }

         void Start()
         {
            Plugin.Start();
         }
      }
#endif
   }
}
