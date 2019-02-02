using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal static class UnityTextParsers
   {
      public static RichTextParser RichTextParser;
      public static GameLogTextParser GameLogTextParser;

      public static void Initialize( Func<string, bool> isTranslatable )
      {
         RichTextParser = new RichTextParser();
         GameLogTextParser = new GameLogTextParser( isTranslatable );
      }
   }
}
