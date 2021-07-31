using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Utilities
{
   internal static class TranslationScopeProvider
   {
      public static int GetScope( object ui )
      {
         if( Settings.EnableTranslationScoping )
         {
            try
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
            catch( System.MissingMemberException e )
            {
               XuaLogger.AutoTranslator.Error( e, "A 'missing member' error occurred while retriving translation scope. Disabling translation scopes." );
               Settings.EnableTranslationScoping = false;
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
