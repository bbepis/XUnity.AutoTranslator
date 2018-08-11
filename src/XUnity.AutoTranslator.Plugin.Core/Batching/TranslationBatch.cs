﻿using System.Collections.Generic;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Batching
{
   public class TranslationBatch
   {
      public TranslationBatch()
      {
         Trackers = new List<TranslationLineTracker>();
      }

      public List<TranslationLineTracker> Trackers { get; private set; }

      public bool IsEmpty => Trackers.Count == 0;

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
            builder.Append( tracker.Job.Key.GetDictionaryLookupKey() );

            if( !( i == Trackers.Count - 1 ) )
            {
               builder.Append( '\n' );
            }
         }
         return builder.ToString();
      }
   }
}
