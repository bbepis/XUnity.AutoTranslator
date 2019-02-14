using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   [Flags]
   internal enum RomajiPostProcessing
   {
      None                         = 0,
      ReplaceMacronWithCircumflex  = 1 << 0,
      RemoveAllDiacritics          = 1 << 1,
      RemoveApostrophes            = 1 << 2
   }
}
