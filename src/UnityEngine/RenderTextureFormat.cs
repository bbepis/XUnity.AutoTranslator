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
   public enum RenderTextureFormat
   {
      ARGB32 = 0,
      Depth = 1,
      ARGBHalf = 2,
      Shadowmap = 3,
      RGB565 = 4,
      ARGB4444 = 5,
      ARGB1555 = 6,
      Default = 7,
      ARGB2101010 = 8,
      DefaultHDR = 9,
      ARGB64 = 10,
      ARGBFloat = 11,
      RGFloat = 12,
      RGHalf = 13,
      RFloat = 14,
      RHalf = 0xF,
      R8 = 0x10,
      ARGBInt = 17,
      RGInt = 18,
      RInt = 19,
      BGRA32 = 20,
      RGB111110Float = 22,
      RG32 = 23,
      RGBAUShort = 24,
      RG16 = 25
   }
}
