using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureComponentExtensions
   {
      public static ImageTranslationInfo GetOrCreateImageTranslationInfo( this object ui )
      {
         return ui.GetOrCreateExtensionData<ImageTranslationInfo>();
      }

      public static TextureTranslationInfo GetOrCreateTextureTranslationInfo( this object texture )
      {
         var tti = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
         tti.Initialize( texture );

         return tti;
      }
   }
}
