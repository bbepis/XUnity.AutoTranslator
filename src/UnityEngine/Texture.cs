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
   public class Texture : Object
   {
      public Texture( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public static int masterTextureLimit
      {
         get;
         set;
      }

      public static AnisotropicFiltering anisotropicFiltering
      {
         get;
         set;
      }

      public virtual int width
      {
         get
         {
            return Internal_GetWidth( this );
         }
         set
         {
            throw new Exception( "not implemented" );
         }
      }

      public virtual int height
      {
         get
         {
            return Internal_GetHeight( this );
         }
         set
         {
            throw new Exception( "not implemented" );
         }
      }

      public virtual TextureDimension dimension
      {
         get
         {
            return Internal_GetDimension( this );
         }
         set
         {
            throw new Exception( "not implemented" );
         }
      }

      public FilterMode filterMode
      {
         get;
         set;
      }

      public int anisoLevel
      {
         get;
         set;
      }

      public TextureWrapMode wrapMode
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public float mipMapBias
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public Vector2 texelSize
      {
         get
         {
            INTERNAL_get_texelSize( out Vector2 value );
            return value;
         }
      }

      public static extern void SetGlobalAnisotropicFilteringLimits( int forcedMin, int globalMax );

      private static extern int Internal_GetWidth( Texture t );

      private static extern int Internal_GetHeight( Texture t );

      private static extern TextureDimension Internal_GetDimension( Texture t );

      private extern void INTERNAL_get_texelSize( out Vector2 value );

      public IntPtr GetNativeTexturePtr()
      {
         INTERNAL_CALL_GetNativeTexturePtr( this, out IntPtr value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetNativeTexturePtr( Texture self, out IntPtr value );

      public extern int GetNativeTextureID();
   }
}
