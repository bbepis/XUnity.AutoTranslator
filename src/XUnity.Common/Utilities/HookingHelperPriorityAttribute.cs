using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   public class HookingHelperPriorityAttribute : Attribute
   {
      public int priority;

      public HookingHelperPriorityAttribute( int priority )
      {
         this.priority = priority;
      }
   }
}
