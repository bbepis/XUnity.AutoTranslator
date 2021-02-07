using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class GameLogTextParser
   {
      public GameLogTextParser()
      {
      }

      public bool CanApply( object ui )
      {
         return ui.SupportsLineParser();
      }

      public ParserResult Parse( string input, int scope, IReadOnlyTextTranslationCache cache )
      {
         var reader = new StringReader( input );
         bool containsTranslatable = false;
         var template = new StringBuilder( input.Length );
         var args = new List<ArgumentedUntranslatedTextInfo>();
         var arg = 'A';

         string line = null;
         while( ( line = reader.ReadLine() ) != null )
         {
            if( !string.IsNullOrEmpty( line ) )
            {
               if( cache.IsTranslatable( line, true, scope ) )
               {
                  // template it!
                  containsTranslatable = true;

                  var key = "[[" + ( arg++ ) + "]]";
                  template.Append( key ).Append( '\n' );
                  args.Add( new ArgumentedUntranslatedTextInfo
                  {
                     Info = new UntranslatedTextInfo( line )
                  } );
               }
               else
               {
                  // add it
                  //containsTranslated = true;
                  template.Append( line ).Append( '\n' );
               }
            }
            else
            {
               // add new line
               template.Append( '\n' );
            }
         }

         if( !containsTranslatable ) return null;

         if( !input.EndsWith( "\r\n" ) && !input.EndsWith( "\n" ) ) template.Remove( template.Length - 1, 1 );

         if( args.Count > 1 )
         {
            return new ParserResult( ParserResultOrigin.GameLogTextParser, input, template.ToString(), false, false, false, true, args );
         }

         return null;
      }
   }
}
