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
   public enum TextureDimension
   {
      Unknown = -1,
      None,
      Any,
      Tex2D,
      Tex3D,
      Cube,
      Tex2DArray,
      CubeArray
   }
}
