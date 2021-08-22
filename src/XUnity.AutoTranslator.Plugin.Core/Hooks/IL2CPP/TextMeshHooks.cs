#if IL2CPP

using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class TextMeshHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextMesh_text_Hook ),
         typeof( GameObject_SetActive_Hook )
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextMesh_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMesh_Methods.set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TextMesh_Methods.set_text;
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr __instance, IntPtr value )
      {
         var instance = new TextMeshComponent( __instance );

         Il2CppUtilities.InvokeMethod( UnityTypes.TextMesh_Methods.set_text, __instance, value );

         _Postfix( instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GameObject_SetActive_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GameObject_Methods.SetActive != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.GameObject_Methods.SetActive;
      }

      static void _Postfix( GameObject __instance, bool active )
      {
         if( active )
         {
            var tms = __instance.GetComponentsInternal( UnityTypes.TextMesh.Il2CppType, false, true, false, false, null );
            foreach( var tm in tms )
            {
               var comp = new TextMeshComponent( tm.Pointer );
               AutoTranslationPlugin.Current.Hook_TextChanged( comp, true );
            }
         }
      }

      unsafe static void ML_Detour( IntPtr __instance, bool active )
      {
         // proxy way to call
         var instance = new GameObject( __instance );
         instance.SetActive( active );

         //// non-proxy way to call
         //System.IntPtr* ptr = stackalloc System.IntPtr[ 1 ];
         //ptr[0] = (IntPtr)( &active );
         //System.IntPtr returnedException = IntPtr.Zero;
         //System.IntPtr intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke( UnityTypes.GameObject_Methods.SetActive, __instance, (void**)ptr, ref returnedException );
         //Il2CppException.RaiseExceptionIfNecessary( returnedException );

         _Postfix( instance, active );
      }
   }
}

#endif
