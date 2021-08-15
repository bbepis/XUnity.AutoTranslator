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
   [Flags]
   public enum MaterialGlobalIlluminationFlags
   {
      None = 0x0,
      RealtimeEmissive = 0x1,
      BakedEmissive = 0x2,
      EmissiveIsBlack = 0x4,
      AnyEmissive = 0x3
   }
}
