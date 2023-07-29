using System;
using System.Reflection.Emit;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class ClrFeatures
   {
      internal static bool SupportsNet4x { get; } = false;

      internal static bool SupportsReflectionEmit { get; } = false;

      static ClrFeatures()
      {
         try
         {
            SupportsNet4x = ClrTypes.Task != null;
         }
         catch( Exception )
         {

         }

         try
         {
            TestReflectionEmit();

            SupportsReflectionEmit = true;
         }
         catch( Exception )
         {
            SupportsReflectionEmit = false;
         }
      }

      private static void TestReflectionEmit()
      {
         Label t1 = default( Label );
         Label t2 = default( Label );
         var ok = t1 == t2;
      }
   }
}
