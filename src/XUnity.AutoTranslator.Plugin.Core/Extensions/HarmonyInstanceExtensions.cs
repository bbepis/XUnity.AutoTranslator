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
                  if( instance == null )
                  {
                     XuaLogger.Current.Warn( "Harmony is not loaded or could not be initialized. Falling back to MonoMod hooks." );
                  }

                  if( Settings.ForceMonoModHooks || instance == null )
                  {
                     if( ClrTypes.Hook == null || ClrTypes.NativeDetour == null )
                     {
                        throw new NotSupportedException( "This runtime does not have MonoMod loaded, so MonoMod hooks cannot be used." );
                     }

                     var mmdetour = type.GetMethod( "MM_Detour", flags );
                     if( mmdetour != null )
                     {
                        object hook;
                        try
                        {
                           hook = ClrTypes.Hook.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                        }
                        catch( Exception e1 ) when( e1.FirstInnerExceptionOfType<NullReferenceException>() != null || e1.FirstInnerExceptionOfType<NotSupportedException>()?.Message?.Contains( "Body-less" ) == true )
                        {
                           hook = ClrTypes.NativeDetour.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                        }

                        hook.GetType().GetMethod( "Apply" ).Invoke( hook, null );
                        type.GetMethod( "MM_Init", flags )?.Invoke( null, new object[] { hook } );

                        XuaLogger.Current.Info( $"Hooked {original.DeclaringType.FullName}.{original.Name} through forced MonoMod hooks." );
                     }
                     else
                     {
                        XuaLogger.Current.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}' because no alternate MonoMod hook has been implemented. Failing hook: '{type.Name}'." );
                     }
                  }
                  else
                  {
                     try
                     {
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

                        if( PatchMethod12 != null )
                        {
                           PatchMethod12.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler } );
                        }
                        else
                        {
                           PatchMethod20.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler, null } );
                        }

                        XuaLogger.Current.Info( $"Hooked {original.DeclaringType.FullName}.{original.Name} through Harmony hooks." );
                     }
                     catch( Exception e ) when( e.FirstInnerExceptionOfType<PlatformNotSupportedException>() != null || e.FirstInnerExceptionOfType<ArgumentException>()?.Message?.Contains( "has no body" ) == true )
                     {
                        if( ClrTypes.Hook != null && ClrTypes.NativeDetour != null )
                        {
                           var mmdetour = type.GetMethod( "MM_Detour", flags );
                           if( mmdetour != null )
                           {
                              object hook;
                              try
                              {
                                 hook = ClrTypes.Hook.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                              }
                              catch( Exception e1 ) when( e1.FirstInnerExceptionOfType<NullReferenceException>() != null || e1.FirstInnerExceptionOfType<NotSupportedException>()?.Message?.Contains( "Body-less" ) == true )
                              {
                                 hook = ClrTypes.NativeDetour.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                              }

                              hook.GetType().GetMethod( "Apply" ).Invoke( hook, null );
                              type.GetMethod( "MM_Init", flags )?.Invoke( null, new object[] { hook } );

                              XuaLogger.Current.Info( $"Hooked {original.DeclaringType.FullName}.{original.Name} through MonoMod hooks." );
                           }
                           else
                           {
                              XuaLogger.Current.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}'. Harmony is not supported in this runtime and no alternate MonoMod hook has been implemented. Failing hook: '{type.Name}'." );
                           }
                        }
                        else
                        {
                           XuaLogger.Current.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}'. MonoMod hooks is not supported in this runtime as MonoMod is not loaded. Failing hook: '{type.Name}'." );
                        }
                     }
                  }
               }
               else
               {
                  XuaLogger.Current.Warn( $"Could not hook '{type.Name}'. Likely due differences between different versions of the engine or text framework." );
               }
            }
         }
         catch( Exception e )
         {
            if( original != null )
            {
               XuaLogger.Current.Warn( e, $"An error occurred while patching property/method '{original.DeclaringType.FullName}.{original.Name}'. Failing hook: '{type.Name}'." );
            }
            else
            {
               XuaLogger.Current.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'." );
            }
         }
      }
   }
}
