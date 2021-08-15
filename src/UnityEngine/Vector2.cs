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
   public struct Vector2
   {
      public float x;

      public float y;

      public const float kEpsilon = 1E-05f;

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
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector2 index!" );
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
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector2 index!" );
            }
         }
      }

      public Vector2 normalized
      {
         get
         {
            Vector2 result = new Vector2( x, y );
            result.Normalize();
            return result;
         }
      }

      public float magnitude => throw new NotImplementedException();

      public float sqrMagnitude => x * x + y * y;

      public static Vector2 zero => new Vector2( 0f, 0f );

      public static Vector2 one => new Vector2( 1f, 1f );

      public static Vector2 up => new Vector2( 0f, 1f );

      public static Vector2 down => new Vector2( 0f, -1f );

      public static Vector2 left => new Vector2( -1f, 0f );

      public static Vector2 right => new Vector2( 1f, 0f );

      public Vector2( float x, float y )
      {
         this.x = x;
         this.y = y;
      }

      public void Set( float newX, float newY )
      {
         x = newX;
         y = newY;
      }

      public static Vector2 Lerp( Vector2 a, Vector2 b, float t ) => throw new NotImplementedException();

      public static Vector2 LerpUnclamped( Vector2 a, Vector2 b, float t )
      {
         return new Vector2( a.x + ( b.x - a.x ) * t, a.y + ( b.y - a.y ) * t );
      }

      public static Vector2 MoveTowards( Vector2 current, Vector2 target, float maxDistanceDelta )
      {
         Vector2 a = target - current;
         float magnitude = a.magnitude;
         if( magnitude <= maxDistanceDelta || magnitude == 0f )
         {
            return target;
         }

         return current + a / magnitude * maxDistanceDelta;
      }

      public static Vector2 Scale( Vector2 a, Vector2 b )
      {
         return new Vector2( a.x * b.x, a.y * b.y );
      }

      public void Scale( Vector2 scale )
      {
         x *= scale.x;
         y *= scale.y;
      }

      public void Normalize()
      {
         float magnitude = this.magnitude;
         if( magnitude > 1E-05f )
         {
            this /= magnitude;
         }
         else
         {
            this = zero;
         }
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();

      public override int GetHashCode()
      {
         return x.GetHashCode() ^ ( y.GetHashCode() << 2 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Vector2 ) )
         {
            return false;
         }

         Vector2 vector = (Vector2)other;
         return x.Equals( vector.x ) && y.Equals( vector.y );
      }

      public static Vector2 Reflect( Vector2 inDirection, Vector2 inNormal )
      {
         return -2f * Dot( inNormal, inDirection ) * inNormal + inDirection;
      }

      public static float Dot( Vector2 lhs, Vector2 rhs )
      {
         return lhs.x * rhs.x + lhs.y * rhs.y;
      }

      public static float Angle( Vector2 from, Vector2 to ) => throw new NotImplementedException();

      public static float Distance( Vector2 a, Vector2 b )
      {
         return ( a - b ).magnitude;
      }

      public static Vector2 ClampMagnitude( Vector2 vector, float maxLength )
      {
         if( vector.sqrMagnitude > maxLength * maxLength )
         {
            return vector.normalized * maxLength;
         }

         return vector;
      }

      public static float SqrMagnitude( Vector2 a )
      {
         return a.x * a.x + a.y * a.y;
      }

      public float SqrMagnitude()
      {
         return x * x + y * y;
      }

      public static Vector2 Min( Vector2 lhs, Vector2 rhs ) => throw new NotImplementedException();

      public static Vector2 Max( Vector2 lhs, Vector2 rhs ) => throw new NotImplementedException();

      public static Vector2 SmoothDamp( Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime ) => throw new NotImplementedException();

      public static Vector2 operator +( Vector2 a, Vector2 b )
      {
         return new Vector2( a.x + b.x, a.y + b.y );
      }

      public static Vector2 operator -( Vector2 a, Vector2 b )
      {
         return new Vector2( a.x - b.x, a.y - b.y );
      }

      public static Vector2 operator -( Vector2 a )
      {
         return new Vector2( 0f - a.x, 0f - a.y );
      }

      public static Vector2 operator *( Vector2 a, float d )
      {
         return new Vector2( a.x * d, a.y * d );
      }

      public static Vector2 operator *( float d, Vector2 a )
      {
         return new Vector2( a.x * d, a.y * d );
      }

      public static Vector2 operator /( Vector2 a, float d )
      {
         return new Vector2( a.x / d, a.y / d );
      }

      public static bool operator ==( Vector2 lhs, Vector2 rhs )
      {
         return ( lhs - rhs ).sqrMagnitude < 9.99999944E-11f;
      }

      public static bool operator !=( Vector2 lhs, Vector2 rhs )
      {
         return !( lhs == rhs );
      }

      public static implicit operator Vector2( Vector3 v )
      {
         return new Vector2( v.x, v.y );
      }

      public static implicit operator Vector3( Vector2 v )
      {
         return new Vector3( v.x, v.y, 0f );
      }
   }
}
