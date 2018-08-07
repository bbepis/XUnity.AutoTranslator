using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public abstract class UnityTextParserBase
   {
      private static readonly HashSet<char> ValidTagNameChars = new HashSet<char>
      {
         'a', 'b', 'c', 'd','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','z',
         'A', 'B', 'C', 'D','E','F','G','H','I','j','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','Z'
      };
      private HashSet<string> _ignored = new HashSet<string>();

      public UnityTextParserBase()
      {

      }

      protected void AddIgnoredTag( string name )
      {
         _ignored.Add( name );
      }

      public ParserResult Parse( string input )
      {
         StringBuilder textSinceLastChange = new StringBuilder();

         StringBuilder template = new StringBuilder();
         Dictionary<string, string> args = new Dictionary<string, string>();
         bool ignoringCurrentTag = false;
         char arg = 'A';
         Stack<string> tags = new Stack<string>();

         var state = ParsingState.Text;
         for( int i = 0 ; i < input.Length ; i++ )
         {
            var c = input[ i ];
            if( c != '<' && c != '>' )
            {
               textSinceLastChange.Append( c );
            }

            var previousState = state;
            switch( previousState )
            {
               case ParsingState.Text:
                  state = ParseText( input, ref i );
                  break;
               case ParsingState.NamingStartTag:
                  state = ParseNamingStartTag( input, ref i );
                  break;
               case ParsingState.NamingEndTag:
                  state = ParseNamingEndTag( input, ref i );
                  break;
               case ParsingState.FinishingStartTag:
                  state = ParseFinishingStartTag( input, ref i );
                  break;
               case ParsingState.FinishingEndTag:
                  state = ParseFinishingEndTag( input, ref i );
                  break;
               default:
                  break;
            }

            bool stateChanged = state != previousState;
            if( stateChanged )
            {
               // whenever the state changes, we want to add text, potentially
               string text;
               if( c == '<' || c == '>' )
               {
                  text = textSinceLastChange.ToString();
                  textSinceLastChange = new StringBuilder();
               }
               else
               {
                  text = TakeAllButLast( textSinceLastChange );
               }
               switch( previousState )
               {
                  case ParsingState.Text:
                     {
                        if( !string.IsNullOrEmpty( text ) )
                        {
                           var key = "{{" + arg + "}}";
                           arg++;

                           args.Add( key, text );
                           template.Append( key );
                        }
                     }
                     break;
                  case ParsingState.NamingStartTag:
                     {
                        ignoringCurrentTag = _ignored.Contains( text );
                        tags.Push( text );

                        if( !ignoringCurrentTag )
                        {
                           template.Append( "<" + text );
                           if( state != ParsingState.FinishingStartTag )
                           {
                              template.Append( ">" );
                           }
                        }
                     }
                     break;
                  case ParsingState.FinishingStartTag:
                     {
                        if( !ignoringCurrentTag )
                        {
                           template.Append( text + ">" );
                        }
                     }
                     break;
                  case ParsingState.NamingEndTag:
                     {
                        if( !ignoringCurrentTag )
                        {
                           template.Append( "<" + text );
                        }

                        if( state != ParsingState.FinishingEndTag )
                        {
                           if( !ignoringCurrentTag )
                           {
                              template.Append( ">" );
                           }

                           var tag = tags.Pop();
                           ignoringCurrentTag = tags.Count > 0 && _ignored.Contains( tags.Peek() );
                        }
                     }
                     break;
                  case ParsingState.FinishingEndTag:
                     {
                        if( !ignoringCurrentTag )
                        {
                           template.Append( text + ">" );
                        }

                        var tag = tags.Pop();
                        ignoringCurrentTag = tags.Count > 0 && _ignored.Contains( tags.Peek() );
                     }
                     break;
               }
            }
         }

         if( state == ParsingState.Text )
         {
            var text = textSinceLastChange.ToString();

            if( !string.IsNullOrEmpty( text ) )
            {
               var key = "{{" + arg + "}}";
               arg++;

               args.Add( key, text );
               template.Append( key );
            }
         }

         // finally, lets merge some of the arguments together
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

      private string TakeAllButLast( StringBuilder builder )
      {
         if( builder.Length > 0 )
         {
            var str = builder.ToString( 0, builder.Length - 1 );
            builder.Remove( 0, builder.Length - 1 );
            return str;
         }
         return string.Empty;
      }

      private ParsingState ParseText( string s, ref int i )
      {
         if( s[ i ] == '<' )
         {
            if( i + 1 < s.Length && s[ i + 1 ] == '/' )
            {
               return ParsingState.NamingEndTag;
            }
            else
            {
               return ParsingState.NamingStartTag;
            }
         }
         else
         {
            return ParsingState.Text;
         }
      }

      private ParsingState ParseNamingStartTag( string s, ref int i )
      {
         if( ValidTagNameChars.Contains( s[ i ] ) )
         {
            return ParsingState.NamingStartTag;
         }
         else if( s[ i ] == '>' )
         {
            // we need to determine if we are inside or outside a tag after this!
            return ParsingState.Text;
         }
         else
         {
            return ParsingState.FinishingStartTag;
         }
      }

      private ParsingState ParseNamingEndTag( string s, ref int i )
      {
         if( ValidTagNameChars.Contains( s[ i ] ) )
         {
            return ParsingState.NamingEndTag;
         }
         else if( s[ i ] == '>' )
         {
            // we need to determine if we are inside or outside a tag after this!
            return ParsingState.Text;
         }
         else
         {
            return ParsingState.FinishingEndTag;
         }
      }

      private ParsingState ParseFinishingStartTag( string s, ref int i )
      {
         if( s[ i ] == '>' )
         {
            return ParsingState.Text;
         }
         else
         {
            return ParsingState.FinishingStartTag;
         }
      }

      private ParsingState ParseFinishingEndTag( string s, ref int i )
      {
         if( s[ i ] == '>' )
         {
            return ParsingState.Text;
         }
         else
         {
            return ParsingState.FinishingEndTag;
         }
      }

      private enum ParsingState
      {
         Text,
         NamingStartTag,
         NamingEndTag,
         FinishingStartTag,
         FinishingEndTag
      }
   }
}
