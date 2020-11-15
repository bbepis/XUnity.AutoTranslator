using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Support;

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
         bool result;
         string untemplated;
         string unmodifiedValue;
         string unmodifiedKey;


         // lookup UNTEMPLATED translations - ALL VARIATIONS
         if( key.IsTemplated && !key.IsFromSpammingComponent )
         {
            // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
            // untemplated                = '   What are you \ndoing here, Sophie?'

            // lookup original
            untemplated = key.Untemplate( key.TemplatedOriginal_Text );
            result = _translations.TryGetValue( untemplated, out value );
            if( result )
            {
               return result;
            }

            // lookup original minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_ExternallyTrimmed = 'What are you \ndoing here, {{A}}?'
               // untemplated                                  = 'What are you \ndoing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_ExternallyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  AddTranslationToCache( unmodifiedKey, unmodifiedValue );

                  value = unmodifiedValue;
                  return result;
               }
            }

            // lookup internally trimmed
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'
               // untemplated                                  = '   What are you doing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_InternallyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = value;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  AddTranslationToCache( unmodifiedKey, unmodifiedValue );

                  value = unmodifiedValue;
                  return result;
               }
            }

            // lookup internally trimmed minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_FullyTrimmed = 'What are you doing here, {{A}}?'
               // untemplated                             = 'What are you doing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_FullyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  AddTranslationToCache( unmodifiedKey, unmodifiedValue );

                  value = unmodifiedValue;
                  return result;
               }
            }
         }

         // lookup original - ALL VARATIONS

         // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
         result = _translations.TryGetValue( key.TemplatedOriginal_Text, out value );
         if( result )
         {
            return result;
         }

         // lookup original minus external whitespace
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_ExternallyTrimmed = 'What are you \ndoing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_ExternallyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;

               AddTranslationToCache( key.TemplatedOriginal_Text, unmodifiedValue );

               value = unmodifiedValue;
               return result;
            }
         }

         // lookup internally trimmed
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_InternallyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               AddTranslationToCache( key.TemplatedOriginal_Text, value ); // FIXED: using templated original

               return result;
            }
         }

         // lookup internally trimmed minus external whitespace
         if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_FullyTrimmed = 'What are you doing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_FullyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;

               AddTranslationToCache( key.TemplatedOriginal_Text, unmodifiedValue );

               value = unmodifiedValue;
               return result;
            }
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
      }

      public void AddTranslationToCache( string key, string value )
      {
         if( !HasTranslated( key ) )
         {
            AddTranslation( key, value );

            // also add a modified version of the translation
            var ukey = new UntranslatedText( key, false, true, Settings.FromLanguageUsesWhitespaceBetweenWords );
            var uvalue = new UntranslatedText( value, false, true, Settings.ToLanguageUsesWhitespaceBetweenWords );
            if( ukey.Original_Text_ExternallyTrimmed != key && !HasTranslated( ukey.Original_Text_ExternallyTrimmed ) )
            {
               AddTranslation( ukey.Original_Text_ExternallyTrimmed, uvalue.Original_Text_ExternallyTrimmed );
            }
            if( ukey.Original_Text_ExternallyTrimmed != ukey.Original_Text_FullyTrimmed && !HasTranslated( ukey.Original_Text_FullyTrimmed ) )
            {
               AddTranslation( ukey.Original_Text_FullyTrimmed, uvalue.Original_Text_FullyTrimmed );
            }

            QueueNewTranslationForDisk( key, value );
         }
      }

      public bool IsTranslatable( string text )
      {
         return !IsTranslation( text );
      }

      private bool IsTranslation( string translation )
      {
         return !HasTranslated( translation ) && _reverseTranslations.ContainsKey( translation );
      }

      private bool HasTranslated( string key )
      {
         return _translations.ContainsKey( key );
      }

      private string GetTextToTranslate( TranslationJob job )
      {
         var removeInternalWhitespace = Settings.IgnoreWhitespaceInDialogue && job.Key.Original_Text.Length > Settings.MinDialogueChars;

         string text;
         if( removeInternalWhitespace )
         {
            text = job.Key.TemplatedOriginal_Text_FullyTrimmed;
         }
         else
         {
            text = job.Key.TemplatedOriginal_Text_ExternallyTrimmed;
         }

         text = PreProcessUntranslatedText( text );

         return text;
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


               if( job.IsTranslatable )
               {
                  var unpreparedUntranslatedText = GetTextToTranslate( job );
                  var untranslatedText = job.Key.PrepareUntranslatedText( unpreparedUntranslatedText );
                  if( CanTranslate( unpreparedUntranslatedText ) )
                  {
                     jobs.Add( job );
                     untranslatedTexts.Add( untranslatedText );
                     _ongoingJobs[ key ] = job;
                     Manager.OngoingTranslations++;

                     if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( "Started: '" + unpreparedUntranslatedText + "'" );
                  }
                  else
                  {
                     XuaLogger.AutoTranslator.Warn( $"Dequeued: '{unpreparedUntranslatedText}' because the current endpoint has already failed this translation 3 times." );
                     job.State = TranslationJobState.Failed;
                     job.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";

                     Manager.InvokeJobFailed( job );
                  }
               }
               else
               {
                  _ongoingJobs[ key ] = job;
                  Manager.OngoingTranslations++;
                  OnSingleTranslationCompleted( job, new[] { key.TemplatedOriginal_Text_ExternallyTrimmed }, false );
               }
            }

            if( jobs.Count > 0 )
            {
               AvailableBatchOperations--;
               var jobsArray = jobs.ToArray();

               CoroutineHelper.Instance.Start(
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

            if( job.IsTranslatable )
            {
               var unpreparedUntranslatedText = GetTextToTranslate( job );
               var untranslatedText = job.Key.PrepareUntranslatedText( unpreparedUntranslatedText );
               if( CanTranslate( unpreparedUntranslatedText ) )
               {
                  _ongoingJobs[ key ] = job;
                  Manager.OngoingTranslations++;

                  if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( "Started: '" + unpreparedUntranslatedText + "'" );
                  CoroutineHelper.Instance.Start(
                     Translate(
                        new[] { untranslatedText },
                        Settings.FromLanguage,
                        Settings.Language,
                        translatedText => OnSingleTranslationCompleted( job, translatedText, true ),
                        ( msg, e ) => OnTranslationFailed( new[] { job }, msg, e ) ) );
               }
               else
               {
                  XuaLogger.AutoTranslator.Warn( $"Dequeued: '{unpreparedUntranslatedText}' because the current endpoint has already failed this translation 3 times." );
                  job.State = TranslationJobState.Failed;
                  job.ErrorMessage = "The endpoint failed to perform this translation 3 or more times.";

                  Manager.InvokeJobFailed( job );
               }
            }
            else
            {
               _ongoingJobs[ key ] = job;
               Manager.OngoingTranslations++;
               OnSingleTranslationCompleted( job, new[] { key.TemplatedOriginal_Text_ExternallyTrimmed }, false );
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

               job.TranslatedText = PostProcessTranslation( job.Key, translatedText, true );
               job.State = TranslationJobState.Succeeded;

               RemoveOngoingTranslation( job.Key );

               if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Info( $"Completed: '{job.Key.TemplatedOriginal_Text}' => '{job.TranslatedText}'" );

               Manager.InvokeJobCompleted( job );
            }
         }
         else
         {
            if( !HasBatchLogicFailed )
            {
               CoroutineHelper.Instance.Start( EnableBatchingAfterDelay() );
            }

            HasBatchLogicFailed = true;
            for( int i = 0; i < jobs.Length; i++ )
            {
               var job = jobs[ i ];

               var key = job.Key;
               AddUnstartedJob( key, job );
               RemoveOngoingTranslation( key );
            }

            XuaLogger.AutoTranslator.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }
      }

      private void OnSingleTranslationCompleted( TranslationJob job, string[] translatedTexts, bool useTranslatorFriendlyArgs )
      {
         var translatedText = translatedTexts[ 0 ];

         ConsecutiveErrors = 0;

         job.TranslatedText = PostProcessTranslation( job.Key, translatedText, useTranslatorFriendlyArgs );
         job.State = TranslationJobState.Succeeded;

         RemoveOngoingTranslation( job.Key );

         if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Info( $"Completed: '{job.Key.TemplatedOriginal_Text}' => '{job.TranslatedText}'" );

         Manager.InvokeJobCompleted( job );
      }

      private string PostProcessTranslation( UntranslatedText key, string translatedText, bool useTranslatorFriendlyArgs )
      {
         var hasTranslation = !string.IsNullOrEmpty( translatedText );
         if( hasTranslation )
         {
            translatedText = key.FixTranslatedText( translatedText, useTranslatorFriendlyArgs );
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

      private string PreProcessUntranslatedText( string text )
      {
         if( Settings.HtmlEntityPreprocessing )
         {
            text = WebUtility.HtmlDecode( text );
         }

         if( Settings.Preprocessors.Count == 0 ) return text;

         foreach( var kvp in Settings.Preprocessors )
         {
            text = text.Replace( kvp.Key, kvp.Value );
         }

         return text;
      }

      private void OnTranslationFailed( TranslationJob[] jobs, string error, Exception e )
      {
         if( e == null )
         {
            XuaLogger.AutoTranslator.Error( error );
         }
         else
         {
            XuaLogger.AutoTranslator.Error( e, error );
         }

         if( jobs.Length == 1 )
         {
            foreach( var job in jobs )
            {
               var key = job.Key;
               job.State = TranslationJobState.Failed;
               job.ErrorMessage = error;

               RemoveOngoingTranslation( key );

               RegisterTranslationFailureFor( key.TemplatedOriginal_Text );

               Manager.InvokeJobFailed( job );

               XuaLogger.AutoTranslator.Error( $"Failed: '{job.Key.TemplatedOriginal_Text}'" );
            }
         }
         else
         {
            if( !HasBatchLogicFailed )
            {
               CoroutineHelper.Instance.Start( EnableBatchingAfterDelay() );
            }

            HasBatchLogicFailed = true;
            for( int i = 0; i < jobs.Length; i++ )
            {
               var job = jobs[ i ];

               var key = job.Key;
               AddUnstartedJob( key, job );
               RemoveOngoingTranslation( key );

               XuaLogger.AutoTranslator.Error( $"Failed: '{job.Key.TemplatedOriginal_Text}'" );
            }

            XuaLogger.AutoTranslator.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }

         if( !HasFailedDueToConsecutiveErrors )
         {
            ConsecutiveErrors++;

            if( HasFailedDueToConsecutiveErrors )
            {
               XuaLogger.AutoTranslator.Error( $"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down translator endpoint." );

               ClearAllJobs();
            }
         }
      }

      private IEnumerator EnableBatchingAfterDelay()
      {
         yield return CoroutineHelper.Instance.CreateWaitForSeconds( 60f );

         HasBatchLogicFailed = false;

         XuaLogger.AutoTranslator.Info( "Re-enabled batching." );
      }

      public TranslationJob EnqueueTranslation(
         object ui,
         UntranslatedText key,
         InternalTranslationResult translationResult,
         ParserTranslationContext context,
         bool checkOtherEndpoints,
         bool saveResultGlobally,
         bool isTranslatable )
      {
         var added = AssociateWithExistingJobIfPossible( ui, key, translationResult, context, saveResultGlobally );
         if( added )
         {
            return null;
         }

         if( checkOtherEndpoints )
         {
            var endpoints = Manager.ConfiguredEndpoints;
            var len = endpoints.Count;
            for( int i = 0; i < len; i++ )
            {
               var endpoint = endpoints[ i ];
               if( endpoint == this ) continue;

               added = endpoint.AssociateWithExistingJobIfPossible( ui, key, translationResult, context, saveResultGlobally );
               if( added )
               {
                  return null;
               }
            }
         }

         if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( "Queued: '" + key.TemplatedOriginal_Text + "'" );

         var newJob = new TranslationJob( this, key, saveResultGlobally, isTranslatable );
         newJob.Associate( key, ui, translationResult, context, saveResultGlobally );

         return AddUnstartedJob( key, newJob );
      }

      private bool AssociateWithExistingJobIfPossible( object ui, UntranslatedText key, InternalTranslationResult translationResult, ParserTranslationContext context, bool saveResultGlobally )
      {
         if( _unstartedJobs.TryGetValue( key, out TranslationJob unstartedJob ) )
         {
            unstartedJob.Associate( key, ui, translationResult, context, saveResultGlobally );
            return true;
         }

         if( _ongoingJobs.TryGetValue( key, out TranslationJob ongoingJob ) )
         {
            ongoingJob.Associate( key, ui, translationResult, context, saveResultGlobally );
            return true;
         }

         return false;
      }

      private TranslationJob AddUnstartedJob( UntranslatedText key, TranslationJob job )
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

            return job;
         }
         return null;
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
            XuaLogger.AutoTranslator.Warn( $"Dequeued: '{job.Key.TemplatedOriginal_Text}'" );
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
               yield return CoroutineHelper.Instance.CreateWaitForSeconds( 1f );

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
