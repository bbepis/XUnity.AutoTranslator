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
   public struct Rect
   {
      private float m_XMin;

      private float m_YMin;

      private float m_Width;

      private float m_Height;

      public static Rect zero => new Rect( 0f, 0f, 0f, 0f );

      public float x
      {
         get
         {
            return m_XMin;
         }
         set
         {
            m_XMin = value;
         }
      }

      public float y
      {
         get
         {
            return m_YMin;
         }
         set
         {
            m_YMin = value;
         }
      }

      public Vector2 position
      {
         get
         {
            return new Vector2( m_XMin, m_YMin );
         }
         set
         {
            m_XMin = value.x;
            m_YMin = value.y;
         }
      }

      public Vector2 center
      {
         get
         {
            return new Vector2( x + m_Width / 2f, y + m_Height / 2f );
         }
         set
         {
            m_XMin = value.x - m_Width / 2f;
            m_YMin = value.y - m_Height / 2f;
         }
      }

      public Vector2 min
      {
         get
         {
            return new Vector2( xMin, yMin );
         }
         set
         {
            xMin = value.x;
            yMin = value.y;
         }
      }

      public Vector2 max
      {
         get
         {
            return new Vector2( xMax, yMax );
         }
         set
         {
            xMax = value.x;
            yMax = value.y;
         }
      }

      public float width
      {
         get
         {
            return m_Width;
         }
         set
         {
            m_Width = value;
         }
      }

      public float height
      {
         get
         {
            return m_Height;
         }
         set
         {
            m_Height = value;
         }
      }

      public Vector2 size
      {
         get
         {
            return new Vector2( m_Width, m_Height );
         }
         set
         {
            m_Width = value.x;
            m_Height = value.y;
         }
      }

      public float xMin
      {
         get
         {
            return m_XMin;
         }
         set
         {
            float xMax = this.xMax;
            m_XMin = value;
            m_Width = xMax - m_XMin;
         }
      }

      public float yMin
      {
         get
         {
            return m_YMin;
         }
         set
         {
            float yMax = this.yMax;
            m_YMin = value;
            m_Height = yMax - m_YMin;
         }
      }

      public float xMax
      {
         get
         {
            return m_Width + m_XMin;
         }
         set
         {
            m_Width = value - m_XMin;
         }
      }

      public float yMax
      {
         get
         {
            return m_Height + m_YMin;
         }
         set
         {
            m_Height = value - m_YMin;
         }
      }

      [Obsolete( "use xMin" )]
      public float left => m_XMin;

      [Obsolete( "use xMax" )]
      public float right => m_XMin + m_Width;

      [Obsolete( "use yMin" )]
      public float top => m_YMin;

      [Obsolete( "use yMax" )]
      public float bottom => m_YMin + m_Height;

      public Rect( float x, float y, float width, float height )
      {
         m_XMin = x;
         m_YMin = y;
         m_Width = width;
         m_Height = height;
      }

      public Rect( Vector2 position, Vector2 size )
      {
         m_XMin = position.x;
         m_YMin = position.y;
         m_Width = size.x;
         m_Height = size.y;
      }

      public Rect( Rect source )
      {
         m_XMin = source.m_XMin;
         m_YMin = source.m_YMin;
         m_Width = source.m_Width;
         m_Height = source.m_Height;
      }

      public static Rect MinMaxRect( float xmin, float ymin, float xmax, float ymax )
      {
         return new Rect( xmin, ymin, xmax - xmin, ymax - ymin );
      }

      public void Set( float x, float y, float width, float height )
      {
         m_XMin = x;
         m_YMin = y;
         m_Width = width;
         m_Height = height;
      }

      public bool Contains( Vector2 point )
      {
         return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
      }

      public bool Contains( Vector3 point )
      {
         return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
      }

      public bool Contains( Vector3 point, bool allowInverse )
      {
         if( !allowInverse )
         {
            return Contains( point );
         }

         bool flag = false;
         if( ( width < 0f && point.x <= xMin && point.x > xMax ) || ( width >= 0f && point.x >= xMin && point.x < xMax ) )
         {
            flag = true;
         }

         if( flag && ( ( height < 0f && point.y <= yMin && point.y > yMax ) || ( height >= 0f && point.y >= yMin && point.y < yMax ) ) )
         {
            return true;
         }

         return false;
      }

      private static Rect OrderMinMax( Rect rect )
      {
         if( rect.xMin > rect.xMax )
         {
            float xMin = rect.xMin;
            rect.xMin = rect.xMax;
            rect.xMax = xMin;
         }

         if( rect.yMin > rect.yMax )
         {
            float yMin = rect.yMin;
            rect.yMin = rect.yMax;
            rect.yMax = yMin;
         }

         return rect;
      }

      public bool Overlaps( Rect other )
      {
         return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
      }

      public bool Overlaps( Rect other, bool allowInverse )
      {
         Rect rect = this;
         if( allowInverse )
         {
            rect = OrderMinMax( rect );
            other = OrderMinMax( other );
         }

         return rect.Overlaps( other );
      }

      public static Vector2 NormalizedToPoint( Rect rectangle, Vector2 normalizedRectCoordinates ) => throw new NotImplementedException();

      public static Vector2 PointToNormalized( Rect rectangle, Vector2 point ) => throw new NotImplementedException();

      public static bool operator !=( Rect lhs, Rect rhs )
      {
         return !( lhs == rhs );
      }

      public static bool operator ==( Rect lhs, Rect rhs )
      {
         return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
      }

      public override int GetHashCode()
      {
         return x.GetHashCode() ^ ( width.GetHashCode() << 2 ) ^ ( y.GetHashCode() >> 2 ) ^ ( height.GetHashCode() >> 1 );
      }

      public override bool Equals( object other )
      {
         if( !( other is Rect ) )
         {
            return false;
         }

         Rect rect = (Rect)other;
         return x.Equals( rect.x ) && y.Equals( rect.y ) && width.Equals( rect.width ) && height.Equals( rect.height );
      }

      public override string ToString() => throw new NotImplementedException();

      public string ToString( string format ) => throw new NotImplementedException();
   }
}
