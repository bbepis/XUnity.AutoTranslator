using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TranslationJob
   {
      public object UI { get; set; }

      public string UntranslatedText { get; set; }

      public string UntranslatedDialogueText { get; set; }

      public string TranslatedText { get; set; }
   }
}
