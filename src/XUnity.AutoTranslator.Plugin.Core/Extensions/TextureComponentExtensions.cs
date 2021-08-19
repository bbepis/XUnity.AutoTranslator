using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureComponentExtensions
   {
      public static ImageTranslationInfo GetOrCreateImageTranslationInfo(this object obj, Texture2D originalTexture)
      {
         if (obj == null) return null;

         var iti = obj.GetOrCreateExtensionData<ImageTranslationInfo>();
         if (iti.Original == null) iti.Initialize(originalTexture);

         return iti;
      }

      public static TextureTranslationInfo GetOrCreateTextureTranslationInfo( this object texture )
      {
         var tti = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
         tti.Initialize( texture );

         return tti;
      }
   }
}
