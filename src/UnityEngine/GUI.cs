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
   public class GUI
   {
#if IL2CPP
      public sealed class WindowFunction : Il2CppSystem.MulticastDelegate
      {
         public WindowFunction( IntPtr value ) : base( value )
         {
         }

         public static implicit operator GUI.WindowFunction( System.Action<int> obj0 ) => throw new NotImplementedException();
      }
#else
      public delegate void WindowFunction( int id );
#endif



      public abstract class Scope : IDisposable
      {
         private bool m_Disposed;

         protected abstract void CloseScope();

         ~Scope()
         {
         }

         public void Dispose() => throw new NotImplementedException();
      }

      public class GroupScope : Scope
      {
         public GroupScope( Rect position )
         {
            BeginGroup( position );
         }

         public GroupScope( Rect position, string text )
         {
            BeginGroup( position, text );
         }

         public GroupScope( Rect position, Texture image )
         {
            BeginGroup( position, image );
         }

         public GroupScope( Rect position, GUIContent content )
         {
            BeginGroup( position, content );
         }

         public GroupScope( Rect position, GUIStyle style )
         {
            BeginGroup( position, style );
         }

         public GroupScope( Rect position, string text, GUIStyle style )
         {
            BeginGroup( position, text, style );
         }

         public GroupScope( Rect position, Texture image, GUIStyle style )
         {
            BeginGroup( position, image, style );
         }

         protected override void CloseScope()
         {
            EndGroup();
         }
      }

      public class ScrollViewScope : Scope
      {
         public Vector2 scrollPosition
         {
            get;
            private set;
         }

         public bool handleScrollWheel
         {
            get;
            set;
         }

         public ScrollViewScope( Rect position, Vector2 scrollPosition, Rect viewRect )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( position, scrollPosition, viewRect );
         }

         public ScrollViewScope( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical );
         }

         public ScrollViewScope( Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( position, scrollPosition, viewRect, horizontalScrollbar, verticalScrollbar );
         }

         public ScrollViewScope( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar );
         }

         internal ScrollViewScope( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background );
         }

         protected override void CloseScope()
         {
            EndScrollView( handleScrollWheel );
         }
      }

      public class ClipScope : Scope
      {
         public ClipScope( Rect position )
         {
            BeginClip( position );
         }

         protected override void CloseScope()
         {
            EndClip();
         }
      }

      private static float s_ScrollStepSize;

      private static int s_ScrollControlId;

      private static int s_HotTextField;

      private static readonly int s_BoxHash;

      private static readonly int s_RepeatButtonHash;

      private static readonly int s_ToggleHash;

      private static readonly int s_ButtonGridHash;

      private static readonly int s_SliderHash;

      private static readonly int s_BeginGroupHash;

      private static readonly int s_ScrollviewHash;

      private static GUISkin s_Skin;

      internal static Rect s_ToolTipRect;

      internal static int scrollTroughSide
      {
         get;
         set;
      }

      internal static DateTime nextScrollStepTime
      {
         get;
         set;
      }

      public static GUISkin skin
      {
         get
         {
            return s_Skin;
         }
         set
         {
            DoSetSkin( value );
         }
      }

      public static Matrix4x4 matrix => throw new NotImplementedException();

      public static string tooltip
      {
         get
         {
            string text = Internal_GetTooltip();
            if( text != null )
            {
               return text;
            }

            return "";
         }
         set
         {
            Internal_SetTooltip( value );
         }
      }

      protected static string mouseTooltip => Internal_GetMouseTooltip();

      protected static Rect tooltipRect
      {
         get
         {
            return s_ToolTipRect;
         }
         set
         {
            s_ToolTipRect = value;
         }
      }

      public static Color color
      {
         get
         {
            INTERNAL_get_color( out Color value );
            return value;
         }
         set
         {
            INTERNAL_set_color( ref value );
         }
      }

      public static Color backgroundColor
      {
         get
         {
            INTERNAL_get_backgroundColor( out Color value );
            return value;
         }
         set
         {
            INTERNAL_set_backgroundColor( ref value );
         }
      }

      public static Color contentColor
      {
         get
         {
            INTERNAL_get_contentColor( out Color value );
            return value;
         }
         set
         {
            INTERNAL_set_contentColor( ref value );
         }
      }

      public static bool changed
      {
         get;
         set;
      }

      public static bool enabled
      {


         get;


         set;
      }

      public static int depth
      {


         get;


         set;
      }

      internal static Material blendMaterial
      {


         get;
      }

      internal static Material blitMaterial
      {


         get;
      }

      internal static bool usePageScrollbars
      {


         get;
      }

      static GUI()
      {
         s_ScrollStepSize = 10f;
         s_HotTextField = -1;
         s_BoxHash = "Box".GetHashCode();
         s_RepeatButtonHash = "repeatButton".GetHashCode();
         s_ToggleHash = "Toggle".GetHashCode();
         s_ButtonGridHash = "ButtonGrid".GetHashCode();
         s_SliderHash = "Slider".GetHashCode();
         s_BeginGroupHash = "BeginGroup".GetHashCode();
         s_ScrollviewHash = "scrollView".GetHashCode();
         nextScrollStepTime = DateTime.Now;
      }

      internal static void DoSetSkin( GUISkin newSkin )
      {
         s_Skin = newSkin;
         newSkin.MakeCurrent();
      }

      internal static void CleanupRoots()
      {
         s_Skin = null;
      }

      public static void Label( Rect position, string text )
      {
         Label( position, GUIContent.Temp( text ), s_Skin.label );
      }

      public static void Label( Rect position, Texture image )
      {
         Label( position, GUIContent.Temp( image ), s_Skin.label );
      }

      public static void Label( Rect position, GUIContent content )
      {
         Label( position, content, s_Skin.label );
      }

      public static void Label( Rect position, string text, GUIStyle style )
      {
         Label( position, GUIContent.Temp( text ), style );
      }

      public static void Label( Rect position, Texture image, GUIStyle style )
      {
         Label( position, GUIContent.Temp( image ), style );
      }

      public static void Label( Rect position, GUIContent content, GUIStyle style )
      {
         DoLabel( position, content, style.m_Ptr );
      }

      public static void DrawTexture( Rect position, Texture image ) => throw new NotImplementedException();

      public static void DrawTexture( Rect position, Texture image, ScaleMode scaleMode )
      {
         DrawTexture( position, image, scaleMode, alphaBlend: true );
      }

      public static void DrawTexture( Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend )
      {
         DrawTexture( position, image, scaleMode, alphaBlend, 0f );
      }

      public static void DrawTexture( Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect )
      {
      }

      internal static bool CalculateScaledTextureRects( Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect ) => throw new NotImplementedException();

      public static void DrawTextureWithTexCoords( Rect position, Texture image, Rect texCoords )
      {
         DrawTextureWithTexCoords( position, image, texCoords, alphaBlend: true );
      }

      public static void DrawTextureWithTexCoords( Rect position, Texture image, Rect texCoords, bool alphaBlend )
      {
      }

      public static void Box( Rect position, string text )
      {
         Box( position, GUIContent.Temp( text ), s_Skin.box );
      }

      public static void Box( Rect position, Texture image )
      {
         Box( position, GUIContent.Temp( image ), s_Skin.box );
      }

      public static void Box( Rect position, GUIContent content )
      {
         Box( position, content, s_Skin.box );
      }

      public static void Box( Rect position, string text, GUIStyle style )
      {
         Box( position, GUIContent.Temp( text ), style );
      }

      public static void Box( Rect position, Texture image, GUIStyle style )
      {
         Box( position, GUIContent.Temp( image ), style );
      }

      public static void Box( Rect position, GUIContent content, GUIStyle style )
      {
      }

      public static bool Button( Rect position, string text )
      {
         return Button( position, GUIContent.Temp( text ), s_Skin.button );
      }

      public static bool Button( Rect position, Texture image )
      {
         return Button( position, GUIContent.Temp( image ), s_Skin.button );
      }

      public static bool Button( Rect position, GUIContent content )
      {
         return Button( position, content, s_Skin.button );
      }

      public static bool Button( Rect position, string text, GUIStyle style )
      {
         return Button( position, GUIContent.Temp( text ), style );
      }

      public static bool Button( Rect position, Texture image, GUIStyle style )
      {
         return Button( position, GUIContent.Temp( image ), style );
      }

      public static bool Button( Rect position, GUIContent content, GUIStyle style )
      {
         return DoButton( position, content, style.m_Ptr );
      }

      public static bool RepeatButton( Rect position, string text )
      {
         return DoRepeatButton( position, GUIContent.Temp( text ), s_Skin.button, FocusType.Passive );
      }

      public static bool RepeatButton( Rect position, Texture image )
      {
         return DoRepeatButton( position, GUIContent.Temp( image ), s_Skin.button, FocusType.Passive );
      }

      public static bool RepeatButton( Rect position, GUIContent content )
      {
         return DoRepeatButton( position, content, s_Skin.button, FocusType.Passive );
      }

      public static bool RepeatButton( Rect position, string text, GUIStyle style )
      {
         return DoRepeatButton( position, GUIContent.Temp( text ), style, FocusType.Passive );
      }

      public static bool RepeatButton( Rect position, Texture image, GUIStyle style )
      {
         return DoRepeatButton( position, GUIContent.Temp( image ), style, FocusType.Passive );
      }

      public static bool RepeatButton( Rect position, GUIContent content, GUIStyle style )
      {
         return DoRepeatButton( position, content, style, FocusType.Passive );
      }

      private static bool DoRepeatButton( Rect position, GUIContent content, GUIStyle style, FocusType focusType ) => throw new NotImplementedException();

      public static string TextField( Rect position, string text ) => throw new NotImplementedException();

      public static string TextField( Rect position, string text, int maxLength ) => throw new NotImplementedException();

      public static string TextField( Rect position, string text, GUIStyle style ) => throw new NotImplementedException();

      public static string TextField( Rect position, string text, int maxLength, GUIStyle style ) => throw new NotImplementedException();

      public static string PasswordField( Rect position, string password, char maskChar )
      {
         return PasswordField( position, password, maskChar, -1, skin.textField );
      }

      public static string PasswordField( Rect position, string password, char maskChar, int maxLength )
      {
         return PasswordField( position, password, maskChar, maxLength, skin.textField );
      }

      public static string PasswordField( Rect position, string password, char maskChar, GUIStyle style )
      {
         return PasswordField( position, password, maskChar, -1, style );
      }

      public static string PasswordField( Rect position, string password, char maskChar, int maxLength, GUIStyle style ) => throw new NotImplementedException();

      internal static string PasswordFieldGetStrToShow( string password, char maskChar ) => throw new NotImplementedException();

      public static string TextArea( Rect position, string text ) => throw new NotImplementedException();

      public static string TextArea( Rect position, string text, int maxLength ) => throw new NotImplementedException();

      public static string TextArea( Rect position, string text, GUIStyle style ) => throw new NotImplementedException();

      public static string TextArea( Rect position, string text, int maxLength, GUIStyle style ) => throw new NotImplementedException();

      private static string TextArea( Rect position, GUIContent content, int maxLength, GUIStyle style ) => throw new NotImplementedException();

      internal static void DoTextField( Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style ) => throw new NotImplementedException();

      internal static void DoTextField( Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText ) => throw new NotImplementedException();

      internal static void DoTextField( Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText, char maskChar ) => throw new NotImplementedException();

      private static void HandleTextFieldEventForTouchscreen( Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, string secureText, char maskChar, TextEditor editor ) => throw new NotImplementedException();

      private static void HandleTextFieldEventForDesktop( Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style, TextEditor editor ) => throw new NotImplementedException();

      public static bool Toggle( Rect position, bool value, string text )
      {
         return Toggle( position, value, GUIContent.Temp( text ), s_Skin.toggle );
      }

      public static bool Toggle( Rect position, bool value, Texture image )
      {
         return Toggle( position, value, GUIContent.Temp( image ), s_Skin.toggle );
      }

      public static bool Toggle( Rect position, bool value, GUIContent content )
      {
         return Toggle( position, value, content, s_Skin.toggle );
      }

      public static bool Toggle( Rect position, bool value, string text, GUIStyle style )
      {
         return Toggle( position, value, GUIContent.Temp( text ), style );
      }

      public static bool Toggle( Rect position, bool value, Texture image, GUIStyle style )
      {
         return Toggle( position, value, GUIContent.Temp( image ), style );
      }

      public static bool Toggle( Rect position, bool value, GUIContent content, GUIStyle style ) => throw new NotImplementedException();

      public static bool Toggle( Rect position, int id, bool value, GUIContent content, GUIStyle style ) => throw new NotImplementedException();

      public static int Toolbar( Rect position, int selected, string[] texts )
      {
         return Toolbar( position, selected, GUIContent.Temp( texts ), s_Skin.button );
      }

      public static int Toolbar( Rect position, int selected, Texture[] images )
      {
         return Toolbar( position, selected, GUIContent.Temp( images ), s_Skin.button );
      }

      public static int Toolbar( Rect position, int selected, GUIContent[] content )
      {
         return Toolbar( position, selected, content, s_Skin.button );
      }

      public static int Toolbar( Rect position, int selected, string[] texts, GUIStyle style )
      {
         return Toolbar( position, selected, GUIContent.Temp( texts ), style );
      }

      public static int Toolbar( Rect position, int selected, Texture[] images, GUIStyle style )
      {
         return Toolbar( position, selected, GUIContent.Temp( images ), style );
      }

      public static int Toolbar( Rect position, int selected, GUIContent[] contents, GUIStyle style ) => throw new NotImplementedException();

      public static int SelectionGrid( Rect position, int selected, string[] texts, int xCount )
      {
         return SelectionGrid( position, selected, GUIContent.Temp( texts ), xCount, null );
      }

      public static int SelectionGrid( Rect position, int selected, Texture[] images, int xCount )
      {
         return SelectionGrid( position, selected, GUIContent.Temp( images ), xCount, null );
      }

      public static int SelectionGrid( Rect position, int selected, GUIContent[] content, int xCount )
      {
         return SelectionGrid( position, selected, content, xCount, null );
      }

      public static int SelectionGrid( Rect position, int selected, string[] texts, int xCount, GUIStyle style )
      {
         return SelectionGrid( position, selected, GUIContent.Temp( texts ), xCount, style );
      }

      public static int SelectionGrid( Rect position, int selected, Texture[] images, int xCount, GUIStyle style )
      {
         return SelectionGrid( position, selected, GUIContent.Temp( images ), xCount, style );
      }

      public static int SelectionGrid( Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style )
      {
         if( style == null )
         {
            style = s_Skin.button;
         }

         return DoButtonGrid( position, selected, contents, xCount, style, style, style, style );
      }

      internal static void FindStyles( ref GUIStyle style, out GUIStyle firstStyle, out GUIStyle midStyle, out GUIStyle lastStyle, string first, string mid, string last )
      {
         if( style == null )
         {
            style = skin.button;
         }

         string name = style.name;
         midStyle = skin.FindStyle( name + mid );
         if( midStyle == null )
         {
            midStyle = style;
         }

         firstStyle = skin.FindStyle( name + first );
         if( firstStyle == null )
         {
            firstStyle = midStyle;
         }

         lastStyle = skin.FindStyle( name + last );
         if( lastStyle == null )
         {
            lastStyle = midStyle;
         }
      }

      internal static int CalcTotalHorizSpacing( int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle ) => throw new NotImplementedException();

      private static int DoButtonGrid( Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle ) => throw new NotImplementedException();

      private static Rect[] CalcMouseRects( Rect position, int count, int xCount, float elemWidth, float elemHeight, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle, bool addBorders ) => throw new NotImplementedException();

      private static int GetButtonGridMouseSelection( Rect[] buttonRects, Vector2 mousePos, bool findNearest ) => throw new NotImplementedException();

      public static float HorizontalSlider( Rect position, float value, float leftValue, float rightValue )
      {
         return Slider( position, value, 0f, leftValue, rightValue, skin.horizontalSlider, skin.horizontalSliderThumb, horiz: true, 0 );
      }

      public static float HorizontalSlider( Rect position, float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb )
      {
         return Slider( position, value, 0f, leftValue, rightValue, slider, thumb, horiz: true, 0 );
      }

      public static float VerticalSlider( Rect position, float value, float topValue, float bottomValue )
      {
         return Slider( position, value, 0f, topValue, bottomValue, skin.verticalSlider, skin.verticalSliderThumb, horiz: false, 0 );
      }

      public static float VerticalSlider( Rect position, float value, float topValue, float bottomValue, GUIStyle slider, GUIStyle thumb )
      {
         return Slider( position, value, 0f, topValue, bottomValue, slider, thumb, horiz: false, 0 );
      }

      public static float Slider( Rect position, float value, float size, float start, float end, GUIStyle slider, GUIStyle thumb, bool horiz, int id ) => throw new NotImplementedException();

      public static float HorizontalScrollbar( Rect position, float value, float size, float leftValue, float rightValue )
      {
         return Scroller( position, value, size, leftValue, rightValue, skin.horizontalScrollbar, skin.horizontalScrollbarThumb, skin.horizontalScrollbarLeftButton, skin.horizontalScrollbarRightButton, horiz: true );
      }

      public static float HorizontalScrollbar( Rect position, float value, float size, float leftValue, float rightValue, GUIStyle style )
      {
         return Scroller( position, value, size, leftValue, rightValue, style, skin.GetStyle( style.name + "thumb" ), skin.GetStyle( style.name + "leftbutton" ), skin.GetStyle( style.name + "rightbutton" ), horiz: true );
      }

      internal static bool ScrollerRepeatButton( int scrollerID, Rect rect, GUIStyle style ) => throw new NotImplementedException();

      public static float VerticalScrollbar( Rect position, float value, float size, float topValue, float bottomValue )
      {
         return Scroller( position, value, size, topValue, bottomValue, skin.verticalScrollbar, skin.verticalScrollbarThumb, skin.verticalScrollbarUpButton, skin.verticalScrollbarDownButton, horiz: false );
      }

      public static float VerticalScrollbar( Rect position, float value, float size, float topValue, float bottomValue, GUIStyle style )
      {
         return Scroller( position, value, size, topValue, bottomValue, style, skin.GetStyle( style.name + "thumb" ), skin.GetStyle( style.name + "upbutton" ), skin.GetStyle( style.name + "downbutton" ), horiz: false );
      }

      internal static float Scroller( Rect position, float value, float size, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton, GUIStyle rightButton, bool horiz ) => throw new NotImplementedException();

      public static void BeginClip( Rect position, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset ) => throw new NotImplementedException();

      public static void BeginGroup( Rect position )
      {
         BeginGroup( position, GUIContent.none, GUIStyle.none );
      }

      public static void BeginGroup( Rect position, string text )
      {
         BeginGroup( position, GUIContent.Temp( text ), GUIStyle.none );
      }

      public static void BeginGroup( Rect position, Texture image )
      {
         BeginGroup( position, GUIContent.Temp( image ), GUIStyle.none );
      }

      public static void BeginGroup( Rect position, GUIContent content )
      {
         BeginGroup( position, content, GUIStyle.none );
      }

      public static void BeginGroup( Rect position, GUIStyle style )
      {
         BeginGroup( position, GUIContent.none, style );
      }

      public static void BeginGroup( Rect position, string text, GUIStyle style )
      {
         BeginGroup( position, GUIContent.Temp( text ), style );
      }

      public static void BeginGroup( Rect position, Texture image, GUIStyle style )
      {
         BeginGroup( position, GUIContent.Temp( image ), style );
      }

      public static void BeginGroup( Rect position, GUIContent content, GUIStyle style ) => throw new NotImplementedException();

      public static void EndGroup() => throw new NotImplementedException();

      public static void BeginClip( Rect position ) => throw new NotImplementedException();

      public static void EndClip() => throw new NotImplementedException();

      public static Vector2 BeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect )
      {
         return BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: false, skin.horizontalScrollbar, skin.verticalScrollbar, skin.scrollView );
      }

      public static Vector2 BeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical )
      {
         return BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, skin.horizontalScrollbar, skin.verticalScrollbar, skin.scrollView );
      }

      public static Vector2 BeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar )
      {
         return BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: false, horizontalScrollbar, verticalScrollbar, skin.scrollView );
      }

      public static Vector2 BeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar )
      {
         return BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, skin.scrollView );
      }

      protected static Vector2 DoBeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background )
      {
         return BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background );
      }

      internal static Vector2 BeginScrollView( Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background ) => throw new NotImplementedException();

      public static void EndScrollView()
      {
         EndScrollView( handleScrollWheel: true );
      }

      public static void EndScrollView( bool handleScrollWheel ) => throw new NotImplementedException();

      internal static ScrollViewState GetTopScrollView() => throw new NotImplementedException();

      public static void ScrollTo( Rect position )
      {
         GetTopScrollView()?.ScrollTo( position );
      }

      public static bool ScrollTowards( Rect position, float maxDelta )
      {
         return GetTopScrollView()?.ScrollTowards( position, maxDelta ) ?? false;
      }

      public static Rect Window( int id, Rect clientRect, WindowFunction func, string text ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect clientRect, WindowFunction func, Texture image ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect clientRect, WindowFunction func, GUIContent content ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect clientRect, WindowFunction func, string text, GUIStyle style ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, string text ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, Texture image ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, GUIContent content ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, string text, GUIStyle style ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style ) => throw new NotImplementedException();

      public static Rect ModalWindow( int id, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style ) => throw new NotImplementedException();

      private static Rect DoWindow( int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, GUISkin skin, bool forceRectOnLayout ) => throw new NotImplementedException();

      private static Rect DoModalWindow( int id, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, GUISkin skin ) => throw new NotImplementedException();

      internal static void CallWindowDelegate( WindowFunction func, int id, int instanceID, GUISkin _skin, int forceRect, float width, float height, GUIStyle style ) => throw new NotImplementedException();

      public static void DragWindow()
      {
         DragWindow( new Rect( 0f, 0f, 10000f, 10000f ) );
      }

      internal static void BeginWindows( int skinMode, int editorWindowInstanceID ) => throw new NotImplementedException();

      internal static void EndWindows() => throw new NotImplementedException();



      private static extern void INTERNAL_get_color( out Color value );



      private static extern void INTERNAL_set_color( ref Color value );



      private static extern void INTERNAL_get_backgroundColor( out Color value );

      private static extern void INTERNAL_set_backgroundColor( ref Color value );

      private static extern void INTERNAL_get_contentColor( out Color value );

      private static extern void INTERNAL_set_contentColor( ref Color value );

      private static extern string Internal_GetTooltip();

      private static extern void Internal_SetTooltip( string value );

      private static extern string Internal_GetMouseTooltip();

      private static void DoLabel( Rect position, GUIContent content, IntPtr style )
      {
         INTERNAL_CALL_DoLabel( ref position, content, style );
      }

      private static extern void INTERNAL_CALL_DoLabel( ref Rect position, GUIContent content, IntPtr style );

      private static extern void InitializeGUIClipTexture();

      private static bool DoButton( Rect position, GUIContent content, IntPtr style )
      {
         return INTERNAL_CALL_DoButton( ref position, content, style );
      }

      private static extern bool INTERNAL_CALL_DoButton( ref Rect position, GUIContent content, IntPtr style );

      public static extern void SetNextControlName( string name );

      public static extern string GetNameOfFocusedControl();

      public static extern void FocusControl( string name );

      internal static bool DoToggle( Rect position, int id, bool value, GUIContent content, IntPtr style )
      {
         return INTERNAL_CALL_DoToggle( ref position, id, value, content, style );
      }

      private static extern bool INTERNAL_CALL_DoToggle( ref Rect position, int id, bool value, GUIContent content, IntPtr style );

      internal static extern void InternalRepaintEditorWindow();

      private static Rect Internal_DoModalWindow( int id, int instanceID, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, GUISkin skin )
      {
         INTERNAL_CALL_Internal_DoModalWindow( id, instanceID, ref clientRect, func, content, style, skin, out Rect value );
         return value;
      }

      private static extern void INTERNAL_CALL_Internal_DoModalWindow( int id, int instanceID, ref Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style, GUISkin skin, out Rect value );

      private static Rect Internal_DoWindow( int id, int instanceID, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, GUISkin skin, bool forceRectOnLayout )
      {
         INTERNAL_CALL_Internal_DoWindow( id, instanceID, ref clientRect, func, title, style, skin, forceRectOnLayout, out Rect value );
         return value;
      }

      private static extern void INTERNAL_CALL_Internal_DoWindow( int id, int instanceID, ref Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, GUISkin skin, bool forceRectOnLayout, out Rect value );

      public static void DragWindow( Rect position )
      {
         INTERNAL_CALL_DragWindow( ref position );
      }

      private static extern void INTERNAL_CALL_DragWindow( ref Rect position );

      public static extern void BringWindowToFront( int windowID );

      public static extern void BringWindowToBack( int windowID );

      public static extern void FocusWindow( int windowID );

      public static extern void UnfocusWindow();

      private static extern void Internal_BeginWindows();

      private static extern void Internal_EndWindows();
   }
}
