using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Shims
{
   internal static class TranslationScopeHelper
   {
      public static int GetScope( object ui )
      {
         if( Settings.EnableTranslationScoping )
         {
            try
            {
               if( ui is ITextComponent text )
               {
                  return text.GetScope();
               }
               else if( ui is Component component )
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
            catch( MissingMemberException e )
            {
               XuaLogger.AutoTranslator.Error( e, "A 'missing member' error occurred while retriving translation scope. Disabling translation scopes." );
               Settings.EnableTranslationScoping = false;
            }
         }
         return TranslationScopes.None;
      }

      private static int GetScopeFromComponent( Component component )
      {
         // DANGER: May not exist in runtime!
         return component.gameObject.scene.buildIndex;
      }

      public static bool SupportsSceneManager()
      {
#if IL2CPP
         return UnityTypes.SceneManager_Methods.add_sceneLoaded != null;
#else
         return UnityFeatures.SupportsSceneManager;
#endif
      }

      public static int GetActiveSceneId()
      {
#if IL2CPP
         return GetActiveSceneIdBySceneManager();
#else
         if( UnityFeatures.SupportsSceneManager )
         {
            return GetActiveSceneIdBySceneManager();
         }
         return GetActiveSceneIdByApplication();
#endif
      }

      private static int GetActiveSceneIdBySceneManager()
      {
         return SceneManager.GetActiveScene().buildIndex;
      }

      public static void RegisterSceneLoadCallback( Action<int> sceneLoaded )
      {
#if IL2CPP
         UnityTypes.SceneManager_Methods.add_sceneLoaded( new Action<Scene, LoadSceneMode>( ( scene, mode ) => sceneLoaded( scene.buildIndex ) ) );
#else
         SceneManagerLoader.EnableSceneLoadScanInternal( sceneLoaded );
#endif
      }

#if MANAGED
      private static int GetActiveSceneIdByApplication()
      {
         return Application.loadedLevel;
      }
#endif
   }

#if MANAGED
   internal static class SceneManagerLoader
   {
      public static void EnableSceneLoadScanInternal( Action<int> sceneLoaded )
      {
         // specified in own method, because of chance that this has changed through Unity lifetime
         SceneManager.sceneLoaded += ( arg1, arg2 ) => sceneLoaded( arg1.buildIndex );
      }
   }
#endif
}
