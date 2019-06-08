using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class UntranslatedText
   {
      public UntranslatedText( string text, bool templatizeByNumbers, bool removeInternalWhitespace )
      {
         OriginalText = text;

         if( templatizeByNumbers )
         {
            TemplatedText = text.TemplatizeByNumbers();
            if( TemplatedText != null )
            {
               text = TemplatedText.Template;
            }
         }
         //else
         //{
         //   TemplatedText = text.TemplatizeByReplacements();
         //   if( TemplatedText != null )
         //   {
         //      text = TemplatedText.Template;
         //   }
         //}

         int i = 0;
         int firstNonWhitespace = 0;
         int lastNonWhitespace = 0;

         StringBuilder leadingBuilder = null;
         while( i < text.Length && char.IsWhiteSpace( text[ i ] ) )
         {
            if( leadingBuilder == null ) leadingBuilder = new StringBuilder( 64 );

            leadingBuilder.Append( text[ i ] );
            i++;
         }
         firstNonWhitespace = i;

         if( firstNonWhitespace != 0 )
         {
            LeadingWhitespace = leadingBuilder?.ToString();
         }

         i = text.Length - 1;
         StringBuilder trailingBuilder = leadingBuilder;
         if( trailingBuilder != null ) trailingBuilder.Length = 0;

         while( i > -1 && char.IsWhiteSpace( text[ i ] ) )
         {
            if( trailingBuilder == null ) trailingBuilder = new StringBuilder( 64 );

            trailingBuilder.Append( text[ i ] );
            i--;
         }
         lastNonWhitespace = i;

         if( lastNonWhitespace != text.Length - 1 )
         {
            TrailingWhitespace = trailingBuilder?.Reverse().ToString();
         }

         // trim internals of 'text'
         if( removeInternalWhitespace && Settings.WhitespaceRemovalStrategy == WhitespaceHandlingStrategy.TrimPerNewline )
         {
            StringBuilder builder = trailingBuilder;
            if( builder != null ) builder.Length = 0;
            else if( builder == null ) builder = new StringBuilder( 64 );

            if( LeadingWhitespace != null )
            {
               builder.Append( LeadingWhitespace );
            }

            int whitespaceStart = -1;
            for( i = firstNonWhitespace; i <= lastNonWhitespace; i++ )
            {
               var c = text[ i ];

               if( c == '\n' )
               {
                  // find first non-whitespace
                  int u = i - 1;
                  while( u > firstNonWhitespace && char.IsWhiteSpace( text[ u ] ) ) u--;

                  int o = i + 1;
                  while( o < lastNonWhitespace && char.IsWhiteSpace( text[ o ] ) ) o++;

                  u++;
                  o--;
                  int l = o - u;
                  char lastCharAdded = default( char );
                  if( l > 0 )
                  {
                     char currentWhitespaceChar = text[ u ];
                     bool addedCurrentWhitespace = false;
                     u++;

                     for( int k = u; k <= o; k++ )
                     {
                        var ch = text[ k ];

                        // keep repeating whitespace
                        if( ch == currentWhitespaceChar )
                        {
                           if( !addedCurrentWhitespace )
                           {
                              builder.Append( currentWhitespaceChar );
                              addedCurrentWhitespace = true;
                           }

                           builder.Append( ch );
                           lastCharAdded = ch;
                        }
                        else
                        {
                           addedCurrentWhitespace = false;
                           currentWhitespaceChar = ch;
                        }
                     }
                  }

                  // we know we have just handled a newline
                  // now we need to check if the last character added is a whitespace character
                  // if it is not, we should add a space
                  if( Settings.UsesWhitespaceBetweenWords )
                  {
                     if( !char.IsWhiteSpace( lastCharAdded ) )
                     {
                        if( builder.Length > 0 && builder[ builder.Length - 1 ] != ' ' )
                        {
                           builder.Append( ' ' );
                        }
                     }
                  }

                  i = o;
                  whitespaceStart = -1;
               }
               else if( !char.IsWhiteSpace( c ) )
               {
                  if( whitespaceStart != -1 )
                  {
                     // add from whitespaceStart to i - 1 of characters
                     for( int b = whitespaceStart; b < i; b++ )
                     {
                        builder.Append( text[ b ] );
                     }

                     whitespaceStart = -1;
                  }

                  builder.Append( c );
               }
               else
               {
                  // is whitespace char different from \n
                  if( whitespaceStart == -1 )
                  {
                     whitespaceStart = i;
                  }
               }
            }

            if( TrailingWhitespace != null )
            {
               builder.Append( TrailingWhitespace );
            }

            TranslatableText = builder.ToString();
         }
         else
         {
            TranslatableText = text;
         }

         int leadingWhitespaceCount = LeadingWhitespace != null ? LeadingWhitespace.Length : 0;
         int trailingWhitespaceCount = TrailingWhitespace != null ? TrailingWhitespace.Length : 0;

         if( leadingWhitespaceCount > 0 || trailingWhitespaceCount > 0 )
         {
            TrimmedTranslatableText = TranslatableText.Substring( leadingWhitespaceCount, TranslatableText.Length - trailingWhitespaceCount - leadingWhitespaceCount );
         }
         else
         {
            TrimmedTranslatableText = TranslatableText;
         }
      }

      public string LeadingWhitespace { get; }

      public string TrailingWhitespace { get; }

      public string TranslatableText { get; }

      public string TrimmedTranslatableText { get; }

      public string OriginalText { get; }

      public TemplatedString TemplatedText { get; }

      public string Untemplate( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.Untemplate( text );
         }

         return text;
      }

      public string RepairTemplate( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.RepairTemplate( text );
         }

         return text;
      }

      public override bool Equals( object obj )
      {
         return obj is UntranslatedText ut && TranslatableText == ut.TranslatableText;
      }

      public override int GetHashCode()
      {
         return TranslatableText.GetHashCode();
      }
   }
}
