using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ExIni;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using UnityEngine.EventSystems;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.AutoTranslator.Plugin.Core.IMGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Batching;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class AutoTranslationPlugin : MonoBehaviour
   {
      private static readonly char[][] TranslationSplitters = new char[][] { new char[] { '\t' }, new char[] { '=' } };

      /// <summary>
      /// Allow the instance to be accessed statically, as only one will exist.
      /// </summary>
      public static AutoTranslationPlugin Current;

      /// <summary>
      /// These are the currently running translation jobs (being translated by an http request).
      /// </summary>
      private List<TranslationJob> _completedJobs = new List<TranslationJob>();
      private Dictionary<string, TranslationJob> _unstartedJobs = new Dictionary<string, TranslationJob>();
      private Dictionary<string, TranslationJob> _ongoingJobs = new Dictionary<string, TranslationJob>();

      /// <summary>
      /// All the translations are stored in this dictionary.
      /// </summary>
      private Dictionary<string, string> _staticTranslations = new Dictionary<string, string>();
      private Dictionary<string, string> _translations = new Dictionary<string, string>();
      private Dictionary<string, string> _reverseTranslations = new Dictionary<string, string>();

      /// <summary>
      /// These are the new translations that has not yet been persisted to the file system.
      /// </summary>
      private object _writeToFileSync = new object();
      private Dictionary<string, string> _newTranslations = new Dictionary<string, string>();
      private HashSet<string> _newUntranslated = new HashSet<string>();

      /// <summary>
      /// Keeps track of things to copy to clipboard.
      /// </summary>
      private List<string> _textsToCopyToClipboardOrdered = new List<string>();
      private HashSet<string> _textsToCopyToClipboard = new HashSet<string>();
      private float _clipboardUpdated = Time.realtimeSinceStartup;

      /// <summary>
      /// The number of http translation errors that has occurred up until now.
      /// </summary>
      private int _consecutiveErrors = 0;

      /// <summary>
      /// This is a hash set that contains all Text components that is currently being worked on by
      /// the translation plugin.
      /// </summary>
      private HashSet<object> _ongoingOperations = new HashSet<object>();

      /// <summary>
      /// This function will check if there are symbols of a given language contained in a string.
      /// </summary>
      private Func<string, bool> _symbolCheck;

      private object _advEngine;
      private float? _nextAdvUpdate;

      private IKnownEndpoint _endpoint;

      private int[] _currentTranslationsQueuedPerSecondRollingWindow = new int[ Settings.TranslationQueueWatchWindow ];
      private float? _timeExceededThreshold;
      private float _translationsQueuedPerSecond;

      private bool _isInTranslatedMode = true;
      private bool _hooksEnabled = true;
      private bool _batchLogicHasFailed = false;

      private int _availableBatchOperations = Settings.MaxAvailableBatchOperations;
      private float _batchOperationSecondCounter = 0;

      private string[] _previouslyQueuedText = new string[ Settings.PreviousTextStaggerCount ];
      private int _staggerTextCursor = 0;
      private int _concurrentStaggers = 0;

      private int _frameForLastQueuedTranslation = -1;
      private int _consecutiveFramesTranslated = 0;

      private int _secondForQueuedTranslation = -1;
      private int _consecutiveSecondsTranslated = 0;

      private bool _changeFont = false;
      private bool _initialized = false;
      private bool _temporarilyDisabled = false;

      public void Initialize()
      {
         Current = this;
         if( Logger.Current == null )
         {
            Logger.Current = new ConsoleLogger();
         }

         try
         {
            Settings.Configure();
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred during configuration. Shutting plugin down." );

            _endpoint = null;
            Settings.IsShutdown = true;

            return;
         }

         if( Settings.EnableConsole ) DebugConsole.Enable();

         HooksSetup.InstallHooks();

         try
         {
            _endpoint = KnownEndpoints.FindEndpoint( Settings.ServiceEndpoint );
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An unexpected error occurred during initialization of endpoint." );
         }

         if( !TextHelper.IsFromLanguageSupported( Settings.FromLanguage ) )
         {
            Logger.Current.Error( $"The plugin has been configured to use the 'FromLanguage={Settings.FromLanguage}'. This language is not supported. Shutting plugin down." );

            _endpoint = null;
            Settings.IsShutdown = true;
         }

         _symbolCheck = TextHelper.GetSymbolCheck( Settings.FromLanguage );

         if( !string.IsNullOrEmpty( Settings.OverrideFont ) )
         {
            var available = Font.GetOSInstalledFontNames();
            if( !available.Contains( Settings.OverrideFont ) )
            {
               Logger.Current.Error( $"The specified override font is not available. Available fonts: " + string.Join( ", ", available ) );
               Settings.OverrideFont = null;
            }
            else
            {
               _changeFont = true;
            }
         }

         LoadTranslations();
         LoadStaticTranslations();

         // start a thread that will periodically removed unused references
         var t1 = new Thread( MaintenanceLoop );
         t1.IsBackground = true;
         t1.Start();

         // start a thread that will periodically save new translations
         var t2 = new Thread( SaveTranslationsLoop );
         t2.IsBackground = true;
         t2.Start();
      }

      private IEnumerable<string> GetTranslationFiles()
      {
         return Directory.GetFiles( Path.Combine( Config.Current.DataPath, Settings.TranslationDirectory ), $"*.txt", SearchOption.AllDirectories )
            .Select( x => x.Replace( "/", "\\" ) );
      }

      private void MaintenanceLoop( object state )
      {
         while( true )
         {
            try
            {
               ObjectExtensions.Cull();
            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "An unexpected error occurred while removing GC'ed resources." );
            }

            Thread.Sleep( 1000 * 60 );
         }
      }

      private void SaveTranslationsLoop( object state )
      {
         try
         {
            while( true )
            {
               if( _newTranslations.Count > 0 )
               {
                  lock( _writeToFileSync )
                  {
                     if( _newTranslations.Count > 0 )
                     {
                        using( var stream = File.Open( Settings.AutoTranslationsFilePath, FileMode.Append, FileAccess.Write ) )
                        using( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
                        {
                           foreach( var kvp in _newTranslations )
                           {
                              writer.WriteLine( TextHelper.Encode( kvp.Key ) + '=' + TextHelper.Encode( kvp.Value ) );
                           }
                           writer.Flush();
                        }
                        _newTranslations.Clear();
                     }
                  }
               }
               else
               {
                  Thread.Sleep( 5000 );
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while saving translations to disk." );
         }
      }

      /// <summary>
      /// Loads the translations found in Translation.{lang}.txt
      /// </summary>
      private void LoadTranslations()
      {
         try
         {
            lock( _writeToFileSync )
            {
               Directory.CreateDirectory( Path.Combine( Config.Current.DataPath, Settings.TranslationDirectory ) );
               Directory.CreateDirectory( Path.GetDirectoryName( Path.Combine( Config.Current.DataPath, Settings.OutputFile ) ) );

               var mainTranslationFile = Settings.AutoTranslationsFilePath.Replace( "/", "\\" );
               LoadTranslationsInFile( mainTranslationFile );
               foreach( var fullFileName in GetTranslationFiles().Reverse().Except( new[] { mainTranslationFile } ) )
               {
                  LoadTranslationsInFile( fullFileName );
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while loading translations." );
         }
      }

      private void LoadTranslationsInFile( string fullFileName )
      {
         if( File.Exists( fullFileName ) )
         {
            Logger.Current.Debug( $"Loading translations from {fullFileName}." );

            string[] translations = File.ReadAllLines( fullFileName, Encoding.UTF8 );
            foreach( string translation in translations )
            {
               for( int i = 0 ; i < TranslationSplitters.Length ; i++ )
               {
                  var splitter = TranslationSplitters[ i ];
                  string[] kvp = translation.Split( splitter, StringSplitOptions.None );
                  if( kvp.Length == 2 )
                  {
                     string key = TextHelper.Decode( kvp[ 0 ].TrimIfConfigured() );
                     string value = TextHelper.Decode( kvp[ 1 ].TrimIfConfigured() );

                     if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) && IsTranslatable( key ) )
                     {
                        AddTranslation( key, value );
                        break;
                     }
                  }
               }
            }
         }
      }

      private void LoadStaticTranslations()
      {
         if( Settings.UseStaticTranslations && Settings.FromLanguage == Settings.DefaultFromLanguage && Settings.Language == Settings.DefaultLanguage )
         {
            var tab = new char[] { '\t' };
            var equals = new char[] { '=' };
            var splitters = new char[][] { tab, equals };

            // load static translations from previous titles
            string[] translations = Properties.Resources.StaticTranslations.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string translation in translations )
            {
               for( int i = 0 ; i < splitters.Length ; i++ )
               {
                  var splitter = splitters[ i ];
                  string[] kvp = translation.Split( splitter, StringSplitOptions.None );
                  if( kvp.Length >= 2 )
                  {
                     string key = TextHelper.Decode( kvp[ 0 ].TrimIfConfigured() );
                     string value = TextHelper.Decode( kvp[ 1 ].TrimIfConfigured() );

                     if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) )
                     {
                        _staticTranslations[ key ] = value;
                        break;
                     }
                  }
               }
            }
         }
      }

      private TranslationJob GetOrCreateTranslationJobFor( object ui, TranslationKey key, TranslationContext context )
      {
         var lookupKey = key.GetDictionaryLookupKey();

         if( _unstartedJobs.TryGetValue( lookupKey, out TranslationJob unstartedJob ) )
         {
            unstartedJob.Associate( context );
            return unstartedJob;
         }

         if( _ongoingJobs.TryGetValue( lookupKey, out TranslationJob ongoingJob ) )
         {
            ongoingJob.Associate( context );
            return ongoingJob;
         }

         foreach( var completedJob in _completedJobs )
         {
            if( completedJob.Key.GetDictionaryLookupKey() == lookupKey )
            {
               completedJob.Associate( context );
               return completedJob;
            }
         }

         Logger.Current.Debug( "Queued translation for: " + lookupKey );

         ongoingJob = new TranslationJob( key );
         if( ui != null )
         {
            ongoingJob.OriginalSources.Add( ui );
         }
         ongoingJob.Associate( context );

         _unstartedJobs.Add( lookupKey, ongoingJob );

         CheckStaggerText( lookupKey );
         CheckConsecutiveFrames();
         CheckConsecutiveSeconds();
         CheckThresholds();

         return ongoingJob;
      }

      private void CheckConsecutiveSeconds()
      {
         var currentSecond = (int)Time.time;
         var lastSecond = currentSecond - 1;

         if( lastSecond == _secondForQueuedTranslation )
         {
            // we also queued something last frame, lets increment our counter
            _consecutiveSecondsTranslated++;

            if( _consecutiveSecondsTranslated > Settings.MaximumConsecutiveSecondsTranslated )
            {
               // Shutdown, this wont be tolerated!!!
               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();

               Settings.IsShutdown = true;
               Logger.Current.Error( $"SPAM DETECTED: Translations were queued every second for more than {Settings.MaximumConsecutiveSecondsTranslated} consecutive seconds. Shutting down plugin." );
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
               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();

               Settings.IsShutdown = true;
               Logger.Current.Error( $"SPAM DETECTED: Translations were queued every frame for more than {Settings.MaximumConsecutiveFramesTranslated} consecutive frames. Shutting down plugin." );
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

      public void PeriodicResetFrameCheck()
      {
         var currentSecond = (int)Time.time;
         if( currentSecond % 100 == 0 )
         {
            _consecutiveFramesTranslated = 0;
         }
      }

      private void CheckStaggerText( string untranslatedText )
      {
         bool wasProblematic = false;

         for( int i = 0 ; i < _previouslyQueuedText.Length ; i++ )
         {
            var previouslyQueuedText = _previouslyQueuedText[ i ];

            if( previouslyQueuedText != null )
            {
               if( untranslatedText.StartsWith( previouslyQueuedText ) || previouslyQueuedText.StartsWith( untranslatedText )
                  || untranslatedText.EndsWith( previouslyQueuedText ) || previouslyQueuedText.EndsWith( untranslatedText ) )
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
               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();

               Settings.IsShutdown = true;
               Logger.Current.Error( $"SPAM DETECTED: Text that is 'scrolling in' is being translated. Disable that feature. Shutting down plugin." );
            }
         }
         else
         {
            _concurrentStaggers = 0;
         }

         _previouslyQueuedText[ _staggerTextCursor % _previouslyQueuedText.Length ] = untranslatedText;
         _staggerTextCursor++;
      }

      private void CheckThresholds()
      {
         if( _unstartedJobs.Count > Settings.MaxUnstartedJobs )
         {
            _unstartedJobs.Clear();
            _completedJobs.Clear();
            _ongoingJobs.Clear();

            Settings.IsShutdown = true;
            Logger.Current.Error( $"SPAM DETECTED: More than {Settings.MaxUnstartedJobs} queued for translations due to unknown reasons. Shutting down plugin." );
         }

         var previousIdx = ( (int)( Time.time - Time.deltaTime ) ) % Settings.TranslationQueueWatchWindow;
         var newIdx = ( (int)Time.time ) % Settings.TranslationQueueWatchWindow;
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
               _timeExceededThreshold = Time.time;
            }

            if( Time.time - _timeExceededThreshold.Value > Settings.MaxSecondsAboveTranslationThreshold )
            {
               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();
               Settings.IsShutdown = true;

               Logger.Current.Error( $"SPAM DETECTED: More than {Settings.MaxTranslationsQueuedPerSecond} translations per seconds queued for a {Settings.MaxSecondsAboveTranslationThreshold} second period. Shutting down plugin." );
            }
         }
         else
         {
            _timeExceededThreshold = null;
         }
      }

      private void IncrementBatchOperations()
      {
         _batchOperationSecondCounter += Time.deltaTime;

         if( _batchOperationSecondCounter > Settings.IncreaseBatchOperationsEvery )
         {
            if( _availableBatchOperations < Settings.MaxAvailableBatchOperations )
            {
               _availableBatchOperations++;
            }

            _batchOperationSecondCounter = 0;
         }
      }

      private void ResetThresholdTimerIfRequired()
      {
         var previousIdx = ( (int)( Time.time - Time.deltaTime ) ) % Settings.TranslationQueueWatchWindow;
         var newIdx = ( (int)Time.time ) % Settings.TranslationQueueWatchWindow;
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

      private void AddTranslation( string key, string value )
      {
         _translations[ key ] = value;
         _reverseTranslations[ value ] = key;
      }

      private void AddTranslation( TranslationKey key, string value )
      {
         _translations[ key.GetDictionaryLookupKey() ] = value;
         _reverseTranslations[ value ] = key.GetDictionaryLookupKey();
      }

      private void QueueNewUntranslatedForClipboard( TranslationKey key )
      {
         if( Settings.CopyToClipboard && Features.SupportsClipboard )
         {
            if( !_textsToCopyToClipboard.Contains( key.RelevantText ) )
            {
               _textsToCopyToClipboard.Add( key.RelevantText );
               _textsToCopyToClipboardOrdered.Add( key.RelevantText );

               _clipboardUpdated = Time.realtimeSinceStartup;
            }
         }
      }

      private void QueueNewUntranslatedForDisk( TranslationKey key )
      {
         _newUntranslated.Add( key.GetDictionaryLookupKey() );
      }

      private void QueueNewTranslationForDisk( TranslationKey key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key.GetDictionaryLookupKey() ] = value;
         }
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key ] = value;
         }
      }

      private bool TryGetTranslation( TranslationKey key, out string value )
      {
         return TryGetTranslation( key.GetDictionaryLookupKey(), out value );
      }

      private bool TryGetTranslation( string key, out string value )
      {
         var result = _translations.TryGetValue( key, out value );
         if( result )
         {
            return result;
         }
         else if( _staticTranslations.Count > 0 )
         {
            if( _staticTranslations.TryGetValue( key, out value ) )
            {
               QueueNewTranslationForDisk( key, value );
               AddTranslation( key, value );
               return true;
            }
         }
         return result;
      }

      public bool TryGetReverseTranslation( string value, out string key )
      {
         return _reverseTranslations.TryGetValue( value, out key );
      }

      public string Hook_TextChanged_WithResult( object ui, string text )
      {
         if( !ui.IsKnownType() ) return null;

         if( _hooksEnabled && !_temporarilyDisabled )
         {
            return TranslateOrQueueWebJob( ui, text );
         }
         return null;
      }

      public void Hook_TextChanged( object ui )
      {
         if( _hooksEnabled && !_temporarilyDisabled )
         {
            TranslateOrQueueWebJob( ui, null );
         }
      }

      private void SetTranslatedText( object ui, string translatedText, TranslationInfo info )
      {
         info?.SetTranslatedText( translatedText );

         if( _isInTranslatedMode )
         {
            SetText( ui, translatedText, true, info );
         }
      }


      /// <summary>
      /// Sets the text of a UI  text, while ensuring this will not fire a text changed event.
      /// </summary>
      private void SetText( object ui, string text, bool isTranslated, TranslationInfo info )
      {
         if( !info?.IsCurrentlySettingText ?? true )
         {
            try
            {
               // TODO: Disable ANY Hook
               _hooksEnabled = false;

               if( info != null )
               {
                  info.IsCurrentlySettingText = true;
               }

               if( _changeFont )
               {
                  if( isTranslated )
                  {
                     info?.ChangeFont( ui );
                  }
                  else
                  {
                     info?.UnchangeFont( ui );
                  }
               }

               if( Settings.EnableUIResizing )
               {
                  if( isTranslated )
                  {
                     info?.ResizeUI( ui );
                  }
                  else
                  {
                     info?.UnresizeUI( ui );
                  }
               }

               // NGUI only behaves if you set the text after the resize behaviour
               ui.SetText( text );

               info?.ResetScrollIn( ui );
            }
            catch( TargetInvocationException )
            {
               // might happen with NGUI
            }
            catch( NullReferenceException )
            {
               // This is likely happened due to a scene change.
            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "An error occurred while setting text on a component." );
            }
            finally
            {
               _hooksEnabled = true;

               if( info != null )
               {
                  info.IsCurrentlySettingText = false;
               }
            }
         }
      }

      /// <summary>
      /// Determines if a text should be translated.
      /// </summary>
      private bool IsTranslatable( string str )
      {
         return _symbolCheck( str ) && str.Length <= Settings.MaxCharactersPerTranslation && !_reverseTranslations.ContainsKey( str );
      }

      private bool IsShortText( string str )
      {
         return str.Length <= ( Settings.MaxCharactersPerTranslation / 2 );
      }

      public bool ShouldTranslate( object ui )
      {
         var component = ui as Component;
         if( component != null )
         {
            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return false;
            }

            var behaviour = component as Behaviour;
            if( behaviour?.isActiveAndEnabled == false )
            {
               return false;
            }

            var inputField = component.gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.InputField )
               ?? component.gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.TMP_InputField );

            return inputField == null;
         }

         return true;
      }

      private string TranslateOrQueueWebJob( object ui, string text )
      {
         var info = ui.GetTranslationInfo();

         if( _ongoingOperations.Contains( ui ) )
         {
            return TranslateImmediate( ui, text, info );
         }

         var supportsStabilization = ui.SupportsStabilization();
         if( Settings.Delay == 0 || !supportsStabilization )
         {
            return TranslateOrQueueWebJobImmediate( ui, text, info, supportsStabilization );
         }
         else
         {
            StartCoroutine(
               DelayForSeconds( Settings.Delay, () =>
               {
                  TranslateOrQueueWebJobImmediate( ui, text, info, supportsStabilization );
               } ) );
         }

         return null;
      }

      public static bool IsCurrentlySetting( TranslationInfo info )
      {
         if( info == null ) return false;

         return info.IsCurrentlySettingText;
      }

      private string TranslateImmediate( object ui, string text, TranslationInfo info )
      {
         // Get the trimmed text
         text = ( text ?? ui.GetText() ).TrimIfConfigured();

         if( !string.IsNullOrEmpty( text ) && IsTranslatable( text ) && ShouldTranslate( ui ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( text );

            var textKey = new TranslationKey( ui, text, ui.IsSpammingComponent(), false );

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TryGetTranslation( textKey, out translation ) )
            {
               if( !string.IsNullOrEmpty( translation ) )
               {
                  SetTranslatedText( ui, textKey.Untemplate( translation ), info );
                  return translation;
               }
            }
         }

         return null;
      }

      /// <summary>
      /// Translates the string of a UI  text or queues it up to be translated
      /// by the HTTP translation service.
      /// </summary>
      private string TranslateOrQueueWebJobImmediate( object ui, string text, TranslationInfo info, bool supportsStabilization, TranslationContext context = null )
      {
         // make sure text exists
         text = text ?? ui.GetText();
         if( context == null )
         {
            // Get the trimmed text
            text = text.TrimIfConfigured();
         }

         //Logger.Current.Debug( ui.GetType().Name + ": " + text );

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !string.IsNullOrEmpty( text ) && IsTranslatable( text ) && ShouldTranslate( ui ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( text );
            var isSpammer = ui.IsSpammingComponent();
            var textKey = new TranslationKey( ui, text, isSpammer, context != null );

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TryGetTranslation( textKey, out translation ) )
            {
               QueueNewUntranslatedForClipboard( textKey );

               if( !string.IsNullOrEmpty( translation ) )
               {
                  if( context == null ) // never set text if operation is contextualized (only a part translation)
                  {
                     SetTranslatedText( ui, textKey.Untemplate( translation ), info );
                  }
                  return translation;
               }
            }
            else
            {
               if( context == null && ui.SupportsRichText() )
               {
                  var parser = UnityTextParsers.GetTextParserByGameEngine();
                  if( parser != null )
                  {
                     var result = parser.Parse( text );
                     if( result.HasRichSyntax )
                     {
                        var isWhitelisted = ui.IsWhitelistedForImmediateRichTextTranslation();

                        translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, isWhitelisted );
                        if( translation != null )
                        {
                           SetTranslatedText( ui, translation, info );
                           return translation;
                        }
                        else if( isWhitelisted )
                        {
                           return null;
                        }
                     }
                  }
               }

               if( supportsStabilization && context == null ) // never stabilize a text that is contextualized or that does not support stabilization
               {
                  // if we dont know what text to translate it to, we need to figure it out.
                  // this might take a while, so add the UI text component to the ongoing operations
                  // list, so we dont start multiple operations for it, as its text might be constantly
                  // changing.
                  _ongoingOperations.Add( ui );

                  // start a coroutine, that will execute once the string of the UI text has stopped
                  // changing. For all texts except 'story' texts, this will add a delay for exactly 
                  // 0.5s to the translation. This is barely noticable.
                  //
                  // on the other hand, for 'story' texts, this will take the time that it takes
                  // for the text to stop 'scrolling' in.
                  try
                  {
                     StartCoroutine(
                        WaitForTextStablization(
                           ui: ui,
                           delay: 1.0f, // 1 second to prevent '1 second tickers' from getting translated
                           maxTries: 60, // 50 tries, about 1 minute
                           currentTries: 0,
                           onMaxTriesExceeded: () =>
                           {
                              _ongoingOperations.Remove( ui );
                           },
                           onTextStabilized: stabilizedText =>
                           {
                              _ongoingOperations.Remove( ui );

                              if( !string.IsNullOrEmpty( stabilizedText ) && IsTranslatable( stabilizedText ) )
                              {
                                 var stabilizedTextKey = new TranslationKey( ui, stabilizedText, false );

                                 QueueNewUntranslatedForClipboard( stabilizedTextKey );

                                 info?.Reset( stabilizedText );

                                 // once the text has stabilized, attempt to look it up
                                 if( TryGetTranslation( stabilizedTextKey, out translation ) )
                                 {
                                    if( !string.IsNullOrEmpty( translation ) )
                                    {
                                       // stabilized, no need to untemplate
                                       SetTranslatedText( ui, translation, info );
                                    }
                                 }
                                 else
                                 {
                                    if( context == null && ui.SupportsRichText() )
                                    {
                                       var parser = UnityTextParsers.GetTextParserByGameEngine();
                                       if( parser != null )
                                       {
                                          var result = parser.Parse( stabilizedText );
                                          if( result.HasRichSyntax )
                                          {
                                             var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, true );
                                             if( translatedText != null )
                                             {
                                                // stabilized, no need to untemplate
                                                SetTranslatedText( ui, translatedText, info );
                                             }
                                             return;
                                          }
                                       }
                                    }

                                    // Lets try not to spam a service that might not be there...
                                    if( _endpoint != null )
                                    {
                                       if( !Settings.IsShutdown )
                                       {
                                          var job = GetOrCreateTranslationJobFor( ui, stabilizedTextKey, context );
                                          job.Components.Add( ui );
                                       }
                                    }
                                    else
                                    {
                                       QueueNewUntranslatedForDisk( stabilizedTextKey );
                                    }
                                 }
                              }

                           } ) );
                  }
                  catch( Exception )
                  {
                     _ongoingOperations.Remove( ui );
                  }
               }
               else if( !isSpammer || ( isSpammer && IsShortText( text ) ) )
               {
                  // Lets try not to spam a service that might not be there...
                  if( _endpoint != null )
                  {
                     if( !Settings.IsShutdown )
                     {
                        var job = GetOrCreateTranslationJobFor( ui, textKey, context );
                     }
                  }
                  else
                  {
                     QueueNewUntranslatedForDisk( textKey );
                  }
               }
            }
         }

         return null;
      }

      private string TranslateOrQueueWebJobImmediateByParserResult( object ui, ParserResult result, bool allowStartJob )
      {
         Dictionary<string, string> translations = new Dictionary<string, string>();

         // attempt to lookup ALL strings immediately; return result if possible; queue operations
         foreach( var kvp in result.Arguments )
         {
            var key = kvp.Key;
            var value = kvp.Value.TrimIfConfigured();
            if( !string.IsNullOrEmpty( value ) && IsTranslatable( value ) )
            {
               string partTranslation;
               if( TryGetTranslation( value, out partTranslation ) )
               {
                  translations.Add( key, partTranslation );
               }
               else if( allowStartJob )
               {
                  // incomplete, must start job
                  var context = new TranslationContext( ui, result );
                  TranslateOrQueueWebJobImmediate( ui, value, null, false, context );
               }
            }
            else
            {
               // the value will do
               translations.Add( key, value );
            }
         }

         if( result.Arguments.Count == translations.Count )
         {
            return result.Untemplate( translations );
         }
         else
         {
            return null; // could not perform complete translation
         }
      }

      /// <summary>
      /// Utility method that allows me to wait to call an action, until
      /// the text has stopped changing. This is important for 'story'
      /// mode text, which 'scrolls' into place slowly.
      /// </summary>
      public IEnumerator WaitForTextStablization( object ui, float delay, int maxTries, int currentTries, Action<string> onTextStabilized, Action onMaxTriesExceeded )
      {
         yield return 0; // wait a single frame to allow any external plugins to complete their hooking logic

         bool succeeded = false;
         while( currentTries < maxTries ) // shortcircuit
         {
            var beforeText = ui.GetText();
            yield return new WaitForSeconds( delay );
            var afterText = ui.GetText();

            if( beforeText == afterText )
            {
               onTextStabilized( afterText.TrimIfConfigured() );
               succeeded = true;
               break;
            }

            currentTries++;
         }

         if( !succeeded )
         {
            onMaxTriesExceeded();
         }
      }

      public IEnumerator DelayForSeconds( float delay, Action onContinue )
      {
         yield return new WaitForSeconds( delay );

         onContinue();
      }

      public void Start()
      {
         if( !_initialized )
         {
            _initialized = true;

            try
            {
               Initialize();
               ManualHook();
            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "An unexpected error occurred during plugin initialization." );
            }
         }
      }

      public void Update()
      {
         try
         {
            if( _endpoint != null )
            {
               _endpoint.OnUpdate();
            }

            if( Features.SupportsClipboard )
            {
               CopyToClipboard();
            }

            if( !Settings.IsShutdown )
            {
               PeriodicResetFrameCheck();
               IncrementBatchOperations();
               ResetThresholdTimerIfRequired();
               KickoffTranslations();
               FinishTranslations();

               if( _nextAdvUpdate.HasValue && Time.time > _nextAdvUpdate )
               {
                  _nextAdvUpdate = null;
                  UpdateUtageText();
               }
            }

            if( Input.anyKey )
            {
               if( Settings.EnablePrintHierarchy && ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) ) && Input.GetKeyDown( KeyCode.Y ) )
               {
                  PrintObjects();
               }
               else if( ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) ) && Input.GetKeyDown( KeyCode.T ) )
               {
                  ToggleTranslation();
               }
               else if( ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) ) && Input.GetKeyDown( KeyCode.D ) )
               {
                  DumpUntranslated();
               }
               else if( ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) ) && Input.GetKeyDown( KeyCode.R ) )
               {
                  ReloadTranslations();
               }
               else if( ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) ) && Input.GetKeyDown( KeyCode.U ) )
               {
                  ManualHook();
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred in Update callback. " );
         }
      }

      // create this as a field instead of local var, to prevent new creation on EVERY game loop
      private readonly List<string> _kickedOff = new List<string>();

      private void KickoffTranslations()
      {
         if( _endpoint == null ) return;

         if( Settings.EnableBatching && _endpoint.SupportsLineSplitting && !_batchLogicHasFailed && _unstartedJobs.Count > 1 && _availableBatchOperations > 0 )
         {
            while( _unstartedJobs.Count > 0 && _availableBatchOperations > 0 )
            {
               if( _endpoint.IsBusy ) break;

               var kvps = _unstartedJobs.Take( Settings.BatchSize ).ToList();
               var batch = new TranslationBatch();

               foreach( var kvp in kvps )
               {
                  var key = kvp.Key;
                  var job = kvp.Value;
                  _kickedOff.Add( key );

                  if( !job.AnyComponentsStillHasOriginalUntranslatedTextOrContextual() ) continue;

                  batch.Add( job );
                  _ongoingJobs[ key ] = job;
               }

               if( !batch.IsEmpty )
               {
                  _availableBatchOperations--;

                  StartCoroutine( _endpoint.Translate( batch.GetFullTranslationKey(), Settings.FromLanguage, Settings.Language, translatedText => OnBatchTranslationCompleted( batch, translatedText ),
                  () => OnTranslationFailed( batch ) ) );
               }
            }
         }
         else
         {
            foreach( var kvp in _unstartedJobs )
            {
               if( _endpoint.IsBusy ) break;

               var key = kvp.Key;
               var job = kvp.Value;
               _kickedOff.Add( key );

               // lets see if the text should still be translated before kicking anything off
               if( !job.AnyComponentsStillHasOriginalUntranslatedTextOrContextual() ) continue;

               _ongoingJobs[ key ] = job;

               StartCoroutine( _endpoint.Translate( job.Key.GetDictionaryLookupKey(), Settings.FromLanguage, Settings.Language, translatedText => OnSingleTranslationCompleted( job, translatedText ),
               () => OnTranslationFailed( job ) ) );
            }
         }

         for( int i = 0 ; i < _kickedOff.Count ; i++ )
         {
            _unstartedJobs.Remove( _kickedOff[ i ] );
         }

         _kickedOff.Clear();
      }

      public void OnBatchTranslationCompleted( TranslationBatch batch, string translatedTextBatch )
      {
         if( !Settings.IsShutdown )
         {
            if( Settings.TranslationCount > Settings.MaxTranslationsBeforeShutdown )
            {
               Settings.IsShutdown = true;
               Logger.Current.Error( $"Maximum translations ({Settings.MaxTranslationsBeforeShutdown}) per session reached. Shutting plugin down." );
            }
         }

         _consecutiveErrors = 0;

         var succeeded = batch.MatchWithTranslations( translatedTextBatch );
         if( succeeded )
         {
            foreach( var tracker in batch.Trackers )
            {
               Settings.TranslationCount++;

               var job = tracker.Job;
               var translatedText = tracker.RawTranslatedText;
               if( !string.IsNullOrEmpty( translatedText ) )
               {
                  if( Settings.ForceSplitTextAfterCharacters > 0 )
                  {
                     translatedText = translatedText.SplitToLines( Settings.ForceSplitTextAfterCharacters, '\n', ' ', '　' );
                  }
                  job.TranslatedText = job.Key.RepairTemplate( translatedText );

                  QueueNewTranslationForDisk( job.Key, translatedText );
                  _completedJobs.Add( job );
               }

               AddTranslation( job.Key, job.TranslatedText );
               job.State = TranslationJobState.Succeeded;
               _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );
            }
         }
         else
         {
            // might as well re-add all translation jobs, and never do this again!
            _batchLogicHasFailed = true;
            foreach( var tracker in batch.Trackers )
            {
               Settings.TranslationCount++;

               var key = tracker.Job.Key.GetDictionaryLookupKey();
               if( !_unstartedJobs.ContainsKey( key ) )
               {
                  _unstartedJobs[ key ] = tracker.Job;
               }
               _ongoingJobs.Remove( key );
            }

            Logger.Current.Error( "A batch operation failed. Disabling batching and restarting failed jobs." );
         }
      }

      private void OnSingleTranslationCompleted( TranslationJob job, string translatedText )
      {
         Settings.TranslationCount++;

         if( !Settings.IsShutdown )
         {
            if( Settings.TranslationCount > Settings.MaxTranslationsBeforeShutdown )
            {
               Settings.IsShutdown = true;
               Logger.Current.Error( $"Maximum translations ({Settings.MaxTranslationsBeforeShutdown}) per session reached. Shutting plugin down." );
            }
         }

         _consecutiveErrors = 0;

         if( !string.IsNullOrEmpty( translatedText ) )
         {
            if( Settings.ForceSplitTextAfterCharacters > 0 )
            {
               translatedText = translatedText.SplitToLines( Settings.ForceSplitTextAfterCharacters, '\n', ' ', '　' );
            }
            job.TranslatedText = job.Key.RepairTemplate( translatedText );

            QueueNewTranslationForDisk( job.Key, translatedText );
            _completedJobs.Add( job );
         }

         AddTranslation( job.Key, job.TranslatedText );
         job.State = TranslationJobState.Succeeded;
         _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );
      }

      private void OnTranslationFailed( TranslationJob job )
      {
         Settings.TranslationCount++; // counts as a translation
         _consecutiveErrors++;

         job.State = TranslationJobState.Failed;
         _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );

         if( !Settings.IsShutdown )
         {
            if( _consecutiveErrors >= Settings.MaxErrors )
            {
               Settings.IsShutdown = true;
               Logger.Current.Error( $"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down plugin." );

               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();
            }
         }
      }

      private void OnTranslationFailed( TranslationBatch batch )
      {
         Settings.TranslationCount++; // counts as a translation
         _consecutiveErrors++;

         foreach( var tracker in batch.Trackers )
         {
            tracker.Job.State = TranslationJobState.Failed;
            _ongoingJobs.Remove( tracker.Job.Key.GetDictionaryLookupKey() );
         }

         if( !Settings.IsShutdown )
         {
            if( _consecutiveErrors >= Settings.MaxErrors )
            {
               Settings.IsShutdown = true;
               Logger.Current.Error( $"{Settings.MaxErrors} or more consecutive errors occurred. Shutting down plugin." );

               _unstartedJobs.Clear();
               _completedJobs.Clear();
               _ongoingJobs.Clear();
            }
         }
      }

      private void FinishTranslations()
      {
         if( _completedJobs.Count > 0 )
         {
            for( int i = _completedJobs.Count - 1 ; i >= 0 ; i-- )
            {
               var job = _completedJobs[ i ];
               _completedJobs.RemoveAt( i );

               foreach( var component in job.Components )
               {
                  // update the original text, but only if it has not been chaanged already for some reason (could be other translator plugin or game itself)
                  try
                  {
                     var text = component.GetText().TrimIfConfigured();
                     if( text == job.Key.OriginalText )
                     {
                        var info = component.GetTranslationInfo();
                        SetTranslatedText( component, job.TranslatedText, info );
                     }
                  }
                  catch( NullReferenceException )
                  {
                     // might fail if compoent is no longer associated to game
                  }
               }

               // handle each context
               foreach( var context in job.Contexts )
               {
                  // are all jobs within this context completed? If so, we can set the text
                  if( context.Jobs.All( x => x.State == TranslationJobState.Succeeded ) )
                  {
                     try
                     {

                        var text = context.Component.GetText().TrimIfConfigured();
                        var result = context.Result;
                        Dictionary<string, string> translations = new Dictionary<string, string>();
                        var translatedText = TranslateOrQueueWebJobImmediateByParserResult( context.Component, result, false );

                        if( !string.IsNullOrEmpty( translatedText ) )
                        {
                           if( !_translations.ContainsKey( context.Result.OriginalText ) )
                           {
                              AddTranslation( context.Result.OriginalText, translatedText );
                              QueueNewTranslationForDisk( context.Result.OriginalText, translatedText );
                           }

                           if( text == result.OriginalText )
                           {
                              if( translatedText != null )
                              {
                                 var info = context.Component.GetTranslationInfo();
                                 SetTranslatedText( context.Component, translatedText, info );
                              }
                           }
                        }
                     }
                     catch( NullReferenceException )
                     {

                     }
                  }
               }


               // Utage support
               if( Constants.Types.AdvEngine != null
                  && job.OriginalSources.Any( x => Constants.Types.AdvCommand.IsAssignableFrom( x.GetType() ) ) )
               {
                  _nextAdvUpdate = Time.time + 0.5f;
               }
            }
         }
      }

      private void UpdateUtageText()
      {
         if( _advEngine == null )
         {
            _advEngine = GameObject.FindObjectOfType( Constants.Types.AdvEngine );
         }

         if( _advEngine != null )
         {
            AccessTools.Method( Constants.Types.AdvEngine, "ChangeLanguage" )?.Invoke( _advEngine, new object[ 0 ] );
         }
      }

      private void ReloadTranslations()
      {
         LoadTranslations();

         foreach( var kvp in ObjectExtensions.GetAllRegisteredObjects() )
         {
            var info = kvp.Value as TranslationInfo;
            if( info != null && !string.IsNullOrEmpty( info.OriginalText ) )
            {
               var key = new TranslationKey( kvp.Key, info.OriginalText, false );
               if( TryGetTranslation( key, out string translatedText ) && !string.IsNullOrEmpty( translatedText ) )
               {
                  SetTranslatedText( kvp.Key, translatedText, info ); // no need to untemplatize the translated text
               }
            }
         }
      }

      private string CalculateDumpFileName()
      {
         int idx = 0;
         string fileName = null;
         do
         {
            idx++;
            fileName = $"UntranslatedDump{idx}.txt";
         }
         while( File.Exists( fileName ) );

         return fileName;
      }

      private void DumpUntranslated()
      {
         if( _newUntranslated.Count > 0 )
         {
            using( var stream = File.Open( CalculateDumpFileName(), FileMode.Append, FileAccess.Write ) )
            using( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
            {
               foreach( var untranslated in _newUntranslated )
               {
                  writer.WriteLine( TextHelper.Encode( untranslated ) + '=' );
               }
               writer.Flush();
            }

            _newUntranslated.Clear();
         }
      }

      private void ToggleTranslation()
      {
         _isInTranslatedMode = !_isInTranslatedMode;
         var objects = ObjectExtensions.GetAllRegisteredObjects();

         Logger.Current.Info( $"Toggling translations of {objects.Count} objects." );

         if( _isInTranslatedMode )
         {
            // make sure we use the translated version of all texts
            foreach( var kvp in objects )
            {
               var ui = kvp.Key;
               try
               {
                  if( ( ui as Component )?.gameObject?.activeSelf ?? false )
                  {
                     var info = (TranslationInfo)kvp.Value;

                     if( info != null && info.IsTranslated )
                     {
                        SetText( ui, info.TranslatedText, true, info );
                     }
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ObjectExtensions.Remove( ui );
               }
            }
         }
         else
         {
            // make sure we use the original version of all texts
            foreach( var kvp in objects )
            {
               var ui = kvp.Key;
               try
               {
                  if( ( ui as Component )?.gameObject?.activeSelf ?? false )
                  {
                     var info = (TranslationInfo)kvp.Value;

                     if( info != null && info.IsTranslated )
                     {
                        SetText( ui, info.OriginalText, true, info );
                     }
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ObjectExtensions.Remove( ui );
               }
            }
         }
      }

      private void CopyToClipboard()
      {
         if( Settings.CopyToClipboard
            && _textsToCopyToClipboardOrdered.Count > 0
            && Time.realtimeSinceStartup - _clipboardUpdated > Settings.ClipboardDebounceTime )
         {
            try
            {
               var builder = new StringBuilder();
               foreach( var text in _textsToCopyToClipboardOrdered )
               {
                  if( text.Length + builder.Length > Settings.MaxClipboardCopyCharacters ) break;

                  builder.AppendLine( text );
               }

               TextEditor editor = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );
               editor.text = builder.ToString();
               editor.SelectAll();
               editor.Copy();

            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "An error while copying text to clipboard." );
            }
            finally
            {
               _textsToCopyToClipboard.Clear();
               _textsToCopyToClipboardOrdered.Clear();
            }
         }
      }

      private void PrintObjects()
      {

         using( var stream = File.Open( Path.Combine( Environment.CurrentDirectory, "hierarchy.txt" ), FileMode.Create ) )
         using( var writer = new StreamWriter( stream ) )
         {
            foreach( var root in GetAllRoots() )
            {
               TraverseChildren( writer, root, "" );
            }

            writer.Flush();
         }
      }

      private void ManualHook()
      {
         foreach( var root in GetAllRoots() )
         {
            TraverseChildrenManualHook( root );
         }
      }

      private IEnumerable<GameObject> GetAllRoots()
      {
         var objects = GameObject.FindObjectsOfType<GameObject>();
         foreach( var obj in objects )
         {
            if( obj.transform != null && obj.transform.parent == null )
            {
               yield return obj;
            }
         }
      }

      private void TraverseChildren( StreamWriter writer, GameObject obj, string identation )
      {
         if( obj != null )
         {
            var layer = LayerMask.LayerToName( obj.layer );
            var components = string.Join( ", ", obj.GetComponents<Component>().Select( x => x?.GetType()?.Name ).Where( x => x != null ).ToArray() );
            var line = string.Format( "{0,-50} {1,100}",
               identation + obj.name + " [" + layer + "]",
               components );

            writer.WriteLine( line );

            if( obj.transform != null )
            {
               for( int i = 0 ; i < obj.transform.childCount ; i++ )
               {
                  var child = obj.transform.GetChild( i );
                  TraverseChildren( writer, child.gameObject, identation + " " );
               }
            }
         }
      }

      private void TraverseChildrenManualHook( GameObject obj )
      {
         if( obj != null )
         {
            var components = obj.GetComponents<Component>();
            foreach( var component in components )
            {
               if( component.IsKnownType() )
               {
                  Hook_TextChanged( component );
               }
            }

            if( obj.transform != null )
            {
               for( int i = 0 ; i < obj.transform.childCount ; i++ )
               {
                  var child = obj.transform.GetChild( i );
                  TraverseChildrenManualHook( child.gameObject );
               }
            }
         }
      }

      public void DisableAutoTranslator()
      {
         _temporarilyDisabled = true;
      }

      public void EnableAutoTranslator()
      {
         _temporarilyDisabled = false;
      }
   }
}
