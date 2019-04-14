using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class TranslationEndpointManager
   {
      private Dictionary<string, byte> _failedTranslations;
      private Dictionary<string, TranslationJob> _unstartedJobs;
      private Dictionary<string, TranslationJob> _ongoingJobs;

      private int _ongoingTranslations;

      // used for prototyping
      private Dictionary<string, string> _translations;

      public TranslationEndpointManager( ITranslateEndpoint endpoint, Exception error )
      {
         Endpoint = endpoint;
         Error = error;
         _ongoingTranslations = 0;

         _failedTranslations = new Dictionary<string, byte>();
         _unstartedJobs = new Dictionary<string, TranslationJob>();
         _ongoingJobs = new Dictionary<string, TranslationJob>();

         _translations = new Dictionary<string, string>();

         HasBatchLogicFailed = false;
         AvailableBatchOperations = Settings.MaxAvailableBatchOperations;
      }

      public TranslationManager Manager { get; set; }

      public ITranslateEndpoint Endpoint { get; }

      public Exception Error { get; }

      public bool IsBusy => _ongoingTranslations >= Endpoint.MaxConcurrency;

      public bool HasBatchLogicFailed { get; set; }

      public int AvailableBatchOperations { get; set; }

      public int ConsecutiveErrors { get; set; }

      public bool CanBatch => Endpoint.MaxTranslationsPerRequest > 1 && _unstartedJobs.Count > 1 && !HasBatchLogicFailed && AvailableBatchOperations > 0;

      public bool HasUnstartedBatch => _unstartedJobs.Count > 0 && AvailableBatchOperations > 0;

      public bool HasUnstartedJob => _unstartedJobs.Count > 0;

      public bool HasFailedDueToConsecutiveErrors => ConsecutiveErrors >= Settings.MaxErrors;

      public bool TryGetTranslation( TranslationKey key, out string value )
      {
         return TryGetTranslation( key.GetDictionaryLookupKey(), out value );
      }

      public bool TryGetTranslation( string key, out string value )
      {
         return _translations.TryGetValue( key, out value );
      }

      private void AddTranslation( TranslationKey key, string value )
      {
         var lookup = key.GetDictionaryLookupKey();
         _translations[ lookup ] = value;
      }

      private void AddTranslation( string key, string value )
      {
         _translations[ key ] = value;
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         // FIXME: Implement
      }

      public void AddTranslationToCache( TranslationKey key, string value )
      {
         AddTranslationToCache( key.GetDictionaryLookupKey(), value );
      }

      public void AddTranslationToCache( string key, string value )
      {
         if( !HasTranslated( key ) )
         {
            AddTranslation( key, value );
            QueueNewTranslationForDisk( key, value );
         }
      }

      private bool HasTranslated( string key )
      {
         return _translations.ContainsKey( key );
      }

      public void HandleNextBatch()
      {
         try
         {
            var kvps = _unstartedJobs.Take( Endpoint.MaxTranslationsPerRequest ).ToList();
            var untranslatedTexts = new List<string>();
            var jobs = new List<TranslationJob>();

            foreach( var kvp in kvps )
            {
               var key = kvp.Key;
               var job = kvp.Value;
               _unstartedJobs.Remove( key );
               Manager.UnstartedTranslations--;

               var untranslatedText = job.Key.GetDictionaryLookupKey();
               if( CanTranslate( untranslatedText ) )
               {
                  jobs.Add( job );
                  untranslatedTexts.Add( untranslatedText );
                  _ongoingJobs[ key ] = job;
                  Manager.OngoingTranslations++;
               }
               else
               {
                  XuaLogger.Current.Warn( $"Dequeued: '{untranslatedText}' because the current endpoint has already failed this translation 3 times." );
                  job.State = TranslationJobState.Failed;
                  job.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";

                  Manager.InvokeJobFailed( job );
               }
            }

            if( jobs.Count > 0 )
            {
               AvailableBatchOperations--;
               var jobsArray = jobs.ToArray();

               foreach( var untranslatedText in untranslatedTexts )
               {
                  XuaLogger.Current.Debug( "Started: '" + untranslatedText + "'" );
               }
               CoroutineHelper.Start(
                  Translate(
                     untranslatedTexts.ToArray(),
                     Settings.FromLanguage,
                     Settings.Language,
                     translatedText => OnBatchTranslationCompleted( jobsArray, translatedText ),
                     ( msg, e ) => OnTranslationFailed( jobsArray, msg, e ) ) );
            }
         }
         finally
         {
            if( _unstartedJobs.Count == 0 )
            {
               Manager.UnscheduleUnstartedJobs( this );
            }
         }
      }


      public void HandleNextJob()
      {
         try
         {
            var kvp = _unstartedJobs.FirstOrDefault();

            var key = kvp.Key;
            var job = kvp.Value;
            _unstartedJobs.Remove( key );
            Manager.UnstartedTranslations--;

            var untranslatedText = job.Key.GetDictionaryLookupKey();
            if( CanTranslate( untranslatedText ) )
            {
               _ongoingJobs[ key ] = job;
               Manager.OngoingTranslations++;

               XuaLogger.Current.Debug( "Started: '" + untranslatedText + "'" );
               CoroutineHelper.Start(
                  Translate(
                     new[] { untranslatedText },
                     Settings.FromLanguage,
                     Settings.Language,
                     translatedText => OnSingleTranslationCompleted( job, translatedText ),
                     ( msg, e ) => OnTranslationFailed( new[] { job }, msg, e ) ) );
            }
            else
            {
               XuaLogger.Current.Warn( $"Dequeued: '{untranslatedText}' because the current endpoint has already failed this translation 3 times." );
               job.State = TranslationJobState.Failed;
               job.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";

               Manager.InvokeJobFailed( job );
            }
         }
         finally
         {
            if( _unstartedJobs.Count == 0 )
            {
               Manager.UnscheduleUnstartedJobs( this );
            }
         }
      }

      private void OnBatchTranslationCompleted( TranslationJob[] jobs, string[] translatedTexts )
      {
         ConsecutiveErrors = 0;

         var succeeded = jobs.Length == translatedTexts.Length;
         if( succeeded )
         {
            for( int i = 0; i < jobs.Length; i++ )
            {
               var job = jobs[ i ];
               var translatedText = translatedTexts[ i ];

               job.TranslatedText = PostProcessTranslation( job.Key, translatedText );
               job.State = TranslationJobState.Succeeded;

               _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );
               Manager.OngoingTranslations--;

               XuaLogger.Current.Info( $"Completed: '{job.Key.GetDictionaryLookupKey()}' => '{job.TranslatedText}'" );

               Manager.InvokeJobCompleted( job );
            }
         }
         else
         {
            if( !HasBatchLogicFailed )
            {
               CoroutineHelper.Start( EnableBatchingAfterDelay() );
            }

            HasBatchLogicFailed = true;
            for( int i = 0; i < jobs.Length; i++ )
            {
               var job = jobs[ i ];

               var key = job.Key.GetDictionaryLookupKey();
               AddUnstartedJob( key, job );
               _ongoingJobs.Remove( key );
               Manager.OngoingTranslations--;
            }

            XuaLogger.Current.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }
      }

      private void OnSingleTranslationCompleted( TranslationJob job, string[] translatedTexts )
      {
         var translatedText = translatedTexts[ 0 ];

         ConsecutiveErrors = 0;

         job.TranslatedText = PostProcessTranslation( job.Key, translatedText );
         job.State = TranslationJobState.Succeeded;

         _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );
         Manager.OngoingTranslations--;

         XuaLogger.Current.Info( $"Completed: '{job.Key.GetDictionaryLookupKey()}' => '{job.TranslatedText}'" );

         Manager.InvokeJobCompleted( job );
      }

      private string PostProcessTranslation( TranslationKey key, string translatedText )
      {
         var hasTranslation = !string.IsNullOrEmpty( translatedText );
         if( hasTranslation )
         {
            translatedText = key.RepairTemplate( translatedText );

            if( Settings.Language == Settings.Romaji && Settings.RomajiPostProcessing != TextPostProcessing.None )
            {
               translatedText = RomanizationHelper.PostProcess( translatedText, Settings.RomajiPostProcessing );
            }
            else if( Settings.TranslationPostProcessing != TextPostProcessing.None )
            {
               translatedText = RomanizationHelper.PostProcess( translatedText, Settings.TranslationPostProcessing );
            }

            if( Settings.ForceSplitTextAfterCharacters > 0 )
            {
               translatedText = translatedText.SplitToLines( Settings.ForceSplitTextAfterCharacters, '\n', ' ', '　' );
            }
         }

         return translatedText;
      }

      private void OnTranslationFailed( TranslationJob[] jobs, string error, Exception e )
      {
         if( e == null )
         {
            XuaLogger.Current.Error( error );
         }
         else
         {
            XuaLogger.Current.Error( e, error );
         }

         if( jobs.Length == 1 )
         {
            foreach( var job in jobs )
            {
               var untranslatedText = job.Key.GetDictionaryLookupKey();
               job.State = TranslationJobState.Failed;
               job.ErrorMessage = error;

               _ongoingJobs.Remove( untranslatedText );
               Manager.OngoingTranslations--;

               RegisterTranslationFailureFor( untranslatedText );

               Manager.InvokeJobFailed( job );
            }
         }
         else
         {
            if( !HasBatchLogicFailed )
            {
               CoroutineHelper.Start( EnableBatchingAfterDelay() );
            }

            HasBatchLogicFailed = true;
            for( int i = 0; i < jobs.Length; i++ )
            {
               var job = jobs[ i ];

               var key = job.Key.GetDictionaryLookupKey();
               AddUnstartedJob( key, job );
               _ongoingJobs.Remove( key );
               Manager.OngoingTranslations--;
            }

            XuaLogger.Current.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }

         if( !HasFailedDueToConsecutiveErrors )
         {
            ConsecutiveErrors++;

            if( HasFailedDueToConsecutiveErrors )
            {
               XuaLogger.Current.Error( $"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down plugin." );

               ClearAllJobs();
            }
         }
      }

      private IEnumerator EnableBatchingAfterDelay()
      {
         yield return new WaitForSeconds( 240 );

         HasBatchLogicFailed = false;

         XuaLogger.Current.Info( "Re-enabled batching." );
      }

      public bool EnqueueTranslation( object ui, TranslationKey key, TranslationResult translationResult, ParserTranslationContext context )
      {
         var lookupKey = key.GetDictionaryLookupKey();

         var added = AssociateWithExistingJobIfPossible( ui, lookupKey, translationResult, context );
         if( added )
         {
            return false;
         }

         var checkOtherEndpoints = translationResult == null;
         if( checkOtherEndpoints )
         {
            var endpoints = Manager.ConfiguredEndpoints;
            var len = endpoints.Count;
            for( int i = 0; i < len; i++ )
            {
               var endpoint = endpoints[ i ];
               if( endpoint == this ) continue;

               added = endpoint.AssociateWithExistingJobIfPossible( ui, lookupKey, translationResult, context );
               if( added )
               {
                  return false;
               }
            }
         }

         XuaLogger.Current.Debug( "Queued: '" + lookupKey + "'" );

         var saveResultGlobally = checkOtherEndpoints;
         var newJob = new TranslationJob( this, key, saveResultGlobally );
         newJob.Associate( ui, translationResult, context );

         return AddUnstartedJob( lookupKey, newJob );
      }

      public bool AssociateWithExistingJobIfPossible( object ui, string key, TranslationResult translationResult, ParserTranslationContext context )
      {
         if( _unstartedJobs.TryGetValue( key, out TranslationJob unstartedJob ) )
         {
            unstartedJob.Associate( ui, translationResult, context );
            return true;
         }

         if( _ongoingJobs.TryGetValue( key, out TranslationJob ongoingJob ) )
         {
            ongoingJob.Associate( ui, translationResult, context );
            return true;
         }

         return false;
      }

      private bool AddUnstartedJob( string key, TranslationJob job )
      {
         if( !_unstartedJobs.ContainsKey( key ) )
         {
            int countBefore = _unstartedJobs.Count;

            _unstartedJobs.Add( key, job );
            Manager.UnstartedTranslations++;

            if( countBefore == 0 )
            {
               Manager.ScheduleUnstartedJobs( this );
            }

            return true;
         }
         return false;
      }

      public void ClearAllJobs()
      {
         var ongoingCount = _ongoingJobs.Count;
         var unstartedCount = _unstartedJobs.Count;

         var unstartedJobs = _unstartedJobs.ToList();

         _ongoingJobs.Clear();
         _unstartedJobs.Clear();

         foreach( var job in unstartedJobs )
         {
            XuaLogger.Current.Warn( $"Dequeued: '{job.Key}'" );
            job.Value.State = TranslationJobState.Failed;
            job.Value.ErrorMessage = "Translation failed because all jobs on endpoint was cleared.";

            Manager.InvokeJobFailed( job.Value );
         }

         Manager.OngoingTranslations -= ongoingCount;
         Manager.UnstartedTranslations -= unstartedCount;

         Manager.UnscheduleUnstartedJobs( this );
      }

      public bool CanTranslate( string untranslatedText )
      {
         if( _failedTranslations.TryGetValue( untranslatedText, out var count ) )
         {
            return count < Settings.MaxFailuresForSameTextPerEndpoint;
         }
         return true;
      }

      public void RegisterTranslationFailureFor( string untranslatedText )
      {
         byte count;
         if( !_failedTranslations.TryGetValue( untranslatedText, out count ) )
         {
            count = 1;
         }
         else
         {
            count++;
         }

         _failedTranslations[ untranslatedText ] = count;
      }

      public IEnumerator Translate( string[] untranslatedTexts, string from, string to, Action<string[]> success, Action<string, Exception> failure )
      {
         var startTime = Time.realtimeSinceStartup;
         var context = new TranslationContext( untranslatedTexts, from, to, success, failure );
         _ongoingTranslations++;

         try
         {
            bool ok = false;
            var iterator = Endpoint.Translate( context );
            if( iterator != null )
            {
            TryMe: try
               {
                  ok = iterator.MoveNext();

                  // check for timeout
                  var now = Time.realtimeSinceStartup;
                  if( now - startTime > Settings.Timeout )
                  {
                     ok = false;
                     context.FailWithoutThrowing( $"Timeout occurred during translation (took more than {Settings.Timeout} seconds)", null );
                  }
               }
               catch( TranslationContextException )
               {
                  ok = false;
               }
               catch( Exception e )
               {
                  ok = false;
                  context.FailWithoutThrowing( "Error occurred during translation.", e );
               }

               if( ok )
               {
                  yield return iterator.Current;
                  goto TryMe;
               }
            }
         }
         finally
         {
            _ongoingTranslations--;

            context.FailIfNotCompleted();
         }
      }
   }
}
