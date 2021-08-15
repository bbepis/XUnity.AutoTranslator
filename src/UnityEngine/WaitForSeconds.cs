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
   public sealed class WaitForSeconds : YieldInstruction
   {
      internal float m_Seconds;

      public WaitForSeconds( float seconds )
      {
         m_Seconds = seconds;
      }
   }
}
