using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.RuntimeHooker;
using XUnity.RuntimeHooker.Core;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class HarmonyInstanceExtensions
   {
      public static readonly MethodInfo PatchMethod12 = ClrTypes.HarmonyInstance?.GetMethod( "Patch", new Type[] { ClrTypes.MethodBase, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod } );
      public static readonly MethodInfo PatchMethod20 = ClrTypes.Harmony?.GetMethod( "Patch", new Type[] { ClrTypes.MethodBase, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod } );

      public static void PatchAll( this object instance, IEnumerable<Type> types )
      {
         foreach( var type in types )
         {
            instance.PatchType( type );
         }
      }

      private static object CreateHarmonyMethod( MethodInfo method, int? priority )
      {
         var harmonyMethod = ClrTypes.HarmonyMethod.GetConstructor( new Type[] { typeof( MethodInfo ) } )
            .Invoke( new object[] { method } );

         if( priority.HasValue )
         {
            var field = ClrTypes.HarmonyMethod.GetField( "priority", BindingFlags.Public | BindingFlags.Instance )
               ?? ClrTypes.HarmonyMethod.GetField( "prioritiy", BindingFlags.Public | BindingFlags.Instance );

            field.SetValue( harmonyMethod, priority.Value );
         }

         return harmonyMethod;
      }

      public static void PatchType( this object instance, Type type )
      {
         MethodBase original = null;
         try
         {
            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var prepare = type.GetMethod( "Prepare", flags );
            if( prepare == null || (bool)prepare.Invoke( null, new object[] { instance } ) )
            {
               original = (MethodBase)type.GetMethod( "TargetMethod", flags ).Invoke( null, new object[] { instance } );
               if( original != null )
               {
                  var requireRuntimeHooker = (bool?)type.GetProperty( "RequireRuntimeHooker", flags )?.GetValue( null, null ) == true;

                  var priority = type.GetCustomAttributes( typeof( HarmonyPriorityShimAttribute ), false )
                     .OfType<HarmonyPriorityShimAttribute>()
                     .FirstOrDefault()
                     ?.priority;

                  var prefix = type.GetMethod( "Prefix", flags );
                  var postfix = type.GetMethod( "Postfix", flags );
                  var transpiler = type.GetMethod( "Transpiler", flags );

                  var harmonyPrefix = prefix != null ? CreateHarmonyMethod( prefix, priority ) : null;
                  var harmonyPostfix = postfix != null ? CreateHarmonyMethod( postfix, priority ) : null;
                  var harmonyTranspiler = transpiler != null ? CreateHarmonyMethod( transpiler, priority ) : null;

                  if( requireRuntimeHooker || Settings.ForceExperimentalHooks )
                  {
                     if( Settings.EnableExperimentalHooks )
                     {
                        XuaLogger.Current.Info( $"Hooking '{original.DeclaringType.FullName}.{original.Name}' through experimental hooks." );

                        var hookPrefix = harmonyPrefix != null ? new HookMethod( prefix, priority ?? -1 ) : null;
                        var hookPostfix = harmonyPostfix != null ? new HookMethod( postfix, priority ?? -1 ) : null;

                        RuntimeMethodPatcher.Patch( original, hookPrefix, hookPostfix );
                     }
                     else
                     {
                        XuaLogger.Current.Info( $"Skipping hook on '{original.DeclaringType.FullName}.{original.Name}' because it requires 'EnableExperimentalHooks' enabled is config file." );
                     }
                  }
                  else
                  {
                     try
                     {
                        if( PatchMethod12 != null )
                        {
                           PatchMethod12.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler } );
                        }
                        else
                        {
                           PatchMethod20.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler, null } );
                        }
                     }
                     catch( Exception e ) when( e.IsCausedBy<PlatformNotSupportedException>() )
                     {
                        if( Settings.EnableExperimentalHooks )
                        {
                           XuaLogger.Current.Info( $"Harmony is not supported in this runtime. Hooking '{original.DeclaringType.FullName}.{original.Name}' through experimental hooks." );

                           var hookPrefix = harmonyPrefix != null ? new HookMethod( prefix, priority ?? -1 ) : null;
                           var hookPostfix = harmonyPostfix != null ? new HookMethod( postfix, priority ?? -1 ) : null;

                           RuntimeMethodPatcher.Patch( original, hookPrefix, hookPostfix );
                        }
                        else
                        {
                           XuaLogger.Current.Error( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}'. Harmony is not supported in this runtime. You can try to 'EnableExperimentalHooks' in the config file to enable support for this game." );
                        }
                     }
                  }
               }
               else
               {
                  XuaLogger.Current.Warn( $"Could not enable hook '{type.Name}'. Likely due differences between different versions of the engine or text framework." );
               }
            }
         }
         catch( Exception e )
         {
            if( original != null )
            {
               XuaLogger.Current.Warn( e, $"An error occurred while patching property/method '{original.DeclaringType.FullName}.{original.Name}'." );
            }
            else
            {
               XuaLogger.Current.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'." );
            }
         }
      }
   }
}
