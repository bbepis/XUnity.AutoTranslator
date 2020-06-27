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
               var original = (IntPtr)type.GetMethod( "TargetMethodPointer", flags ).Invoke( null, null );
               if( original != IntPtr.Zero )
               {
                  var mldetour = type.GetMethod( "ML_Detour", flags )?.MethodHandle.GetFunctionPointer();
                  if( mldetour.HasValue && mldetour.Value != IntPtr.Zero )
                  {
                     Imports.Hook( original, mldetour.Value );

                     XuaLogger.Common.Debug( $"Hooked {type.Name} through forced MelonMod Imports hook method." );
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
         catch( Exception e )
         {
            XuaLogger.Common.Warn( e, $"An error occurred while patching property/method. Failing hook: '{type.Name}'." );
         }
      }
   }
}
