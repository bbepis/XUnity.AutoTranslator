using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class ComponentExtensions
   {
      private static readonly string TextPropertyName = "text";
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";

      public static string GetText( this object ui )
      {
         if( ui == null ) return null;

         string text = null;
         var type = ui.GetType();

         if( ui is Text )
         {
            text = ( (Text)ui ).text;
         }
         else if( ui is GUIContent )
         {
            text = ( (GUIContent)ui ).text;
         }
         else
         {
            // fallback to reflective approach
            text = (string)ui.GetType()?.GetProperty( TextPropertyName )?.GetValue( ui, null );
         }

         return text ?? string.Empty;
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ui is Text )
         {
            ( (Text)ui ).text = text;
         }
         else if( ui is GUIContent )
         {
            ( (GUIContent)ui ).text = text;
         }
         else
         {
            // fallback to reflective approach
            type.GetProperty( TextPropertyName )?.GetSetMethod()?.Invoke( ui, new[] { text } );
         }
      }

      public static string GetTextureName( this Texture texture )
      {
         if( !string.IsNullOrEmpty( texture.name ) ) return texture.name;
         return "Unnamed";
      }

      public static Texture2D GetTexture( this object ui )
      {
         if( ui == null ) return null;

         if( ui is Image image )
         {
            return image.mainTexture as Texture2D;
         }
         else if( ui is RawImage rawImage )
         {
            return rawImage.mainTexture as Texture2D;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();
            var texture = type.GetProperty( MainTexturePropertyName )?.GetValue( ui, null )
               ?? type.GetProperty( TexturePropertyName )?.GetValue( ui, null );

            return texture as Texture2D;
         }
      }

      public static void SetAllDirtyEx( this object ui )
      {
         if( ui == null ) return;

         if( ui is Graphic graphic )
         {
            graphic.SetAllDirty();
         }
         else if( ui is Material material )
         {
            material.mainTexture = material.mainTexture;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();

            AccessTools.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }
   }

   public static class UILabelExtensions
   {
      private static readonly string SpacingYPropertyName = "spacingY";
      private static readonly string FloatSpacingYPropertyName = "floatSpacingY";
      private static readonly string UseFloatSpacingPropertyName = "useFloatSpacing";

      public static object GetSpacingY( this object uiLabel )
      {
         var type = uiLabel.GetType();
         var useFloatSpacing = (bool)type.GetProperty( UseFloatSpacingPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         if( useFloatSpacing )
         {
            return type.GetProperty( FloatSpacingYPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         }
         else
         {
            return type.GetProperty( SpacingYPropertyName )?.GetGetMethod()?.Invoke( uiLabel, null );
         }
      }

      public static void SetSpacingY( this object uiLabel, object spacing )
      {
         var type = uiLabel.GetType();
         if( spacing is float )
         {
            type.GetProperty( FloatSpacingYPropertyName )?.GetSetMethod()?.Invoke( uiLabel, new[] { spacing } );
         }
         else
         {
            type.GetProperty( SpacingYPropertyName )?.GetSetMethod()?.Invoke( uiLabel, new[] { spacing } );
         }
      }

      public static object Multiply( this object numeric, float scale )
      {
         if( numeric is float )
         {
            return (float)numeric * scale;
         }
         else
         {
            return (int)( (int)numeric * scale );
         }
      }
   }
}
