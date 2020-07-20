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
      public static RegexSplittingTextParser RegexSplittingTextParser;
      public static GameLogTextParser GameLogTextParser;

      public static void Initialize()
      {
         RichTextParser = new RichTextParser();
         RegexSplittingTextParser = new RegexSplittingTextParser();
         GameLogTextParser = new GameLogTextParser();
      }
   }
}
