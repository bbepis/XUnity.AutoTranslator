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
   public struct Color
   {
      public float r;

      public float g;

      public float b;

      public float a;

      public static Color red => new Color( 1f, 0f, 0f, 1f );

      public static Color green => new Color( 0f, 1f, 0f, 1f );

      public static Color blue => new Color( 0f, 0f, 1f, 1f );

      public static Color white => new Color( 1f, 1f, 1f, 1f );

      public static Color black => new Color( 0f, 0f, 0f, 1f );

      public static Color yellow => new Color( 1f, 0.921568632f, 0.0156862754f, 1f );

      public static Color cyan => new Color( 0f, 1f, 1f, 1f );

      public static Color magenta => new Color( 1f, 0f, 1f, 1f );

      public static Color gray => new Color( 0.5f, 0.5f, 0.5f, 1f );

      public static Color grey => new Color( 0.5f, 0.5f, 0.5f, 1f );

      public static Color clear => new Color( 0f, 0f, 0f, 0f );

      public float grayscale => 0.299f * r + 0.587f * g + 0.114f * b;

      public Color linear => throw new NotImplementedException();

      public Color gamma => throw new NotImplementedException();

      public float maxColorComponent => throw new NotImplementedException();

      public float this[ int index ]
      {
         get
         {
            switch( index )
            {
               case 0:
                  return r;
               case 1:
                  return g;
               case 2:
                  return b;
               case 3:
                  return a;
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector3 index!" );
            }
         }
         set
         {
            switch( index )
            {
               case 0:
                  r = value;
                  break;
               case 1:
                  g = value;
                  break;
               case 2:
                  b = value;
                  break;
               case 3:
                  a = value;
                  break;
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector3 index!" );
            }
         }
      }

      public Color( float r, float g, float b, float a )
      {
         this.r = r;
         this.g = g;
         this.b = b;
         this.a = a;
      }

      public Color( float r, float g, float b )
      {
         this.r = r;
         this.g = g;
         this.b = b;
         a = 1f;
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();

      public override int GetHashCode()
      {
         return ( (Vector4)this ).GetHashCode();
      }

      public override bool Equals( object other )
      {
         if( !( other is Color ) )
         {
            return false;
         }

         Color color = (Color)other;
         return r.Equals( color.r ) && g.Equals( color.g ) && b.Equals( color.b ) && a.Equals( color.a );
      }

      public static Color operator +( Color a, Color b )
      {
         return new Color( a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a );
      }

      public static Color operator -( Color a, Color b )
      {
         return new Color( a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a );
      }

      public static Color operator *( Color a, Color b )
      {
         return new Color( a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a );
      }

      public static Color operator *( Color a, float b )
      {
         return new Color( a.r * b, a.g * b, a.b * b, a.a * b );
      }

      public static Color operator *( float b, Color a )
      {
         return new Color( a.r * b, a.g * b, a.b * b, a.a * b );
      }

      public static Color operator /( Color a, float b )
      {
         return new Color( a.r / b, a.g / b, a.b / b, a.a / b );
      }

      public static bool operator ==( Color lhs, Color rhs )
      {
         return (Vector4)lhs == (Vector4)rhs;
      }

      public static bool operator !=( Color lhs, Color rhs )
      {
         return !( lhs == rhs );
      }

      public static Color Lerp( Color a, Color b, float t ) => throw new NotImplementedException();

      public static Color LerpUnclamped( Color a, Color b, float t )
      {
         return new Color( a.r + ( b.r - a.r ) * t, a.g + ( b.g - a.g ) * t, a.b + ( b.b - a.b ) * t, a.a + ( b.a - a.a ) * t );
      }

      internal Color RGBMultiplied( float multiplier )
      {
         return new Color( r * multiplier, g * multiplier, b * multiplier, a );
      }

      internal Color AlphaMultiplied( float multiplier )
      {
         return new Color( r, g, b, a * multiplier );
      }

      internal Color RGBMultiplied( Color multiplier )
      {
         return new Color( r * multiplier.r, g * multiplier.g, b * multiplier.b, a );
      }

      public static implicit operator Vector4( Color c )
      {
         return new Vector4( c.r, c.g, c.b, c.a );
      }

      public static implicit operator Color( Vector4 v )
      {
         return new Color( v.x, v.y, v.z, v.w );
      }

      public static void RGBToHSV( Color rgbColor, out float H, out float S, out float V )
      {
         if( rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r )
         {
            RGBToHSVHelper( 4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V );
         }
         else if( rgbColor.g > rgbColor.r )
         {
            RGBToHSVHelper( 2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V );
         }
         else
         {
            RGBToHSVHelper( 0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V );
         }
      }

      private static void RGBToHSVHelper( float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V )
      {
         V = dominantcolor;
         if( V != 0f )
         {
            float num = 0f;
            num = ( ( !( colorone > colortwo ) ) ? colorone : colortwo );
            float num2 = V - num;
            if( num2 != 0f )
            {
               S = num2 / V;
               H = offset + ( colorone - colortwo ) / num2;
            }
            else
            {
               S = 0f;
               H = offset + ( colorone - colortwo );
            }

            H /= 6f;
            if( H < 0f )
            {
               H += 1f;
            }
         }
         else
         {
            S = 0f;
            H = 0f;
         }
      }

      public static Color HSVToRGB( float H, float S, float V )
      {
         return HSVToRGB( H, S, V, hdr: true );
      }

      public static Color HSVToRGB( float H, float S, float V, bool hdr ) => throw new NotImplementedException();
   }
}
