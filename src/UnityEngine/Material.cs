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
   public class Material : Object
   {
      public Material( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();
      public Shader shader
      {
         get;
         set;
      }

      public Color color
      {
         get
         {
            return GetColor( "_Color" );
         }
         set
         {
            SetColor( "_Color", value );
         }
      }

      public Texture mainTexture
      {
         get
         {
            return GetTexture( "_MainTex" );
         }
         set
         {
            SetTexture( "_MainTex", value );
         }
      }

      public Vector2 mainTextureOffset
      {
         get
         {
            return GetTextureOffset( "_MainTex" );
         }
         set
         {
            SetTextureOffset( "_MainTex", value );
         }
      }

      public Vector2 mainTextureScale
      {
         get
         {
            return GetTextureScale( "_MainTex" );
         }
         set
         {
            SetTextureScale( "_MainTex", value );
         }
      }

      public int passCount
      {
         get;
      }

      public int renderQueue
      {
         get;
         set;
      }

      public string[] shaderKeywords
      {
         get;
         set;
      }

      public MaterialGlobalIlluminationFlags globalIlluminationFlags
      {
         get;
         set;
      }

      public bool enableInstancing
      {
         get;
         set;
      }

      public bool doubleSidedGI
      {
         get;
         set;
      }

      [Obsolete( "Creating materials from shader source string is no longer supported. Use Shader assets instead." )]
      public Material( string contents ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Material( Shader shader ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Material( Material source ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      private extern void SetFloatImpl( int nameID, float value );

      private extern void SetIntImpl( int nameID, int value );

      private void SetColorImpl( int nameID, Color value )
      {
         INTERNAL_CALL_SetColorImpl( this, nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetColorImpl( Material self, int nameID, ref Color value );

      private void SetVectorImpl( int nameID, Vector4 value )
      {
         INTERNAL_CALL_SetVectorImpl( this, nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetVectorImpl( Material self, int nameID, ref Vector4 value );

      private void SetMatrixImpl( int nameID, Matrix4x4 value )
      {
         INTERNAL_CALL_SetMatrixImpl( this, nameID, ref value );
      }

      private static extern void INTERNAL_CALL_SetMatrixImpl( Material self, int nameID, ref Matrix4x4 value );

      private extern void SetTextureImpl( int nameID, Texture value );

      private extern void SetFloatArrayImpl( int nameID, float[] values );

      private extern void SetVectorArrayImpl( int nameID, Vector4[] values );

      private extern void SetMatrixArrayImpl( int nameID, Matrix4x4[] values );

      private static extern Array ExtractArrayFromList( object list );

      private extern float GetFloatImpl( int nameID );

      private extern int GetIntImpl( int nameID );

      private Color GetColorImpl( int nameID )
      {
         INTERNAL_CALL_GetColorImpl( this, nameID, out Color value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetColorImpl( Material self, int nameID, out Color value );

      private Vector4 GetVectorImpl( int nameID )
      {
         INTERNAL_CALL_GetVectorImpl( this, nameID, out Vector4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetVectorImpl( Material self, int nameID, out Vector4 value );

      private Matrix4x4 GetMatrixImpl( int nameID )
      {
         INTERNAL_CALL_GetMatrixImpl( this, nameID, out Matrix4x4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetMatrixImpl( Material self, int nameID, out Matrix4x4 value );

      private extern Texture GetTextureImpl( int nameID );

      private extern float[] GetFloatArrayImpl( int nameID );

      private extern Vector4[] GetVectorArrayImpl( int nameID );

      private extern Matrix4x4[] GetMatrixArrayImpl( int nameID );

      private extern void GetFloatArrayImplList( int nameID, object list );

      private extern void GetVectorArrayImplList( int nameID, object list );

      private extern void GetMatrixArrayImplList( int nameID, object list );

      private extern void SetColorArrayImpl( int nameID, Color[] values );

      private extern void SetColorArrayImplList( int nameID, object values );

      private extern Color[] GetColorArrayImpl( int nameID );

      private extern void GetColorArrayImplList( int nameID, object list );

      private Vector4 GetTextureScaleAndOffsetImpl( int nameID )
      {
         INTERNAL_CALL_GetTextureScaleAndOffsetImpl( this, nameID, out Vector4 value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetTextureScaleAndOffsetImpl( Material self, int nameID, out Vector4 value );

      private void SetTextureOffsetImpl( int nameID, Vector2 offset )
      {
         INTERNAL_CALL_SetTextureOffsetImpl( this, nameID, ref offset );
      }

      private static extern void INTERNAL_CALL_SetTextureOffsetImpl( Material self, int nameID, ref Vector2 offset );

      private void SetTextureScaleImpl( int nameID, Vector2 scale )
      {
         INTERNAL_CALL_SetTextureScaleImpl( this, nameID, ref scale );
      }

      private static extern void INTERNAL_CALL_SetTextureScaleImpl( Material self, int nameID, ref Vector2 scale );

      public bool HasProperty( string propertyName )
      {
         return HasProperty( Shader.PropertyToID( propertyName ) );
      }

      public extern bool HasProperty( int nameID );

      public extern string GetTag( string tag, bool searchFallbacks, string defaultValue );

      public string GetTag( string tag, bool searchFallbacks )
      {
         string defaultValue = "";
         return GetTag( tag, searchFallbacks, defaultValue );
      }

      public extern void SetOverrideTag( string tag, string val );

      public extern void SetShaderPassEnabled( string passName, bool enabled );

      public extern bool GetShaderPassEnabled( string passName );

      public extern void Lerp( Material start, Material end, float t );

      public extern bool SetPass( int pass );

      public extern string GetPassName( int pass );

      public extern int FindPass( string passName );

      [Obsolete( "Creating materials from shader source string will be removed in the future. Use Shader assets instead." )]
      public static Material Create( string scriptContents )
      {
         return new Material( scriptContents );
      }

      private static extern void Internal_CreateWithString( Material mono, string contents );

      private static extern void Internal_CreateWithShader( Material mono, Shader shader );

      private static extern void Internal_CreateWithMaterial( Material mono, Material source );

      public extern void CopyPropertiesFromMaterial( Material mat );

      public extern void EnableKeyword( string keyword );

      public extern void DisableKeyword( string keyword );

      public extern bool IsKeywordEnabled( string keyword );

      public void SetFloat( string name, float value )
      {
         SetFloat( Shader.PropertyToID( name ), value );
      }

      public void SetFloat( int nameID, float value )
      {
         SetFloatImpl( nameID, value );
      }

      public void SetInt( string name, int value )
      {
         SetInt( Shader.PropertyToID( name ), value );
      }

      public void SetInt( int nameID, int value )
      {
         SetIntImpl( nameID, value );
      }

      public void SetColor( string name, Color value )
      {
         SetColor( Shader.PropertyToID( name ), value );
      }

      public void SetColor( int nameID, Color value )
      {
         SetColorImpl( nameID, value );
      }

      public void SetVector( string name, Vector4 value )
      {
         SetVector( Shader.PropertyToID( name ), value );
      }

      public void SetVector( int nameID, Vector4 value )
      {
         SetVectorImpl( nameID, value );
      }

      public void SetMatrix( string name, Matrix4x4 value )
      {
         SetMatrix( Shader.PropertyToID( name ), value );
      }

      public void SetMatrix( int nameID, Matrix4x4 value )
      {
         SetMatrixImpl( nameID, value );
      }

      public void SetTexture( string name, Texture value )
      {
         SetTexture( Shader.PropertyToID( name ), value );
      }

      public void SetTexture( int nameID, Texture value )
      {
         SetTextureImpl( nameID, value );
      }

      public void SetTextureOffset( string name, Vector2 value )
      {
         SetTextureOffset( Shader.PropertyToID( name ), value );
      }

      public void SetTextureOffset( int nameID, Vector2 value )
      {
         SetTextureOffsetImpl( nameID, value );
      }

      public void SetTextureScale( string name, Vector2 value )
      {
         SetTextureScale( Shader.PropertyToID( name ), value );
      }

      public void SetTextureScale( int nameID, Vector2 value )
      {
         SetTextureScaleImpl( nameID, value );
      }

      public void SetFloatArray( string name, List<float> values )
      {
         SetFloatArray( Shader.PropertyToID( name ), values );
      }

      public void SetFloatArray( int nameID, List<float> values )
      {
         SetFloatArray( nameID, (float[])ExtractArrayFromList( values ) );
      }

      public void SetFloatArray( string name, float[] values )
      {
         SetFloatArray( Shader.PropertyToID( name ), values );
      }

      public void SetFloatArray( int nameID, float[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetFloatArrayImpl( nameID, values );
      }

      public void SetColorArray( string name, List<Color> values )
      {
         SetColorArray( Shader.PropertyToID( name ), values );
      }

      public void SetColorArray( int nameID, List<Color> values )
      {
         SetColorArray( nameID, (Color[])ExtractArrayFromList( values ) );
      }

      public void SetColorArray( string name, Color[] values )
      {
         SetColorArray( Shader.PropertyToID( name ), values );
      }

      public void SetColorArray( int nameID, Color[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetColorArrayImpl( nameID, values );
      }

      public void SetVectorArray( string name, List<Vector4> values )
      {
         SetVectorArray( Shader.PropertyToID( name ), values );
      }

      public void SetVectorArray( int nameID, List<Vector4> values )
      {
         SetVectorArray( nameID, (Vector4[])ExtractArrayFromList( values ) );
      }

      public void SetVectorArray( string name, Vector4[] values )
      {
         SetVectorArray( Shader.PropertyToID( name ), values );
      }

      public void SetVectorArray( int nameID, Vector4[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetVectorArrayImpl( nameID, values );
      }

      public void SetMatrixArray( string name, List<Matrix4x4> values )
      {
         SetMatrixArray( Shader.PropertyToID( name ), values );
      }

      public void SetMatrixArray( int nameID, List<Matrix4x4> values )
      {
         SetMatrixArray( nameID, (Matrix4x4[])ExtractArrayFromList( values ) );
      }

      public void SetMatrixArray( string name, Matrix4x4[] values )
      {
         SetMatrixArray( Shader.PropertyToID( name ), values );
      }

      public void SetMatrixArray( int nameID, Matrix4x4[] values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         if( values.Length == 0 )
         {
            throw new ArgumentException( "Zero-sized array is not allowed." );
         }

         SetMatrixArrayImpl( nameID, values );
      }

      public float GetFloat( string name )
      {
         return GetFloat( Shader.PropertyToID( name ) );
      }

      public float GetFloat( int nameID )
      {
         return GetFloatImpl( nameID );
      }

      public int GetInt( string name )
      {
         return GetInt( Shader.PropertyToID( name ) );
      }

      public int GetInt( int nameID )
      {
         return GetIntImpl( nameID );
      }

      public Color GetColor( string name )
      {
         return GetColor( Shader.PropertyToID( name ) );
      }

      public Color GetColor( int nameID )
      {
         return GetColorImpl( nameID );
      }

      public Vector4 GetVector( string name )
      {
         return GetVector( Shader.PropertyToID( name ) );
      }

      public Vector4 GetVector( int nameID )
      {
         return GetVectorImpl( nameID );
      }

      public Matrix4x4 GetMatrix( string name )
      {
         return GetMatrix( Shader.PropertyToID( name ) );
      }

      public Matrix4x4 GetMatrix( int nameID )
      {
         return GetMatrixImpl( nameID );
      }

      public void GetFloatArray( string name, List<float> values )
      {
         GetFloatArray( Shader.PropertyToID( name ), values );
      }

      public void GetFloatArray( int nameID, List<float> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetFloatArrayImplList( nameID, values );
      }

      public float[] GetFloatArray( string name )
      {
         return GetFloatArray( Shader.PropertyToID( name ) );
      }

      public float[] GetFloatArray( int nameID )
      {
         return GetFloatArrayImpl( nameID );
      }

      public void GetVectorArray( string name, List<Vector4> values )
      {
         GetVectorArray( Shader.PropertyToID( name ), values );
      }

      public void GetVectorArray( int nameID, List<Vector4> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetVectorArrayImplList( nameID, values );
      }

      public Color[] GetColorArray( string name )
      {
         return GetColorArray( Shader.PropertyToID( name ) );
      }

      public Color[] GetColorArray( int nameID )
      {
         return GetColorArrayImpl( nameID );
      }

      public void GetColorArray( string name, List<Color> values )
      {
         GetColorArray( Shader.PropertyToID( name ), values );
      }

      public void GetColorArray( int nameID, List<Color> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetColorArrayImplList( nameID, values );
      }

      public Vector4[] GetVectorArray( string name )
      {
         return GetVectorArray( Shader.PropertyToID( name ) );
      }

      public Vector4[] GetVectorArray( int nameID )
      {
         return GetVectorArrayImpl( nameID );
      }

      public void GetMatrixArray( string name, List<Matrix4x4> values )
      {
         GetMatrixArray( Shader.PropertyToID( name ), values );
      }

      public void GetMatrixArray( int nameID, List<Matrix4x4> values )
      {
         if( values == null )
         {
            throw new ArgumentNullException( "values" );
         }

         GetMatrixArrayImplList( nameID, values );
      }

      public Matrix4x4[] GetMatrixArray( string name )
      {
         return GetMatrixArray( Shader.PropertyToID( name ) );
      }

      public Matrix4x4[] GetMatrixArray( int nameID )
      {
         return GetMatrixArrayImpl( nameID );
      }

      public Texture GetTexture( string name )
      {
         return GetTexture( Shader.PropertyToID( name ) );
      }

      public Texture GetTexture( int nameID )
      {
         return GetTextureImpl( nameID );
      }

      public Vector2 GetTextureOffset( string name )
      {
         return GetTextureOffset( Shader.PropertyToID( name ) );
      }

      public Vector2 GetTextureOffset( int nameID )
      {
         Vector4 textureScaleAndOffsetImpl = GetTextureScaleAndOffsetImpl( nameID );
         return new Vector2( textureScaleAndOffsetImpl.z, textureScaleAndOffsetImpl.w );
      }

      public Vector2 GetTextureScale( string name )
      {
         return GetTextureScale( Shader.PropertyToID( name ) );
      }

      public Vector2 GetTextureScale( int nameID )
      {
         Vector4 textureScaleAndOffsetImpl = GetTextureScaleAndOffsetImpl( nameID );
         return new Vector2( textureScaleAndOffsetImpl.x, textureScaleAndOffsetImpl.y );
      }
   }
}
