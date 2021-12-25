#if MANAGED

using System;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class TextMeshHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextMesh_text_Hook ),
         typeof( GameObject_active_Hook ),
         typeof( GameObject_SetActive_Hook )
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextMesh_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMesh != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextMesh.ClrType, "text" )?.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GameObject_active_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( GameObject ), "active" )?.GetSetMethod();
      }

      static void Postfix( GameObject __instance, bool active )
      {
         if( active )
         {
            var tms = __instance.GetComponentsInChildren( UnityTypes.TextMesh.UnityType );
            foreach( var tm in tms )
            {
               AutoTranslationPlugin.Current.Hook_TextChanged( tm, true );
            }
         }
      }

      static Action<GameObject, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<GameObject, bool>>();
      }

      static void MM_Detour( GameObject __instance, bool value )
      {
         _original( __instance, value );

         Postfix( __instance, value );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GameObject_SetActive_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( GameObject ), "SetActive", typeof( bool ) );
      }

      static void Postfix( GameObject __instance, bool active )
      {
         if( active )
         {
            var tms = __instance.GetComponentsInChildren( UnityTypes.TextMesh.UnityType );
            foreach( var tm in tms )
            {
               AutoTranslationPlugin.Current.Hook_TextChanged( tm, true );
            }
         }
      }

      static Action<GameObject, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<GameObject, bool>>();
      }

      static void MM_Detour( GameObject __instance, bool value )
      {
         _original( __instance, value );

         Postfix( __instance, value );
      }
   }
}

#endif
