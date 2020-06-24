using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class UtageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( AdvEngine_JumpScenario_Hook ),
         typeof( UnityEventBase_Invoke_Hook ),
         typeof( UnityEventBase_PrepareInvoke_Hook ),
         typeof( AdvPage_RemakeTextData_Hook ),
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
         return AccessToolsShim.Method( UnityTypes.AdvEngine, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         UtageHelper.FixLabel( ref label );
      }

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
   }
   
   internal static class UnityEventBase_Invoke_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UnityEventBase != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UnityEventBase, "Invoke", new Type[] { typeof( object[] ) } );
      }

      static bool Prefix()
      {
         return Settings.InvokeEvents;
      }

      static Action<object, object[]> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, object[]>>();
      }

      static void MM_Detour( object __instance, object[] args )
      {
         var ok = Prefix();

         if( ok )
         {
            _original( __instance, args );
         }
      }
   }
   
   internal static class UnityEventBase_PrepareInvoke_Hook
   {
      private static MethodInfo Method;
      private static object DefaultResult;

      static bool Prepare( object instance )
      {
         try
         {
            Method = AccessToolsShim.Method( UnityTypes.UnityEventBase, "PrepareInvoke" );
            DefaultResult = Activator.CreateInstance( typeof( List<> ).MakeGenericType( UnityTypes.BaseInvokableCall ) );

            return Method != null;
         }
         catch
         {
            
         }

         return false;
      }

      static MethodBase TargetMethod( object instance )
      {
         return Method;
      }

      static void Postfix( ref object __result )
      {
         if( !Settings.InvokeEvents )
         {
            __result = DefaultResult;
         }
      }

      static Func<object, object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<object, object>>();
      }

      static object MM_Detour( object __instance )
      {
         var result = _original( __instance );

         Postfix( ref result );

         return result;
      }
   }
   
   internal static class AdvPage_RemakeTextData_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AdvPage != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AdvPage, "RemakeTextData" );
      }

      static bool Prefix( object __instance )
      {
         if( Settings.RemakeTextData != null )
         {
            Settings.RemakeTextData( __instance );

            return false;
         }

         return true;
      }

      static Action<object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object>>();
      }

      static void MM_Detour( object __instance )
      {
         var ok = Prefix( __instance );

         if( ok )
         {
            _original( __instance );
         }
      }
   }
}
