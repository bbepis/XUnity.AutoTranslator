using System;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Fonts;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class FontCreationTests
   {
      private class SourceFont { }
      private enum RenderMode { SDFAA }
      private enum PopulationMode { Dynamic }

      private class CompatibleAsset
      {
         public static object CreateFontAsset( string name, string style, int size ) { return null; }
         public static object CreateFontAsset( SourceFont font, int size, int padding, RenderMode render, int width, int height, PopulationMode mode, bool multiAtlas, bool features = false ) { return null; }
      }

      private class IncompatibleAsset
      {
         public static object CreateFontAsset( SourceFont font, int size, int padding, RenderMode render, int width, int height, PopulationMode mode, bool multiAtlas, bool required ) { return null; }
      }

      [Fact]
      public void FindsFontOverloadWithOptionalTrailingArguments()
      {
         var method = FontHelper.FindLegacyFontCreationMethod( typeof( CompatibleAsset ), typeof( SourceFont ) );
         Assert.NotNull( method );
         Assert.Equal( typeof( SourceFont ), method.GetParameters()[ 0 ].ParameterType );
         Assert.Equal( false, method.GetParameters()[ 8 ].DefaultValue );
      }

      [Fact]
      public void RejectsIncompatibleOverloads()
      {
         Assert.Null( FontHelper.FindLegacyFontCreationMethod( typeof( IncompatibleAsset ), typeof( SourceFont ) ) );
         Assert.Null( FontHelper.FindLegacyFontCreationMethod( typeof( CompatibleAsset ), typeof( string ) ) );
      }
   }
}
