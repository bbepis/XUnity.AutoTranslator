using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   [Flags]
   internal enum TranslationType
   {
      None = 0x00,
      Full = 0x01,
      Token = 0x02
   }
}
