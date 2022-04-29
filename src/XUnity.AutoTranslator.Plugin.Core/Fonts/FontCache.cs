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
      private static bool _hasReadOverrideFontTextMeshPro = false;
      private static UnityEngine.Object OverrideFontTextMeshPro;
      private static bool _hasReadFallbackFontTextMeshPro = false;
      private static UnityEngine.Object FallbackFontTextMeshPro;

      public static Font GetOrCreate( int size )
      {
         if( !CachedFonts.TryGetValue( size, out Font font ) )
         {
            font = FontHelper.GetTextFont( size );
            CachedFonts.Add( size, font );
         }
         return font;
      }

      public static object GetOrCreateOverrideFontTextMeshPro()
      {
         if( !_hasReadOverrideFontTextMeshPro )
         {
            try
            {
               _hasReadOverrideFontTextMeshPro = true;
               OverrideFontTextMeshPro = FontHelper.GetTextMeshProFont( Settings.OverrideFontTextMeshPro );
            }
#if IL2CPP
            catch( Exception e ) when( e.ToString().ToLowerInvariant().Contains( "missing" ) || e.ToString().ToLowerInvariant().Contains( "not found" ) )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while loading text mesh pro override font. Retrying load with custom proxies..." );

               try
               {
                  OverrideFontTextMeshPro = FontHelper.GetTextMeshProFontByCustomProxies( Settings.OverrideFontTextMeshPro );
               }
               catch( Exception ex )
               {
                  XuaLogger.AutoTranslator.Error( ex, "An error occurred while loading text mesh pro override font: " + Settings.OverrideFontTextMeshPro );
               }
            }
#endif
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while loading text mesh pro override font: " + Settings.OverrideFontTextMeshPro );
            }
         }

         return OverrideFontTextMeshPro;
      }

      public static UnityEngine.Object GetOrCreateFallbackFontTextMeshPro()
      {
         if( !_hasReadFallbackFontTextMeshPro )
         {
            try
            {
               _hasReadFallbackFontTextMeshPro = true;
               FallbackFontTextMeshPro = FontHelper.GetTextMeshProFont( Settings.FallbackFontTextMeshPro );
            }
#if IL2CPP
            catch( Exception e ) when( e.ToString().ToLowerInvariant().Contains( "missing" ) || e.ToString().ToLowerInvariant().Contains( "not found" ) )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while loading text mesh pro fallback font. Retrying load with custom proxies..." );

               try
               {
                  FallbackFontTextMeshPro = FontHelper.GetTextMeshProFontByCustomProxies( Settings.FallbackFontTextMeshPro );
               }
               catch( Exception ex )
               {
                  XuaLogger.AutoTranslator.Error( ex, "An error occurred while loading text mesh pro fallback font: " + Settings.FallbackFontTextMeshPro );
               }
            }
#endif
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while loading text mesh pro fallback font: " + Settings.FallbackFontTextMeshPro );
            }
         }

         return FallbackFontTextMeshPro;
      }
   }
}
