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
   public struct Vector3
   {
      public const float kEpsilon = 1E-05f;

      public float x;

      public float y;

      public float z;

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
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector3 index!" );
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
               default:
                  throw new IndexOutOfRangeException( "Invalid Vector3 index!" );
            }
         }
      }

      public Vector3 normalized => Normalize( this );

      public float magnitude => throw new NotImplementedException();

      public float sqrMagnitude => x * x + y * y + z * z;

      public static Vector3 zero => new Vector3( 0f, 0f, 0f );

      public static Vector3 one => new Vector3( 1f, 1f, 1f );

      public static Vector3 forward => new Vector3( 0f, 0f, 1f );

      public static Vector3 back => new Vector3( 0f, 0f, -1f );

      public static Vector3 up => new Vector3( 0f, 1f, 0f );

      public static Vector3 down => new Vector3( 0f, -1f, 0f );

      public static Vector3 left => new Vector3( -1f, 0f, 0f );

      public static Vector3 right => new Vector3( 1f, 0f, 0f );

      [Obsolete( "Use Vector3.forward instead." )]
      public static Vector3 fwd => new Vector3( 0f, 0f, 1f );

      public Vector3( float x, float y, float z )
      {
         this.x = x;
         this.y = y;
         this.z = z;
      }

      public Vector3( float x, float y )
      {
         this.x = x;
         this.y = y;
         z = 0f;
      }

      public static Vector3 Slerp( Vector3 a, Vector3 b, float t ) => throw new NotImplementedException();

      private static extern void INTERNAL_CALL_Slerp( ref Vector3 a, ref Vector3 b, float t, out Vector3 value );

      public static Vector3 SlerpUnclamped( Vector3 a, Vector3 b, float t )
      {
         INTERNAL_CALL_SlerpUnclamped( ref a, ref b, t, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_SlerpUnclamped( ref Vector3 a, ref Vector3 b, float t, out Vector3 value );

      private static void Internal_OrthoNormalize2( ref Vector3 a, ref Vector3 b )
      {
         INTERNAL_CALL_Internal_OrthoNormalize2( ref a, ref b );
      }

      private static extern void INTERNAL_CALL_Internal_OrthoNormalize2( ref Vector3 a, ref Vector3 b );

      private static void Internal_OrthoNormalize3( ref Vector3 a, ref Vector3 b, ref Vector3 c )
      {
         INTERNAL_CALL_Internal_OrthoNormalize3( ref a, ref b, ref c );
      }

      private static extern void INTERNAL_CALL_Internal_OrthoNormalize3( ref Vector3 a, ref Vector3 b, ref Vector3 c );

      public static void OrthoNormalize( ref Vector3 normal, ref Vector3 tangent )
      {
         Internal_OrthoNormalize2( ref normal, ref tangent );
      }

      public static void OrthoNormalize( ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal )
      {
         Internal_OrthoNormalize3( ref normal, ref tangent, ref binormal );
      }

      public static Vector3 RotateTowards( Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta ) => throw new NotImplementedException();

      [Obsolete( "Use Vector3.ProjectOnPlane instead." )]
      public static Vector3 Exclude( Vector3 excludeThis, Vector3 fromThat )
      {
         return fromThat - Project( fromThat, excludeThis );
      }

      public static Vector3 Lerp( Vector3 a, Vector3 b, float t ) => throw new NotImplementedException();

      public static Vector3 LerpUnclamped( Vector3 a, Vector3 b, float t )
      {
         return new Vector3( a.x + ( b.x - a.x ) * t, a.y + ( b.y - a.y ) * t, a.z + ( b.z - a.z ) * t );
      }

      public static Vector3 MoveTowards( Vector3 current, Vector3 target, float maxDistanceDelta )
      {
         Vector3 a = target - current;
         float magnitude = a.magnitude;
         if( magnitude <= maxDistanceDelta || magnitude == 0f )
         {
            return target;
         }

         return current + a / magnitude * maxDistanceDelta;
      }

      public static Vector3 SmoothDamp( Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed )
      {
         float deltaTime = Time.deltaTime;
         return SmoothDamp( current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime );
      }

      public static Vector3 SmoothDamp( Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime )
      {
         float deltaTime = Time.deltaTime;
         float maxSpeed = float.PositiveInfinity;
         return SmoothDamp( current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime );
      }

      public static Vector3 SmoothDamp( Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime ) => throw new NotImplementedException();

      public void Set( float new_x, float new_y, float new_z )
      {
         x = new_x;
         y = new_y;
         z = new_z;
      }

      public static Vector3 Scale( Vector3 a, Vector3 b )
      {
         return new Vector3( a.x * b.x, a.y * b.y, a.z * b.z );
      }

      public void Scale( Vector3 scale )
      {
         x *= scale.x;
         y *= scale.y;
         z *= scale.z;
      }

      public static Vector3 Cross( Vector3 lhs, Vector3 rhs )
      {
         return new Vector3( lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x );
      }

      public override int GetHashCode()
      {
         return x.GetHashCode() ^ ( y.GetHashCode() << 2 ) ^ ( z.GetHashCode() >> 2 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Vector3 ) )
         {
            return false;
         }

         Vector3 vector = (Vector3)other;
         return x.Equals( vector.x ) && y.Equals( vector.y ) && z.Equals( vector.z );
      }

      public static Vector3 Reflect( Vector3 inDirection, Vector3 inNormal )
      {
         return -2f * Dot( inNormal, inDirection ) * inNormal + inDirection;
      }

      public static Vector3 Normalize( Vector3 value )
      {
         float num = Magnitude( value );
         if( num > 1E-05f )
         {
            return value / num;
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

      public static float Dot( Vector3 lhs, Vector3 rhs )
      {
         return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
      }

      public static Vector3 Project( Vector3 vector, Vector3 onNormal ) => throw new NotImplementedException();

      public static Vector3 ProjectOnPlane( Vector3 vector, Vector3 planeNormal )
      {
         return vector - Project( vector, planeNormal );
      }

      public static float Angle( Vector3 from, Vector3 to ) => throw new NotImplementedException();

      public static float Distance( Vector3 a, Vector3 b ) => throw new NotImplementedException();

      public static Vector3 ClampMagnitude( Vector3 vector, float maxLength )
      {
         if( vector.sqrMagnitude > maxLength * maxLength )
         {
            return vector.normalized * maxLength;
         }

         return vector;
      }

      public static float Magnitude( Vector3 a ) => throw new NotImplementedException();

      public static float SqrMagnitude( Vector3 a )
      {
         return a.x * a.x + a.y * a.y + a.z * a.z;
      }

      public static Vector3 Min( Vector3 lhs, Vector3 rhs ) => throw new NotImplementedException();

      public static Vector3 Max( Vector3 lhs, Vector3 rhs ) => throw new NotImplementedException();

      public static Vector3 operator +( Vector3 a, Vector3 b )
      {
         return new Vector3( a.x + b.x, a.y + b.y, a.z + b.z );
      }

      public static Vector3 operator -( Vector3 a, Vector3 b )
      {
         return new Vector3( a.x - b.x, a.y - b.y, a.z - b.z );
      }

      public static Vector3 operator -( Vector3 a )
      {
         return new Vector3( 0f - a.x, 0f - a.y, 0f - a.z );
      }

      public static Vector3 operator *( Vector3 a, float d )
      {
         return new Vector3( a.x * d, a.y * d, a.z * d );
      }

      public static Vector3 operator *( float d, Vector3 a )
      {
         return new Vector3( a.x * d, a.y * d, a.z * d );
      }

      public static Vector3 operator /( Vector3 a, float d )
      {
         return new Vector3( a.x / d, a.y / d, a.z / d );
      }

      public static bool operator ==( Vector3 lhs, Vector3 rhs )
      {
         return SqrMagnitude( lhs - rhs ) < 9.99999944E-11f;
      }

      public static bool operator !=( Vector3 lhs, Vector3 rhs )
      {
         return !( lhs == rhs );
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();

      public static float AngleBetween( Vector3 from, Vector3 to ) => throw new NotImplementedException();
   }
}
