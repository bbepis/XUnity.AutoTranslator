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
      public TranslationKeys( string key, bool templatizeByNumbers )
      {
         OriginalText = key;

         if( Settings.IgnoreWhitespaceInDialogue && key.Length > Settings.MinDialogueChars )
         {
            RelevantKey = key.RemoveWhitespace();
         }
         else
         {
            RelevantKey = key;
         }

         if( templatizeByNumbers )
         {
            TemplatedKey = RelevantKey.TemplatizeByNumbers();
         }
         else
         {
            TemplatedKey = null;
         }
      }

      public TemplatedString TemplatedKey { get; }

      public string RelevantKey { get; }

      public string OriginalText { get; set; }

      public string GetDictionaryLookupKey()
      {
         if( TemplatedKey != null )
         {
            return TemplatedKey.Template;
         }
         return RelevantKey;
      }

      public string Untemplate( string text )
      {
         if( TemplatedKey != null )
         {
            return TemplatedKey.Untemplate( text );
         }

         return text;
      }

      public string RepairTemplate( string text )
      {
         if( TemplatedKey != null )
         {
            return TemplatedKey.RepairTemplate( text );
         }

         return text;
      }
   }
}
