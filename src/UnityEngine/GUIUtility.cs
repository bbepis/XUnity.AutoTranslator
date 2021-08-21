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
   public class GUIUtility
   {
      internal static int s_SkinMode;

      internal static int s_OriginalID;

      internal static Vector2 s_EditorScreenPointOffset = Vector2.zero;

      internal static float pixelsPerPoint => Internal_GetPixelsPerPoint();

      internal static bool guiIsExiting
      {
         get;
         set;
      }

      public static int hotControl
      {
         get
         {
            return Internal_GetHotControl();
         }
         set
         {
            Internal_SetHotControl( value );
         }
      }

      public static int keyboardControl
      {
         get
         {
            return Internal_GetKeyboardControl();
         }
         set
         {
            Internal_SetKeyboardControl( value );
         }
      }

      public static string systemCopyBuffer
      {
   
   
         get;
   
   
         set;
      }

      internal static bool mouseUsed
      {
   
   
         get;
   
   
         set;
      }

      public static bool hasModalWindow
      {
   
   
         get;
      }

      internal static bool textFieldInput
      {
   
   
         get;
   
   
         set;
      }

      public static int GetControlID( FocusType focus )
      {
         return GetControlID( 0, focus );
      }

      public static int GetControlID( GUIContent contents, FocusType focus )
      {
         return GetControlID( contents.hash, focus );
      }

      public static int GetControlID( FocusType focus, Rect position )
      {
         return Internal_GetNextControlID2( 0, focus, position );
      }

      public static int GetControlID( int hint, FocusType focus, Rect position )
      {
         return Internal_GetNextControlID2( hint, focus, position );
      }

      public static int GetControlID( GUIContent contents, FocusType focus, Rect position )
      {
         return Internal_GetNextControlID2( contents.hash, focus, position );
      }

#if IL2CPP
      public static object GetStateObject( Il2CppSystem.Type t, int controlID ) => throw new NotImplementedException();
#else
      public static object GetStateObject( Type t, int controlID ) => throw new NotImplementedException();
#endif


      public static object QueryStateObject( Type t, int controlID ) => throw new NotImplementedException();

      public static void ExitGUI() => throw new NotImplementedException();

      internal static GUISkin GetDefaultSkin()
      {
         return Internal_GetDefaultSkin( s_SkinMode );
      }

      internal static GUISkin GetBuiltinSkin( int skin )
      {
         return Internal_GetBuiltinSkin( skin ) as GUISkin;
      }

      internal static bool ProcessEvent( int instanceID, IntPtr nativeEventPtr )
      {
         return false;
      }

      internal static void CleanupRoots()
      {
      }

      internal static void BeginGUI( int skinMode, int instanceID, int useGUILayout ) => throw new NotImplementedException();

      internal static void EndGUI( int layoutType ) => throw new NotImplementedException();

      internal static bool EndGUIFromException( Exception exception )
      {
         Internal_ExitGUI();
         return ShouldRethrowException( exception );
      }

      internal static bool EndContainerGUIFromException( Exception exception )
      {
         return ShouldRethrowException( exception );
      }

      internal static bool ShouldRethrowException( Exception exception ) => throw new NotImplementedException();

      internal static void CheckOnGUI()
      {
         if( Internal_GetGUIDepth() <= 0 )
         {
            throw new ArgumentException( "You can only call GUI functions from inside OnGUI." );
         }
      }

      public static Vector2 GUIToScreenPoint( Vector2 guiPoint ) => throw new NotImplementedException();

      internal static Rect GUIToScreenRect( Rect guiRect )
      {
         Vector2 vector = GUIToScreenPoint( new Vector2( guiRect.x, guiRect.y ) );
         guiRect.x = vector.x;
         guiRect.y = vector.y;
         return guiRect;
      }

      public static Vector2 ScreenToGUIPoint( Vector2 screenPoint ) => throw new NotImplementedException();

      public static Rect ScreenToGUIRect( Rect screenRect )
      {
         Vector2 vector = ScreenToGUIPoint( new Vector2( screenRect.x, screenRect.y ) );
         screenRect.x = vector.x;
         screenRect.y = vector.y;
         return screenRect;
      }

      public static void RotateAroundPivot( float angle, Vector2 pivotPoint ) => throw new NotImplementedException();

      public static void ScaleAroundPivot( Vector2 scale, Vector2 pivotPoint ) => throw new NotImplementedException();



      private static extern float Internal_GetPixelsPerPoint();



      public static extern int GetControlID( int hint, FocusType focus );

      private static int Internal_GetNextControlID2( int hint, FocusType focusType, Rect rect )
      {
         return INTERNAL_CALL_Internal_GetNextControlID2( hint, focusType, ref rect );
      }



      private static extern int INTERNAL_CALL_Internal_GetNextControlID2( int hint, FocusType focusType, ref Rect rect );



      internal static extern int GetPermanentControlID();



      private static extern int Internal_GetHotControl();



      private static extern void Internal_SetHotControl( int value );



      internal static extern void UpdateUndoName();



      internal static extern bool GetChanged();



      internal static extern void SetChanged( bool changed );



      private static extern int Internal_GetKeyboardControl();



      private static extern void Internal_SetKeyboardControl( int value );



      internal static extern void SetDidGUIWindowsEatLastEvent( bool value );



      private static extern GUISkin Internal_GetDefaultSkin( int skinMode );



      private static extern Object Internal_GetBuiltinSkin( int skin );



      private static extern void Internal_ExitGUI();



      internal static extern int Internal_GetGUIDepth();



      private static extern void Internal_BeginContainer( int instanceID );



      private static extern void Internal_EndContainer();
   }
}
