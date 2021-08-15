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
   public enum TextureFormat
   {
      Alpha8 = 1,
      ARGB4444 = 2,
      RGB24 = 3,
      RGBA32 = 4,
      ARGB32 = 5,
      RGB565 = 7,
      R16 = 9,
      DXT1 = 10,
      DXT5 = 12,
      RGBA4444 = 13,
      BGRA32 = 14,
      RHalf = 0xF,
      RGHalf = 0x10,
      RGBAHalf = 17,
      RFloat = 18,
      RGFloat = 19,
      RGBAFloat = 20,
      YUY2 = 21,
      RGB9e5Float = 22,
      BC4 = 26,
      BC5 = 27,
      BC6H = 24,
      BC7 = 25,
      DXT1Crunched = 28,
      DXT5Crunched = 29,
      PVRTC_RGB2 = 30,
      PVRTC_RGBA2 = 0x1F,
      PVRTC_RGB4 = 0x20,
      PVRTC_RGBA4 = 33,
      ETC_RGB4 = 34,
      ATC_RGB4 = 35,
      ATC_RGBA8 = 36,
      EAC_R = 41,
      EAC_R_SIGNED = 42,
      EAC_RG = 43,
      EAC_RG_SIGNED = 44,
      ETC2_RGB = 45,
      ETC2_RGBA1 = 46,
      ETC2_RGBA8 = 47,
      ASTC_RGB_4x4 = 48,
      ASTC_RGB_5x5 = 49,
      ASTC_RGB_6x6 = 50,
      ASTC_RGB_8x8 = 51,
      ASTC_RGB_10x10 = 52,
      ASTC_RGB_12x12 = 53,
      ASTC_RGBA_4x4 = 54,
      ASTC_RGBA_5x5 = 55,
      ASTC_RGBA_6x6 = 56,
      ASTC_RGBA_8x8 = 57,
      ASTC_RGBA_10x10 = 58,
      ASTC_RGBA_12x12 = 59,
      ETC_RGB4_3DS = 60,
      ETC_RGBA8_3DS = 61,
      RG16 = 62,
      R8 = 0x3F
   }
}
