#if MANAGED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class IMGUIPluginTranslationHooks
   {
      private static HashSet<MethodInfo> HandledMethods = new HashSet<MethodInfo>();
      private static HashSet<MethodInfo> HookedMethods = new HashSet<MethodInfo>();

      public static void HookIfConfigured( GUI.WindowFunction function )
      {
         if( AutoTranslationPlugin.Current.PluginTextCaches.Count == 0 ) return;

         HookIfConfigured( function.Method );
      }

      public static void ResetHandledForAllInAssembly( Assembly assembly )
      {
         HandledMethods.RemoveWhere( x => x.DeclaringType.Assembly.Equals( assembly ) );
      }

      public static void HookIfConfigured( MethodInfo method )
      {
         if( !HandledMethods.Contains( method ) )
         {
            HandledMethods.Add( method );

            if( !HookedMethods.Contains( method ) )
            {
               var methodName = method.DeclaringType.FullName.ToString() + "." + method.Name;
               try
               {
                  var assembly = method.DeclaringType.Assembly;
                  if( !AutoTranslationPlugin.Current.PluginTextCaches.TryGetValue( assembly.GetName().Name, out var cache ) )
                  {
                     return;
                  }

                  XuaLogger.AutoTranslator.Info( "Attempting to hook " + methodName + " to enable plugin specific translations." );

                  var behaviour = method.DeclaringType.Assembly
                     .GetTypes()
                     .FirstOrDefault( x => typeof( MonoBehaviour ).IsAssignableFrom( x ) );

                  if( behaviour == null )
                  {
                     XuaLogger.AutoTranslator.Warn( "Could not find any MonoBehaviours in assembly owning method the method: " + methodName );
                     return;
                  }
                  var behaviourType = behaviour.GetType();

                  var translationCache = AutoTranslationPlugin.Current.TextCache.GetOrCreateCompositeCache( cache );

                  var pluginCallbackType = typeof( PluginCallbacks_Function_Hook<> ).MakeGenericType( behaviourType );
                  pluginCallbackType
                     .GetMethod( "Register", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
                     .Invoke( null, new object[] { method } );
                  pluginCallbackType
                     .GetMethod( "SetTextCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
                     .Invoke( null, new object[] { translationCache } );

                  HookingHelper.PatchType( pluginCallbackType, Settings.ForceMonoModHooks );

                  HookedMethods.Add( method );

                  pluginCallbackType
                     .GetMethod( "Clean", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
                     .Invoke( null, null );
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Error( e, "An error occurred while attempting to hook " + methodName + " to disable translation in window." );
               }
            }
         }
      }
   }

   internal static class PluginCallbacks_Function_Hook<T>
   {
      private static MethodInfo _nextMethod;
      private static IReadOnlyTextTranslationCache _cache;

      public static void SetTextCache( IReadOnlyTextTranslationCache cache )
      {
         _cache = cache;
      }

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

      //static MethodInfo Get_MM_Detour()
      //{
      //   if( _nextMethod.IsStatic )
      //   {
      //      return typeof( PluginCallbacks_Function_Hook<T> ).GetMethod( "MM_Detour_Static", BindingFlags.NonPublic | BindingFlags.Static );
      //   }
      //   else
      //   {
      //      return typeof( PluginCallbacks_Function_Hook<T> ).GetMethod( "MM_Detour_Instance", BindingFlags.NonPublic | BindingFlags.Static );
      //   }
      //}

      static void Prefix()
      {
         CallOrigin.TextCache = _cache;
      }

      static void Finalizer()
      {
         CallOrigin.TextCache = null;
      }

      //static void MM_Detour_Instance( Action<object, int> orig, object self, int id )
      //{
      //   try
      //   {
      //      CallOrigin.TextCache = _cache;

      //      orig( self, id );
      //   }
      //   finally
      //   {
      //      CallOrigin.TextCache = null;
      //   }
      //}

      //static void MM_Detour_Static( Action<int> orig, int id )
      //{
      //   try
      //   {
      //      CallOrigin.TextCache = _cache;

      //      orig( id );
      //   }
      //   finally
      //   {
      //      CallOrigin.TextCache = null;
      //   }
      //}
   }
}

#endif
