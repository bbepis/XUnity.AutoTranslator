using System;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class StringBuilderExtensions
   {
      public static bool EndsWithWhitespaceOrNewline( this StringBuilder builder )
      {
         if( builder.Length == 0 ) return true;

         var lastChar = builder[ builder.Length - 1 ];
         return char.IsWhiteSpace( lastChar ) || lastChar == '\n';
      }
   }
}
