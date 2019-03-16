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
                  var overrider = type.GetMethod( "Override", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                  var callerProperty = type.GetProperty( "Caller", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

                  if( overrider != null && callerProperty != null )
                  {
                     if( Settings.EnableExperimentalHooks )
                     {
                        long replacementMethodLocation = MemoryHelper.GetMethodStartLocation( overrider );
                        long originalMethodLocation = MemoryHelper.GetMethodStartLocation( original );
                        byte[] originalCode = null;

                        try
                        {
                           originalCode = MemoryHelper.GetInstructionsAtLocationRequiredToWriteJump( originalMethodLocation );
                           var caller = new JumpedMethodCaller( originalMethodLocation, replacementMethodLocation, originalCode );
                           callerProperty.SetValue( null, caller, null );

                           MemoryHelper.WriteJump( true, originalMethodLocation, replacementMethodLocation );
                        }
                        catch
                        {
                           if( originalCode != null )
                           {
                              MemoryHelper.RestoreInstructionsAtLocation( true, originalMethodLocation, originalCode );
                           }

                           throw;
                        }
                     }
                  }
                  else
                  {
                     var priority = type.GetCustomAttributes( typeof( HarmonyPriority ), false )
                        .OfType<HarmonyPriority>()
                        .FirstOrDefault()
                        ?.info.prioritiy;

                     var prefix = type.GetMethod( "Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                     var postfix = type.GetMethod( "Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                     var transpiler = type.GetMethod( "Transpiler", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

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

                     PatchMethod.Invoke( instance, new object[] { original, harmonyPrefix, harmonyPostfix, harmonyTranspiler } );
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
            XuaLogger.Current.Warn( e, "An error occurred while patching a property/method on a class. Failing class: " + type.Name );
         }
      }
   }
}
