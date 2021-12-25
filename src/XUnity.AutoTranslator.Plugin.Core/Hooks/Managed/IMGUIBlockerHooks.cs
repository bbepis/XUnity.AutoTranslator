#if MANAGED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.UI;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class IMGUIBlocker
   {
      private static HashSet<MethodInfo> HandledMethods = new HashSet<MethodInfo>();

      public static void BlockIfConfigured( UnityEngine.GUI.WindowFunction function, int windowId )
      {
         if( Settings.BlacklistedIMGUIPlugins.Count == 0 ) return;

         var method = function.Method;
         if( !HandledMethods.Contains( method ) )
         {
            HandledMethods.Add( method );

            try
            {
               if( IsBlackslisted( method ) )
               {
                  XuaLogger.AutoTranslator.Info( "Attempting to hook " + method.DeclaringType.FullName.ToString() + "." + method.Name + " to disable translation in window." );

                  IMGUIWindow_Function_Hook.Register( method );
                  HookingHelper.PatchType( typeof( IMGUIWindow_Function_Hook ), Settings.ForceMonoModHooks );
                  IMGUIWindow_Function_Hook.Clean();
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while attempting to hook " + method.DeclaringType.FullName.ToString() + " to disable translation in window." );
            }
         }
      }

      public static bool IsBlackslisted( MethodInfo method )
      {
         var cls = method.DeclaringType;
         var assembly = cls.Assembly;
         if( assembly == typeof( IMGUIBlocker ).Assembly ) return false;
         
         return AutoTranslationPlugin.Current.IsTemporarilyDisabled() // this would indicate that the plugin has itself specified wants to not be translated
            || IsBlacklistedName( method.Name )
            || IsBlacklistedName( cls.Name )
            || IsBlacklistedName( assembly.GetName().Name );
      }

      public static bool IsBlacklistedName( string name )
      {
         return Settings.BlacklistedIMGUIPlugins.Any( x => name.IndexOf( x, StringComparison.OrdinalIgnoreCase ) > -1 );
      }
   }

   internal static class IMGUIWindow_Function_Hook
   {
      private static MethodInfo _nextMethod;

      public static void Register( MethodInfo method )
      {
         _nextMethod = method;
      }

      public static void Clean()
      {
         _nextMethod = null;
      }

      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return _nextMethod;
      }

      static void Prefix()
      {
         AutoTranslationPlugin.Current.DisableAutoTranslator();
      }

      static void Finalizer()
      {
         AutoTranslationPlugin.Current.EnableAutoTranslator();
      }

      static MethodInfo Get_MM_Detour()
      {
         if( _nextMethod.IsStatic )
         {
            return typeof( IMGUIWindow_Function_Hook ).GetMethod( "MM_Detour_Static", BindingFlags.NonPublic | BindingFlags.Static );
         }
         else
         {
            return typeof( IMGUIWindow_Function_Hook ).GetMethod( "MM_Detour_Instance", BindingFlags.NonPublic | BindingFlags.Static );
         }
      }

      static void MM_Detour_Instance( Action<object, int> orig, object self, int id )
      {
         try
         {
            AutoTranslationPlugin.Current.DisableAutoTranslator();

            orig( self, id );
         }
         finally
         {
            AutoTranslationPlugin.Current.EnableAutoTranslator();
         }
      }

      static void MM_Detour_Static( Action<int> orig, int id )
      {
         try
         {
            AutoTranslationPlugin.Current.DisableAutoTranslator();

            orig( id );
         }
         finally
         {
            AutoTranslationPlugin.Current.EnableAutoTranslator();
         }
      }
   }
}

#endif
