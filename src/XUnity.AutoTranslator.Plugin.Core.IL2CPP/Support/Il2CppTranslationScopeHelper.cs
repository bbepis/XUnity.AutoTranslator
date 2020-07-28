using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Utilities;

namespace XUnity.AutoTranslator.Plugin.Shims
{
   internal class Il2CppTranslationScopeHelper : ITranslationScopeHelper
   {
      public int GetScope( object ui )
      {
         if( Settings.EnableTranslationScoping )
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
         return TranslationScopes.None;
      }

      public bool SupportsSceneManager()
      {
         return true;
      }

      public int GetActiveSceneId()
      {
         return SceneManager.GetActiveScene().buildIndex;
      }

      public void RegisterSceneLoadCallback( Action<int> sceneLoaded )
      {
         SceneManager.add_sceneLoaded(
            new Action<Scene, LoadSceneMode>( ( scene, mode ) => sceneLoaded( scene.buildIndex ) ) );
      }
   }
}
