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
   public class Transform : Component, IEnumerable
   {
      private sealed class Enumerator : IEnumerator
      {
         private Transform outer;

         private int currentIndex = -1;

         public object Current => outer.GetChild( currentIndex );

         internal Enumerator( Transform outer )
         {
            this.outer = outer;
         }

         public bool MoveNext()
         {
            int childCount = outer.childCount;
            return ++currentIndex < childCount;
         }

         public void Reset()
         {
            currentIndex = -1;
         }
      }

      public Vector3 position
      {
         get
         {
            INTERNAL_get_position( out Vector3 value );
            return value;
         }
         set
         {
            INTERNAL_set_position( ref value );
         }
      }

      public Vector3 localPosition
      {
         get
         {
            INTERNAL_get_localPosition( out Vector3 value );
            return value;
         }
         set
         {
            INTERNAL_set_localPosition( ref value );
         }
      }

      public Vector3 eulerAngles
      {
         get
         {
            return rotation.eulerAngles;
         }
         set
         {
            rotation = Quaternion.Euler( value );
         }
      }

      public Vector3 localEulerAngles
      {
         get
         {
            return localRotation.eulerAngles;
         }
         set
         {
            localRotation = Quaternion.Euler( value );
         }
      }

      public Vector3 right
      {
         get
         {
            return rotation * Vector3.right;
         }
         set
         {
            rotation = Quaternion.FromToRotation( Vector3.right, value );
         }
      }

      public Vector3 up
      {
         get
         {
            return rotation * Vector3.up;
         }
         set
         {
            rotation = Quaternion.FromToRotation( Vector3.up, value );
         }
      }

      public Vector3 forward
      {
         get
         {
            return rotation * Vector3.forward;
         }
         set
         {
            rotation = Quaternion.LookRotation( value );
         }
      }

      public Quaternion rotation
      {
         get
         {
            INTERNAL_get_rotation( out Quaternion value );
            return value;
         }
         set
         {
            INTERNAL_set_rotation( ref value );
         }
      }

      public Quaternion localRotation
      {
         get
         {
            INTERNAL_get_localRotation( out Quaternion value );
            return value;
         }
         set
         {
            INTERNAL_set_localRotation( ref value );
         }
      }

      public Vector3 localScale
      {
         get
         {
            INTERNAL_get_localScale( out Vector3 value );
            return value;
         }
         set
         {
            INTERNAL_set_localScale( ref value );
         }
      }

      public Transform parent
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public Matrix4x4 worldToLocalMatrix
      {
         get
         {
            INTERNAL_get_worldToLocalMatrix( out Matrix4x4 value );
            return value;
         }
      }

      public Matrix4x4 localToWorldMatrix
      {
         get
         {
            INTERNAL_get_localToWorldMatrix( out Matrix4x4 value );
            return value;
         }
      }

      public Transform root => throw new NotImplementedException();

      public int childCount => throw new NotImplementedException();

      public Vector3 lossyScale
      {
         get
         {
            INTERNAL_get_lossyScale( out Vector3 value );
            return value;
         }
      }

      public bool hasChanged
      {
         get;
         set;
      }

      public int hierarchyCapacity
      {
         get;
         set;
      }

      public int hierarchyCount
      {
         get;
      }

      protected Transform() : base( IntPtr.Zero ) => throw new NotImplementedException();

      private extern void INTERNAL_get_position( out Vector3 value );

      private extern void INTERNAL_set_position( ref Vector3 value );

      private extern void INTERNAL_get_localPosition( out Vector3 value );

      private extern void INTERNAL_set_localPosition( ref Vector3 value );

      private extern void INTERNAL_get_rotation( out Quaternion value );

      private extern void INTERNAL_set_rotation( ref Quaternion value );

      private extern void INTERNAL_get_localRotation( out Quaternion value );

      private extern void INTERNAL_set_localRotation( ref Quaternion value );

      private extern void INTERNAL_get_localScale( out Vector3 value );

      private extern void INTERNAL_set_localScale( ref Vector3 value );

      public void SetParent( Transform parent )
      {
         SetParent( parent, worldPositionStays: true );
      }

      public extern void SetParent( Transform parent, bool worldPositionStays );

      private extern void INTERNAL_get_worldToLocalMatrix( out Matrix4x4 value );

      private extern void INTERNAL_get_localToWorldMatrix( out Matrix4x4 value );

      public void SetPositionAndRotation( Vector3 position, Quaternion rotation )
      {
         INTERNAL_CALL_SetPositionAndRotation( this, ref position, ref rotation );
      }

      private static extern void INTERNAL_CALL_SetPositionAndRotation( Transform self, ref Vector3 position, ref Quaternion rotation );

      public void Translate( Vector3 translation ) => throw new NotImplementedException();

      public void Translate( Vector3 translation, Space relativeTo ) => throw new NotImplementedException();

      public void Translate( float x, float y, float z ) => throw new NotImplementedException();

      public void Translate( float x, float y, float z, Space relativeTo ) => throw new NotImplementedException();

      public void Translate( Vector3 translation, Transform relativeTo )
      {
         if( (bool)relativeTo )
         {
            position += relativeTo.TransformDirection( translation );
         }
         else
         {
            position += translation;
         }
      }

      public void Translate( float x, float y, float z, Transform relativeTo )
      {
         Translate( new Vector3( x, y, z ), relativeTo );
      }

      public void Rotate( Vector3 eulerAngles ) => throw new NotImplementedException();

      public void Rotate( Vector3 eulerAngles, Space relativeTo ) => throw new NotImplementedException();

      public void Rotate( float xAngle, float yAngle, float zAngle ) => throw new NotImplementedException();

      public void Rotate( float xAngle, float yAngle, float zAngle, Space relativeTo ) => throw new NotImplementedException();

      internal void RotateAroundInternal( Vector3 axis, float angle )
      {
         INTERNAL_CALL_RotateAroundInternal( this, ref axis, angle );
      }

      private static extern void INTERNAL_CALL_RotateAroundInternal( Transform self, ref Vector3 axis, float angle );

      public void Rotate( Vector3 axis, float angle ) => throw new NotImplementedException();

      public void Rotate( Vector3 axis, float angle, Space relativeTo ) => throw new NotImplementedException();

      public void RotateAround( Vector3 point, Vector3 axis, float angle )
      {
         Vector3 position = this.position;
         Quaternion rotation = Quaternion.AngleAxis( angle, axis );
         Vector3 point2 = position - point;
         point2 = rotation * point2;
         position = ( this.position = point + point2 );
         RotateAroundInternal( axis, angle * ( (float)Math.PI / 180f ) );
      }

      public void LookAt( Transform target )
      {
         Vector3 up = Vector3.up;
         LookAt( target, up );
      }

      public void LookAt( Transform target, Vector3 worldUp )
      {
         if( (bool)target )
         {
            LookAt( target.position, worldUp );
         }
      }

      public void LookAt( Vector3 worldPosition, Vector3 worldUp )
      {
         INTERNAL_CALL_LookAt( this, ref worldPosition, ref worldUp );
      }

      public void LookAt( Vector3 worldPosition )
      {
         Vector3 worldUp = Vector3.up;
         INTERNAL_CALL_LookAt( this, ref worldPosition, ref worldUp );
      }

      private static extern void INTERNAL_CALL_LookAt( Transform self, ref Vector3 worldPosition, ref Vector3 worldUp );

      public Vector3 TransformDirection( Vector3 direction )
      {
         INTERNAL_CALL_TransformDirection( this, ref direction, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_TransformDirection( Transform self, ref Vector3 direction, out Vector3 value );

      public Vector3 TransformDirection( float x, float y, float z )
      {
         return TransformDirection( new Vector3( x, y, z ) );
      }

      public Vector3 InverseTransformDirection( Vector3 direction )
      {
         INTERNAL_CALL_InverseTransformDirection( this, ref direction, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_InverseTransformDirection( Transform self, ref Vector3 direction, out Vector3 value );

      public Vector3 InverseTransformDirection( float x, float y, float z )
      {
         return InverseTransformDirection( new Vector3( x, y, z ) );
      }

      public Vector3 TransformVector( Vector3 vector )
      {
         INTERNAL_CALL_TransformVector( this, ref vector, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_TransformVector( Transform self, ref Vector3 vector, out Vector3 value );

      public Vector3 TransformVector( float x, float y, float z )
      {
         return TransformVector( new Vector3( x, y, z ) );
      }

      public Vector3 InverseTransformVector( Vector3 vector )
      {
         INTERNAL_CALL_InverseTransformVector( this, ref vector, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_InverseTransformVector( Transform self, ref Vector3 vector, out Vector3 value );

      public Vector3 InverseTransformVector( float x, float y, float z )
      {
         return InverseTransformVector( new Vector3( x, y, z ) );
      }

      public Vector3 TransformPoint( Vector3 position )
      {
         INTERNAL_CALL_TransformPoint( this, ref position, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_TransformPoint( Transform self, ref Vector3 position, out Vector3 value );

      public Vector3 TransformPoint( float x, float y, float z )
      {
         return TransformPoint( new Vector3( x, y, z ) );
      }

      public Vector3 InverseTransformPoint( Vector3 position )
      {
         INTERNAL_CALL_InverseTransformPoint( this, ref position, out Vector3 value );
         return value;
      }

      private static extern void INTERNAL_CALL_InverseTransformPoint( Transform self, ref Vector3 position, out Vector3 value );

      public Vector3 InverseTransformPoint( float x, float y, float z )
      {
         return InverseTransformPoint( new Vector3( x, y, z ) );
      }

      public extern void DetachChildren();

      public extern void SetAsFirstSibling();

      public extern void SetAsLastSibling();

      public extern void SetSiblingIndex( int index );

      public extern int GetSiblingIndex();

      public extern Transform Find( string name );

      private extern void INTERNAL_get_lossyScale( out Vector3 value );

      public extern bool IsChildOf( Transform parent );

      [Obsolete( "FindChild has been deprecated. Use Find instead (UnityUpgradable) -> Find([mscorlib] System.String)", false )]
      public Transform FindChild( string name )
      {
         return Find( name );
      }

      public IEnumerator GetEnumerator()
      {
         return new Enumerator( this );
      }

      [Obsolete( "use Transform.Rotate instead." )]
      public void RotateAround( Vector3 axis, float angle )
      {
         INTERNAL_CALL_RotateAround( this, ref axis, angle );
      }

      private static extern void INTERNAL_CALL_RotateAround( Transform self, ref Vector3 axis, float angle );

      [Obsolete( "use Transform.Rotate instead." )]
      public void RotateAroundLocal( Vector3 axis, float angle )
      {
         INTERNAL_CALL_RotateAroundLocal( this, ref axis, angle );
      }

      private static extern void INTERNAL_CALL_RotateAroundLocal( Transform self, ref Vector3 axis, float angle );

      public extern Transform GetChild( int index );

      public extern int GetChildCount();
   }
}
