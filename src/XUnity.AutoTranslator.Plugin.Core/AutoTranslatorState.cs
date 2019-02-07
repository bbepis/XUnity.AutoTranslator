using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Class representing publicly available state of the plugin.
   /// </summary>
   public static class AutoTranslatorState
   {
      /// <summary>
      /// Gets the number of translation requests that has been completed.
      /// </summary>
      public static int TranslationCount => Settings.TranslationCount;
   }
}
