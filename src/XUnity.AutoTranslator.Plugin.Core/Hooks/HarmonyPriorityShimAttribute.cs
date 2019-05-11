using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal class HarmonyPriorityShimAttribute : Attribute
   {
      public int priority;

      public HarmonyPriorityShimAttribute( int priority )
      {
         this.priority = priority;
      }
   }
}
