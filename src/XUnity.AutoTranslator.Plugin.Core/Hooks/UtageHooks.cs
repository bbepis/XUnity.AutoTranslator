using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class UtageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( AdvEngine_JumpScenario_Hook ),
         typeof( UguiNovelTextGenerator_LengthOfView_Hook ),

#if MANAGED
         typeof( TextArea2D_text_Hook ),
         typeof( TextArea2D_TextData_Hook ),
         typeof( TextData_ctor_Hook ),
#endif
      };
   }
   
   internal static class AdvEngine_JumpScenario_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AdvEngine != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AdvEngine?.ClrType, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         UtageHelper.FixLabel( ref label );
      }

#if MANAGED
      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string value )
      {
         Prefix( ref value );

         _original( __instance, value );
      }
#endif
   }

   internal static class UguiNovelTextGenerator_LengthOfView_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UguiNovelTextGenerator != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UguiNovelTextGenerator?.ClrType, "LengthOfView" ).GetSetMethod( true );
      }

      static void Prefix( ref int value )
      {
         value = -1;
      }

#if MANAGED
      static Action<object, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, int>>();
      }

      static void MM_Detour( object __instance, int value )
      {
         Prefix( ref value );

         _original( __instance, value );
      }
#endif
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextArea2D_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextArea2D != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextArea2D?.ClrType, "text" )?.GetSetMethod();
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextArea2D.ClrType );
#endif

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, string>>();
      }

      static void MM_Detour( Component __instance, string value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextArea2D_TextData_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextArea2D != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextArea2D.ClrType, "TextData" )?.GetSetMethod();
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextArea2D.ClrType );
#endif

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, object>>();
      }

      static void MM_Detour( Component __instance, object value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextData_ctor_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextData != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return UnityTypes.TextData.ClrType.GetConstructor( new[] { typeof( string ) } );
      }

      static void Postfix( object __instance, string text )
      {
         __instance.SetExtensionData( text );
      }

#if MANAGED
      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string text )
      {
         _original( __instance, text );

         Postfix( __instance, text );
      }
#endif
   }
}
