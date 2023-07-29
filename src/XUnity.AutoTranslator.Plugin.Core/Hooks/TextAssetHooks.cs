using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class TextAssetHooks
   {
      public static readonly Type[] All = new[]
      {
         typeof( TextAsset_bytes_Hook ),
         typeof( TextAsset_text_Hook ),
      };
   }

   internal static class TextAsset_bytes_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextAsset != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextAsset?.ClrType, "bytes" )?.GetGetMethod();
      }

#if MANAGED
      static void Postfix( TextAsset __instance, ref byte[] __result )
#else
      static void Postfix( TextAsset __instance, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> __result )
#endif
      {
         if( __result == null ) return;

         var ext = __instance.GetExtensionData<TextAssetExtensionData>();
         if( ext != null )
         {
            __result = ext.Data;
         }
      }

#if MANAGED
      delegate byte[] OriginalMethod( TextAsset __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static byte[] MM_Detour( TextAsset __instance )
      {
         var __result = _original( __instance );
         Postfix( __instance, ref __result );
         return __result;
      }
#endif
   }

   internal static class TextAsset_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextAsset != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextAsset?.ClrType, "text" )?.GetGetMethod();
      }

      static void Postfix( TextAsset __instance, ref string __result )
      {
         if( __result == null ) return;

         var ext = __instance.GetExtensionData<TextAssetExtensionData>();
         if( ext != null )
         {
            __result = ext.Text;
         }
      }

#if MANAGED
      delegate string OriginalMethod( TextAsset __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static string MM_Detour( TextAsset __instance )
      {
         var __result = _original( __instance );
         Postfix( __instance, ref __result );
         return __result;
      }
#endif
   }
}
