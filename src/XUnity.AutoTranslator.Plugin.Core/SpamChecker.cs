using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class SpamChecker
   {
      private TranslationManager _translationManager;

      public SpamChecker( TranslationManager translationManager )
      {
         _translationManager = translationManager;
      }
   }
}
