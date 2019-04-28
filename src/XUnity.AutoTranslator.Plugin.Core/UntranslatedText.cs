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

            char currentWhitespaceChar = default( char );
            bool addedCurrentWhitespace = false;
            for( i = firstNonWhitespace; i <= lastNonWhitespace; i++ )
            {
               var c = text[ i ];
               if( !char.IsWhiteSpace( c ) )
               {
                  builder.Append( c );

                  currentWhitespaceChar = default( char );
               }
               else
               {
                  // keep repeating whitespace
                  if( c == currentWhitespaceChar )
                  {
                     if( !addedCurrentWhitespace )
                     {
                        builder.Append( currentWhitespaceChar );
                        addedCurrentWhitespace = true;
                     }

                     builder.Append( c );
                  }
                  else
                  {
                     addedCurrentWhitespace = false;
                     currentWhitespaceChar = c;
                  }
               }

               if( Settings.UsesWhitespaceBetweenWords && ( c == '\n' || c == '\r' ) )
               {
                  if( builder.Length > 0 && builder[ builder.Length - 1 ] != ' ' )
                  {
                     builder.Append( ' ' );
                  }

                  var nextI = i + 1;
                  if( nextI < lastNonWhitespace && text[ nextI ] == '\n' )
                  {
                     i++;
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
            TrimmedText = TranslatableText.Substring( leadingWhitespaceCount, TranslatableText.Length - trailingWhitespaceCount - leadingWhitespaceCount );
         }
         else
         {
            TrimmedText = TranslatableText;
         }
      }

      public string LeadingWhitespace { get; }

      public string TrailingWhitespace { get; }

      public string TrimmedText { get; }

      public string TranslatableText { get; }

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
