using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class RichTextParser
   {
      private static readonly char[] TagNameEnders = new char[] { '=', ' ' };
      private static readonly Regex TagRegex = new Regex( "(<.*?>)" );
      private static readonly HashSet<string> IgnoreTags = new HashSet<string> { "ruby", "group" };
      private static readonly HashSet<string> KnownTags = new HashSet<string> { "b", "i", "size", "color", "em", "sup", "sub", "dash", "space", "u", "strike", "param", "format", "emoji", "speed", "sound", "line-height" };

      public RichTextParser()
      {

      }

      public bool CanApply( object ui )
      {
         return Settings.HandleRichText && ui.SupportsRichText();
      }

      private static bool IsAllLatin( string value, int endIdx )
      {
         for( int j = 0; j < endIdx; j++ )
         {
            var c = value[ j ];
            if( !( ( c >= '\u0041' && c <= '\u005a' ) || ( c >= '\u0061' && c <= '\u007a' ) || c == '-' || c == '_' ) )
            {
               return false;
            }
         }
         return true;
      }

      private static bool StartsWithPound( string value, int endIdx )
      {
         return 0 < value.Length && value[ 0 ] == '#';
      }


      public ParserResult Parse( string input, int scope )
      {
         if( !Settings.HandleRichText ) return null;

         var allMatches = new List<ArgumentedUntranslatedTextInfo>();
         var template = new StringBuilder( input.Length );
         var arg = 'A';

         bool addedArgumentLastTime = false;
         foreach( var tagOrText in TagRegex.Split( input ) )
         {
            if( tagOrText.Length > 0 )
            {
               bool isTag = tagOrText[ 0 ] == '<' && tagOrText[ tagOrText.Length - 1 ] == '>';
               bool isEndTag = isTag && tagOrText.Length > 1 && tagOrText[ 1 ] == '/';

               string tagText = null;
               if( isEndTag )
               {
                  tagText = tagOrText.Substring( 2, tagOrText.Length - 3 );
               }
               else if( isTag )
               {
                  tagText = tagOrText.Substring( 1, tagOrText.Length - 2 );
               }

               if( isTag )
               {
                  string tagName;
                  var parts = tagText.Split( TagNameEnders );
                  if( parts.Length >= 2 )
                  {
                     tagName = parts[ 0 ];
                  }
                  else
                  {
                     tagName = tagText;
                  }

                  var endIdx = tagName.Length;
                  var isKnown = KnownTags.Contains( tagName )
                      || ( IsAllLatin( tagName, endIdx ) && !IgnoreTags.Contains( tagName ) )
                      || StartsWithPound( tagName, endIdx );

                  if( isKnown )
                  {
                     template.Append( tagOrText );
                     addedArgumentLastTime = false;
                  }
               }
               else
               {
                  if( addedArgumentLastTime )
                  {
                     var info = allMatches[ allMatches.Count - 1 ];
                     info.Info.UntranslatedText += tagOrText;
                  }
                  else
                  {
                     var argument = "[[" + ( arg++ ) + "]]";
                     var info = new ArgumentedUntranslatedTextInfo
                     {
                        Key = argument,
                        Info = new UntranslatedTextInfo( tagOrText )
                     };
                     allMatches.Add( info );
                     template.Append( argument );
                  }
                  addedArgumentLastTime = true;
               }
            }
         }

         // construct context info for each match!
         for( int i = 0; i < allMatches.Count; i++ )
         {
            var item = allMatches[ i ];
            for( int j = 0; j < i; j++ )
            {
               item.Info.ContextBefore.Add( allMatches[ j ].Info.UntranslatedText );
            }

            for( int j = i + 1; j < allMatches.Count; j++ )
            {
               item.Info.ContextAfter.Add( allMatches[ j ].Info.UntranslatedText );
            }
         }

         var templateString = template.ToString();
         var success = arg != 'A' && templateString.Length > 5;
         if( !success ) return null;

         return new ParserResult( ParserResultOrigin.RichTextParser, input, templateString, false, true, true, false, allMatches );
      }
   }
}
