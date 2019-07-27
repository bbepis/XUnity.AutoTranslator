using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Utilities
{
   internal static class TranslationScopeProvider
   {
      public static int GetScope( object ui )
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
               return SceneManagerHelper.GetActiveSceneId();
            }
         }
         return TranslationScopes.None;
      }

      public static int GetScopeFromComponent( Component component )
      {
         // DANGER: May not exist in runtime!
         return component.gameObject.scene.buildIndex;
      }
   }
}
