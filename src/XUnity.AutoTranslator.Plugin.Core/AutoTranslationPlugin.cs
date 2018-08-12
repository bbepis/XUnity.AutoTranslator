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

      public void Initialize()
      {
         Current = this;
         Logger.Current = new ConsoleLogger();

         Settings.Configure();

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

         _symbolCheck = TextHelper.GetSymbolCheck( Settings.FromLanguage );

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

      private string[] GetTranslationFiles()
      {
         return Directory.GetFiles( Path.Combine( Config.Current.DataPath, Settings.TranslationDirectory ), $"*.txt", SearchOption.AllDirectories ) // FIXME: Add $"*{Language}.txt"
            .Union( new[] { Settings.AutoTranslationsFilePath } )
            .Select( x => x.Replace( "/", "\\" ) )
            .Distinct()
            .OrderBy( x => x )
            .ToArray();
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
               var tab = new char[] { '\t' };
               var equals = new char[] { '=' };
               var splitters = new char[][] { tab, equals };

               foreach( var fullFileName in GetTranslationFiles() )
               {
                  if( File.Exists( fullFileName ) )
                  {
                     string[] translations = File.ReadAllLines( fullFileName, Encoding.UTF8 );
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
                                 AddTranslation( key, value );
                                 break;
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while loading translations." );
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

         CheckThresholds();

         return ongoingJob;
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
         if( Settings.CopyToClipboard )
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
         var lookup = key.GetDictionaryLookupKey();
         var result = _translations.TryGetValue( lookup, out value );
         if( result )
         {
            return result;
         }
         else if( _staticTranslations.Count > 0 )
         {
            if( _staticTranslations.TryGetValue( lookup, out value ) )
            {
               QueueNewTranslationForDisk( lookup, value );
               AddTranslation( lookup, value );
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
         if( _hooksEnabled )
         {
            return TranslateOrQueueWebJob( ui, text, true );
         }
         return null;
      }

      public void Hook_TextChanged( object ui )
      {
         if( _hooksEnabled )
         {
            TranslateOrQueueWebJob( ui, null, false );
         }
      }

      public void Hook_TextInitialized( object ui )
      {
         if( _hooksEnabled )
         {
            TranslateOrQueueWebJob( ui, null, true );
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

               ui.SetText( text );

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

      public bool ShouldTranslate( object ui )
      {
         var cui = ui as Component;
         if( cui != null )
         {
            var go = cui.gameObject;
            var isDummy = go.IsDummy();
            if( isDummy )
            {
               return false;
            }

            var inputField = cui.gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.InputField )
               ?? cui.gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.TMP_InputField );

            return inputField == null;
         }

         return true;
      }

      private string TranslateOrQueueWebJob( object ui, string text, bool isAwakening )
      {
         var info = ui.GetTranslationInfo( isAwakening );
         if( !info?.IsAwake ?? false )
         {
            return null;
         }

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

            var textKey = new TranslationKey( text, ui.IsSpammingComponent(), false );

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
         // Get the trimmed text
         text = ( text ?? ui.GetText() ).TrimIfConfigured();

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !string.IsNullOrEmpty( text ) && IsTranslatable( text ) && ShouldTranslate( ui ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( text );
            var textKey = new TranslationKey( text, ui.IsSpammingComponent(), context != null );


            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TryGetTranslation( textKey, out translation ) )
            {
               QueueNewUntranslatedForClipboard( textKey );

               if( !string.IsNullOrEmpty( translation ) )
               {
                  SetTranslatedText( ui, textKey.Untemplate( translation ), info );
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
                        translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, true );
                        if( translation != null )
                        {
                           SetTranslatedText( ui, translation, info ); // get rid of textKey here!!
                        }
                        return translation;
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
                                 var stabilizedTextKey = new TranslationKey( stabilizedText, false );

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
                                       if( _consecutiveErrors < Settings.MaxErrors && !Settings.IsShutdown )
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
               else
               {
                  // Lets try not to spam a service that might not be there...
                  if( _endpoint != null )
                  {
                     if( _consecutiveErrors < Settings.MaxErrors && !Settings.IsShutdown )
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
               var valueKey = new TranslationKey( value, false, true );
               string partTranslation;
               if( TryGetTranslation( valueKey, out partTranslation ) )
               {
                  translations.Add( key, partTranslation );
               }
               else if( allowStartJob )
               {
                  // incomplete, must start job
                  var context = new TranslationContext( ui, result );
                  TranslateOrQueueWebJobImmediate( null, value, null, false, context );
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
         if( currentTries < maxTries ) // shortcircuit
         {
            var beforeText = ui.GetText();
            yield return new WaitForSeconds( delay );
            var afterText = ui.GetText();

            if( beforeText == afterText )
            {
               onTextStabilized( afterText.TrimIfConfigured() );
            }
            else
            {
               StartCoroutine( WaitForTextStablization( ui, delay, maxTries, currentTries + 1, onTextStabilized, onMaxTriesExceeded ) );
            }
         }
         else
         {
            onMaxTriesExceeded();
         }
      }

      public IEnumerator DelayForSeconds( float delay, Action onContinue )
      {
         yield return new WaitForSeconds( delay );

         onContinue();
      }

      public void Update()
      {
         try
         {
            if( _endpoint != null )
            {
               _endpoint.OnUpdate();
            }

            CopyToClipboard();

            if( !Settings.IsShutdown )
            {
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

         var succeeded = batch.MatchWithTranslations( translatedTextBatch );
         if( succeeded )
         {
            foreach( var tracker in batch.Trackers )
            {
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
         _consecutiveErrors++;

         job.State = TranslationJobState.Failed;
         _ongoingJobs.Remove( job.Key.GetDictionaryLookupKey() );

         if( !Settings.IsShutdown )
         {
            if( _consecutiveErrors > Settings.MaxErrors )
            {
               if( _endpoint.ShouldGetSecondChanceAfterFailure() )
               {
                  Logger.Current.Warn( $"More than {Settings.MaxErrors} consecutive errors occurred. Entering fallback mode." );
                  _consecutiveErrors = 0;
               }
               else
               {
                  Settings.IsShutdown = true;
                  Logger.Current.Error( $"More than {Settings.MaxErrors} consecutive errors occurred. Shutting down plugin." );

                  _unstartedJobs.Clear();
                  _completedJobs.Clear();
                  _ongoingJobs.Clear();
               }
            }
         }
      }

      private void OnTranslationFailed( TranslationBatch batch )
      {
         _consecutiveErrors++;

         foreach( var tracker in batch.Trackers )
         {
            tracker.Job.State = TranslationJobState.Failed;
            _ongoingJobs.Remove( tracker.Job.Key.GetDictionaryLookupKey() );
         }

         if( !Settings.IsShutdown )
         {
            if( _consecutiveErrors > Settings.MaxErrors )
            {
               if( _endpoint.ShouldGetSecondChanceAfterFailure() )
               {
                  Logger.Current.Warn( $"More than {Settings.MaxErrors} consecutive errors occurred. Entering fallback mode." );
                  _consecutiveErrors = 0;
               }
               else
               {
                  Settings.IsShutdown = true;
                  Logger.Current.Error( $"More than {Settings.MaxErrors} consecutive errors occurred. Shutting down plugin." );

                  _unstartedJobs.Clear();
                  _completedJobs.Clear();
                  _ongoingJobs.Clear();
               }
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
                        var info = component.GetTranslationInfo( false );
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
                        var translatedText = TranslateOrQueueWebJobImmediateByParserResult( null, result, false );

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
                                 var info = context.Component.GetTranslationInfo( false );
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
               var key = new TranslationKey( info.OriginalText, false );
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

      private IEnumerable<GameObject> GetAllRoots()
      {
         var objects = GameObject.FindObjectsOfType<GameObject>();
         foreach( var obj in objects )
         {
            if( obj.transform.parent == null )
            {
               yield return obj;
            }
         }
      }

      private void TraverseChildren( StreamWriter writer, GameObject obj, string identation )
      {
         var layer = LayerMask.LayerToName( obj.gameObject.layer );
         var components = string.Join( ", ", obj.GetComponents<Component>().Select( x => x.GetType().Name ).ToArray() );
         var line = string.Format( "{0,-50} {1,100}",
            identation + obj.gameObject.name + " [" + layer + "]",
            components );

         writer.WriteLine( line );

         for( int i = 0 ; i < obj.transform.childCount ; i++ )
         {
            var child = obj.transform.GetChild( i );
            TraverseChildren( writer, child.gameObject, identation + " " );
         }
      }
   }
}
