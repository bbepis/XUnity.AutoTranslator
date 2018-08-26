using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public static class UnityTextParsers
   {
      private static readonly RichTextParser RichTextParser = new RichTextParser();

      public static RichTextParser GetTextParserByGameEngine()
      {
         return RichTextParser;
      }
   }
}
