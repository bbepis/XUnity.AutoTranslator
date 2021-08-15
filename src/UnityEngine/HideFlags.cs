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
   public enum HideFlags
   {
      None = 0x0,
      HideInHierarchy = 0x1,
      HideInInspector = 0x2,
      DontSaveInEditor = 0x4,
      NotEditable = 0x8,
      DontSaveInBuild = 0x10,
      DontUnloadUnusedAsset = 0x20,
      DontSave = 0x34,
      HideAndDontSave = 0x3D
   }
}
