using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts
{
   internal static class FontCache
   {
      private static readonly Dictionary<int, Font> CachedFonts = new Dictionary<int, Font>();
      private static bool _hasReadTextMeshProFont = false;
      private static object TextMeshProOverrideFont;

      public static Font GetOrCreate( int size )
      {
         if( !CachedFonts.TryGetValue( size, out Font font ) )
         {
            font = FontHelper.GetTextFont( size );
            CachedFonts.Add( size, font );
         }
         return font;
      }

      public static object GetOrCreateTextMeshProFont()
      {
         if( !_hasReadTextMeshProFont )
         {
            try
            {
               _hasReadTextMeshProFont = true;
               TextMeshProOverrideFont = FontHelper.GetTextMeshProFont();
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while loading text mesh pro override font: " + Settings.OverrideFontTextMeshPro );
            }
         }

         return TextMeshProOverrideFont;
      }
   }
}
