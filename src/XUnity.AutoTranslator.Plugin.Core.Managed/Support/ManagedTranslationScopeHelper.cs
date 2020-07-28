using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Shims
{
   internal class ManagedTranslationScopeHelper : ITranslationScopeHelper
   {
      public int GetScope( object ui )
      {
         if( Settings.EnableTranslationScoping )
         {
            if( ui is Component component )
            {
               return GetScopeFromComponent( component );
            }
            else if( ui is GUIContent guic ) // not same as spamming component because we allow nulls
            {
               return TranslationScopes.None;
            }
            else
            {
               // TODO: Could be an array of all loaded scenes instead!
               return GetActiveSceneId();
            }
         }
         return TranslationScopes.None;
      }

      private int GetScopeFromComponent( object component )
      {
         // DANGER: May not exist in runtime!
         return ((Component)component).gameObject.scene.buildIndex;
      }

      public bool SupportsSceneManager()
      {
         return UnityFeatures.SupportsSceneManager;
      }

      public int GetActiveSceneId()
      {
         if( UnityFeatures.SupportsSceneManager )
         {
            return GetActiveSceneIdBySceneManager();
         }
         return GetActiveSceneIdByApplication();
      }

      private static int GetActiveSceneIdBySceneManager()
      {
         return SceneManager.GetActiveScene().buildIndex;
      }

      private static int GetActiveSceneIdByApplication()
      {
         return Application.loadedLevel;
      }

      public void RegisterSceneLoadCallback( Action<int> sceneLoaded )
      {
         SceneManagerLoader.EnableSceneLoadScanInternal( sceneLoaded );
      }
   }

   internal static class SceneManagerLoader
   {
      public static void EnableSceneLoadScanInternal( Action<int> sceneLoaded )
      {
         // specified in own method, because of chance that this has changed through Unity lifetime
         SceneManager.sceneLoaded += ( arg1, arg2 ) => sceneLoaded( arg1.buildIndex );
      }
   }
}
