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
   public sealed class SpriteRenderer : Renderer
   {
      public Sprite sprite
      {
         get
         {
            return GetSprite_INTERNAL();
         }
         set
         {
            SetSprite_INTERNAL( value );
         }
      }

      public SpriteDrawMode drawMode
      {
         get;
         set;
      }

      internal bool shouldSupportTiling
      {
         get;
      }

      public Vector2 size
      {
         get
         {
            INTERNAL_get_size( out Vector2 value );
            return value;
         }
         set
         {
            INTERNAL_set_size( ref value );
         }
      }

      public float adaptiveModeThreshold
      {
         get;
         set;
      }

      public SpriteTileMode tileMode
      {
         get;
         set;
      }

      public Color color
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

      public bool flipX
      {
         get;
         set;
      }

      public bool flipY
      {
         get;
         set;
      }

      private extern void INTERNAL_get_size( out Vector2 value );

      private extern void INTERNAL_set_size( ref Vector2 value );

      private extern Sprite GetSprite_INTERNAL();

      private extern void SetSprite_INTERNAL( Sprite sprite );

      private extern void INTERNAL_get_color( out Color value );

      private extern void INTERNAL_set_color( ref Color value );

      internal Bounds GetSpriteBounds()
      {
         INTERNAL_CALL_GetSpriteBounds( this, out Bounds value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetSpriteBounds( SpriteRenderer self, out Bounds value );
   }
}
