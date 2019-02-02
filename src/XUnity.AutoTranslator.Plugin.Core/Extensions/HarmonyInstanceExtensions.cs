using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class HarmonyInstanceExtensions
   {
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
            var parentMethodInfos = type.GetHarmonyMethods();
            if( parentMethodInfos != null && parentMethodInfos.Count() > 0 )
            {
               var info = HarmonyMethod.Merge( parentMethodInfos );
               var processor = new PatchProcessor( instance, type, info );
               processor.Patch();
            }
         }
         catch( Exception e )
         {
            Logger.Current.Warn( e, "An error occurred while patching a property/method on a class. Failing class: " + type.Name );
         }
      }
   }
}
