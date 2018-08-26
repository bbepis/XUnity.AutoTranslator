using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts
{
   public static class FontCache
   {
      private static readonly Dictionary<int, Font> CachedFonts = new Dictionary<int, Font>();

      public static Font GetOrCreate( int size )
      {
         if( !CachedFonts.TryGetValue( size, out Font font ) )
         {
            font = Font.CreateDynamicFontFromOSFont( Settings.OverrideFont, size );
            GameObject.DontDestroyOnLoad( font );
            CachedFonts.Add( size, font );
         }
         return font;
      }
   }
}
