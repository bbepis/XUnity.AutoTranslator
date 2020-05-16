using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   /// <param name="target"></param>
   /// <param name="args"></param>
   /// <returns></returns>
   public delegate object FastReflectionDelegate( object target, params object[] args );
}
