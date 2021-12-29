using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class SpamChecker
   {
      private int[] _currentTranslationsQueuedPerSecondRollingWindow = new int[ Settings.TranslationQueueWatchWindow ];
      private float? _timeExceededThreshold;
      private float _translationsQueuedPerSecond;

      private string[] _previouslyQueuedText = new string[ Settings.PreviousTextStaggerCount ];
      private int _staggerTextCursor = 0;
      private int _concurrentStaggers = 0;
      private int _lastStaggerCheckFrame = -1;

      private int _frameForLastQueuedTranslation = -1;
      private int _consecutiveFramesTranslated = 0;

      private int _secondForQueuedTranslation = -1;
      private int _consecutiveSecondsTranslated = 0;

      private TranslationManager _translationManager;

      public SpamChecker( TranslationManager translationManager )
      {
         _translationManager = translationManager;
      }

      public void PerformChecks( string untranslatedText, TranslationEndpointManager endpoint )
      {
         CheckStaggerText( untranslatedText );
         CheckConsecutiveFrames();
         CheckConsecutiveSeconds( endpoint );
         CheckThresholds( endpoint );
      }

      public void Update()
      {
         PeriodicResetFrameCheck();
         ResetThresholdTimerIfRequired();
      }

      private void CheckConsecutiveSeconds( TranslationEndpointManager endpoint )
      {
         var currentSecond = (int)Time.time;
         var lastSecond = currentSecond - 1;

         if( lastSecond == _secondForQueuedTranslation )
         {
            // we also queued something last frame, lets increment our counter
            _consecutiveSecondsTranslated++;

            if( _consecutiveSecondsTranslated > Settings.MaximumConsecutiveSecondsTranslated && endpoint.EnableSpamChecks )
            {
               // Shutdown, this wont be tolerated!!!
               _translationManager.ClearAllJobs();

               Settings.IsShutdown = true;
               XuaLogger.AutoTranslator.Error( $"SPAM DETECTED: Translations were queued every second for more than {Settings.MaximumConsecutiveSecondsTranslated} consecutive seconds. Shutting down plugin." );
            }

         }
         else if( currentSecond == _secondForQueuedTranslation )
         {
            // do nothing, there may be multiple translations per frame, that wont increase this counter
         }
         else
         {
            // but if multiple Update frames has passed, we will reset the counter
            _consecutiveSecondsTranslated = 0;
         }

         _secondForQueuedTranslation = currentSecond;
      }

      private void CheckConsecutiveFrames()
      {
         var currentFrame = Time.frameCount;
         var lastFrame = currentFrame - 1;

         if( lastFrame == _frameForLastQueuedTranslation )
         {
            // we also queued something last frame, lets increment our counter
            _consecutiveFramesTranslated++;

            if( _consecutiveFramesTranslated > Settings.MaximumConsecutiveFramesTranslated )
            {
               // Shutdown, this wont be tolerated!!!
               _translationManager.ClearAllJobs();

               Settings.IsShutdown = true;
               XuaLogger.AutoTranslator.Error( $"SPAM DETECTED: Translations were queued every frame for more than {Settings.MaximumConsecutiveFramesTranslated} consecutive frames. Shutting down plugin." );
            }

         }
         else if( currentFrame == _frameForLastQueuedTranslation )
         {
            // do nothing, there may be multiple translations per frame, that wont increase this counter
         }
         else if( _consecutiveFramesTranslated > 0 )
         {
            // but if multiple Update frames has passed, we will reset the counter
            _consecutiveFramesTranslated--;
         }

         _frameForLastQueuedTranslation = currentFrame;
      }

      private void PeriodicResetFrameCheck()
      {
         var currentSecond = (int)Time.time;
         if( currentSecond % 100 == 0 )
         {
            _consecutiveFramesTranslated = 0;
         }
      }

      private void CheckStaggerText( string untranslatedText )
      {
         var currentFrame = Time.frameCount;
         if( currentFrame != _lastStaggerCheckFrame )
         {
            _lastStaggerCheckFrame = currentFrame;

            bool wasProblematic = false;

            for( int i = 0; i < _previouslyQueuedText.Length; i++ )
            {
               var previouslyQueuedText = _previouslyQueuedText[ i ];

               if( previouslyQueuedText != null )
               {
                  if( untranslatedText.RemindsOf( previouslyQueuedText ) )
                  {
                     wasProblematic = true;
                     break;
                  }

               }
            }

            if( wasProblematic )
            {
               _concurrentStaggers++;
               if( _concurrentStaggers > Settings.MaximumStaggers )
               {
                  _translationManager.ClearAllJobs();

                  Settings.IsShutdown = true;
                  XuaLogger.AutoTranslator.Error( $"SPAM DETECTED: Text that is 'scrolling in' is being translated. Disable that feature. Shutting down plugin." );
               }
            }
            else
            {
               _concurrentStaggers = 0;
            }

            _previouslyQueuedText[ _staggerTextCursor % _previouslyQueuedText.Length ] = untranslatedText;
            _staggerTextCursor++;
         }
      }

      private void CheckThresholds( TranslationEndpointManager endpoint )
      {
         if( _translationManager.UnstartedTranslations > Settings.MaxUnstartedJobs )
         {
            _translationManager.ClearAllJobs();

            Settings.IsShutdown = true;
            XuaLogger.AutoTranslator.Error( $"SPAM DETECTED: More than {Settings.MaxUnstartedJobs} queued for translations due to unknown reasons. Shutting down plugin." );
         }

         var time = Time.time;
         var previousIdx = ( (int)( time - Time.deltaTime ) ) % Settings.TranslationQueueWatchWindow;
         var newIdx = ( (int)time ) % Settings.TranslationQueueWatchWindow;
         if( previousIdx != newIdx )
         {
            _currentTranslationsQueuedPerSecondRollingWindow[ newIdx ] = 0;
         }
         _currentTranslationsQueuedPerSecondRollingWindow[ newIdx ]++;

         var translationsInWindow = _currentTranslationsQueuedPerSecondRollingWindow.Sum();
         _translationsQueuedPerSecond = (float)translationsInWindow / Settings.TranslationQueueWatchWindow;
         if( _translationsQueuedPerSecond > Settings.MaxTranslationsQueuedPerSecond )
         {
            if( !_timeExceededThreshold.HasValue )
            {
               _timeExceededThreshold = time;
            }

            if( time - _timeExceededThreshold.Value > Settings.MaxSecondsAboveTranslationThreshold && endpoint.EnableSpamChecks )
            {
               _translationManager.ClearAllJobs();

               Settings.IsShutdown = true;
               XuaLogger.AutoTranslator.Error( $"SPAM DETECTED: More than {Settings.MaxTranslationsQueuedPerSecond} translations per seconds queued for a {Settings.MaxSecondsAboveTranslationThreshold} second period. Shutting down plugin." );
            }
         }
         else
         {
            _timeExceededThreshold = null;
         }
      }

      private void ResetThresholdTimerIfRequired()
      {
         var time = Time.time;
         var previousIdx = ( (int)( time - Time.deltaTime ) ) % Settings.TranslationQueueWatchWindow;
         var newIdx = ( (int)time ) % Settings.TranslationQueueWatchWindow;
         if( previousIdx != newIdx )
         {
            _currentTranslationsQueuedPerSecondRollingWindow[ newIdx ] = 0;
         }

         var translationsInWindow = _currentTranslationsQueuedPerSecondRollingWindow.Sum();
         _translationsQueuedPerSecond = (float)translationsInWindow / Settings.TranslationQueueWatchWindow;

         if( _translationsQueuedPerSecond <= Settings.MaxTranslationsQueuedPerSecond )
         {
            _timeExceededThreshold = null;
         }
      }
   }
}
