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
   [StructLayout( LayoutKind.Sequential )]
   public sealed class Coroutine : YieldInstruction
   {
      internal IntPtr m_Ptr;

      private Coroutine()
      {
      }
   }
}
