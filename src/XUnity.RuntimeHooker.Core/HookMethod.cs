using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace XUnity.RuntimeHooker.Core
{
   public class HookMethod
   {
      public HookMethod( MethodBase method )
         : this( method, HookPriority.Normal )
      {
      }

      public HookMethod( MethodBase method, int priority )
      {
         Method = method;
         Priority = priority;
      }

      public MethodBase Method { get; set; }

      public int Priority { get; set; }
   }
}
