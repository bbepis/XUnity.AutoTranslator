using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public class RichTextParser
   {
      private static readonly Regex TagRegex = new Regex( "<.*?>", RegexOptions.Compiled );
      private static readonly HashSet<string> IgnoreTags = new HashSet<string> { "ruby", "/ruby" };

      public RichTextParser()
      {

      }

      public ParserResult Parse( string input )
      {
         var matches = TagRegex.Matches( input );

         var args = new Dictionary<string, string>();
         var template = new StringBuilder( input.Length );
         var offset = 0;
         var arg = 'A';

         foreach( Match m in matches )
         {
            var tag = m.Value;
            var value = tag.Substring( 1, tag.Length - 2 );
            bool isEndTag = value.StartsWith( "/" );
            if( isEndTag )
            {
               value = value.Substring( 1, value.Length - 1 );
            }

            var parts = value.Split( '=' );
            if( parts.Length == 2 )
            {
               value = parts[ 0 ];
            }

            // add normal text
            var end = m.Index;
            var start = offset;
            var text = input.Substring( start, end - start );
            var argument = "{{" + ( arg++ ) + "}}";
            args.Add( argument, text );
            template.Append( argument );

            offset = end + m.Length;

            var ignoreTag = IgnoreTags.Contains( value );
            if( !ignoreTag )
            {
               template.Append( m.Value );
            }
         }

         // catch any remaining text
         if( offset < input.Length )
         {
            var argument = "{{" + ( arg++ ) + "}}";
            var text = input.Substring( offset, input.Length - offset );
            args.Add( argument, text );
            template.Append( argument );
         }


         var templateString = template.ToString();
         int idx = -1;
         while( ( idx = templateString.IndexOf( "}}{{" ) ) != -1 )
         {
            var arg1 = templateString[ idx - 1 ];
            var arg2 = templateString[ idx + 4 ];

            var key1 = "{{" + arg1 + "}}";
            var key2 = "{{" + arg2 + "}}";

            var text1 = args[ key1 ];
            var text2 = args[ key2 ];

            var fullText = text1 + text2;
            var fullKey = key1 + key2;
            var newKey = "{{" + ( ++arg ) + "}}";

            args.Remove( key1 );
            args.Remove( key2 );
            args.Add( newKey, fullText );
            templateString = templateString.Replace( fullKey, newKey );
         }

         return new ParserResult( input, templateString, args );
      }
   }
}
