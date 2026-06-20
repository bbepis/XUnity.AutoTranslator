using System;
using System.IO;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts
{
   internal static class FontHelper
   {
      public static UnityEngine.Object GetTextMeshProFont( string assetBundle )
      {
         UnityEngine.Object font = null;

         var overrideFontPath = Path.Combine( Paths.GameRoot, assetBundle );
         if( File.Exists( overrideFontPath ) )
         {
            XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from asset bundle." );

            AssetBundle bundle = null;
            if( UnityTypes.AssetBundle_Methods.LoadFromFile != null )
            {
               bundle = (AssetBundle)UnityTypes.AssetBundle_Methods.LoadFromFile.Invoke( null, new object[] { overrideFontPath } );
            }
            else if( UnityTypes.AssetBundle_Methods.CreateFromFile != null )
            {
               bundle = (AssetBundle)UnityTypes.AssetBundle_Methods.CreateFromFile.Invoke( null, new object[] { overrideFontPath } );
            }
            else
            {
#if IL2CPP
               return GetTextMeshProFontByCustomProxies( assetBundle );
#else
               XuaLogger.AutoTranslator.Error( "Could not find an appropriate asset bundle load method while loading font: " + overrideFontPath );
               return null;
#endif
            }

            if( bundle == null )
            {
               XuaLogger.AutoTranslator.Warn( "Could not load asset bundle while loading font: " + overrideFontPath );
               return null;
            }

            if( UnityTypes.TMP_FontAsset != null )
            {
               if( UnityTypes.AssetBundle_Methods.LoadAllAssets != null )
               {
#if MANAGED
                  var assets = (UnityEngine.Object[])UnityTypes.AssetBundle_Methods.LoadAllAssets.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
#else
                  var assets = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object>)UnityTypes.AssetBundle_Methods.LoadAllAssets.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
#endif
                  font = assets?.FirstOrDefault();
               }
               else if( UnityTypes.AssetBundle_Methods.LoadAll != null )
               {
#if MANAGED
                  var assets = (UnityEngine.Object[])UnityTypes.AssetBundle_Methods.LoadAll.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
#else
                  var assets = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object>)UnityTypes.AssetBundle_Methods.LoadAll.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
#endif
                  font = assets?.FirstOrDefault();
               }
            }
         }
         else
         {
            bool isAssetBundleNameInsideSystemFont = false;
            try
            {
               string[] systemFontNames = GetOSInstalledFontNames();
               for( int i = 0; i < systemFontNames.Length; i++ )
               {
                  if( systemFontNames[ i ] == assetBundle )
                  {
                     isAssetBundleNameInsideSystemFont = true;
                     break;
                  }
               }
            }
            catch( Exception ex )
            {
               XuaLogger.AutoTranslator.Error( $"Unable to fetch installed font names: {ex.Message}" );
            }

            if( UnityTypes.TMP_FontAsset_Methods.CreateFontAsset != null && isAssetBundleNameInsideSystemFont )
            {
               XuaLogger.AutoTranslator.Info( $"A font {assetBundle} is installed on the system. Attempting to create a TextMesh Pro font from it." );
               font = (UnityEngine.Object)UnityTypes.TMP_FontAsset_Methods.CreateFontAsset.Invoke( null, new object[] { assetBundle, "", 90 } );
            }
            else
            {
               if( UnityTypes.TMP_FontAsset_Methods.CreateFontAsset == null )
               {
                  XuaLogger.AutoTranslator.Warn( "TMP_FontAsset.CreateFontAsset not found. TextMeshPro version might be below 3.2.0." );
               }

               XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from internal Resources API." );
               font = Resources.Load( assetBundle );
            }
         }

         if( font != null )
         {
            var versionProperty = UnityTypes.TMP_FontAsset_Properties.Version;
            var version = (string)versionProperty?.Get( font ) ?? "Unknown";
            XuaLogger.AutoTranslator.Info( $"Loaded TextMesh Pro font uses version: {version}" );

            if( versionProperty != null && Settings.TextMeshProVersion != null && version != Settings.TextMeshProVersion )
            {
               XuaLogger.AutoTranslator.Warn( $"TextMesh Pro version mismatch. Font asset version: {version}, TextMesh Pro version: {Settings.TextMeshProVersion}" );
            }

            GameObject.DontDestroyOnLoad( font );
         }
         else
         {
            XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + assetBundle );
         }

         return font;
      }

#if IL2CPP
      public static UnityEngine.Object GetTextMeshProFontByCustomProxies( string assetBundle )
      {
         UnityEngine.Object font = null;

         var overrideFontPath = Path.Combine( Paths.GameRoot, assetBundle );
         if( File.Exists( overrideFontPath ) )
         {
            XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from asset bundle." );

            var bundle = AssetBundleProxy.LoadFromFile( overrideFontPath );

            if( bundle == null )
            {
               XuaLogger.AutoTranslator.Warn( "Could not load asset bundle while loading font: " + overrideFontPath );
               return null;
            }

            if( UnityTypes.TMP_FontAsset != null )
            {
               var assets = bundle.LoadAllAssets( UnityTypes.TMP_FontAsset.UnityType );
               font = assets?.FirstOrDefault();
            }
         }

         if( font != null )
         {
            var versionProperty = UnityTypes.TMP_FontAsset_Properties.Version;
            var version = (string)versionProperty?.Get( font ) ?? "Unknown";
            XuaLogger.AutoTranslator.Info( $"Loaded TextMesh Pro font uses version: {version}" );

            if( versionProperty != null && Settings.TextMeshProVersion != null && version != Settings.TextMeshProVersion )
            {
               XuaLogger.AutoTranslator.Warn( $"TextMesh Pro version mismatch. Font asset version: {version}, TextMesh Pro version: {Settings.TextMeshProVersion}" );
            }

            GameObject.DontDestroyOnLoad( font );
         }
         else
         {
            XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + assetBundle );
         }

         return font;
      }
#endif

      public static Font GetTextFont( int size )
      {
         var font = Font.CreateDynamicFontFromOSFont( Settings.OverrideFont, size );
         GameObject.DontDestroyOnLoad( font );

         return font;
      }

      public static string[] GetOSInstalledFontNames()
      {
         return Font.GetOSInstalledFontNames();
      }
   }
}
