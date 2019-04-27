using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class UntranslatedTextInfo
   {
      public UntranslatedTextInfo( string trimmedOriginalText, string untranslatedTextKey, string untranslatedText, int prependedNewlineCount, TemplatedString templatedText )
      {
         TrimmedOriginalText = trimmedOriginalText;
         UntranslatedText = untranslatedText;
         UntranslatedTextKey = untranslatedTextKey;
         PrependedNewlines = prependedNewlineCount;
         TemplatedText = templatedText;
      }

      public int PrependedNewlines { get; set; }

      public TemplatedString TemplatedText { get; }

      public string UntranslatedTextKey { get; }

      public string UntranslatedText { get; }

      public string TrimmedOriginalText { get; }

      public string GetUntranslatedText()
      {
         // Should NOT contain prepended newlines (problem with template text?)

         if( TemplatedText != null )
         {
            return TemplatedText.Template;
         }
         return UntranslatedText;
      }

      public string GetCacheKey()
      {
         // Should contain prepended newlines (problem with template text?)

         if( TemplatedText != null )
         {
            return TemplatedText.Template;
         }
         return UntranslatedTextKey;
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
