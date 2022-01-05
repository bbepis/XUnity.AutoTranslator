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
   public sealed class Shader : Object
   {
      public Shader( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public bool isSupported
      {
         get;
      }

      public int maximumLOD
      {
         get;
         set;
      }

      public static int globalMaximumLOD
      {
         get;
         set;
      }

      public static string globalRenderPipeline
      {
         get;
         set;
      }

      public int renderQueue
      {
         get;
      }

      internal DisableBatchingType disableBatching
      {
         get;
      }

      [Obsolete( "Use Graphics.activeTier instead (UnityUpgradable) -> UnityEngine.Graphics.activeTier", false )]
      public static ShaderHardwareTier globalShaderHardwareTier => throw new NotImplementedException();

      public static extern Shader Find( string name );

      internal static extern Shader FindBuiltin( string name );

      public static extern void EnableKeyword( string keyword );

      public static extern void DisableKeyword( string keyword );

      public static extern bool IsKeywordEnabled( string keyword );

      private static extern void SetGlobalFloatImpl( int nameID, float value );

      private static extern void SetGlobalIntImpl( int nameID, int value );

      private static void SetGlobalVectorImpl( int nameID, Vector4 value )
      {
         INTERNAL_CALL_SetGlobalVectorImpl( nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetGlobalVectorImpl( int nameID, ref Vector4 value );

      private static void SetGlobalColorImpl( int nameID, Color value )
      {
         INTERNAL_CALL_SetGlobalColorImpl( nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetGlobalColorImpl( int nameID, ref Color value );

      private static void SetGlobalMatrixImpl( int nameID, Matrix4x4 value )
      {
         INTERNAL_CALL_SetGlobalMatrixImpl( nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetGlobalMatrixImpl( int nameID, ref Matrix4x4 value );

      private static extern void SetGlobalTextureImpl( int nameID, Texture value );

      private static extern Array ExtractArrayFromList( object list );

      private static extern void SetGlobalFloatArrayImpl( int nameID, float[] values );

      private static extern void SetGlobalVectorArrayImpl( int nameID, Vector4[] values );

      private static extern void SetGlobalMatrixArrayImpl( int nameID, Matrix4x4[] values );

      private static extern float GetGlobalFloatImpl( int nameID );

      private static extern int GetGlobalIntImpl( int nameID );

      private static Vector4 GetGlobalVectorImpl( int nameID )
      {
         INTERNAL_CALL_GetGlobalVectorImpl( nameID, out Vector4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetGlobalVectorImpl( int nameID, out Vector4 value );

      private static Color GetGlobalColorImpl( int nameID )
      {
         INTERNAL_CALL_GetGlobalColorImpl( nameID, out Color value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetGlobalColorImpl( int nameID, out Color value );

      private static Matrix4x4 GetGlobalMatrixImpl( int nameID )
      {
         INTERNAL_CALL_GetGlobalMatrixImpl( nameID, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetGlobalMatrixImpl( int nameID, out Matrix4x4 value );

      private static extern Texture GetGlobalTextureImpl( int nameID );

      private static extern float[] GetGlobalFloatArrayImpl( int nameID );

      private static extern Vector4[] GetGlobalVectorArrayImpl( int nameID );

      private static extern Matrix4x4[] GetGlobalMatrixArrayImpl( int nameID );

      private static extern void GetGlobalFloatArrayImplList( int nameID, object list );

      private static extern void GetGlobalVectorArrayImplList( int nameID, object list );

      private static extern void GetGlobalMatrixArrayImplList( int nameID, object list );

      public static extern int PropertyToID( string name );

      public static extern void WarmupAllShaders();

      public static void SetGlobalFloat( string name, float value )
      {
         SetGlobalFloat( PropertyToID( name ), value );
      }

      public static void SetGlobalFloat( int nameID, float value )
      {
         SetGlobalFloatImpl( nameID, value );
      }

      public static void SetGlobalInt( string name, int value )
      {
         SetGlobalInt( PropertyToID( name ), value );
      }

      public static void SetGlobalInt( int nameID, int value )
      {
         SetGlobalIntImpl( nameID, value );
      }

      public static void SetGlobalVector( string name, Vector4 value )
      {
         SetGlobalVector( PropertyToID( name ), value );
      }

      public static void SetGlobalVector( int nameID, Vector4 value )
      {
         SetGlobalVectorImpl( nameID, value );
      }

      public static void SetGlobalColor( string name, Color value )
      {
         SetGlobalColor( PropertyToID( name ), value );
      }

      public static void SetGlobalColor( int nameID, Color value )
      {
         SetGlobalColorImpl( nameID, value );
      }

      public static void SetGlobalMatrix( string name, Matrix4x4 value )
      {
         SetGlobalMatrix( PropertyToID( name ), value );
      }

      public static void SetGlobalMatrix( int nameID, Matrix4x4 value )
      {
         SetGlobalMatrixImpl( nameID, value );
      }

      public static void SetGlobalTexture( string name, Texture value )
      {
         SetGlobalTexture( PropertyToID( name ), value );
      }

      public static void SetGlobalTexture( int nameID, Texture value )
      {
         SetGlobalTextureImpl( nameID, value );
      }

      public static void SetGlobalFloatArray( string name, List<float> values )
      {
         SetGlobalFloatArray( PropertyToID( name ), values );
      }

      public static void SetGlobalFloatArray( int nameID, List<float> values )
      {
         SetGlobalFloatArray( nameID, (float[])ExtractArrayFromList( values ) );
      }

      public static void SetGlobalFloatArray( string name, float[] values )
      {
         SetGlobalFloatArray( PropertyToID( name ), values );
      }

      public static void SetGlobalFloatArray( int nameID, float[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetGlobalFloatArrayImpl( nameID, values );
      }

      public static void SetGlobalVectorArray( string name, List<Vector4> values )
      {
         SetGlobalVectorArray( PropertyToID( name ), values );
      }

      public static void SetGlobalVectorArray( int nameID, List<Vector4> values )
      {
         SetGlobalVectorArray( nameID, (Vector4[])ExtractArrayFromList( values ) );
      }

      public static void SetGlobalVectorArray( string name, Vector4[] values )
      {
         SetGlobalVectorArray( PropertyToID( name ), values );
      }

      public static void SetGlobalVectorArray( int nameID, Vector4[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetGlobalVectorArrayImpl( nameID, values );
      }

      public static void SetGlobalMatrixArray( string name, List<Matrix4x4> values )
      {
         SetGlobalMatrixArray( PropertyToID( name ), values );
      }

      public static void SetGlobalMatrixArray( int nameID, List<Matrix4x4> values )
      {
         SetGlobalMatrixArray( nameID, (Matrix4x4[])ExtractArrayFromList( values ) );
      }

      public static void SetGlobalMatrixArray( string name, Matrix4x4[] values )
      {
         SetGlobalMatrixArray( PropertyToID( name ), values );
      }

      public static void SetGlobalMatrixArray( int nameID, Matrix4x4[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetGlobalMatrixArrayImpl( nameID, values );
      }

      public static float GetGlobalFloat( string name )
      {
         return GetGlobalFloat( PropertyToID( name ) );
      }

      public static float GetGlobalFloat( int nameID )
      {
         return GetGlobalFloatImpl( nameID );
      }

      public static int GetGlobalInt( string name )
      {
         return GetGlobalInt( PropertyToID( name ) );
      }

      public static int GetGlobalInt( int nameID )
      {
         return GetGlobalIntImpl( nameID );
      }

      public static Vector4 GetGlobalVector( string name )
      {
         return GetGlobalVector( PropertyToID( name ) );
      }

      public static Vector4 GetGlobalVector( int nameID )
      {
         return GetGlobalVectorImpl( nameID );
      }

      public static Color GetGlobalColor( string name )
      {
         return GetGlobalColor( PropertyToID( name ) );
      }

      public static Color GetGlobalColor( int nameID )
      {
         return GetGlobalColorImpl( nameID );
      }

      public static Matrix4x4 GetGlobalMatrix( string name )
      {
         return GetGlobalMatrix( PropertyToID( name ) );
      }

      public static Matrix4x4 GetGlobalMatrix( int nameID )
      {
         return GetGlobalMatrixImpl( nameID );
      }

      public static Texture GetGlobalTexture( string name )
      {
         return GetGlobalTexture( PropertyToID( name ) );
      }

      public static Texture GetGlobalTexture( int nameID )
      {
         return GetGlobalTextureImpl( nameID );
      }

      public static void GetGlobalFloatArray( string name, List<float> values )
      {
         GetGlobalFloatArray( PropertyToID( name ), values );
      }

      public static void GetGlobalFloatArray( int nameID, List<float> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetGlobalFloatArrayImplList( nameID, values );
      }

      public static float[] GetGlobalFloatArray( string name )
      {
         return GetGlobalFloatArray( PropertyToID( name ) );
      }

      public static float[] GetGlobalFloatArray( int nameID )
      {
         return GetGlobalFloatArrayImpl( nameID );
      }

      public static void GetGlobalVectorArray( string name, List<Vector4> values )
      {
         GetGlobalVectorArray( PropertyToID( name ), values );
      }

      public static void GetGlobalVectorArray( int nameID, List<Vector4> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetGlobalVectorArrayImplList( nameID, values );
      }

      public static Vector4[] GetGlobalVectorArray( string name )
      {
         return GetGlobalVectorArray( PropertyToID( name ) );
      }

      public static Vector4[] GetGlobalVectorArray( int nameID )
      {
         return GetGlobalVectorArrayImpl( nameID );
      }

      public static void GetGlobalMatrixArray( string name, List<Matrix4x4> values )
      {
         GetGlobalMatrixArray( PropertyToID( name ), values );
      }

      public static void GetGlobalMatrixArray( int nameID, List<Matrix4x4> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetGlobalMatrixArrayImplList( nameID, values );
      }

      public static Matrix4x4[] GetGlobalMatrixArray( string name )
      {
         return GetGlobalMatrixArray( PropertyToID( name ) );
      }

      public static Matrix4x4[] GetGlobalMatrixArray( int nameID )
      {
         return GetGlobalMatrixArrayImpl( nameID );
      }
   }
}
