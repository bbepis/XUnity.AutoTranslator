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
   [Serializable]
   [StructLayout( LayoutKind.Sequential )]
   public sealed class RectOffset
   {
      [NonSerialized]
      internal IntPtr m_Ptr;

      private readonly object m_SourceStyle;

      public int left
      {


         get;


         set;
      }

      public int right
      {


         get;


         set;
      }

      public int top
      {


         get;


         set;
      }

      public int bottom
      {


         get;


         set;
      }

      public int horizontal
      {


         get;
      }

      public int vertical
      {


         get;
      }

      public RectOffset()
      {
         Init();
      }

      internal RectOffset( object sourceStyle, IntPtr source )
      {
         m_SourceStyle = sourceStyle;
         m_Ptr = source;
      }

      public RectOffset( int left, int right, int top, int bottom )
      {
         Init();
         this.left = left;
         this.right = right;
         this.top = top;
         this.bottom = bottom;
      }




      private extern void Init();




      private extern void Cleanup();

      public Rect Add( Rect rect )
      {
         INTERNAL_CALL_Add( this, ref rect, out Rect value );
         return value;
      }



      private static extern void INTERNAL_CALL_Add( RectOffset self, ref Rect rect, out Rect value );

      public Rect Remove( Rect rect )
      {
         INTERNAL_CALL_Remove( this, ref rect, out Rect value );
         return value;
      }



      private static extern void INTERNAL_CALL_Remove( RectOffset self, ref Rect rect, out Rect value );

      ~RectOffset()
      {
         if( m_SourceStyle == null )
         {
            Cleanup();
         }
      }

      public override string ToString() => throw new NotImplementedException();
   }
}
