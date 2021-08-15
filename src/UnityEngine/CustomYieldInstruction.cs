using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public abstract class CustomYieldInstruction : IEnumerator
   {
      public abstract bool keepWaiting
      {
         get;
      }

      public object Current => null;

      public bool MoveNext()
      {
         return keepWaiting;
      }

      public void Reset()
      {
      }
   }
}
