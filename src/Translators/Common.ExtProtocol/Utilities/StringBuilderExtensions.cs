using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtProtocol.Utilities
{
   /// <summary>
   /// StringBuilder extensions.
   /// </summary>
   public static class StringBuilderExtensions
   {
      /// <summary>
      /// Gets a bool indicating if the current line ends in whitespace or newline.
      /// </summary>
      /// <param name="builder"></param>
      /// <returns></returns>
      public static bool EndsWithWhitespaceOrNewline( this StringBuilder builder )
      {
         if( builder.Length == 0 ) return true;

         var lastChar = builder[ builder.Length - 1 ];
         return char.IsWhiteSpace( lastChar ) || lastChar == '\n';
      }
   }
}
