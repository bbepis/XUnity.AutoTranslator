using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public struct TranslationKey
   {
      public TranslationKey( object ui, string key, bool templatizeByNumbers, bool neverRemoveWhitespace = false )
      {
         OriginalText = key;

         if( !neverRemoveWhitespace
            && ( ( Settings.IgnoreWhitespaceInDialogue && key.Length > Settings.MinDialogueChars ) || ( ui.IgnoreAllWhitespace() ) ) )
         {
            RelevantText = key.RemoveWhitespace();
         }
         else
         {
            RelevantText = key;
         }

         if( templatizeByNumbers )
         {
            TemplatedText = RelevantText.TemplatizeByNumbers();
         }
         else
         {
            TemplatedText = null;
         }
      }

      public TemplatedString TemplatedText { get; }

      public string RelevantText { get; }

      public string OriginalText { get; set; }

      public string GetDictionaryLookupKey()
      {
         if( TemplatedText != null )
         {
            return TemplatedText.Template;
         }
         return RelevantText;
      }

      public string Untemplate( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.Untemplate( text );
         }

         return text;
      }

      public string RepairTemplate( string text )
      {
         if( TemplatedText != null )
         {
            return TemplatedText.RepairTemplate( text );
         }

         return text;
      }
   }
}
