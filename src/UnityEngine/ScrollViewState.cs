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
   internal class ScrollViewState
   {
      public Rect position;

      public Rect visibleRect;

      public Rect viewRect;

      public Vector2 scrollPosition;

      public bool apply;

      public ScrollViewState()
      {
      }

      public void ScrollTo( Rect pos )
      {
         ScrollTowards( pos, float.PositiveInfinity );
      }

      public bool ScrollTowards( Rect pos, float maxDelta )
      {
         Vector2 vector = ScrollNeeded( pos );
         if( vector.sqrMagnitude < 0.0001f )
         {
            return false;
         }

         if( maxDelta == 0f )
         {
            return true;
         }

         if( vector.magnitude > maxDelta )
         {
            vector = vector.normalized * maxDelta;
         }

         scrollPosition += vector;
         apply = true;
         return true;
      }

      private Vector2 ScrollNeeded( Rect pos )
      {
         Rect rect = visibleRect;
         rect.x += scrollPosition.x;
         rect.y += scrollPosition.y;
         float num = pos.width - visibleRect.width;
         if( num > 0f )
         {
            pos.width -= num;
            pos.x += num * 0.5f;
         }

         num = pos.height - visibleRect.height;
         if( num > 0f )
         {
            pos.height -= num;
            pos.y += num * 0.5f;
         }

         Vector2 zero = Vector2.zero;
         if( pos.xMax > rect.xMax )
         {
            zero.x += pos.xMax - rect.xMax;
         }
         else if( pos.xMin < rect.xMin )
         {
            zero.x -= rect.xMin - pos.xMin;
         }

         if( pos.yMax > rect.yMax )
         {
            zero.y += pos.yMax - rect.yMax;
         }
         else if( pos.yMin < rect.yMin )
         {
            zero.y -= rect.yMin - pos.yMin;
         }

         Rect rect2 = viewRect;
         return zero;
      }
   }
}
