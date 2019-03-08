using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

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
         try
         {
            var prepare = type.GetMethod( "Prepare", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
            if( prepare == null || (bool)prepare.Invoke( null, new object[] { instance } ) )
            {
               var original = (MethodBase)type.GetMethod( "TargetMethod", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ).Invoke( null, new object[] { instance } );
               if( original != null )
               {
                  var prefix = type.GetMethod( "Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                  var postfix = type.GetMethod( "Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                  var transpiler = type.GetMethod( "Transpiler", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

                  var harmonyPrefix = prefix != null ? new HarmonyMethod( prefix ) : null;
                  var harmonyPostfix = postfix != null ? new HarmonyMethod( postfix ) : null;
                  var harmonyTranspiler = transpiler != null ? new HarmonyMethod( transpiler ) : null;

                  PatchMethod.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler } );
               }
               else
               {
                  XuaLogger.Current.Warn( $"Could not enable hook '{type.Name}'. Likely due differences between different versions of the engine or text framework." );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Warn( e, "An error occurred while patching a property/method on a class. Failing class: " + type.Name );
         }
      }
   }
}
