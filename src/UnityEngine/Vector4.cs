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
   public struct Vector4
   {
      public const float kEpsilon = 1E-05f;

      public float x;

      public float y;

      public float z;

      public float w;

      public float this[ int index ]
      {
         get
         {
            switch( index )
            {
               case 0:
                  return x;
               case 1:
                  return y;
               case 2:
                  return z;
               case 3:
                  return w;
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector4 index!" );
            }
         }
         set
         {
            switch( index )
            {
               case 0:
                  x = value;
                  break;
               case 1:
                  y = value;
                  break;
               case 2:
                  z = value;
                  break;
               case 3:
                  w = value;
                  break;
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector4 index!" );
            }
         }
      }

      public Vector4 normalized => Normalize( this );

      public float magnitude => throw new NotImplementedException();

      public float sqrMagnitude => Dot( this, this );

      public static Vector4 zero => new Vector4( 0f, 0f, 0f, 0f );

      public static Vector4 one => new Vector4( 1f, 1f, 1f, 1f );

      public Vector4( float x, float y, float z, float w )
      {
         this.x = x;
         this.y = y;
         this.z = z;
         this.w = w;
      }

      public Vector4( float x, float y, float z )
      {
         this.x = x;
         this.y = y;
         this.z = z;
         w = 0f;
      }

      public Vector4( float x, float y )
      {
         this.x = x;
         this.y = y;
         z = 0f;
         w = 0f;
      }

      public void Set( float new_x, float new_y, float new_z, float new_w )
      {
         x = new_x;
         y = new_y;
         z = new_z;
         w = new_w;
      }

      public static Vector4 Lerp( Vector4 a, Vector4 b, float t ) => throw new NotImplementedException();

      public static Vector4 LerpUnclamped( Vector4 a, Vector4 b, float t )
      {
         return new Vector4( a.x + ( b.x - a.x ) * t, a.y + ( b.y - a.y ) * t, a.z + ( b.z - a.z ) * t, a.w + ( b.w - a.w ) * t );
      }

      public static Vector4 MoveTowards( Vector4 current, Vector4 target, float maxDistanceDelta )
      {
         Vector4 a = target - current;
         float magnitude = a.magnitude;
         if( magnitude <= maxDistanceDelta || magnitude == 0f )
         {
            return target;
         }

         return current + a / magnitude * maxDistanceDelta;
      }

      public static Vector4 Scale( Vector4 a, Vector4 b )
      {
         return new Vector4( a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w );
      }

      public void Scale( Vector4 scale )
      {
         x *= scale.x;
         y *= scale.y;
         z *= scale.z;
         w *= scale.w;
      }

      public override int GetHashCode()
      {
         return x.GetHashCode() ^ ( y.GetHashCode() << 2 ) ^ ( z.GetHashCode() >> 2 ) ^ ( w.GetHashCode() >> 1 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Vector4 ) )
         {
            return false;
         }

         Vector4 vector = (Vector4)other;
         return x.Equals( vector.x ) && y.Equals( vector.y ) && z.Equals( vector.z ) && w.Equals( vector.w );
      }

      public static Vector4 Normalize( Vector4 a )
      {
         float num = Magnitude( a );
         if( num > 1E-05f )
         {
            return a / num;
         }

         return zero;
      }

      public void Normalize()
      {
         float num = Magnitude( this );
         if( num > 1E-05f )
         {
            this /= num;
         }
         else
         {
            this = zero;
         }
      }

      public static float Dot( Vector4 a, Vector4 b )
      {
         return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
      }

      public static Vector4 Project( Vector4 a, Vector4 b )
      {
         return b * Dot( a, b ) / Dot( b, b );
      }

      public static float Distance( Vector4 a, Vector4 b )
      {
         return Magnitude( a - b );
      }

      public static float Magnitude( Vector4 a ) => throw new NotImplementedException();

      public static Vector4 Min( Vector4 lhs, Vector4 rhs ) => throw new NotImplementedException();

      public static Vector4 Max( Vector4 lhs, Vector4 rhs ) => throw new NotImplementedException();

      public static Vector4 operator +( Vector4 a, Vector4 b )
      {
         return new Vector4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w );
      }

      public static Vector4 operator -( Vector4 a, Vector4 b )
      {
         return new Vector4( a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w );
      }

      public static Vector4 operator -( Vector4 a )
      {
         return new Vector4( 0f - a.x, 0f - a.y, 0f - a.z, 0f - a.w );
      }

      public static Vector4 operator *( Vector4 a, float d )
      {
         return new Vector4( a.x * d, a.y * d, a.z * d, a.w * d );
      }

      public static Vector4 operator *( float d, Vector4 a )
      {
         return new Vector4( a.x * d, a.y * d, a.z * d, a.w * d );
      }

      public static Vector4 operator /( Vector4 a, float d )
      {
         return new Vector4( a.x / d, a.y / d, a.z / d, a.w / d );
      }

      public static bool operator ==( Vector4 lhs, Vector4 rhs )
      {
         return SqrMagnitude( lhs - rhs ) < 9.99999944E-11f;
      }

      public static bool operator !=( Vector4 lhs, Vector4 rhs )
      {
         return !( lhs == rhs );
      }

      public static implicit operator Vector4( Vector3 v )
      {
         return new Vector4( v.x, v.y, v.z, 0f );
      }

      public static implicit operator Vector3( Vector4 v )
      {
         return new Vector3( v.x, v.y, v.z );
      }

      public static implicit operator Vector4( Vector2 v ) => throw new NotImplementedException();

      public static implicit operator Vector2( Vector4 v ) => throw new NotImplementedException();

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();

      public static float SqrMagnitude( Vector4 a )
      {
         return Dot( a, a );
      }

      public float SqrMagnitude()
      {
         return Dot( this, this );
      }
   }
}
