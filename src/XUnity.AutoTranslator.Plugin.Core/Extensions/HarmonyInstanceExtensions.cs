using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using Harmony.ILCopying;
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
      public static readonly MethodInfo PatchMethod = ClrTypes.HarmonyInstance.GetMethod( "Patch", new Type[] { ClrTypes.MethodBase, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod } );

      public static void PatchAll( this HarmonyInstance instance, IEnumerable<Type> types )
      {
         foreach( var type in types )
         {
            instance.PatchType( type );
         }
      }

      public static void PatchType( this HarmonyInstance instance, Type type )
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

                  var priority = type.GetCustomAttributes( typeof( HarmonyPriority ), false )
                     .OfType<HarmonyPriority>()
                     .FirstOrDefault()
                     ?.info.prioritiy;

                  var prefix = type.GetMethod( "Prefix", flags );
                  var postfix = type.GetMethod( "Postfix", flags );
                  var transpiler = type.GetMethod( "Transpiler", flags );

                  var harmonyPrefix = prefix != null ? new HarmonyMethod( prefix ) : null;
                  var harmonyPostfix = postfix != null ? new HarmonyMethod( postfix ) : null;
                  var harmonyTranspiler = transpiler != null ? new HarmonyMethod( transpiler ) : null;

                  if( priority.HasValue )
                  {
                     if( harmonyPrefix != null )
                     {
                        harmonyPrefix.prioritiy = priority.Value;
                     }

                     if( harmonyPostfix != null )
                     {
                        harmonyPostfix.prioritiy = priority.Value;
                     }
                  }

                  if( requireRuntimeHooker || Settings.ForceExperimentalHooks )
                  {
                     if( Settings.EnableExperimentalHooks )
                     {
                        XuaLogger.Current.Info( $"Hooking '{original.DeclaringType.FullName}.{original.Name}' through experimental hooks." );

                        var hookPrefix = harmonyPrefix != null ? new HookMethod( harmonyPrefix.method, harmonyPrefix.prioritiy ) : null;
                        var hookPostfix = harmonyPostfix != null ? new HookMethod( harmonyPostfix.method, harmonyPostfix.prioritiy ) : null;

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
                        PatchMethod.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler } );
                     }
                     catch( Exception e ) when( e.IsCausedBy<PlatformNotSupportedException>() )
                     {
                        if( Settings.EnableExperimentalHooks )
                        {
                           XuaLogger.Current.Info( $"Harmony is not supported in this runtime. Hooking '{original.DeclaringType.FullName}.{original.Name}' through experimental hooks." );

                           var hookPrefix = harmonyPrefix != null ? new HookMethod( harmonyPrefix.method, harmonyPrefix.prioritiy ) : null;
                           var hookPostfix = harmonyPostfix != null ? new HookMethod( harmonyPostfix.method, harmonyPostfix.prioritiy ) : null;

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
               XuaLogger.Current.Warn( e, $"An error occurred while patching a property/method. Failing hook: '{type.Name}'." );
            }
         }
      }
   }
}
