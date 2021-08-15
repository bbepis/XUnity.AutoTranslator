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
   public class AsyncOperation : YieldInstruction
   {
      internal IntPtr m_Ptr;

      public bool isDone
      {
         get;
      }

      public float progress
      {
         get;
      }

      public int priority
      {
         get;
         set;
      }

      public bool allowSceneActivation
      {
         get;
         set;
      }

      private extern void InternalDestroy();

      ~AsyncOperation()
      {
         InternalDestroy();
      }
   }
}
