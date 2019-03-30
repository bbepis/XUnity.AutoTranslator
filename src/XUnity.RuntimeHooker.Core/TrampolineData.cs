using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.RuntimeHooker.Core
{
   public class TrampolineData
   {
      public JumpedMethodInfo JumpedMethodInfo;
      public MethodBase Method;
      public Func<object, object[], object> FastInvoke;
      public object[] Parameters;
      public List<HookCallbackInvoker> Postfixes;
      public List<HookCallbackInvoker> Prefixes;
   }
}
