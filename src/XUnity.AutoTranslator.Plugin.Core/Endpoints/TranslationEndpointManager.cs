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
      private Dictionary<UntranslatedText, TranslationJob> _unstartedJobs;
      private Dictionary<UntranslatedText, TranslationJob> _ongoingJobs;

      private int _ongoingTranslations;

      // used for prototyping
      private Dictionary<string, string> _translations;
      private Dictionary<string, string> _reverseTranslations;

      public TranslationEndpointManager( ITranslateEndpoint endpoint, Exception error )
      {
         Endpoint = endpoint;
         Error = error;
         _ongoingTranslations = 0;

         _failedTranslations = new Dictionary<string, byte>();
         _unstartedJobs = new Dictionary<UntranslatedText, TranslationJob>();
         _ongoingJobs = new Dictionary<UntranslatedText, TranslationJob>();

         _translations = new Dictionary<string, string>();
         _reverseTranslations = new Dictionary<string, string>();

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

      public bool TryGetTranslation( UntranslatedText key, out string value )
      {
         var unmodifiedKey = key.TranslatableText;
         var result = _translations.TryGetValue( unmodifiedKey, out value );
         if( result )
         {
            return result;
         }

         var modifiedKey = key.TrimmedTranslatableText;
         result = _translations.TryGetValue( modifiedKey, out value );
         if( result )
         {
            // add an unmodifiedKey to the dictionary
            var unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
            AddTranslationToCache( unmodifiedKey, unmodifiedValue );

            value = unmodifiedValue;
            return result;
         }

         return result;
      }

      private void AddTranslation( string key, string value )
      {
         if( key != null && value != null )
         {
            _translations[ key ] = value;
            _reverseTranslations[ value ] = key;
         }
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         // FIXME: Implement
      }

      public void AddTranslationToCache( string key, string value )
      {
         // UNRELEASED: Not included in current release
         //if( !HasTranslated( key ) )
         //{
         //   AddTranslation( key, value );
         //   QueueNewTranslationForDisk( key, value );
         //}
      }

      public bool IsTranslatable( string text )
      {
         return LanguageHelper.IsTranslatable( text ) && !IsTranslation( text );
      }

      private bool IsTranslation( string translation )
      {
         return _reverseTranslations.ContainsKey( translation );
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

               var untranslatedText = job.Key.TrimmedTranslatableText;
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

            var untranslatedText = job.Key.TrimmedTranslatableText;
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

               RemoveOngoingTranslation( job.Key );

               XuaLogger.Current.Info( $"Completed: '{job.Key.TrimmedTranslatableText}' => '{job.TranslatedText}'" );

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

               var key = job.Key;
               AddUnstartedJob( key, job );
               RemoveOngoingTranslation( key );
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

         RemoveOngoingTranslation( job.Key );

         XuaLogger.Current.Info( $"Completed: '{job.Key.TrimmedTranslatableText}' => '{job.TranslatedText}'" );

         Manager.InvokeJobCompleted( job );
      }

      private string PostProcessTranslation( UntranslatedText key, string translatedText )
      {
         var hasTranslation = !string.IsNullOrEmpty( translatedText );
         if( hasTranslation )
         {
            translatedText = key.RepairTemplate( translatedText );
            translatedText = key.LeadingWhitespace + translatedText + key.TrailingWhitespace;

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
               var key = job.Key;
               job.State = TranslationJobState.Failed;
               job.ErrorMessage = error;

               RemoveOngoingTranslation( key );

               RegisterTranslationFailureFor( key.TrimmedTranslatableText );

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

               var key = job.Key;
               AddUnstartedJob( key, job );
               RemoveOngoingTranslation( key );
            }

            XuaLogger.Current.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }

         if( !HasFailedDueToConsecutiveErrors )
         {
            ConsecutiveErrors++;

            if( HasFailedDueToConsecutiveErrors )
            {
               XuaLogger.Current.Error( $"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down translator endpoint." );

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

      public bool EnqueueTranslation( object ui, UntranslatedText key, TranslationResult translationResult, ParserTranslationContext context )
      {
         var added = AssociateWithExistingJobIfPossible( ui, key, translationResult, context );
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

               added = endpoint.AssociateWithExistingJobIfPossible( ui, key, translationResult, context );
               if( added )
               {
                  return false;
               }
            }
         }

         XuaLogger.Current.Debug( "Queued: '" + key.TrimmedTranslatableText + "'" );

         var saveResultGlobally = checkOtherEndpoints;
         var newJob = new TranslationJob( this, key, saveResultGlobally );
         newJob.Associate( ui, translationResult, context );

         return AddUnstartedJob( key, newJob );
      }

      private bool AssociateWithExistingJobIfPossible( object ui, UntranslatedText key, TranslationResult translationResult, ParserTranslationContext context )
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

      private bool AddUnstartedJob( UntranslatedText key, TranslationJob job )
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

      private void RemoveOngoingTranslation( UntranslatedText key )
      {
         if( _ongoingJobs.Remove( key ) )
         {
            Manager.OngoingTranslations--;
         }
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
            XuaLogger.Current.Warn( $"Dequeued: '{job.Key.TrimmedTranslatableText}'" );
            job.Value.State = TranslationJobState.Failed;
            job.Value.ErrorMessage = "Translation failed because all jobs on endpoint was cleared.";

            Manager.InvokeJobFailed( job.Value );
         }

         Manager.OngoingTranslations -= ongoingCount;
         Manager.UnstartedTranslations -= unstartedCount;

         Manager.UnscheduleUnstartedJobs( this );
      }

      private bool CanTranslate( string untranslatedText )
      {
         if( _failedTranslations.TryGetValue( untranslatedText, out var count ) )
         {
            return count < Settings.MaxFailuresForSameTextPerEndpoint;
         }
         return true;
      }

      private void RegisterTranslationFailureFor( string untranslatedText )
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
            if( Settings.SimulateDelayedError )
            {
               yield return new WaitForSeconds( 1 );

               context.FailWithoutThrowing( "Simulating delayed error. Press CTRL+ALT+NP8 to disable!", null );
            }
            else if( Settings.SimulateError )
            {
               context.FailWithoutThrowing( "Simulating error. Press CTRL+ALT+NP9 to disable!", null );
            }
            else
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
         }
         finally
         {
            _ongoingTranslations--;

            context.FailIfNotCompleted();
         }
      }
   }
}
