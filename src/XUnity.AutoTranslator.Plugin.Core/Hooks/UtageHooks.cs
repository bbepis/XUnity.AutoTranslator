using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
   
   //internal static class AdvCommand_ParseCellLocalizedText_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return ClrTypes.AdvCommand != null && AdvPage_RemakeTextData_Hook.TargetMethod( instance ) == null;
   //   }

   //   static MethodBase TargetMethod( object instance )
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
   
   internal static class AdvEngine_JumpScenario_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.AdvEngine != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.AdvEngine, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         UtageHelper.FixLabel( ref label );
      }
   }
   
   internal static class UnityEventBase_Invoke_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.UnityEventBase != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.UnityEventBase, "Invoke", new Type[] { typeof( object[] ) } );
      }

      static bool Prefix()
      {
         return Settings.InvokeEvents;
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
            Method = AccessToolsShim.Method( ClrTypes.UnityEventBase, "PrepareInvoke" );
            DefaultResult = Activator.CreateInstance( typeof( List<> ).MakeGenericType( ClrTypes.BaseInvokableCall ) );

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
   }
   
   internal static class AdvPage_RemakeTextData_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.AdvPage != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.AdvPage, "RemakeTextData" );
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
