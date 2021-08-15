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
   public struct Matrix4x4
   {
      public float m00;

      public float m10;

      public float m20;

      public float m30;

      public float m01;

      public float m11;

      public float m21;

      public float m31;

      public float m02;

      public float m12;

      public float m22;

      public float m32;

      public float m03;

      public float m13;

      public float m23;

      public float m33;

      public Matrix4x4 inverse => Inverse( this );

      public Matrix4x4 transpose => Transpose( this );

      public bool isIdentity => throw new NotImplementedException();

      public float determinant => Determinant( this );

      public float this[ int row, int column ]
      {
         get
         {
            return this[ row + column * 4 ];
         }
         set
         {
            this[ row + column * 4 ] = value;
         }
      }

      public float this[ int index ]
      {
         get
         {
            switch( index )
            {
               case 0:
                  return m00;
               case 1:
                  return m10;
               case 2:
                  return m20;
               case 3:
                  return m30;
               case 4:
                  return m01;
               case 5:
                  return m11;
               case 6:
                  return m21;
               case 7:
                  return m31;
               case 8:
                  return m02;
               case 9:
                  return m12;
               case 10:
                  return m22;
               case 11:
                  return m32;
               case 12:
                  return m03;
               case 13:
                  return m13;
               case 14:
                  return m23;
               case 15:
                  return m33;
               default:
                  throw new IndexOutOfRangeException( "Invalid matrix index!" );
            }
         }
         set
         {
            switch( index )
            {
               case 0:
                  m00 = value;
                  break;
               case 1:
                  m10 = value;
                  break;
               case 2:
                  m20 = value;
                  break;
               case 3:
                  m30 = value;
                  break;
               case 4:
                  m01 = value;
                  break;
               case 5:
                  m11 = value;
                  break;
               case 6:
                  m21 = value;
                  break;
               case 7:
                  m31 = value;
                  break;
               case 8:
                  m02 = value;
                  break;
               case 9:
                  m12 = value;
                  break;
               case 10:
                  m22 = value;
                  break;
               case 11:
                  m32 = value;
                  break;
               case 12:
                  m03 = value;
                  break;
               case 13:
                  m13 = value;
                  break;
               case 14:
                  m23 = value;
                  break;
               case 15:
                  m33 = value;
                  break;
               default:
                  throw new IndexOutOfRangeException( "Invalid matrix index!" );
            }
         }
      }

      public static Matrix4x4 zero
      {
         get
         {
            Matrix4x4 result = default( Matrix4x4 );
            result.m00 = 0f;
            result.m01 = 0f;
            result.m02 = 0f;
            result.m03 = 0f;
            result.m10 = 0f;
            result.m11 = 0f;
            result.m12 = 0f;
            result.m13 = 0f;
            result.m20 = 0f;
            result.m21 = 0f;
            result.m22 = 0f;
            result.m23 = 0f;
            result.m30 = 0f;
            result.m31 = 0f;
            result.m32 = 0f;
            result.m33 = 0f;
            return result;
         }
      }

      public static Matrix4x4 identity
      {
         get
         {
            Matrix4x4 result = default( Matrix4x4 );
            result.m00 = 1f;
            result.m01 = 0f;
            result.m02 = 0f;
            result.m03 = 0f;
            result.m10 = 0f;
            result.m11 = 1f;
            result.m12 = 0f;
            result.m13 = 0f;
            result.m20 = 0f;
            result.m21 = 0f;
            result.m22 = 1f;
            result.m23 = 0f;
            result.m30 = 0f;
            result.m31 = 0f;
            result.m32 = 0f;
            result.m33 = 1f;
            return result;
         }
      }

      public static Matrix4x4 Inverse( Matrix4x4 m )
      {
         INTERNAL_CALL_Inverse( ref m, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_Inverse( ref Matrix4x4 m, out Matrix4x4 value );

      public static Matrix4x4 Transpose( Matrix4x4 m )
      {
         INTERNAL_CALL_Transpose( ref m, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_Transpose( ref Matrix4x4 m, out Matrix4x4 value );

      internal static bool Invert( Matrix4x4 inMatrix, out Matrix4x4 dest )
      {
         return INTERNAL_CALL_Invert( ref inMatrix, out dest );
      }

      private static extern bool INTERNAL_CALL_Invert( ref Matrix4x4 inMatrix, out Matrix4x4 dest );

      public static float Determinant( Matrix4x4 m )
      {
         return INTERNAL_CALL_Determinant( ref m );
      }

      private static extern float INTERNAL_CALL_Determinant( ref Matrix4x4 m );

      public void SetTRS( Vector3 pos, Quaternion q, Vector3 s )
      {
         this = TRS( pos, q, s );
      }

      public static Matrix4x4 TRS( Vector3 pos, Quaternion q, Vector3 s )
      {
         INTERNAL_CALL_TRS( ref pos, ref q, ref s, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_TRS( ref Vector3 pos, ref Quaternion q, ref Vector3 s, out Matrix4x4 value );

      public static Matrix4x4 Ortho( float left, float right, float bottom, float top, float zNear, float zFar )
      {
         INTERNAL_CALL_Ortho( left, right, bottom, top, zNear, zFar, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_Ortho( float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 value );

      public static Matrix4x4 Perspective( float fov, float aspect, float zNear, float zFar )
      {
         INTERNAL_CALL_Perspective( fov, aspect, zNear, zFar, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_Perspective( float fov, float aspect, float zNear, float zFar, out Matrix4x4 value );

      public static Matrix4x4 LookAt( Vector3 from, Vector3 to, Vector3 up )
      {
         INTERNAL_CALL_LookAt( ref from, ref to, ref up, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_LookAt( ref Vector3 from, ref Vector3 to, ref Vector3 up, out Matrix4x4 value );

      public override int GetHashCode()
      {
         return GetColumn( 0 ).GetHashCode() ^ ( GetColumn( 1 ).GetHashCode() << 2 ) ^ ( GetColumn( 2 ).GetHashCode() >> 2 ) ^ ( GetColumn( 3 ).GetHashCode() >> 1 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Matrix4x4 ) )
         {
            return false;
         }

         Matrix4x4 matrix4x = (Matrix4x4)other;
         return GetColumn( 0 ).Equals( matrix4x.GetColumn( 0 ) ) && GetColumn( 1 ).Equals( matrix4x.GetColumn( 1 ) ) && GetColumn( 2 ).Equals( matrix4x.GetColumn( 2 ) ) && GetColumn( 3 ).Equals( matrix4x.GetColumn( 3 ) );
      }

      public static Matrix4x4 operator *( Matrix4x4 lhs, Matrix4x4 rhs )
      {
         Matrix4x4 result = default( Matrix4x4 );
         result.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
         result.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
         result.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
         result.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;
         result.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
         result.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
         result.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
         result.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;
         result.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
         result.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
         result.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
         result.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;
         result.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
         result.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
         result.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
         result.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
         return result;
      }

      public static Vector4 operator *( Matrix4x4 lhs, Vector4 v )
      {
         Vector4 result = default( Vector4 );
         result.x = lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z + lhs.m03 * v.w;
         result.y = lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z + lhs.m13 * v.w;
         result.z = lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z + lhs.m23 * v.w;
         result.w = lhs.m30 * v.x + lhs.m31 * v.y + lhs.m32 * v.z + lhs.m33 * v.w;
         return result;
      }

      public static bool operator ==( Matrix4x4 lhs, Matrix4x4 rhs )
      {
         return lhs.GetColumn( 0 ) == rhs.GetColumn( 0 ) && lhs.GetColumn( 1 ) == rhs.GetColumn( 1 ) && lhs.GetColumn( 2 ) == rhs.GetColumn( 2 ) && lhs.GetColumn( 3 ) == rhs.GetColumn( 3 );
      }

      public static bool operator !=( Matrix4x4 lhs, Matrix4x4 rhs )
      {
         return !( lhs == rhs );
      }

      public Vector4 GetColumn( int i )
      {
         return new Vector4( this[ 0, i ], this[ 1, i ], this[ 2, i ], this[ 3, i ] );
      }

      public Vector4 GetRow( int i )
      {
         return new Vector4( this[ i, 0 ], this[ i, 1 ], this[ i, 2 ], this[ i, 3 ] );
      }

      public void SetColumn( int i, Vector4 v )
      {
         this[ 0, i ] = v.x;
         this[ 1, i ] = v.y;
         this[ 2, i ] = v.z;
         this[ 3, i ] = v.w;
      }

      public void SetRow( int i, Vector4 v )
      {
         this[ i, 0 ] = v.x;
         this[ i, 1 ] = v.y;
         this[ i, 2 ] = v.z;
         this[ i, 3 ] = v.w;
      }

      public Vector3 MultiplyPoint( Vector3 v )
      {
         Vector3 result = default( Vector3 );
         result.x = m00 * v.x + m01 * v.y + m02 * v.z + m03;
         result.y = m10 * v.x + m11 * v.y + m12 * v.z + m13;
         result.z = m20 * v.x + m21 * v.y + m22 * v.z + m23;
         float num = m30 * v.x + m31 * v.y + m32 * v.z + m33;
         num = 1f / num;
         result.x *= num;
         result.y *= num;
         result.z *= num;
         return result;
      }

      public Vector3 MultiplyPoint3x4( Vector3 v )
      {
         Vector3 result = default( Vector3 );
         result.x = m00 * v.x + m01 * v.y + m02 * v.z + m03;
         result.y = m10 * v.x + m11 * v.y + m12 * v.z + m13;
         result.z = m20 * v.x + m21 * v.y + m22 * v.z + m23;
         return result;
      }

      public Vector3 MultiplyVector( Vector3 v )
      {
         Vector3 result = default( Vector3 );
         result.x = m00 * v.x + m01 * v.y + m02 * v.z;
         result.y = m10 * v.x + m11 * v.y + m12 * v.z;
         result.z = m20 * v.x + m21 * v.y + m22 * v.z;
         return result;
      }

      public static Matrix4x4 Scale( Vector3 v )
      {
         Matrix4x4 result = default( Matrix4x4 );
         result.m00 = v.x;
         result.m01 = 0f;
         result.m02 = 0f;
         result.m03 = 0f;
         result.m10 = 0f;
         result.m11 = v.y;
         result.m12 = 0f;
         result.m13 = 0f;
         result.m20 = 0f;
         result.m21 = 0f;
         result.m22 = v.z;
         result.m23 = 0f;
         result.m30 = 0f;
         result.m31 = 0f;
         result.m32 = 0f;
         result.m33 = 1f;
         return result;
      }

      public static Matrix4x4 Translate( Vector3 v )
      {
         Matrix4x4 result = default( Matrix4x4 );
         result.m00 = 1f;
         result.m01 = 0f;
         result.m02 = 0f;
         result.m03 = v.x;
         result.m10 = 0f;
         result.m11 = 1f;
         result.m12 = 0f;
         result.m13 = v.y;
         result.m20 = 0f;
         result.m21 = 0f;
         result.m22 = 1f;
         result.m23 = v.z;
         result.m30 = 0f;
         result.m31 = 0f;
         result.m32 = 0f;
         result.m33 = 1f;
         return result;
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();
   }
}
