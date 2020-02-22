using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
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
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( TextAsset ), "bytes" ).GetGetMethod();
      }

      delegate byte[] OriginalMethod( TextAsset self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static byte[] MM_Detour( TextAsset self )
      {
         var ext = self.GetExtensionData<TextAssetExtensionData>();
         if( ext != null )
         {
            return ext.Data;
         }

         var result = _original( self );

         return result;
      }
   }

   internal static class TextAsset_text_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( TextAsset ), "text" ).GetGetMethod();
      }

      delegate string OriginalMethod( TextAsset self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static string MM_Detour( TextAsset self )
      {
         var ext = self.GetExtensionData<TextAssetExtensionData>();
         if( ext != null )
         {
            return ext.Text;
         }

         var result = _original( self );

         return result;
      }
   }
}
