using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class TemplatedStringTests
   {
      [Theory]
      [InlineData( "I am ZMAZ", "ZMAZ", "{{A}}", "I am {{A}}" )]
      [InlineData( "I am Z M A Z", "ZMAZ", "{{A}}", "I am {{A}}" )]
      [InlineData( "I am Z M A Z   ZMAZ", "ZMAZ", "{{A}}", "I am {{A}}   {{A}}" )]
      [InlineData( "I am Z m a Z   zMaZ", "ZMAZ", "{{A}}", "I am {{A}}   {{A}}" )]
      [InlineData( "I am Z m  a    Z   zMaZ", "ZMAZ", "{{A}}", "I am {{A}}   {{A}}" )]
      [InlineData( "ZMAZI am Z m  a    Z   zMaZ", "ZMAZ", "{{A}}", "{{A}}I am {{A}}   {{A}}" )]
      [InlineData( "ZMAZI am ZMAZX   zMaZ", "ZMAZ", "{{A}}", "{{A}}I am {{A}}X   {{A}}" )]
      [InlineData( "ZMAZI am XZMAZ   zMaZ", "ZMAZ", "{{A}}", "{{A}}I am X{{A}}   {{A}}" )]
      [InlineData( "ZMAZI am ZMAZZ   zMaZ", "ZMAZ", "{{A}}", "{{A}}I am {{A}}Z   {{A}}" )]
      [InlineData( "ZMAZI am ZZMAZ   zMaZ", "ZMAZ", "{{A}}", "{{A}}I am Z{{A}}   {{A}}" )]
      [InlineData( "I am ZMAZZMBZ", "ZMAZ", "{{A}}", "I am {{A}}ZMBZ" )]
      [InlineData( "I am ZZMAZZZMBZ", "ZMAZ", "{{A}}", "I am Z{{A}}ZZMBZ" )]
      [InlineData( "I am ZZM  AZZZMBZ", "ZMAZ", "{{A}}", "I am Z{{A}}ZZMBZ" )]
      public void Can_Replace_Approximate_Matches( string text, string translatorFriendlyKey, string key, string expectedResult )
      {
         var result = TemplatedString.ReplaceApproximateMatches( text, translatorFriendlyKey, key );

         Assert.Equal( expectedResult, result );
      }
   }
}
