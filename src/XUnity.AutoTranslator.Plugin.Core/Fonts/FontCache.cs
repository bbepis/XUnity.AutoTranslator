using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts
{
   internal static class FontCache
   {
      private static readonly Dictionary<int, Font> CachedFonts = new Dictionary<int, Font>();
      private static UnityEngine.Object TextMeshProOverrideFont;
      private static bool _hasReadTextMeshProFont = false;

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

      public static object GetOrCreateTextMeshProFont()
      {
         if( !_hasReadTextMeshProFont )
         {
            _hasReadTextMeshProFont = true;
            TextMeshProOverrideFont = Resources.Load( Settings.OverrideFontTextMeshPro );

            if( TextMeshProOverrideFont != null )
            {
               GameObject.DontDestroyOnLoad( TextMeshProOverrideFont );
            }
            else
            {
               XuaLogger.Current.Warn( "Could not find the TextMeshPro font resource: " + Settings.OverrideFontTextMeshPro );
            }
         }

         return TextMeshProOverrideFont;
      }
   }
}
