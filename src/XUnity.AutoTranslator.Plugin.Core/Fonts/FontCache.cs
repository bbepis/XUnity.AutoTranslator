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
            try
            {
               _hasReadTextMeshProFont = true;

               var overrideFontPath = Path.Combine( Paths.GameRoot, Settings.OverrideFontTextMeshPro );
               if( File.Exists( overrideFontPath ) )
               {
                  XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from asset bundle." );

                  AssetBundle bundle = null;
                  if( ClrTypes.AssetBundle_Methods.LoadFromFile != null )
                  {
                     bundle = (AssetBundle)ClrTypes.AssetBundle_Methods.LoadFromFile.Invoke( null, new object[] { overrideFontPath } );
                  }
                  else if( ClrTypes.AssetBundle_Methods.CreateFromFile != null )
                  {
                     bundle = (AssetBundle)ClrTypes.AssetBundle_Methods.CreateFromFile.Invoke( null, new object[] { overrideFontPath } );
                  }
                  else
                  {
                     XuaLogger.AutoTranslator.Error( "Could not find an appropriate asset bundle load method while loading font: " + overrideFontPath );
                     return null;
                  }

                  if( bundle == null )
                  {
                     XuaLogger.AutoTranslator.Warn( "Could not load asset bundle while loading font: " + overrideFontPath );
                     return null;
                  }

                  if( ClrTypes.AssetBundle_Methods.LoadAllAssets != null )
                  {
                     var assets = (UnityEngine.Object[])ClrTypes.AssetBundle_Methods.LoadAllAssets.Invoke( bundle, new object[] { ClrTypes.FontAsset } );
                     TextMeshProOverrideFont = assets?.FirstOrDefault();
                  }
                  else if( ClrTypes.AssetBundle_Methods.LoadAll != null )
                  {
                     var assets = (UnityEngine.Object[])ClrTypes.AssetBundle_Methods.LoadAll.Invoke( bundle, new object[] { ClrTypes.FontAsset } );
                     TextMeshProOverrideFont = assets?.FirstOrDefault();
                  }
               }
               else
               {
                  XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from internal Resources API." );

                  TextMeshProOverrideFont = Resources.Load( Settings.OverrideFontTextMeshPro );
               }

               if( TextMeshProOverrideFont != null )
               {
                  GameObject.DontDestroyOnLoad( TextMeshProOverrideFont );
               }
               else
               {
                  XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + Settings.OverrideFontTextMeshPro );
               }
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
