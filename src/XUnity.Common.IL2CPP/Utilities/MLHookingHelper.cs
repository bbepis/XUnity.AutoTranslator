using Harmony;
using MelonLoader;
using System;
using System.Collections.Generic;
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
   public static class MLHookingHelper
   {
      private static readonly HarmonyInstance Harmony;

      static MLHookingHelper()
      {
         try
         {
            Harmony = HarmonyInstance.Create( "xunity.common.hookinghelper" );
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
      public static void PatchAll( IEnumerable<Type> types )
      {
         foreach( var type in types )
         {
            PatchType( type );
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="type"></param>
      /// <param name="forceMonoModHooks"></param>
      public static void PatchType( Type type )
      {
         try
         {
            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var prepare = type.GetMethod( "Prepare", flags );
            if( prepare == null || (bool)prepare.Invoke( null, new object[] { null } ) )
            {
               bool hooked = false;
               if( Harmony != null )
               {
                  var targetMethodInfo = (MethodBase)type.GetMethod( "TargetMethod", flags )?.Invoke( null, null );
                  var postfix = (MethodInfo)type.GetMethod( "Postfix", flags );
                  var prefix = (MethodInfo)type.GetMethod( "Prefix", flags );

                  //XuaLogger.Common.Info( type.Name + ": " + targetMethodInfo?.Name );

                  if( targetMethodInfo != null && ( postfix != null || prefix != null ) )
                  {
                     var harmonyPostfix = postfix != null ? new HarmonyMethod( postfix ) : null;
                     var harmonyPrefix = prefix != null ? new HarmonyMethod( prefix ) : null;

                     try
                     {
                        Harmony.Patch( targetMethodInfo, harmonyPrefix, harmonyPostfix, null );
                        hooked = true;

                        XuaLogger.Common.Debug( $"Hooked {type.Name} through Harmony." );
                     }
                     catch( Exception e )
                     {
                        XuaLogger.Common.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'. Retrying with MelonLoader Import hook...." );
                     }
                  }
               }

               if( !hooked )
               {
                  var original = (IntPtr)type.GetMethod( "TargetMethodPointer", flags ).Invoke( null, null );
                  if( original != IntPtr.Zero )
                  {
                     var mldetour = type.GetMethod( "ML_Detour", flags )?.MethodHandle.GetFunctionPointer();
                     if( mldetour.HasValue && mldetour.Value != IntPtr.Zero )
                     {
                        Imports.Hook( original, mldetour.Value );

                        XuaLogger.Common.Debug( $"Hooked {type.Name} through MelonMod Imports hook method." );
                     }
                     else
                     {
                        XuaLogger.Common.Warn( $"Could not hook '{type.Name}' because no detour method was found." );
                     }
                  }
                  else
                  {
                     XuaLogger.Common.Warn( $"Could not hook '{type.Name}'. Likely due differences between different versions of the engine or text framework." );
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Common.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'." );
         }
      }
   }
}
