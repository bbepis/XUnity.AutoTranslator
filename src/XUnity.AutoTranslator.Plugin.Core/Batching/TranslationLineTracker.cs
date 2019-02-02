using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core.Batching
{
   internal class TranslationLineTracker
   {
      public TranslationLineTracker( TranslationJob job )
      {
         Job = job;
         LinesCount = job.Key.GetDictionaryLookupKey().Count( c => c == '\n' ) + 1;
      }

      public string RawTranslatedText { get; set; }

      public TranslationJob Job { get; private set; }

      public int LinesCount { get; private set; }
   }
}
