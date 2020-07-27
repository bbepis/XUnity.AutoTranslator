using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureTranslationInfoExtensions
   {
      public static Texture2D GetOriginalTexture( this TextureTranslationInfo that )
      {
         return (Texture2D)that.Original.Target;
      }

      public static Texture2D GetTranslatedTexture( this TextureTranslationInfo that )
      {
         return (Texture2D)that.Translated;
      }
   }
}
