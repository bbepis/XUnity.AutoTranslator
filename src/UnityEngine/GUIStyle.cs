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
   public sealed class GUIStyle
   {
      public IntPtr m_Ptr { get; set; }

      [NonSerialized]
      private GUIStyleState m_Normal;

      [NonSerialized]
      private GUIStyleState m_Hover;

      [NonSerialized]
      private GUIStyleState m_Active;

      [NonSerialized]
      private GUIStyleState m_Focused;

      [NonSerialized]
      private GUIStyleState m_OnNormal;

      [NonSerialized]
      private GUIStyleState m_OnHover;

      [NonSerialized]
      private GUIStyleState m_OnActive;

      [NonSerialized]
      private GUIStyleState m_OnFocused;

      [NonSerialized]
      private RectOffset m_Border;

      [NonSerialized]
      private RectOffset m_Padding;

      [NonSerialized]
      private RectOffset m_Margin;

      [NonSerialized]
      private RectOffset m_Overflow;

      [NonSerialized]
      private Font m_FontInternal;

      internal static bool showKeyboardFocus = true;

      private static GUIStyle s_None;

      public GUIStyleState normal
      {
         get
         {
            if( m_Normal == null )
            {
               m_Normal = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 0 ) );
            }

            return m_Normal;
         }
         set
         {
            AssignStyleState( 0, value.m_Ptr );
         }
      }

      public GUIStyleState hover
      {
         get
         {
            if( m_Hover == null )
            {
               m_Hover = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 1 ) );
            }

            return m_Hover;
         }
         set
         {
            AssignStyleState( 1, value.m_Ptr );
         }
      }

      public GUIStyleState active
      {
         get
         {
            if( m_Active == null )
            {
               m_Active = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 2 ) );
            }

            return m_Active;
         }
         set
         {
            AssignStyleState( 2, value.m_Ptr );
         }
      }

      public GUIStyleState onNormal
      {
         get
         {
            if( m_OnNormal == null )
            {
               m_OnNormal = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 4 ) );
            }

            return m_OnNormal;
         }
         set
         {
            AssignStyleState( 4, value.m_Ptr );
         }
      }

      public GUIStyleState onHover
      {
         get
         {
            if( m_OnHover == null )
            {
               m_OnHover = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 5 ) );
            }

            return m_OnHover;
         }
         set
         {
            AssignStyleState( 5, value.m_Ptr );
         }
      }

      public GUIStyleState onActive
      {
         get
         {
            if( m_OnActive == null )
            {
               m_OnActive = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 6 ) );
            }

            return m_OnActive;
         }
         set
         {
            AssignStyleState( 6, value.m_Ptr );
         }
      }

      public GUIStyleState focused
      {
         get
         {
            if( m_Focused == null )
            {
               m_Focused = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 3 ) );
            }

            return m_Focused;
         }
         set
         {
            AssignStyleState( 3, value.m_Ptr );
         }
      }

      public GUIStyleState onFocused
      {
         get
         {
            if( m_OnFocused == null )
            {
               m_OnFocused = GUIStyleState.GetGUIStyleState( this, GetStyleStatePtr( 7 ) );
            }

            return m_OnFocused;
         }
         set
         {
            AssignStyleState( 7, value.m_Ptr );
         }
      }

      public RectOffset border
      {
         get
         {
            if( m_Border == null )
            {
               m_Border = new RectOffset( this, GetRectOffsetPtr( 0 ) );
            }

            return m_Border;
         }
         set
         {
            AssignRectOffset( 0, value.m_Ptr );
         }
      }

      public RectOffset margin
      {
         get
         {
            if( m_Margin == null )
            {
               m_Margin = new RectOffset( this, GetRectOffsetPtr( 1 ) );
            }

            return m_Margin;
         }
         set
         {
            AssignRectOffset( 1, value.m_Ptr );
         }
      }

      public RectOffset padding
      {
         get
         {
            if( m_Padding == null )
            {
               m_Padding = new RectOffset( this, GetRectOffsetPtr( 2 ) );
            }

            return m_Padding;
         }
         set
         {
            AssignRectOffset( 2, value.m_Ptr );
         }
      }

      public RectOffset overflow
      {
         get
         {
            if( m_Overflow == null )
            {
               m_Overflow = new RectOffset( this, GetRectOffsetPtr( 3 ) );
            }

            return m_Overflow;
         }
         set
         {
            AssignRectOffset( 3, value.m_Ptr );
         }
      }

      [Obsolete( "warning Don't use clipOffset - put things inside BeginGroup instead. This functionality will be removed in a later version." )]
      public Vector2 clipOffset
      {
         get
         {
            return Internal_clipOffset;
         }
         set
         {
            Internal_clipOffset = value;
         }
      }

      public Font font
      {
         get
         {
            return GetFontInternal();
         }
         set
         {
            SetFontInternal( value );
            m_FontInternal = value;
         }
      }

      public float lineHeight => throw new NotImplementedException();

      public static GUIStyle none
      {
         get
         {
            if( s_None == null )
            {
               s_None = new GUIStyle();
            }

            return s_None;
         }
      }

      public bool isHeightDependantOnWidth => fixedHeight == 0f && wordWrap && imagePosition != ImagePosition.ImageOnly;

      public string name
      {


         get;


         set;
      }

      public ImagePosition imagePosition
      {


         get;


         set;
      }

      public TextAnchor alignment
      {


         get;


         set;
      }

      public bool wordWrap
      {


         get;


         set;
      }

      public TextClipping clipping
      {


         get;


         set;
      }

      public Vector2 contentOffset
      {
         get
         {
            INTERNAL_get_contentOffset( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_contentOffset( ref value );
         }
      }

      internal Vector2 Internal_clipOffset
      {
         get
         {
            INTERNAL_get_Internal_clipOffset( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_Internal_clipOffset( ref value );
         }
      }

      public float fixedWidth
      {


         get;


         set;
      }

      public float fixedHeight
      {


         get;


         set;
      }

      public bool stretchWidth
      {


         get;


         set;
      }

      public bool stretchHeight
      {


         get;


         set;
      }

      public int fontSize
      {


         get;


         set;
      }

      public FontStyle fontStyle
      {


         get;


         set;
      }

      public bool richText
      {


         get;


         set;
      }

      public GUIStyle()
      {
         Init();
      }

      public GUIStyle( GUIStyle other )
      {
         InitCopy( other );
      }

      ~GUIStyle()
      {
         Cleanup();
      }

      internal static void CleanupRoots()
      {
         s_None = null;
      }

      internal void InternalOnAfterDeserialize()
      {
         m_FontInternal = GetFontInternalDuringLoadingThread();
         m_Normal = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 0 ) );
         m_Hover = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 1 ) );
         m_Active = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 2 ) );
         m_Focused = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 3 ) );
         m_OnNormal = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 4 ) );
         m_OnHover = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 5 ) );
         m_OnActive = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 6 ) );
         m_OnFocused = GUIStyleState.ProduceGUIStyleStateFromDeserialization( this, GetStyleStatePtr( 7 ) );
      }

      private static void Internal_Draw( IntPtr target, Rect position, GUIContent content, bool isHover, bool isActive, bool on, bool hasKeyboardFocus ) => throw new NotImplementedException();

      public void Draw( Rect position, bool isHover, bool isActive, bool on, bool hasKeyboardFocus )
      {
         Internal_Draw( m_Ptr, position, GUIContent.none, isHover, isActive, on, hasKeyboardFocus );
      }

      public void Draw( Rect position, string text, bool isHover, bool isActive, bool on, bool hasKeyboardFocus )
      {
         Internal_Draw( m_Ptr, position, GUIContent.Temp( text ), isHover, isActive, on, hasKeyboardFocus );
      }

      public void Draw( Rect position, Texture image, bool isHover, bool isActive, bool on, bool hasKeyboardFocus )
      {
         Internal_Draw( m_Ptr, position, GUIContent.Temp( image ), isHover, isActive, on, hasKeyboardFocus );
      }

      public void Draw( Rect position, GUIContent content, bool isHover, bool isActive, bool on, bool hasKeyboardFocus )
      {
         Internal_Draw( m_Ptr, position, content, isHover, isActive, on, hasKeyboardFocus );
      }

      public void Draw( Rect position, GUIContent content, int controlID )
      {
         Draw( position, content, controlID, on: false );
      }

      public void Draw( Rect position, GUIContent content, int controlID, bool on )
      {
         if( content != null )
         {
            Internal_Draw2( m_Ptr, position, content, controlID, on );
         }
      }

      public void DrawCursor( Rect position, GUIContent content, int controlID, int Character ) => throw new NotImplementedException();

      internal void DrawWithTextSelection( Rect position, GUIContent content, int controlID, int firstSelectedCharacter, int lastSelectedCharacter, bool drawSelectionAsComposition ) => throw new NotImplementedException();

      public void DrawWithTextSelection( Rect position, GUIContent content, int controlID, int firstSelectedCharacter, int lastSelectedCharacter )
      {
         DrawWithTextSelection( position, content, controlID, firstSelectedCharacter, lastSelectedCharacter, drawSelectionAsComposition: false );
      }

      public static implicit operator GUIStyle( string str ) => throw new NotImplementedException();

      public Vector2 GetCursorPixelPosition( Rect position, GUIContent content, int cursorStringIndex )
      {
         Internal_GetCursorPixelPosition( m_Ptr, position, content, cursorStringIndex, out Vector2 ret );
         return ret;
      }

      public int GetCursorStringIndex( Rect position, GUIContent content, Vector2 cursorPixelPosition )
      {
         return Internal_GetCursorStringIndex( m_Ptr, position, content, cursorPixelPosition );
      }

      internal int GetNumCharactersThatFitWithinWidth( string text, float width )
      {
         return Internal_GetNumCharactersThatFitWithinWidth( m_Ptr, text, width );
      }

      public Vector2 CalcSize( GUIContent content )
      {
         Internal_CalcSize( m_Ptr, content, out Vector2 ret );
         return ret;
      }

      internal Vector2 CalcSizeWithConstraints( GUIContent content, Vector2 constraints )
      {
         Internal_CalcSizeWithConstraints( m_Ptr, content, constraints, out Vector2 ret );
         return ret;
      }

      public Vector2 CalcScreenSize( Vector2 contentSize ) => throw new NotImplementedException();

      public float CalcHeight( GUIContent content, float width )
      {
         return Internal_CalcHeight( m_Ptr, content, width );
      }

      public void CalcMinMaxWidth( GUIContent content, out float minWidth, out float maxWidth )
      {
         Internal_CalcMinMaxWidth( m_Ptr, content, out minWidth, out maxWidth );
      }

      public override string ToString() => throw new NotImplementedException();




      private extern void Init();




      private extern void InitCopy( GUIStyle other );




      private extern void Cleanup();


      private IntPtr GetStyleStatePtr( int idx )
      {
         INTERNAL_CALL_GetStyleStatePtr( this, idx, out IntPtr value );
         return value;
      }



      private static extern void INTERNAL_CALL_GetStyleStatePtr( GUIStyle self, int idx, out IntPtr value );



      private extern void AssignStyleState( int idx, IntPtr srcStyleState );

      private IntPtr GetRectOffsetPtr( int idx )
      {
         INTERNAL_CALL_GetRectOffsetPtr( this, idx, out IntPtr value );
         return value;
      }



      private static extern void INTERNAL_CALL_GetRectOffsetPtr( GUIStyle self, int idx, out IntPtr value );



      private extern void AssignRectOffset( int idx, IntPtr srcRectOffset );



      private extern void INTERNAL_get_contentOffset( out Vector2 value );



      private extern void INTERNAL_set_contentOffset( ref Vector2 value );



      private extern void INTERNAL_get_Internal_clipOffset( out Vector2 value );



      private extern void INTERNAL_set_Internal_clipOffset( ref Vector2 value );



      private static extern float Internal_GetLineHeight( IntPtr target );



      private extern void SetFontInternal( Font value );




      private extern Font GetFontInternalDuringLoadingThread();



      private extern Font GetFontInternal();




      private static void Internal_Draw2( IntPtr style, Rect position, GUIContent content, int controlID, bool on )
      {
         INTERNAL_CALL_Internal_Draw2( style, ref position, content, controlID, on );
      }



      private static extern void INTERNAL_CALL_Internal_Draw2( IntPtr style, ref Rect position, GUIContent content, int controlID, bool on );

      internal void SetMouseTooltip( string tooltip, Rect screenRect )
      {
         INTERNAL_CALL_SetMouseTooltip( this, tooltip, ref screenRect );
      }



      private static extern void INTERNAL_CALL_SetMouseTooltip( GUIStyle self, string tooltip, ref Rect screenRect );

      private static void Internal_DrawPrefixLabel( IntPtr style, Rect position, GUIContent content, int controlID, bool on )
      {
         INTERNAL_CALL_Internal_DrawPrefixLabel( style, ref position, content, controlID, on );
      }



      private static extern void INTERNAL_CALL_Internal_DrawPrefixLabel( IntPtr style, ref Rect position, GUIContent content, int controlID, bool on );



      private static extern float Internal_GetCursorFlashOffset();

      private static void Internal_DrawCursor( IntPtr target, Rect position, GUIContent content, int pos, Color cursorColor )
      {
         INTERNAL_CALL_Internal_DrawCursor( target, ref position, content, pos, ref cursorColor );
      }



      private static extern void INTERNAL_CALL_Internal_DrawCursor( IntPtr target, ref Rect position, GUIContent content, int pos, ref Color cursorColor );





      internal static extern void SetDefaultFont( Font font );

      internal static void Internal_GetCursorPixelPosition( IntPtr target, Rect position, GUIContent content, int cursorStringIndex, out Vector2 ret )
      {
         INTERNAL_CALL_Internal_GetCursorPixelPosition( target, ref position, content, cursorStringIndex, out ret );
      }



      private static extern void INTERNAL_CALL_Internal_GetCursorPixelPosition( IntPtr target, ref Rect position, GUIContent content, int cursorStringIndex, out Vector2 ret );

      internal static int Internal_GetCursorStringIndex( IntPtr target, Rect position, GUIContent content, Vector2 cursorPixelPosition )
      {
         return INTERNAL_CALL_Internal_GetCursorStringIndex( target, ref position, content, ref cursorPixelPosition );
      }



      private static extern int INTERNAL_CALL_Internal_GetCursorStringIndex( IntPtr target, ref Rect position, GUIContent content, ref Vector2 cursorPixelPosition );



      internal static extern int Internal_GetNumCharactersThatFitWithinWidth( IntPtr target, string text, float width );



      internal static extern void Internal_CalcSize( IntPtr target, GUIContent content, out Vector2 ret );

      internal static void Internal_CalcSizeWithConstraints( IntPtr target, GUIContent content, Vector2 maxSize, out Vector2 ret )
      {
         INTERNAL_CALL_Internal_CalcSizeWithConstraints( target, content, ref maxSize, out ret );
      }



      private static extern void INTERNAL_CALL_Internal_CalcSizeWithConstraints( IntPtr target, GUIContent content, ref Vector2 maxSize, out Vector2 ret );



      private static extern float Internal_CalcHeight( IntPtr target, GUIContent content, float width );



      private static extern void Internal_CalcMinMaxWidth( IntPtr target, GUIContent content, out float minWidth, out float maxWidth );

      public static System.IntPtr Internal_Copy( GUIStyle self, GUIStyle other ) => throw new Exception();
   }
}
