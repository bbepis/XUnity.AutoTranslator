using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugins.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureComponentExtensions
   {
      private static readonly string SetAllDirtyMethodName = "SetAllDirty";
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string CapitalMainTexturePropertyName = "MainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";

      public static string GetPath( this object ui )
      {
         if( ui is Component comp )
         {
            var go = comp.gameObject;
            return go.GetPath();
         }
         return null;
      }

      public static Texture2D GetTexture( this object ui )
      {
         if( ui == null ) return null;

         if( ui is SpriteRenderer spriteRenderer )
         {
            return spriteRenderer.sprite?.texture;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();
            var texture = type.CachedProperty( MainTexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( TexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( CapitalMainTexturePropertyName )?.Get( ui );

            return texture as Texture2D;
         }
      }

      public static void SetAllDirtyEx( this object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.Graphic != null && ClrTypes.Graphic.IsAssignableFrom( type ) )
         {
            ClrTypes.Graphic.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else
         {
            AccessToolsShim.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }
   }
}
