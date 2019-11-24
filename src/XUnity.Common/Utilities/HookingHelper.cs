using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class HookingHelper
   {
      private static readonly MethodInfo PatchMethod12 = ClrTypes.HarmonyInstance?.GetMethod( "Patch", new Type[] { ClrTypes.MethodBase, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod } );
      private static readonly MethodInfo PatchMethod20 = ClrTypes.Harmony?.GetMethod( "Patch", new Type[] { ClrTypes.MethodBase, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod, ClrTypes.HarmonyMethod } );
      private static readonly object Harmony;

      static HookingHelper()
      {
         try
         {
            if( ClrTypes.HarmonyInstance != null )
            {
               Harmony = ClrTypes.HarmonyInstance.GetMethod( "Create", BindingFlags.Static | BindingFlags.Public )
                  .Invoke( null, new object[] { "xunity.common.hookinghelper" } );
            }
            else if( ClrTypes.Harmony != null )
            {
               Harmony = ClrTypes.Harmony.GetConstructor( new Type[] { typeof( string ) } )
                  .Invoke( new object[] { "xunity.common.hookinghelper" } );
            }
            else
            {
               XuaLogger.Common.Error( "An unexpected exception occurred during harmony initialization, likely caused by unknown Harmony version. Harmony hooks will be unavailable!" );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Common.Error( e, "An unexpected exception occurred during harmony initialization. Harmony hooks will be unavailable!" );
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="types"></param>
      /// <param name="forceMonoModHooks"></param>
      public static void PatchAll( IEnumerable<Type> types, bool forceMonoModHooks )
      {
         foreach( var type in types )
         {
            PatchType( type, forceMonoModHooks );
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="type"></param>
      /// <param name="forceMonoModHooks"></param>
      public static void PatchType( Type type, bool forceMonoModHooks )
      {
         MethodBase original = null;
         try
         {
            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var prepare = type.GetMethod( "Prepare", flags );
            if( prepare == null || (bool)prepare.Invoke( null, new object[] { Harmony } ) )
            {
               original = (MethodBase)type.GetMethod( "TargetMethod", flags ).Invoke( null, new object[] { Harmony } );
               if( original != null )
               {
                  var prefix = type.GetMethod( "Prefix", flags );
                  var postfix = type.GetMethod( "Postfix", flags );
                  if( Harmony == null )
                  {
                     XuaLogger.Common.Warn( "Harmony is not loaded or could not be initialized. Falling back to MonoMod hooks." );
                  }

                  if( forceMonoModHooks || Harmony == null || ( prefix == null && postfix == null ) )
                  {
                     if( ClrTypes.Hook == null || ClrTypes.NativeDetour == null )
                     {
                        throw new NotSupportedException( "This runtime does not have MonoMod loaded, so MonoMod hooks cannot be used." );
                     }

                     var mmdetour = type.GetMethod( "Get_MM_Detour", flags )?.Invoke( null, null ) ?? type.GetMethod( "MM_Detour", flags );
                     if( mmdetour != null )
                     {
                        string suffix = "(managed)";
                        object hook;
                        try
                        {
                           hook = ClrTypes.Hook.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodInfo ) } ).Invoke( new object[] { original, mmdetour } );
                        }
                        catch( Exception e1 ) when( e1.FirstInnerExceptionOfType<NullReferenceException>() != null || e1.FirstInnerExceptionOfType<NotSupportedException>()?.Message?.Contains( "Body-less" ) == true )
                        {
                           suffix = "(native)";
                           hook = ClrTypes.NativeDetour.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                        }

                        hook.GetType().GetMethod( "Apply" ).Invoke( hook, null );
                        type.GetMethod( "MM_Init", flags )?.Invoke( null, new object[] { hook } );

                        XuaLogger.Common.Debug( $"Hooked {original.DeclaringType.FullName}.{original.Name} through forced MonoMod hooks. {suffix}" );
                     }
                     else
                     {
                        XuaLogger.Common.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}' because no alternate MonoMod hook has been implemented. Failing hook: '{type.Name}'." );
                     }
                  }
                  else
                  {
                     try
                     {
                        var priority = type.GetCustomAttributes( typeof( HookingHelperPriorityAttribute ), false )
                           .OfType<HookingHelperPriorityAttribute>()
                           .FirstOrDefault()
                           ?.priority;

                        var harmonyPrefix = prefix != null ? CreateHarmonyMethod( prefix, priority ) : null;
                        var harmonyPostfix = postfix != null ? CreateHarmonyMethod( postfix, priority ) : null;

                        if( PatchMethod12 != null )
                        {
                           PatchMethod12.Invoke( Harmony, new object[] { original, harmonyPrefix, harmonyPostfix, null } );
                        }
                        else
                        {
                           PatchMethod20.Invoke( Harmony, new object[] { original, harmonyPrefix, harmonyPostfix, null, null } );
                        }

                        XuaLogger.Common.Debug( $"Hooked {original.DeclaringType.FullName}.{original.Name} through Harmony hooks." );
                     }
                     catch( Exception e ) when( e.FirstInnerExceptionOfType<PlatformNotSupportedException>() != null || e.FirstInnerExceptionOfType<ArgumentException>()?.Message?.Contains( "has no body" ) == true )
                     {
                        if( ClrTypes.Hook != null && ClrTypes.NativeDetour != null )
                        {
                           var mmdetour = type.GetMethod( "Get_MM_Detour", flags )?.Invoke( null, null ) ?? type.GetMethod( "MM_Detour", flags );
                           if( mmdetour != null )
                           {
                              string suffix = "(managed)";
                              object hook;
                              try
                              {
                                 hook = ClrTypes.Hook.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodInfo ) } ).Invoke( new object[] { original, mmdetour } );
                              }
                              catch( Exception e1 ) when( e1.FirstInnerExceptionOfType<NullReferenceException>() != null || e1.FirstInnerExceptionOfType<NotSupportedException>()?.Message?.Contains( "Body-less" ) == true )
                              {
                                 suffix = "(native)";
                                 hook = ClrTypes.NativeDetour.GetConstructor( new Type[] { typeof( MethodBase ), typeof( MethodBase ) } ).Invoke( new object[] { original, mmdetour } );
                              }

                              hook.GetType().GetMethod( "Apply" ).Invoke( hook, null );
                              type.GetMethod( "MM_Init", flags )?.Invoke( null, new object[] { hook } );

                              XuaLogger.Common.Debug( $"Hooked {original.DeclaringType.FullName}.{original.Name} through MonoMod hooks. {suffix}" );
                           }
                           else
                           {
                              XuaLogger.Common.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}'. Harmony is not supported in this runtime and no alternate MonoMod hook has been implemented. Failing hook: '{type.Name}'." );
                           }
                        }
                        else
                        {
                           XuaLogger.Common.Warn( $"Cannot hook '{original.DeclaringType.FullName}.{original.Name}'. MonoMod hooks is not supported in this runtime as MonoMod is not loaded. Failing hook: '{type.Name}'." );
                        }
                     }
                  }
               }
               else
               {
                  XuaLogger.Common.Warn( $"Could not hook '{type.Name}'. Likely due differences between different versions of the engine or text framework." );
               }
            }
         }
         catch( Exception e )
         {
            if( original != null )
            {
               XuaLogger.Common.Warn( e, $"An error occurred while patching property/method '{original.DeclaringType.FullName}.{original.Name}'. Failing hook: '{type.Name}'." );
            }
            else
            {
               XuaLogger.Common.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'." );
            }
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
   }
}
