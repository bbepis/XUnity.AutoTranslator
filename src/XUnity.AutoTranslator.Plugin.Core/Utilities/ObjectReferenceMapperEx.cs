using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class ObjectReferenceMapperEx
   {
      public static TextTranslationInfo GetOrCreateTextTranslationInfo( this object ui )
      {
         if( ui.SupportsStabilization() && ui.IsKnownTextType() )
         {
            var info = ui.GetOrCreateExtensionData<TextTranslationInfo>();

            return info;
         }

         return null;
      }

      public static TextTranslationInfo GetTextTranslationInfo( this object ui )
      {
         if( ui.SupportsStabilization() && ui.IsKnownTextType() )
         {
            var info = ui.GetExtensionData<TextTranslationInfo>();

            return info;
         }

         return null;
      }

      public static ImageTranslationInfo GetOrCreateImageTranslationInfo( this object obj )
      {
         return obj.GetOrCreateExtensionData<ImageTranslationInfo>();
      }

      public static TextureTranslationInfo GetOrCreateTextureTranslationInfo( this Texture2D texture )
      {
         var tti = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
         if( tti.Original == null ) tti.SetOriginal( texture );

         return tti;
      }
   }
}
