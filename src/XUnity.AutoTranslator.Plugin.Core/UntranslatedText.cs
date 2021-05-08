using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class UntranslatedText
   {
      private static string PerformInternalTrimming( string text, bool whitespaceBetweenWords, ref StringBuilder builder )
      {
         if( builder != null ) builder.Length = 0;
         else if( builder == null ) builder = new StringBuilder( 64 );

         bool modified = false;
         int len = text.Length;
         int whitespaceStart = -1;
         int i;
         for( i = 0; i < len; i++ )
         {
            var c = text[ i ];

            if( c == '\n' )
            {
               // find first non-whitespace
               int u = i - 1;
               while( u >= 0 && char.IsWhiteSpace( text[ u ] ) ) u--;

               int o = i + 1;
               while( o < len && char.IsWhiteSpace( text[ o ] ) ) o++;

               u++;
               o--;
               int l = o - u;
               char lastCharAdded = default( char );
               if( l > 0 )
               {
                  int addedCharacters = 0;
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
                           addedCharacters++;
                           builder.Append( currentWhitespaceChar );
                           addedCurrentWhitespace = true;
                        }

                        addedCharacters++;
                        builder.Append( ch );
                        lastCharAdded = ch;
                     }
                     else if( currentWhitespaceChar == '\r' && ch == '\n' )
                     {
                        // if this is FOLLOWED by another \r\n, we should remove it!
                        var maybeRi = k + 1;
                        var maybeNi = k + 2;
                        if( k + 2 <= o )
                        {
                           var maybeR = text[ maybeRi ];
                           var maybeN = text[ maybeNi ];
                           if( maybeR == '\r' && maybeN == '\n' )
                           {
                              if( !addedCurrentWhitespace )
                              {
                                 addedCharacters++;
                                 builder.Append( '\r' );
                                 builder.Append( '\n' );
                                 addedCurrentWhitespace = true;
                              }

                              addedCharacters++;
                              builder.Append( '\r' );
                              builder.Append( '\n' );
                              lastCharAdded = '\n';

                              k++;
                           }
                        }
                     }
                     else
                     {
                        addedCurrentWhitespace = false;
                        currentWhitespaceChar = ch;
                     }
                  }

                  if( addedCharacters - 1 != l )
                  {
                     modified = true;
                  }
               }
               else
               {
                  modified = true;
               }

               // we know we have just handled a newline
               // now we need to check if the last character added is a whitespace character
               // if it is not, we should add a space
               if( whitespaceBetweenWords )
               {
                  if( !char.IsWhiteSpace( lastCharAdded ) )
                  {
                     if( builder.Length > 0 && builder[ builder.Length - 1 ] != ' ' )
                     {
                        modified = true;
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

         if( whitespaceStart != -1 )
         {
            // add from whitespaceStart to i - 1 of characters
            for( int b = whitespaceStart; b < i; b++ )
            {
               builder.Append( text[ b ] );
            }
         }

         if( modified )
         {
            return builder.ToString();
         }
         else
         {
            return text;
         }
      }

      private static string SurroundWithWhitespace( string text, string leadingWhitespace, string trailingWhitespace, ref StringBuilder builder )
      {
         if( leadingWhitespace != null || trailingWhitespace != null )
         {
            if( builder != null ) builder.Length = 0;
            else if( builder == null ) builder = new StringBuilder( 64 );

            if( leadingWhitespace != null )
            {
               builder.Append( leadingWhitespace );
            }
            builder.Append( text );
            if( trailingWhitespace != null )
            {
               builder.Append( trailingWhitespace );
            }

            return builder.ToString();
         }
         return text;
      }

      private bool? _isOnlyTemplate;

      public UntranslatedText( string originalText, bool isFromSpammingComponent, bool removeInternalWhitespace, bool whitespaceBetweenWords, bool enableTemplating, bool templateAllNumbersAway )
      {
         IsFromSpammingComponent = isFromSpammingComponent;

         // Calculate the original and original templated texts
         Original_Text = originalText;
         if( enableTemplating )
         {
            if( isFromSpammingComponent )
            {
               TemplatedText = originalText.TemplatizeByNumbers();
               if( TemplatedText != null )
               {
                  originalText = TemplatedText.Template;
               }
            }
            else
            {
               TemplatedText = templateAllNumbersAway ? originalText.TemplatizeByReplacementsAndNumbers() : originalText.TemplatizeByReplacements();
               if( TemplatedText != null )
               {
                  originalText = TemplatedText.Template;
               }
            }
         }

         TemplatedOriginal_Text = originalText;

         var isTemplated = IsTemplated;

         // Calculate leading/trailing whitespace
         int i = 0;
         int firstNonWhitespace = 0;
         int lastNonWhitespace = 0;

#warning should we use Original_Text to do this whitespace stuff instead?
         StringBuilder leadingBuilder = null;
         while( i < originalText.Length && char.IsWhiteSpace( originalText[ i ] ) )
         {
            if( leadingBuilder == null ) leadingBuilder = new StringBuilder( 64 );

            leadingBuilder.Append( originalText[ i ] );
            i++;
         }
         firstNonWhitespace = i;

         if( firstNonWhitespace != 0 )
         {
            LeadingWhitespace = leadingBuilder?.ToString();
         }

         StringBuilder trailingBuilder = leadingBuilder;
         if( i != originalText.Length )
         {
            i = originalText.Length - 1;
            if( trailingBuilder != null ) trailingBuilder.Length = 0;

            while( i > -1 && char.IsWhiteSpace( originalText[ i ] ) )
            {
               if( trailingBuilder == null ) trailingBuilder = new StringBuilder( 64 );

               trailingBuilder.Append( originalText[ i ] );
               i--;
            }
            lastNonWhitespace = i;

            if( lastNonWhitespace != originalText.Length - 1 )
            {
               TrailingWhitespace = trailingBuilder?.Reverse().ToString();
            }
         }

         //  Calculate externally trimmed texts
         int leadingWhitespaceCount = LeadingWhitespace != null ? LeadingWhitespace.Length : 0;
         int trailingWhitespaceCount = TrailingWhitespace != null ? TrailingWhitespace.Length : 0;

         if( leadingWhitespaceCount > 0 || trailingWhitespaceCount > 0 )
         {
            Original_Text_ExternallyTrimmed = Original_Text.Substring( leadingWhitespaceCount, Original_Text.Length - trailingWhitespaceCount - leadingWhitespaceCount );
         }
         else
         {
            Original_Text_ExternallyTrimmed = Original_Text;
         }

         if( isTemplated )
         {
            TemplatedOriginal_Text_ExternallyTrimmed = TemplatedOriginal_Text.Substring( leadingWhitespaceCount, TemplatedOriginal_Text.Length - trailingWhitespaceCount - leadingWhitespaceCount );
         }
         else
         {
            TemplatedOriginal_Text_ExternallyTrimmed = Original_Text_ExternallyTrimmed;
         }

         // Calculate internally trimmed texts
         if( removeInternalWhitespace )
         {
            Original_Text_FullyTrimmed = PerformInternalTrimming( Original_Text_ExternallyTrimmed, whitespaceBetweenWords, ref trailingBuilder );

            var internalTrimmingHadNoImpact = ReferenceEquals( Original_Text_FullyTrimmed, Original_Text_ExternallyTrimmed );
            Original_Text_InternallyTrimmed = internalTrimmingHadNoImpact
               ? Original_Text
               : SurroundWithWhitespace( Original_Text_FullyTrimmed, LeadingWhitespace, TrailingWhitespace, ref trailingBuilder );
         }
         else
         {
            Original_Text_FullyTrimmed = Original_Text_ExternallyTrimmed;
            Original_Text_InternallyTrimmed = Original_Text;
         }

         if( isTemplated )
         {
            if( removeInternalWhitespace )
            {
               TemplatedOriginal_Text_FullyTrimmed = PerformInternalTrimming( TemplatedOriginal_Text_ExternallyTrimmed, whitespaceBetweenWords, ref trailingBuilder );

               var internalTrimmingHadNoImpact = ReferenceEquals( TemplatedOriginal_Text_FullyTrimmed, TemplatedOriginal_Text_ExternallyTrimmed );
               TemplatedOriginal_Text_InternallyTrimmed = internalTrimmingHadNoImpact
                  ? TemplatedOriginal_Text
                  : SurroundWithWhitespace( TemplatedOriginal_Text_FullyTrimmed, LeadingWhitespace, TrailingWhitespace, ref trailingBuilder );
            }
            else
            {
               TemplatedOriginal_Text_FullyTrimmed = TemplatedOriginal_Text_ExternallyTrimmed;
               TemplatedOriginal_Text_InternallyTrimmed = TemplatedOriginal_Text;
            }
         }
         else
         {
            TemplatedOriginal_Text_FullyTrimmed = Original_Text_FullyTrimmed;
            TemplatedOriginal_Text_InternallyTrimmed = Original_Text_InternallyTrimmed;
         }
      }

      public bool IsFromSpammingComponent { get; set; }

      public string LeadingWhitespace { get; }

      public string TrailingWhitespace { get; }

      public string Original_Text { get; }
      public string Original_Text_ExternallyTrimmed { get; }
      public string Original_Text_InternallyTrimmed { get; }
      public string Original_Text_FullyTrimmed { get; }

      public string TemplatedOriginal_Text { get; }
      public string TemplatedOriginal_Text_ExternallyTrimmed { get; }
      public string TemplatedOriginal_Text_InternallyTrimmed { get; }
      public string TemplatedOriginal_Text_FullyTrimmed { get; }

      public TemplatedString TemplatedText { get; }

      public bool IsTemplated => TemplatedText != null;

      public bool IsOnlyTemplate
      {
         get
         {
            if( !_isOnlyTemplate.HasValue )
            {
               _isOnlyTemplate = IsTemplated && !TemplatingHelper.ContainsUntemplatedCharacters( TemplatedOriginal_Text_ExternallyTrimmed );
            }

            return _isOnlyTemplate.Value;
         }
      }

      public string Untemplate( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.Untemplate( text );
         }

         return text;
      }

      public string PrepareUntranslatedText( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.PrepareUntranslatedText( text );
         }

         return text;
      }

      public string FixTranslatedText( string text, bool useTranslatorFriendlyArgs )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.FixTranslatedText( text, useTranslatorFriendlyArgs );
         }

         return text;
      }

      public override bool Equals( object obj )
      {
         return obj is UntranslatedText ut && TemplatedOriginal_Text_InternallyTrimmed == ut.TemplatedOriginal_Text_InternallyTrimmed;
      }

      public override int GetHashCode()
      {
         return TemplatedOriginal_Text_InternallyTrimmed.GetHashCode();
      }
   }
}
