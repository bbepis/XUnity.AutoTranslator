using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class HarmonyLoader
   {
      public static void Load()
      {
         // Ensure that we have loaded Harmony
         try
         {
            Harmony12Loader.Load();
         }
         catch
         {
            // will throw an exception if 2.0 is used. But that does not matter
            // because the assembly will be loaded, even if it is 2.0 and that
            // is the only purpose of this method
         }
      }
   }

   internal static class Harmony12Loader
   {
      public static void Load()
      {
         var flags = Harmony.AccessTools.all;
      }
   }
}
