using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class RegexTranslationTests
   {
      [Theory( DisplayName = "Can_Create_Regex" )]
      [InlineData( "r:\"^タイプ([０-９0-9]+)$\"", "r:\"Type $1\"" )]
      public void Can_Create_Regex( string key, string value )
      {
         var regex = new RegexTranslation( key, value );
      }
   }
}
