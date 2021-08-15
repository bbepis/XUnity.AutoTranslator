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
   public sealed class GL
   {
      public const int TRIANGLES = 4;

      public const int TRIANGLE_STRIP = 5;

      public const int QUADS = 7;

      public const int LINES = 1;

      public const int LINE_STRIP = 2;

      public static Matrix4x4 modelview
      {
         get
         {
            INTERNAL_get_modelview( out Matrix4x4 value );
            return value;
         }
         set
         {
            INTERNAL_set_modelview( ref value );
         }
      }

      public static bool wireframe
      {
   
   
         get;
   
   
         set;
      }

      public static bool sRGBWrite
      {
   
   
         get;
   
   
         set;
      }

      public static bool invertCulling
      {
   
   
         get;
   
   
         set;
      }



      public static extern void Vertex3( float x, float y, float z );

      public static void Vertex( Vector3 v )
      {
         INTERNAL_CALL_Vertex( ref v );
      }



      private static extern void INTERNAL_CALL_Vertex( ref Vector3 v );

      public static void Color( Color c )
      {
         INTERNAL_CALL_Color( ref c );
      }



      private static extern void INTERNAL_CALL_Color( ref Color c );

      public static void TexCoord( Vector3 v )
      {
         INTERNAL_CALL_TexCoord( ref v );
      }



      private static extern void INTERNAL_CALL_TexCoord( ref Vector3 v );



      public static extern void TexCoord2( float x, float y );



      public static extern void TexCoord3( float x, float y, float z );



      public static extern void MultiTexCoord2( int unit, float x, float y );



      public static extern void MultiTexCoord3( int unit, float x, float y, float z );

      public static void MultiTexCoord( int unit, Vector3 v )
      {
         INTERNAL_CALL_MultiTexCoord( unit, ref v );
      }



      private static extern void INTERNAL_CALL_MultiTexCoord( int unit, ref Vector3 v );



      private static extern void BeginInternal( int mode );

      public static void Begin( int mode )
      {
         BeginInternal( mode );
      }



      public static extern void End();



      public static extern void PushMatrix();



      public static extern void PopMatrix();



      public static extern void LoadIdentity();



      public static extern void LoadOrtho();



      public static extern void LoadPixelMatrix();



      private static extern void LoadPixelMatrixArgs( float left, float right, float bottom, float top );

      public static void LoadPixelMatrix( float left, float right, float bottom, float top )
      {
         LoadPixelMatrixArgs( left, right, bottom, top );
      }

      public static void Viewport( Rect pixelRect )
      {
         INTERNAL_CALL_Viewport( ref pixelRect );
      }



      private static extern void INTERNAL_CALL_Viewport( ref Rect pixelRect );

      public static void LoadProjectionMatrix( Matrix4x4 mat )
      {
         INTERNAL_CALL_LoadProjectionMatrix( ref mat );
      }



      private static extern void INTERNAL_CALL_LoadProjectionMatrix( ref Matrix4x4 mat );



      private static extern void INTERNAL_get_modelview( out Matrix4x4 value );



      private static extern void INTERNAL_set_modelview( ref Matrix4x4 value );

      public static void MultMatrix( Matrix4x4 mat )
      {
         INTERNAL_CALL_MultMatrix( ref mat );
      }



      private static extern void INTERNAL_CALL_MultMatrix( ref Matrix4x4 mat );

      public static Matrix4x4 GetGPUProjectionMatrix( Matrix4x4 proj, bool renderIntoTexture )
      {
         INTERNAL_CALL_GetGPUProjectionMatrix( ref proj, renderIntoTexture, out Matrix4x4 value );
         return value;
      }



      private static extern void INTERNAL_CALL_GetGPUProjectionMatrix( ref Matrix4x4 proj, bool renderIntoTexture, out Matrix4x4 value );


      [Obsolete( "Use invertCulling property" )]

      public static extern void SetRevertBackfacing( bool revertBackFaces );

      public static void Clear( bool clearDepth, bool clearColor, Color backgroundColor )
      {
         float depth = 1f;
         Clear( clearDepth, clearColor, backgroundColor, depth );
      }

      public static void Clear( bool clearDepth, bool clearColor, Color backgroundColor, float depth )
      {
         Internal_Clear( clearDepth, clearColor, backgroundColor, depth );
      }

      private static void Internal_Clear( bool clearDepth, bool clearColor, Color backgroundColor, float depth )
      {
         INTERNAL_CALL_Internal_Clear( clearDepth, clearColor, ref backgroundColor, depth );
      }



      private static extern void INTERNAL_CALL_Internal_Clear( bool clearDepth, bool clearColor, ref Color backgroundColor, float depth );






      public static extern void Flush();



      public static extern void InvalidateState();


      [Obsolete( "IssuePluginEvent(eventID) is deprecated. Use IssuePluginEvent(callback, eventID) instead." )]

      public static extern void IssuePluginEvent( int eventID );

      public static void IssuePluginEvent( IntPtr callback, int eventID )
      {
         if( callback == IntPtr.Zero )
         {
            throw new ArgumentException( "Null callback specified." );
         }

         IssuePluginEventInternal( callback, eventID );
      }



      private static extern void IssuePluginEventInternal( IntPtr callback, int eventID );



      public static extern void RenderTargetBarrier();
   }
}
