using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public class HookingHelperPriorityAttribute : Attribute
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public int priority;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="priority"></param>
      public HookingHelperPriorityAttribute( int priority )
      {
         this.priority = priority;
      }
   }
}
