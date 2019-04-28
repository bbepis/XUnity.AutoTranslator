using System;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
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

      internal static StringBuilder Reverse( this StringBuilder text )
      {
         if( text.Length > 1 )
         {
            int pivotPos = text.Length / 2;
            for( int i = 0; i < pivotPos; i++ )
            {
               int iRight = text.Length - ( i + 1 );
               char rightChar = text[ i ];
               char leftChar = text[ iRight ];
               text[ i ] = leftChar;
               text[ iRight ] = rightChar;
            }
         }

         return text;
      }
   }
}
