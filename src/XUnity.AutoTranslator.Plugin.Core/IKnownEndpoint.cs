using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public interface IKnownEndpoint
   {
      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine", so it can be implemented
      /// in an async fashion.
      /// </summary>
      IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action failure );

      /// <summary>
      /// Gets a boolean indicating if we are allowed to call "Translate".
      /// </summary>
      bool IsBusy { get; }

      /// <summary>
      /// Called before plugin shutdown and can return true to prevent plugin shutdown, if the plugin
      /// can provide a secondary strategy for translation.
      /// </summary>
      /// <returns></returns>
      bool ShouldGetSecondChanceAfterFailure();

      /// <summary>
      /// "Update" game loop method.
      /// </summary>
      void OnUpdate();

      bool SupportsLineSplitting { get; }
   }

   public class TranslationBatch
   {
      public TranslationBatch()
      {
         Trackers = new List<TranslationLineTracker>();
      }

      public List<TranslationLineTracker> Trackers { get; private set; }

      public int TotalLinesCount { get; set; }

      public void Add( TranslationJob job )
      {
         var lines = new TranslationLineTracker( job );
         Trackers.Add( lines );
         TotalLinesCount += lines.LinesCount;
      }

      public bool MatchWithTranslations( string allTranslations )
      {
         var lines = allTranslations.Split( '\n' );

         if( lines.Length != TotalLinesCount ) return false;

         int current = 0;
         foreach( var tracker in Trackers )
         {
            var builder = new StringBuilder( 32 );
            for( int i = 0 ; i < tracker.LinesCount ; i++ )
            {
               var translation = lines[ current++ ];
               builder.Append( translation );

               // ADD NEW LINE IF NEEDED
               if( !( i == tracker.LinesCount - 1 ) ) // if not last line
               {
                  builder.Append( '\n' );
               }
            }
            var fullTranslation = builder.ToString();

            tracker.RawTranslatedText = fullTranslation;
         }

         return true;
      }

      public string GetFullTranslationKey()
      {
         var builder = new StringBuilder();
         for( int i = 0 ; i < Trackers.Count ; i++ )
         {
            var tracker = Trackers[ i ];
            builder.Append( tracker.Job.Keys.GetDictionaryLookupKey() );

            if( !( i == Trackers.Count - 1 ) )
            {
               builder.Append( '\n' );
            }
         }
         return builder.ToString();
      }
   }

   public class TranslationLineTracker
   {
      public TranslationLineTracker( TranslationJob job )
      {
         Job = job;
         LinesCount = job.Keys.GetDictionaryLookupKey().Count( c => c == '\n' ) + 1;
      }

      public string RawTranslatedText { get; set; }

      public TranslationJob Job { get; private set; }

      public int LinesCount { get; private set; }
   }
}
