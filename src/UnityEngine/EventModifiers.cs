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
   public enum EventModifiers
   {
      None = 0x0,
      Shift = 0x1,
      Control = 0x2,
      Alt = 0x4,
      Command = 0x8,
      Numeric = 0x10,
      CapsLock = 0x20,
      FunctionKey = 0x40
   }
}
