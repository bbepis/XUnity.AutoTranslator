using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class RomanizationHelperTests
   {
      [Theory( DisplayName = "Can_Replace_WideChars_With_AsciiChars" )]
      [InlineData( "～０１２３４５６７８９", "~0123456789" )]
      [InlineData( "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ［＼］＾＿｀ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ　", @"ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz " )]
      [InlineData( "ABCDEF ", "ABCDEF " )]
      [InlineData( "「こう見えて怒っているんですよ?……失礼しますね」", "「こう見えて怒っているんですよ?……失礼しますね」" )]
      public void Can_Replace_WideNumerics_With_AsciiNumerics( string originalText, string expectedText )
      {
         var result = RomanizationHelper.ReplaceWideCharacters( originalText );

         Assert.Equal( expectedText, result );
      }
   }
}
