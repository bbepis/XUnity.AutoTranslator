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
   public struct Color32
   {
      public byte r;

      public byte g;

      public byte b;

      public byte a;

      public Color32( byte r, byte g, byte b, byte a )
      {
         this.r = r;
         this.g = g;
         this.b = b;
         this.a = a;
      }

      public static implicit operator Color32( Color c ) => throw new NotImplementedException();

      public static implicit operator Color( Color32 c )
      {
         return new Color( (float)(int)c.r / 255f, (float)(int)c.g / 255f, (float)(int)c.b / 255f, (float)(int)c.a / 255f );
      }

      public static Color32 Lerp( Color32 a, Color32 b, float t ) => throw new NotImplementedException();

      public static Color32 LerpUnclamped( Color32 a, Color32 b, float t )
      {
         return new Color32( (byte)( (float)(int)a.r + (float)( b.r - a.r ) * t ), (byte)( (float)(int)a.g + (float)( b.g - a.g ) * t ), (byte)( (float)(int)a.b + (float)( b.b - a.b ) * t ), (byte)( (float)(int)a.a + (float)( b.a - a.a ) * t ) );
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();
   }
}
