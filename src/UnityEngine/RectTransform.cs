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
   public sealed class RectTransform : Transform
   {
      public delegate void ReapplyDrivenProperties( RectTransform driven );

      public enum Edge
      {
         Left,
         Right,
         Top,
         Bottom
      }

      public enum Axis
      {
         Horizontal,
         Vertical
      }

      public Rect rect
      {
         get
         {
            INTERNAL_get_rect( out Rect value );
            return value;
         }
      }

      public Vector2 anchorMin
      {
         get
         {
            INTERNAL_get_anchorMin( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_anchorMin( ref value );
         }
      }

      public Vector2 anchorMax
      {
         get
         {
            INTERNAL_get_anchorMax( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_anchorMax( ref value );
         }
      }

      public Vector3 anchoredPosition3D
      {
         get
         {
            Vector2 anchoredPosition = this.anchoredPosition;
            float x = anchoredPosition.x;
            float y = anchoredPosition.y;
            Vector3 localPosition = base.localPosition;
            return new Vector3( x, y, localPosition.z );
         }
         set
         {
            anchoredPosition = new Vector2( value.x, value.y );
            Vector3 localPosition = base.localPosition;
            localPosition.z = value.z;
            base.localPosition = localPosition;
         }
      }

      public Vector2 anchoredPosition
      {
         get
         {
            INTERNAL_get_anchoredPosition( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_anchoredPosition( ref value );
         }
      }

      public Vector2 sizeDelta
      {
         get
         {
            INTERNAL_get_sizeDelta( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_sizeDelta( ref value );
         }
      }

      public Vector2 pivot
      {
         get
         {
            INTERNAL_get_pivot( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_pivot( ref value );
         }
      }

      internal Object drivenByObject
      {
   
   
         get;
   
   
         set;
      }

      public Vector2 offsetMin
      {
         get
         {
            return anchoredPosition - Vector2.Scale( sizeDelta, pivot );
         }
         set
         {
            Vector2 vector = value - ( anchoredPosition - Vector2.Scale( sizeDelta, pivot ) );
            sizeDelta -= vector;
            anchoredPosition += Vector2.Scale( vector, Vector2.one - pivot );
         }
      }

      public Vector2 offsetMax
      {
         get
         {
            return anchoredPosition + Vector2.Scale( sizeDelta, Vector2.one - pivot );
         }
         set
         {
            Vector2 vector = value - ( anchoredPosition + Vector2.Scale( sizeDelta, Vector2.one - pivot ) );
            sizeDelta += vector;
            anchoredPosition += Vector2.Scale( vector, pivot );
         }
      }

      public static event ReapplyDrivenProperties reapplyDrivenProperties;



      private extern void INTERNAL_get_rect( out Rect value );



      private extern void INTERNAL_get_anchorMin( out Vector2 value );



      private extern void INTERNAL_set_anchorMin( ref Vector2 value );



      private extern void INTERNAL_get_anchorMax( out Vector2 value );



      private extern void INTERNAL_set_anchorMax( ref Vector2 value );



      private extern void INTERNAL_get_anchoredPosition( out Vector2 value );



      private extern void INTERNAL_set_anchoredPosition( ref Vector2 value );



      private extern void INTERNAL_get_sizeDelta( out Vector2 value );



      private extern void INTERNAL_set_sizeDelta( ref Vector2 value );



      private extern void INTERNAL_get_pivot( out Vector2 value );



      private extern void INTERNAL_set_pivot( ref Vector2 value );

      internal static void SendReapplyDrivenProperties( RectTransform driven )
      {
         if( RectTransform.reapplyDrivenProperties != null )
         {
            RectTransform.reapplyDrivenProperties( driven );
         }
      }

      public void GetLocalCorners( Vector3[] fourCornersArray )
      {
         if( fourCornersArray == null || fourCornersArray.Length < 4 )
         {
            return;
         }

         Rect rect = this.rect;
         float x = rect.x;
         float y = rect.y;
         float xMax = rect.xMax;
         float yMax = rect.yMax;
         ref Vector3 reference = ref fourCornersArray[ 0 ];
         reference = new Vector3( x, y, 0f );
         ref Vector3 reference2 = ref fourCornersArray[ 1 ];
         reference2 = new Vector3( x, yMax, 0f );
         ref Vector3 reference3 = ref fourCornersArray[ 2 ];
         reference3 = new Vector3( xMax, yMax, 0f );
         ref Vector3 reference4 = ref fourCornersArray[ 3 ];
         reference4 = new Vector3( xMax, y, 0f );
      }

      public void GetWorldCorners( Vector3[] fourCornersArray )
      {
         if( fourCornersArray == null || fourCornersArray.Length < 4 )
         {
            return;
         }

         GetLocalCorners( fourCornersArray );
         Transform transform = base.transform;
         for( int i = 0 ; i < 4 ; i++ )
         {
            ref Vector3 reference = ref fourCornersArray[ i ];
            reference = transform.TransformPoint( fourCornersArray[ i ] );
         }
      }

      internal Rect GetRectInParentSpace()
      {
         Rect rect = this.rect;
         Vector2 vector = offsetMin + Vector2.Scale( pivot, rect.size );
         Transform parent = base.transform.parent;
         if( (bool)parent )
         {
            RectTransform component = parent.GetComponent<RectTransform>();
            if( (bool)component )
            {
               vector += Vector2.Scale( anchorMin, component.rect.size );
            }
         }

         rect.x += vector.x;
         rect.y += vector.y;
         return rect;
      }

      public void SetInsetAndSizeFromParentEdge( Edge edge, float inset, float size )
      {
         int index = ( edge == Edge.Top || edge == Edge.Bottom ) ? 1 : 0;
         bool flag = edge == Edge.Top || edge == Edge.Right;
         float value = flag ? 1 : 0;
         Vector2 anchorMin = this.anchorMin;
         anchorMin[ index ] = value;
         this.anchorMin = anchorMin;
         anchorMin = anchorMax;
         anchorMin[ index ] = value;
         anchorMax = anchorMin;
         Vector2 sizeDelta = this.sizeDelta;
         sizeDelta[ index ] = size;
         this.sizeDelta = sizeDelta;
         Vector2 anchoredPosition = this.anchoredPosition;
         anchoredPosition[ index ] = ( ( !flag ) ? ( inset + size * pivot[ index ] ) : ( 0f - inset - size * ( 1f - pivot[ index ] ) ) );
         this.anchoredPosition = anchoredPosition;
      }

      public void SetSizeWithCurrentAnchors( Axis axis, float size )
      {
         Vector2 sizeDelta = this.sizeDelta;
         sizeDelta[ (int)axis ] = size - GetParentSize()[ (int)axis ] * ( anchorMax[ (int)axis ] - anchorMin[ (int)axis ] );
         this.sizeDelta = sizeDelta;
      }

      private Vector2 GetParentSize()
      {
         RectTransform rectTransform = base.parent as RectTransform;
         if( !rectTransform )
         {
            return Vector2.zero;
         }

         return rectTransform.rect.size;
      }
   }
}
