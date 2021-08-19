using System;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Shims
{
   internal class Il2CppTranslationScopeHelper : ITranslationScopeHelper
   {
      private static readonly Action<UnityAction<Scene, LoadSceneMode>> SceneManager_add_sceneLoaded;

      static Il2CppTranslationScopeHelper()
      {
         try
         {
            SceneManager_add_sceneLoaded =
               (Action<UnityAction<Scene, LoadSceneMode>>)ExpressionHelper.CreateTypedFastInvoke(
                  typeof( SceneManager ).GetMethod(
                     "add_sceneLoaded",
                     BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
                     null,
                     new Type[] { typeof( UnityAction<Scene, LoadSceneMode> ) },
                     null
                  ) );
         }
         catch { }
      }

      public int GetScope( object ui )
      {
         if( Settings.EnableTranslationScoping )
         {
            try
            {
               if( ui is ITextComponent text )
               {
                  return text.GetScope();
               }
               else
               {
                  // TODO: Could be an array of all loaded scenes instead!
                  return GetActiveSceneId();
               }
            }
            catch( System.MissingMemberException e )
            {
               XuaLogger.AutoTranslator.Error( e, "A 'missing member' error occurred while retriving translation scope. Disabling translation scopes." );
               Settings.EnableTranslationScoping = false;
            }
         }
         return TranslationScopes.None;
      }

      public bool SupportsSceneManager()
      {
         return SceneManager_add_sceneLoaded != null;
      }

      public int GetActiveSceneId()
      {
         return SceneManager.GetActiveScene().buildIndex;
      }

      public void RegisterSceneLoadCallback( Action<int> sceneLoaded )
      {
         SceneManager_add_sceneLoaded( new Action<Scene, LoadSceneMode>( ( scene, mode ) => sceneLoaded( scene.buildIndex ) ) );

         //SceneManager.add_sceneLoaded(
         //   new Action<Scene, LoadSceneMode>( ( scene, mode ) => sceneLoaded( scene.buildIndex ) ) );
      }
   }
}
