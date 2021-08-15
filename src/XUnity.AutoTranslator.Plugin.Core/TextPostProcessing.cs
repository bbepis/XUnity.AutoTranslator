using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   [Flags]
   internal enum TextPostProcessing
   {
      None                         = 0,
      ReplaceMacronWithCircumflex  = 1 << 0,
      RemoveAllDiacritics          = 1 << 1,
      RemoveApostrophes            = 1 << 2,
      ReplaceWideCharacters        = 1 << 3,
      ReplaceHtmlEntities          = 1 << 4,
   }
}
