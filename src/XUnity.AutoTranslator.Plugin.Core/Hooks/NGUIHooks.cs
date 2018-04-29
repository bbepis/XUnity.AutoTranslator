using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI
{
   public delegate void NGUITextChanged( object graphic );

   public static class NGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextPropertyHook )
      };

      public static event NGUITextChanged TextChanged;

      public static void FireTextChanged( object graphic )
      {
         TextChanged?.Invoke( graphic );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.Types.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( Constants.Types.UILabel, "text" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         NGUIHooks.FireTextChanged( __instance );
      }
   }
}
