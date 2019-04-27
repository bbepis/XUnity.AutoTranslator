using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class StringExtensionTests
   {
      [Theory( DisplayName = "Can_Trim_Leading_Newlines" )]
      [InlineData( "\r\n \r\nHello", "Hello", 2 )]
      [InlineData( "\r\n \r\nHello\n", "Hello\n", 2 )]
      [InlineData( "\r\r\r\r\n \n　Hello", "Hello", 2 )]
      public void Can_Trim_Leading_Newlines( string input, string expectedOutput, int expectedNewlineCount )
      {
         var output = input.TrimLeadingNewlines( out int newlineCount );

         Assert.Equal( output, expectedOutput );
         Assert.Equal( expectedNewlineCount, newlineCount );
      }
   }
}
