using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Shim
{
   public abstract class CustomYieldInstructionShim : IEnumerator
   {
      // Methods
      protected CustomYieldInstructionShim()
      {
      }

      public bool MoveNext()
      {
         return keepWaiting;
      }

      public void Reset()
      {
      }

      // Properties
      public object Current
      {
         get
         {
            return null;
         }
      }

      public abstract bool keepWaiting { get; }
   }
}
