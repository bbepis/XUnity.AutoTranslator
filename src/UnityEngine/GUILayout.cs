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
   public class GUILayout
   {
      private sealed class LayoutedWindow
      {
         private readonly GUI.WindowFunction m_Func;

         private readonly Rect m_ScreenRect;

         private readonly GUILayoutOption[] m_Options;

         private readonly GUIStyle m_Style;

         internal LayoutedWindow( GUI.WindowFunction f, Rect screenRect, GUIContent content, GUILayoutOption[] options, GUIStyle style )
         {
            m_Func = f;
            m_ScreenRect = screenRect;
            m_Options = options;
            m_Style = style;
         }

         public void DoWindow( int windowID ) => throw new NotImplementedException();
      }

      public class HorizontalScope : GUI.Scope
      {
         public HorizontalScope( params GUILayoutOption[] options )
         {
            BeginHorizontal( options );
         }

         public HorizontalScope( GUIStyle style, params GUILayoutOption[] options )
         {
            BeginHorizontal( style, options );
         }

         public HorizontalScope( string text, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginHorizontal( text, style, options );
         }

         public HorizontalScope( Texture image, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginHorizontal( image, style, options );
         }

         public HorizontalScope( GUIContent content, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginHorizontal( content, style, options );
         }

         protected override void CloseScope()
         {
            EndHorizontal();
         }
      }

      public class VerticalScope : GUI.Scope
      {
         public VerticalScope( params GUILayoutOption[] options )
         {
            BeginVertical( options );
         }

         public VerticalScope( GUIStyle style, params GUILayoutOption[] options )
         {
            BeginVertical( style, options );
         }

         public VerticalScope( string text, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginVertical( text, style, options );
         }

         public VerticalScope( Texture image, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginVertical( image, style, options );
         }

         public VerticalScope( GUIContent content, GUIStyle style, params GUILayoutOption[] options )
         {
            BeginVertical( content, style, options );
         }

         protected override void CloseScope()
         {
            EndVertical();
         }
      }

      public class AreaScope : GUI.Scope
      {
         public AreaScope( Rect screenRect )
         {
            BeginArea( screenRect );
         }

         public AreaScope( Rect screenRect, string text )
         {
            BeginArea( screenRect, text );
         }

         public AreaScope( Rect screenRect, Texture image )
         {
            BeginArea( screenRect, image );
         }

         public AreaScope( Rect screenRect, GUIContent content )
         {
            BeginArea( screenRect, content );
         }

         public AreaScope( Rect screenRect, string text, GUIStyle style )
         {
            BeginArea( screenRect, text, style );
         }

         public AreaScope( Rect screenRect, Texture image, GUIStyle style )
         {
            BeginArea( screenRect, image, style );
         }

         public AreaScope( Rect screenRect, GUIContent content, GUIStyle style )
         {
            BeginArea( screenRect, content, style );
         }

         protected override void CloseScope()
         {
            EndArea();
         }
      }

      public class ScrollViewScope : GUI.Scope
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

         public ScrollViewScope( Vector2 scrollPosition, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, options );
         }

         public ScrollViewScope( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, alwaysShowHorizontal, alwaysShowVertical, options );
         }

         public ScrollViewScope( Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, horizontalScrollbar, verticalScrollbar, options );
         }

         public ScrollViewScope( Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, style, options );
         }

         public ScrollViewScope( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, options );
         }

         public ScrollViewScope( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options )
         {
            handleScrollWheel = true;
            this.scrollPosition = BeginScrollView( scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background, options );
         }

         protected override void CloseScope()
         {
            EndScrollView( handleScrollWheel );
         }
      }

      public static void Label( Texture image, params GUILayoutOption[] options )
      {
         DoLabel( GUIContent.Temp( image ), GUI.skin.label, options );
      }

      public static void Label( string text, params GUILayoutOption[] options )
      {
         DoLabel( GUIContent.Temp( text ), GUI.skin.label, options );
      }

      public static void Label( GUIContent content, params GUILayoutOption[] options )
      {
         DoLabel( content, GUI.skin.label, options );
      }

      public static void Label( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         DoLabel( GUIContent.Temp( image ), style, options );
      }

#if IL2CPP
      public static void Label( string text, GUIStyle style, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GUILayoutOption> options )
#else
      public static void Label( string text, GUIStyle style, params GUILayoutOption[] options )
#endif
      {
         DoLabel( GUIContent.Temp( text ), style, options );
      }

      public static void Label( GUIContent content, GUIStyle style, params GUILayoutOption[] options )
      {
         DoLabel( content, style, options );
      }

      private static void DoLabel( GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static void Box( Texture image, params GUILayoutOption[] options )
      {
         DoBox( GUIContent.Temp( image ), GUI.skin.box, options );
      }

      public static void Box( string text, params GUILayoutOption[] options )
      {
         DoBox( GUIContent.Temp( text ), GUI.skin.box, options );
      }

      public static void Box( GUIContent content, params GUILayoutOption[] options )
      {
         DoBox( content, GUI.skin.box, options );
      }

      public static void Box( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         DoBox( GUIContent.Temp( image ), style, options );
      }

      public static void Box( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         DoBox( GUIContent.Temp( text ), style, options );
      }

      public static void Box( GUIContent content, GUIStyle style, params GUILayoutOption[] options )
      {
         DoBox( content, style, options );
      }

      private static void DoBox( GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static bool Button( Texture image, params GUILayoutOption[] options )
      {
         return DoButton( GUIContent.Temp( image ), GUI.skin.button, options );
      }

      public static bool Button( string text, params GUILayoutOption[] options )
      {
         return DoButton( GUIContent.Temp( text ), GUI.skin.button, options );
      }

      public static bool Button( GUIContent content, params GUILayoutOption[] options )
      {
         return DoButton( content, GUI.skin.button, options );
      }

      public static bool Button( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoButton( GUIContent.Temp( image ), style, options );
      }

      public static bool Button( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoButton( GUIContent.Temp( text ), style, options );
      }

#if IL2CPP
      public static bool Button( GUIContent content, GUIStyle style, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GUILayoutOption> options ) => throw new NotImplementedException();
#else
      public static bool Button( GUIContent content, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();
#endif

      private static bool DoButton( GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static bool RepeatButton( Texture image, params GUILayoutOption[] options )
      {
         return DoRepeatButton( GUIContent.Temp( image ), GUI.skin.button, options );
      }

      public static bool RepeatButton( string text, params GUILayoutOption[] options )
      {
         return DoRepeatButton( GUIContent.Temp( text ), GUI.skin.button, options );
      }

      public static bool RepeatButton( GUIContent content, params GUILayoutOption[] options )
      {
         return DoRepeatButton( content, GUI.skin.button, options );
      }

      public static bool RepeatButton( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoRepeatButton( GUIContent.Temp( image ), style, options );
      }

      public static bool RepeatButton( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoRepeatButton( GUIContent.Temp( text ), style, options );
      }

      public static bool RepeatButton( GUIContent content, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoRepeatButton( content, style, options );
      }

      private static bool DoRepeatButton( GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static string TextField( string text, params GUILayoutOption[] options )
      {
         return DoTextField( text, -1, multiline: false, GUI.skin.textField, options );
      }

      public static string TextField( string text, int maxLength, params GUILayoutOption[] options )
      {
         return DoTextField( text, maxLength, multiline: false, GUI.skin.textField, options );
      }

      public static string TextField( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoTextField( text, -1, multiline: false, style, options );
      }

      public static string TextField( string text, int maxLength, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoTextField( text, maxLength, multiline: true, style, options );
      }

      public static string PasswordField( string password, char maskChar, params GUILayoutOption[] options )
      {
         return PasswordField( password, maskChar, -1, GUI.skin.textField, options );
      }

      public static string PasswordField( string password, char maskChar, int maxLength, params GUILayoutOption[] options )
      {
         return PasswordField( password, maskChar, maxLength, GUI.skin.textField, options );
      }

      public static string PasswordField( string password, char maskChar, GUIStyle style, params GUILayoutOption[] options )
      {
         return PasswordField( password, maskChar, -1, style, options );
      }

      public static string PasswordField( string password, char maskChar, int maxLength, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static string TextArea( string text, params GUILayoutOption[] options )
      {
         return DoTextField( text, -1, multiline: true, GUI.skin.textArea, options );
      }

      public static string TextArea( string text, int maxLength, params GUILayoutOption[] options )
      {
         return DoTextField( text, maxLength, multiline: true, GUI.skin.textArea, options );
      }

      public static string TextArea( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoTextField( text, -1, multiline: true, style, options );
      }

      public static string TextArea( string text, int maxLength, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoTextField( text, maxLength, multiline: true, style, options );
      }

      private static string DoTextField( string text, int maxLength, bool multiline, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static bool Toggle( bool value, Texture image, params GUILayoutOption[] options )
      {
         return DoToggle( value, GUIContent.Temp( image ), GUI.skin.toggle, options );
      }

      public static bool Toggle( bool value, string text, params GUILayoutOption[] options )
      {
         return DoToggle( value, GUIContent.Temp( text ), GUI.skin.toggle, options );
      }

      public static bool Toggle( bool value, GUIContent content, params GUILayoutOption[] options )
      {
         return DoToggle( value, content, GUI.skin.toggle, options );
      }

      public static bool Toggle( bool value, Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoToggle( value, GUIContent.Temp( image ), style, options );
      }

      public static bool Toggle( bool value, string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoToggle( value, GUIContent.Temp( text ), style, options );
      }

      public static bool Toggle( bool value, GUIContent content, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoToggle( value, content, style, options );
      }

      private static bool DoToggle( bool value, GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static int Toolbar( int selected, string[] texts, params GUILayoutOption[] options )
      {
         return Toolbar( selected, GUIContent.Temp( texts ), GUI.skin.button, options );
      }

      public static int Toolbar( int selected, Texture[] images, params GUILayoutOption[] options )
      {
         return Toolbar( selected, GUIContent.Temp( images ), GUI.skin.button, options );
      }

      public static int Toolbar( int selected, GUIContent[] content, params GUILayoutOption[] options )
      {
         return Toolbar( selected, content, GUI.skin.button, options );
      }

      public static int Toolbar( int selected, string[] texts, GUIStyle style, params GUILayoutOption[] options )
      {
         return Toolbar( selected, GUIContent.Temp( texts ), style, options );
      }

      public static int Toolbar( int selected, Texture[] images, GUIStyle style, params GUILayoutOption[] options )
      {
         return Toolbar( selected, GUIContent.Temp( images ), style, options );
      }

      public static int Toolbar( int selected, GUIContent[] contents, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static int SelectionGrid( int selected, string[] texts, int xCount, params GUILayoutOption[] options )
      {
         return SelectionGrid( selected, GUIContent.Temp( texts ), xCount, GUI.skin.button, options );
      }

      public static int SelectionGrid( int selected, Texture[] images, int xCount, params GUILayoutOption[] options )
      {
         return SelectionGrid( selected, GUIContent.Temp( images ), xCount, GUI.skin.button, options );
      }

      public static int SelectionGrid( int selected, GUIContent[] content, int xCount, params GUILayoutOption[] options )
      {
         return SelectionGrid( selected, content, xCount, GUI.skin.button, options );
      }

      public static int SelectionGrid( int selected, string[] texts, int xCount, GUIStyle style, params GUILayoutOption[] options )
      {
         return SelectionGrid( selected, GUIContent.Temp( texts ), xCount, style, options );
      }

      public static int SelectionGrid( int selected, Texture[] images, int xCount, GUIStyle style, params GUILayoutOption[] options )
      {
         return SelectionGrid( selected, GUIContent.Temp( images ), xCount, style, options );
      }

      public static int SelectionGrid( int selected, GUIContent[] contents, int xCount, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static float HorizontalSlider( float value, float leftValue, float rightValue, params GUILayoutOption[] options )
      {
         return DoHorizontalSlider( value, leftValue, rightValue, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, options );
      }

      public static float HorizontalSlider( float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options )
      {
         return DoHorizontalSlider( value, leftValue, rightValue, slider, thumb, options );
      }

      private static float DoHorizontalSlider( float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static float VerticalSlider( float value, float leftValue, float rightValue, params GUILayoutOption[] options )
      {
         return DoVerticalSlider( value, leftValue, rightValue, GUI.skin.verticalSlider, GUI.skin.verticalSliderThumb, options );
      }

      public static float VerticalSlider( float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options )
      {
         return DoVerticalSlider( value, leftValue, rightValue, slider, thumb, options );
      }

      private static float DoVerticalSlider( float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static float HorizontalScrollbar( float value, float size, float leftValue, float rightValue, params GUILayoutOption[] options )
      {
         return HorizontalScrollbar( value, size, leftValue, rightValue, GUI.skin.horizontalScrollbar, options );
      }

      public static float HorizontalScrollbar( float value, float size, float leftValue, float rightValue, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static float VerticalScrollbar( float value, float size, float topValue, float bottomValue, params GUILayoutOption[] options )
      {
         return VerticalScrollbar( value, size, topValue, bottomValue, GUI.skin.verticalScrollbar, options );
      }

      public static float VerticalScrollbar( float value, float size, float topValue, float bottomValue, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static void Space( float pixels ) => throw new NotImplementedException();

      public static void FlexibleSpace() => throw new NotImplementedException();

      public static void BeginHorizontal( params GUILayoutOption[] options )
      {
         BeginHorizontal( GUIContent.none, GUIStyle.none, options );
      }

      public static void BeginHorizontal( GUIStyle style, params GUILayoutOption[] options )
      {
         BeginHorizontal( GUIContent.none, style, options );
      }

      public static void BeginHorizontal( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         BeginHorizontal( GUIContent.Temp( text ), style, options );
      }

      public static void BeginHorizontal( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         BeginHorizontal( GUIContent.Temp( image ), style, options );
      }

      public static void BeginHorizontal( GUIContent content, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static void EndHorizontal() => throw new NotImplementedException();

      public static void BeginVertical( params GUILayoutOption[] options )
      {
         BeginVertical( GUIContent.none, GUIStyle.none, options );
      }

      public static void BeginVertical( GUIStyle style, params GUILayoutOption[] options )
      {
         BeginVertical( GUIContent.none, style, options );
      }

      public static void BeginVertical( string text, GUIStyle style, params GUILayoutOption[] options )
      {
         BeginVertical( GUIContent.Temp( text ), style, options );
      }

      public static void BeginVertical( Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         BeginVertical( GUIContent.Temp( image ), style, options );
      }

      public static void BeginVertical( GUIContent content, GUIStyle style, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static void EndVertical() => throw new NotImplementedException();

      public static void BeginArea( Rect screenRect )
      {
         BeginArea( screenRect, GUIContent.none, GUIStyle.none );
      }

      public static void BeginArea( Rect screenRect, string text )
      {
         BeginArea( screenRect, GUIContent.Temp( text ), GUIStyle.none );
      }

      public static void BeginArea( Rect screenRect, Texture image )
      {
         BeginArea( screenRect, GUIContent.Temp( image ), GUIStyle.none );
      }

      public static void BeginArea( Rect screenRect, GUIContent content )
      {
         BeginArea( screenRect, content, GUIStyle.none );
      }

      public static void BeginArea( Rect screenRect, GUIStyle style )
      {
         BeginArea( screenRect, GUIContent.none, style );
      }

      public static void BeginArea( Rect screenRect, string text, GUIStyle style )
      {
         BeginArea( screenRect, GUIContent.Temp( text ), style );
      }

      public static void BeginArea( Rect screenRect, Texture image, GUIStyle style )
      {
         BeginArea( screenRect, GUIContent.Temp( image ), style );
      }

      public static void BeginArea( Rect screenRect, GUIContent content, GUIStyle style ) => throw new NotImplementedException();

      public static void EndArea() => throw new NotImplementedException();

      public static Vector2 BeginScrollView( Vector2 scrollPosition, params GUILayoutOption[] options )
      {
         return BeginScrollView( scrollPosition, alwaysShowHorizontal: false, alwaysShowVertical: false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options )
      {
         return BeginScrollView( scrollPosition, alwaysShowHorizontal, alwaysShowVertical, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options )
      {
         return BeginScrollView( scrollPosition, alwaysShowHorizontal: false, alwaysShowVertical: false, horizontalScrollbar, verticalScrollbar, GUI.skin.scrollView, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, GUIStyle style )
      {
         GUILayoutOption[] options = null;
         return BeginScrollView( scrollPosition, style, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options )
      {
         string name = style.name;
         GUIStyle gUIStyle = GUI.skin.FindStyle( name + "VerticalScrollbar" );
         if( gUIStyle == null )
         {
            gUIStyle = GUI.skin.verticalScrollbar;
         }

         GUIStyle gUIStyle2 = GUI.skin.FindStyle( name + "HorizontalScrollbar" );
         if( gUIStyle2 == null )
         {
            gUIStyle2 = GUI.skin.horizontalScrollbar;
         }

         return BeginScrollView( scrollPosition, alwaysShowHorizontal: false, alwaysShowVertical: false, gUIStyle2, gUIStyle, style, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options )
      {
         return BeginScrollView( scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, GUI.skin.scrollView, options );
      }

      public static Vector2 BeginScrollView( Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options ) => throw new NotImplementedException();

      public static void EndScrollView()
      {
         EndScrollView( handleScrollWheel: true );
      }

      internal static void EndScrollView( bool handleScrollWheel ) => throw new NotImplementedException();

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, string text, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, GUIContent.Temp( text ), GUI.skin.window, options );
      }

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, Texture image, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, GUIContent.Temp( image ), GUI.skin.window, options );
      }

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, GUIContent content, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, content, GUI.skin.window, options );
      }

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, string text, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, GUIContent.Temp( text ), style, options );
      }

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, Texture image, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, GUIContent.Temp( image ), style, options );
      }

      public static Rect Window( int id, Rect screenRect, GUI.WindowFunction func, GUIContent content, GUIStyle style, params GUILayoutOption[] options )
      {
         return DoWindow( id, screenRect, func, content, style, options );
      }

      private static Rect DoWindow( int id, Rect screenRect, GUI.WindowFunction func, GUIContent content, GUIStyle style, GUILayoutOption[] options ) => throw new NotImplementedException();

      public static GUILayoutOption Width( float width )
      {
         return new GUILayoutOption( GUILayoutOption.Type.fixedWidth, width );
      }

      public static GUILayoutOption MinWidth( float minWidth )
      {
         return new GUILayoutOption( GUILayoutOption.Type.minWidth, minWidth );
      }

      public static GUILayoutOption MaxWidth( float maxWidth )
      {
         return new GUILayoutOption( GUILayoutOption.Type.maxWidth, maxWidth );
      }

      public static GUILayoutOption Height( float height )
      {
         return new GUILayoutOption( GUILayoutOption.Type.fixedHeight, height );
      }

      public static GUILayoutOption MinHeight( float minHeight )
      {
         return new GUILayoutOption( GUILayoutOption.Type.minHeight, minHeight );
      }

      public static GUILayoutOption MaxHeight( float maxHeight )
      {
         return new GUILayoutOption( GUILayoutOption.Type.maxHeight, maxHeight );
      }

      public static GUILayoutOption ExpandWidth( bool expand )
      {
         return new GUILayoutOption( GUILayoutOption.Type.stretchWidth, expand ? 1 : 0 );
      }

      public static GUILayoutOption ExpandHeight( bool expand )
      {
         return new GUILayoutOption( GUILayoutOption.Type.stretchHeight, expand ? 1 : 0 );
      }
   }
}
