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
   public sealed class GUISettings
   {
      private bool m_DoubleClickSelectsWord = true;

      private bool m_TripleClickSelectsLine = true;

      private Color m_CursorColor = Color.white;

      private float m_CursorFlashSpeed = -1f;

      private Color m_SelectionColor = new Color( 0.5f, 0.5f, 1f );

      public bool doubleClickSelectsWord
      {
         get
         {
            return m_DoubleClickSelectsWord;
         }
         set
         {
            m_DoubleClickSelectsWord = value;
         }
      }

      public bool tripleClickSelectsLine
      {
         get
         {
            return m_TripleClickSelectsLine;
         }
         set
         {
            m_TripleClickSelectsLine = value;
         }
      }

      public Color cursorColor
      {
         get
         {
            return m_CursorColor;
         }
         set
         {
            m_CursorColor = value;
         }
      }

      public float cursorFlashSpeed
      {
         get
         {
            if( m_CursorFlashSpeed >= 0f )
            {
               return m_CursorFlashSpeed;
            }

            return Internal_GetCursorFlashSpeed();
         }
         set
         {
            m_CursorFlashSpeed = value;
         }
      }

      public Color selectionColor
      {
         get
         {
            return m_SelectionColor;
         }
         set
         {
            m_SelectionColor = value;
         }
      }

      private static extern float Internal_GetCursorFlashSpeed();
   }
}
