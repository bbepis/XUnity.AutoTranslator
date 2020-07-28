using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class Il2CppFontHelper : IFontHelper
   {
      public string[] GetOSInstalledFontNames()
      {
         // PROBLEM: Proxy method does not exist!

         return new string[ 0 ];
         //return Font.GetOSInstalledFontNames();
      }

      public object GetTextFont( int size )
      {
         // PROBLEM: Proxy method does not exist!

         //var font = Font.CreateDynamicFontFromOSFont( Settings.OverrideFont, size );
         //GameObject.DontDestroyOnLoad( font );
         //return font;

         return null;
      }

      public object GetTextMeshProFont()
      {
         object font = null;

         // PROBLEM: Cannot Load all assets in bundle of type!!

         //var overrideFontPath = Path.Combine( PathsHelper.Instance.GameRoot, Settings.OverrideFontTextMeshPro );
         //if( File.Exists( overrideFontPath ) )
         //{
         //   XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from asset bundle." );

         //   var data = File.ReadAllBytes( overrideFontPath );
         //   var il2cppData =  new Il2CppStructArray<byte>( data.Length );
         //   for( int i = 0; i < data.Length; i++ )
         //   {
         //      il2cppData[ i ] = data[ i ];
         //   }

         //   AssetBundle bundle = AssetBundle.LoadFromMemory( il2cppData );
         //   if( bundle == null )
         //   {
         //      XuaLogger.AutoTranslator.Warn( "Could not load asset bundle while loading font: " + overrideFontPath );
         //      return null;
         //   }

         //   font = bundle.LoadAsset( "Lolol", UnityTypes.FontAsset.Il2CppType );
         //}
         //else
         //{
         //   XuaLogger.AutoTranslator.Info( "Attempting to load TextMesh Pro font from internal Resources API." );

         //   font = Resources.Load( Settings.OverrideFontTextMeshPro );
         //}

         //if( font != null )
         //{
         //   GameObject.DontDestroyOnLoad( (UnityEngine.Object)font );
         //}
         //else
         //{
         //   XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + Settings.OverrideFontTextMeshPro );
         //}

         return font;
      }
   }
}
