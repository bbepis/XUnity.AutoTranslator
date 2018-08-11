using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public static class UnityTextParsers
   {
      private static readonly UtageTextParser UtageTextParser = new UtageTextParser();

      public static UnityTextParserBase GetTextParserByGameEngine()
      {
         if( Types.AdvEngine != null )
         {
            return UtageTextParser;
         }
         return null;
      }
   }
}
