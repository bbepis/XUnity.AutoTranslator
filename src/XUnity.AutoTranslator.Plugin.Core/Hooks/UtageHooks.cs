using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class UtageHooks
   {
      public static readonly Type[] All = new[] {
         //typeof( AdvCommand_ParseCellLocalizedText_Hook ),
         typeof( AdvEngine_JumpScenario_Hook ),
         typeof( UnityEventBase_Invoke_Hook ),
         typeof( UnityEventBase_PrepareInvoke_Hook ),
         typeof( AdvPage_RemakeTextData_Hook ),
      };
   }

   //[Harmony]
   //internal static class AdvCommand_ParseCellLocalizedText_Hook
   //{
   //   static bool Prepare( HarmonyInstance instance )
   //   {
   //      return ClrTypes.AdvCommand != null;
   //   }

   //   static MethodBase TargetMethod( HarmonyInstance instance )
   //   {
   //      return AccessTools.Method( ClrTypes.AdvCommand, "ParseCellLocalizedText", new Type[] { } );
   //   }

   //   static void Postfix( object __instance, ref string __result )
   //   {
   //      var result = AutoTranslationPlugin.Current.Hook_TextChanged_WithResult( __instance, __result );
   //      if( !string.IsNullOrEmpty( result ) )
   //      {
   //         __result = result;
   //      }
   //   }
   //}

   [Harmony]
   internal static class AdvEngine_JumpScenario_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.AdvEngine != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.AdvEngine, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         UtageHelper.FixLabel( ref label );
      }
   }

   [Harmony]
   internal static class UnityEventBase_Invoke_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UnityEventBase != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.UnityEventBase, "Invoke", new Type[] { typeof( object[] ) } );
      }

      static bool Prefix()
      {
         return Settings.InvokeEvents;
      }
   }

   [Harmony]
   internal static class UnityEventBase_PrepareInvoke_Hook
   {
      private static MethodInfo Method;
      private static object DefaultResult;

      static bool Prepare( HarmonyInstance instance )
      {
         try
         {
            Method = AccessTools.Method( ClrTypes.UnityEventBase, "PrepareInvoke" );
            DefaultResult = Activator.CreateInstance( typeof( List<> ).MakeGenericType( ClrTypes.BaseInvokableCall ) );

            return Method != null;
         }
         catch
         {
            
         }

         return false;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
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
   }

   [Harmony]
   internal static class AdvPage_RemakeTextData_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.AdvPage != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.AdvPage, "RemakeTextData" );
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
   }
}
