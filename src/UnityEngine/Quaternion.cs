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
   public struct Quaternion
   {
      public float x;

      public float y;

      public float z;

      public float w;

      public const float kEpsilon = 1E-06f;

      public Vector3 eulerAngles
      {
         get
         {
            return Internal_MakePositive( Internal_ToEulerRad( this ) * 57.29578f );
         }
         set
         {
            this = Internal_FromEulerRad( value * ( (float)Math.PI / 180f ) );
         }
      }

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
                  throw new IndexOutOfRangeException( "Invalid Quaternion index!" );
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
                  throw new IndexOutOfRangeException( "Invalid Quaternion index!" );
            }
         }
      }

      public static Quaternion identity => new Quaternion( 0f, 0f, 0f, 1f );

      public Quaternion( float x, float y, float z, float w )
      {
         this.x = x;
         this.y = y;
         this.z = z;
         this.w = w;
      }

      public static Quaternion AngleAxis( float angle, Vector3 axis )
      {
         INTERNAL_CALL_AngleAxis( angle, ref axis, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_AngleAxis( float angle, ref Vector3 axis, out Quaternion value );

      public void ToAngleAxis( out float angle, out Vector3 axis )
      {
         Internal_ToAxisAngleRad( this, out axis, out angle );
         angle *= 57.29578f;
      }

      public static Quaternion FromToRotation( Vector3 fromDirection, Vector3 toDirection )
      {
         INTERNAL_CALL_FromToRotation( ref fromDirection, ref toDirection, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_FromToRotation( ref Vector3 fromDirection, ref Vector3 toDirection, out Quaternion value );

      public void SetFromToRotation( Vector3 fromDirection, Vector3 toDirection )
      {
         this = FromToRotation( fromDirection, toDirection );
      }

      public static Quaternion LookRotation( Vector3 forward, Vector3 upwards )
      {
         INTERNAL_CALL_LookRotation( ref forward, ref upwards, out Quaternion value );
         return value;
      }

      public static Quaternion LookRotation( Vector3 forward )
      {
         Vector3 upwards = Vector3.up;
         INTERNAL_CALL_LookRotation( ref forward, ref upwards, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_LookRotation( ref Vector3 forward, ref Vector3 upwards, out Quaternion value );

      public static Quaternion Slerp( Quaternion a, Quaternion b, float t )
      {
         INTERNAL_CALL_Slerp( ref a, ref b, t, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_Slerp( ref Quaternion a, ref Quaternion b, float t, out Quaternion value );

      public static Quaternion SlerpUnclamped( Quaternion a, Quaternion b, float t )
      {
         INTERNAL_CALL_SlerpUnclamped( ref a, ref b, t, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_SlerpUnclamped( ref Quaternion a, ref Quaternion b, float t, out Quaternion value );

      public static Quaternion Lerp( Quaternion a, Quaternion b, float t )
      {
         INTERNAL_CALL_Lerp( ref a, ref b, t, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_Lerp( ref Quaternion a, ref Quaternion b, float t, out Quaternion value );

      public static Quaternion LerpUnclamped( Quaternion a, Quaternion b, float t )
      {
         INTERNAL_CALL_LerpUnclamped( ref a, ref b, t, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_LerpUnclamped( ref Quaternion a, ref Quaternion b, float t, out Quaternion value );

      public static Quaternion RotateTowards( Quaternion from, Quaternion to, float maxDegreesDelta ) => throw new NotImplementedException();

      public static Quaternion Inverse( Quaternion rotation )
      {
         INTERNAL_CALL_Inverse( ref rotation, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_Inverse( ref Quaternion rotation, out Quaternion value );

      public static Quaternion Euler( float x, float y, float z )
      {
         return Internal_FromEulerRad( new Vector3( x, y, z ) * ( (float)Math.PI / 180f ) );
      }

      public static Quaternion Euler( Vector3 euler )
      {
         return Internal_FromEulerRad( euler * ( (float)Math.PI / 180f ) );
      }

      private static Vector3 Internal_ToEulerRad( Quaternion rotation )
      {
         INTERNAL_CALL_Internal_ToEulerRad( ref rotation, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_Internal_ToEulerRad( ref Quaternion rotation, out Vector3 value );

      private static Quaternion Internal_FromEulerRad( Vector3 euler )
      {
         INTERNAL_CALL_Internal_FromEulerRad( ref euler, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_Internal_FromEulerRad( ref Vector3 euler, out Quaternion value );

      private static void Internal_ToAxisAngleRad( Quaternion q, out Vector3 axis, out float angle )
      {
         INTERNAL_CALL_Internal_ToAxisAngleRad( ref q, out axis, out angle );
      }

      private static extern void INTERNAL_CALL_Internal_ToAxisAngleRad( ref Quaternion q, out Vector3 axis, out float angle );

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Quaternion EulerRotation( float x, float y, float z )
      {
         return Internal_FromEulerRad( new Vector3( x, y, z ) );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Quaternion EulerRotation( Vector3 euler )
      {
         return Internal_FromEulerRad( euler );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public void SetEulerRotation( float x, float y, float z )
      {
         this = Internal_FromEulerRad( new Vector3( x, y, z ) );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public void SetEulerRotation( Vector3 euler )
      {
         this = Internal_FromEulerRad( euler );
      }

      [Obsolete( "Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees" )]
      public Vector3 ToEuler()
      {
         return Internal_ToEulerRad( this );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Quaternion EulerAngles( float x, float y, float z )
      {
         return Internal_FromEulerRad( new Vector3( x, y, z ) );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Quaternion EulerAngles( Vector3 euler )
      {
         return Internal_FromEulerRad( euler );
      }

      [Obsolete( "Use Quaternion.ToAngleAxis instead. This function was deprecated because it uses radians instead of degrees" )]
      public void ToAxisAngle( out Vector3 axis, out float angle )
      {
         Internal_ToAxisAngleRad( this, out axis, out angle );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public void SetEulerAngles( float x, float y, float z )
      {
         SetEulerRotation( new Vector3( x, y, z ) );
      }

      [Obsolete( "Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees" )]
      public void SetEulerAngles( Vector3 euler )
      {
         this = EulerRotation( euler );
      }

      [Obsolete( "Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Vector3 ToEulerAngles( Quaternion rotation )
      {
         return Internal_ToEulerRad( rotation );
      }

      [Obsolete( "Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees" )]
      public Vector3 ToEulerAngles()
      {
         return Internal_ToEulerRad( this );
      }

      [Obsolete( "Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees" )]
      public static Quaternion AxisAngle( Vector3 axis, float angle )
      {
         INTERNAL_CALL_AxisAngle( ref axis, angle, out Quaternion value );
         return value;
      }

      private static extern void INTERNAL_CALL_AxisAngle( ref Vector3 axis, float angle, out Quaternion value );

      [Obsolete( "Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees" )]
      public void SetAxisAngle( Vector3 axis, float angle )
      {
         this = AxisAngle( axis, angle );
      }

      public void Set( float new_x, float new_y, float new_z, float new_w )
      {
         x = new_x;
         y = new_y;
         z = new_z;
         w = new_w;
      }

      public static Quaternion operator *( Quaternion lhs, Quaternion rhs )
      {
         return new Quaternion( lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z );
      }

      public static Vector3 operator *( Quaternion rotation, Vector3 point )
      {
         float num = rotation.x * 2f;
         float num2 = rotation.y * 2f;
         float num3 = rotation.z * 2f;
         float num4 = rotation.x * num;
         float num5 = rotation.y * num2;
         float num6 = rotation.z * num3;
         float num7 = rotation.x * num2;
         float num8 = rotation.x * num3;
         float num9 = rotation.y * num3;
         float num10 = rotation.w * num;
         float num11 = rotation.w * num2;
         float num12 = rotation.w * num3;
         Vector3 result = default( Vector3 );
         result.x = ( 1f - ( num5 + num6 ) ) * point.x + ( num7 - num12 ) * point.y + ( num8 + num11 ) * point.z;
         result.y = ( num7 + num12 ) * point.x + ( 1f - ( num4 + num6 ) ) * point.y + ( num9 - num10 ) * point.z;
         result.z = ( num8 - num11 ) * point.x + ( num9 + num10 ) * point.y + ( 1f - ( num4 + num5 ) ) * point.z;
         return result;
      }

      public static bool operator ==( Quaternion lhs, Quaternion rhs )
      {
         return Dot( lhs, rhs ) > 0.999999f;
      }

      public static bool operator !=( Quaternion lhs, Quaternion rhs )
      {
         return !( lhs == rhs );
      }

      public static float Dot( Quaternion a, Quaternion b )
      {
         return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
      }

      public void SetLookRotation( Vector3 view )
      {
         Vector3 up = Vector3.up;
         SetLookRotation( view, up );
      }

      public void SetLookRotation( Vector3 view, Vector3 up ) => throw new NotImplementedException();

      public static float Angle( Quaternion a, Quaternion b ) => throw new NotImplementedException();

      private static Vector3 Internal_MakePositive( Vector3 euler )
      {
         float num = -0.005729578f;
         float num2 = 360f + num;
         if( euler.x < num )
         {
            euler.x += 360f;
         }
         else if( euler.x > num2 )
         {
            euler.x -= 360f;
         }

         if( euler.y < num )
         {
            euler.y += 360f;
         }
         else if( euler.y > num2 )
         {
            euler.y -= 360f;
         }

         if( euler.z < num )
         {
            euler.z += 360f;
         }
         else if( euler.z > num2 )
         {
            euler.z -= 360f;
         }

         return euler;
      }

      public override int GetHashCode()
      {
         return x.GetHashCode() ^ ( y.GetHashCode() << 2 ) ^ ( z.GetHashCode() >> 2 ) ^ ( w.GetHashCode() >> 1 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Quaternion ) )
         {
            return false;
         }

         Quaternion quaternion = (Quaternion)other;
         return x.Equals( quaternion.x ) && y.Equals( quaternion.y ) && z.Equals( quaternion.z ) && w.Equals( quaternion.w );
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();
   }
}
