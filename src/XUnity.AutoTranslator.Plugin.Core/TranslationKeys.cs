using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public struct TranslationKeys
   {
      public TranslationKeys( string key )
      {
         OriginalKey = key;
         DialogueKey = key.RemoveWhitespace();
      }

      public string OriginalKey { get; }

      public string DialogueKey { get; }

      //public string ForcedRelevantKey => IsDialogue ? DialogueKey : OriginalKey;

      public string RelevantKey => IsDialogue && Settings.IgnoreWhitespaceInDialogue ? DialogueKey : OriginalKey;

      public bool IsDialogue => OriginalKey.Length > Settings.MinDialogueChars;
   }
}
