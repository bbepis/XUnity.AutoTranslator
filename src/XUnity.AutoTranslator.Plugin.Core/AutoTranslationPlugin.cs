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

      /// <summary>
      /// All the translations are stored in this dictionary.
      /// </summary>
      private Dictionary<string, string> _translations = new Dictionary<string, string>();

      /// <summary>
      /// These are the new translations that has not yet been persisted to the file system.
      /// </summary>
      private object _writeToFileSync = new object();
      private Dictionary<string, string> _newTranslations = new Dictionary<string, string>();
      private HashSet<string> _newUntranslated = new HashSet<string>();
      private HashSet<string> _translatedTexts = new HashSet<string>();

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
      private HashSet<string> _startedOperationsForNonStabilizableComponents = new HashSet<string>();

      /// <summary>
      /// This function will check if there are symbols of a given language contained in a string.
      /// </summary>
      private Func<string, bool> _symbolCheck;

      private int[] _currentTranslationsQueuedPerSecondRollingWindow = new int[ Settings.TranslationQueueWatchWindow ];
      private float? _timeExceededThreshold;

      private bool _isInTranslatedMode = true;
      private bool _hooksEnabled = true;

      public void Initialize()
      {
         Current = this;

         Settings.Configure();

         HooksSetup.InstallHooks( Override_TextChanged );

         AutoTranslateClient.Configure();

         _symbolCheck = TextHelper.GetSymbolCheck( Settings.FromLanguage );

         LoadTranslations();

         // start a thread that will periodically removed unused references
         var t1 = new Thread( RemovedUnusedReferences );
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

      private void RemovedUnusedReferences( object state )
      {
         while( true )
         {
            try
            {
               ObjectExtensions.Cull();
            }
            catch( Exception e )
            {
               Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An unexpected error occurred while removing GC'ed resources." + Environment.NewLine + e );
            }
            finally
            {
               Thread.Sleep( 1000 * 60 );
            }
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
            Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An error occurred while saving translations to disk. " + Environment.NewLine + e );
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

               foreach( var fullFileName in GetTranslationFiles() )
               {
                  if( File.Exists( fullFileName ) )
                  {
                     string[] translations = File.ReadAllLines( fullFileName, Encoding.UTF8 );
                     foreach( string translation in translations )
                     {
                        string[] kvp = translation.Split( new char[] { '=', '\t' }, StringSplitOptions.None );
                        if( kvp.Length >= 2 )
                        {
                           string key = TextHelper.Decode( kvp[ 0 ].Trim() );
                           string value = TextHelper.Decode( kvp[ 1 ].Trim() );

                           if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) )
                           {
                              AddTranslation( key, value );
                           }
                        }
                     }
                  }
               }
            }
         }
         catch( Exception e )
         {
            Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An error occurred while loading translations. " + Environment.NewLine + e );
         }
      }

      private TranslationJob GetOrCreateTranslationJobFor( TranslationKeys key )
      {
         if( _unstartedJobs.TryGetValue( key.RelevantKey, out TranslationJob job ) )
         {
            return job;
         }

         foreach( var completedJob in _completedJobs )
         {
            if( completedJob.Keys.RelevantKey == key.RelevantKey )
            {
               return completedJob;
            }
         }

         job = new TranslationJob( key );
         _unstartedJobs.Add( key.RelevantKey, job );

         CheckThresholds();

         return job;
      }

      private void CheckThresholds()
      {
         var previousIdx = ( (int)( Time.time - Time.deltaTime ) ) % Settings.TranslationQueueWatchWindow;
         var newIdx = ( (int)Time.time ) % Settings.TranslationQueueWatchWindow;
         if( previousIdx != newIdx )
         {
            _currentTranslationsQueuedPerSecondRollingWindow[ newIdx ] = 0;
         }
         _currentTranslationsQueuedPerSecondRollingWindow[ newIdx ]++;

         var translationsInWindow = _currentTranslationsQueuedPerSecondRollingWindow.Sum();
         var translationsPerSecond = (float)translationsInWindow / Settings.TranslationQueueWatchWindow;
         if( translationsPerSecond > Settings.MaxTranslationsQueuedPerSecond )
         {
            if( !_timeExceededThreshold.HasValue )
            {
               _timeExceededThreshold = Time.time;
            }

            if( Time.time - _timeExceededThreshold.Value > Settings.MaxSecondsAboveTranslationThreshold || _unstartedJobs.Count > Settings.MaxUnstartedJobs )
            {
               _unstartedJobs.Clear();
               _completedJobs.Clear();
               Settings.IsShutdown = true;

               Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: Shutting down... spam detected." );
            }
         }
         else
         {
            _timeExceededThreshold = null;
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
         var translationsPerSecond = (float)translationsInWindow / Settings.TranslationQueueWatchWindow;

         if( translationsPerSecond <= Settings.MaxTranslationsQueuedPerSecond )
         {
            _timeExceededThreshold = null;
         }
      }

      private void AddTranslation( string key, string value )
      {
         _translations[ key ] = value;
         _translatedTexts.Add( value );
      }

      private void AddTranslation( TranslationKeys key, string value )
      {
         _translations[ key.RelevantKey ] = value;
         _translatedTexts.Add( value );
      }

      private void QueueNewUntranslatedForClipboard( TranslationKeys key )
      {
         if( Settings.CopyToClipboard )
         {
            if( !_textsToCopyToClipboard.Contains( key.RelevantKey ) )
            {
               _textsToCopyToClipboard.Add( key.RelevantKey );
               _textsToCopyToClipboardOrdered.Add( key.RelevantKey );

               _clipboardUpdated = Time.realtimeSinceStartup;
            }
         }
      }

      private void QueueNewUntranslatedForDisk( TranslationKeys key )
      {
         _newUntranslated.Add( key.RelevantKey );
      }

      private void QueueNewTranslationForDisk( TranslationKeys key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key.RelevantKey ] = value;
         }
      }

      private bool TryGetTranslation( TranslationKeys key, out string value )
      {
         return ( Settings.IgnoreWhitespaceInDialogue && key.IsDialogue && _translations.TryGetValue( key.DialogueKey, out value ) ) 
            || _translations.TryGetValue( key.OriginalKey, out value );
      }

      private string Override_TextChanged( object ui, string text )
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

      private void SetTranslatedText( object ui, string text, TranslationInfo info )
      {
         info?.SetTranslatedText( text );

         if( _isInTranslatedMode )
         {
            SetText( ui, text, true, info );
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

               if( isTranslated )
               {
                  info?.ResizeUI( ui );
               }
               else
               {
                  info?.UnresizeUI( ui );
               }
            }
            catch( Exception e )
            {
               Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An error occurred while setting text on a component." + Environment.NewLine + e );
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
         return _symbolCheck( str ) && str.Length <= Settings.MaxCharactersPerTranslation && !_translatedTexts.Contains( str );
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
            return null;
         }


         if( Settings.Delay == 0 || !SupportsStabilization( ui ) )
         {
            return TranslateOrQueueWebJobImmediate( ui, text, info );
         }
         else
         {
            StartCoroutine(
               DelayForSeconds( Settings.Delay, () =>
               {
                  TranslateOrQueueWebJobImmediate( ui, text, info );
               } ) );
         }

         return null;
      }

      public static bool IsCurrentlySetting( TranslationInfo info )
      {
         if( info == null ) return false;

         return info.IsCurrentlySettingText;
      }

      /// <summary>
      /// Translates the string of a UI  text or queues it up to be translated
      /// by the HTTP translation service.
      /// </summary>
      private string TranslateOrQueueWebJobImmediate( object ui, string text, TranslationInfo info )
      {
         // Get the trimmed text
         text = ( text ?? ui.GetText() ).Trim();

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !string.IsNullOrEmpty( text ) && IsTranslatable( text ) && ShouldTranslate( ui ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( text );

            var textKey = new TranslationKeys( text );

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TryGetTranslation( textKey, out translation ) )
            {
               QueueNewUntranslatedForClipboard( textKey );

               if( !string.IsNullOrEmpty( translation ) )
               {
                  SetTranslatedText( ui, translation, info );
                  return translation;
               }
            }
            else
            {
               if( SupportsStabilization( ui ) )
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
                           delay: 0.5f,
                           maxTries: 100, // 100 tries == 50 seconds
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
                                 var stabilizedTextKey = new TranslationKeys( stabilizedText );

                                 QueueNewUntranslatedForClipboard( stabilizedTextKey );

                                 info?.Reset( stabilizedText );

                                 // once the text has stabilized, attempt to look it up
                                 if( TryGetTranslation( stabilizedTextKey, out translation ) )
                                 {
                                    if( !string.IsNullOrEmpty( translation ) )
                                    {
                                       SetTranslatedText( ui, translation, info );
                                    }
                                 }
                                 else
                                 {
                                    // Lets try not to spam a service that might not be there...
                                    if( AutoTranslateClient.IsConfigured )
                                    {
                                       if( _consecutiveErrors < Settings.MaxErrors && !Settings.IsShutdown )
                                       {
                                          var job = GetOrCreateTranslationJobFor( stabilizedTextKey );
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
                  if( !_startedOperationsForNonStabilizableComponents.Contains( text ) && !text.ContainsNumbers() )
                  {
                     _startedOperationsForNonStabilizableComponents.Add( text );

                     QueueNewUntranslatedForClipboard( textKey );

                     // Lets try not to spam a service that might not be there...
                     if( AutoTranslateClient.IsConfigured )
                     {
                        if( _consecutiveErrors < Settings.MaxErrors && !Settings.IsShutdown )
                        {
                           GetOrCreateTranslationJobFor( textKey );
                        }
                     }
                     else
                     {
                        QueueNewUntranslatedForDisk( textKey );
                     }
                  }
               }
            }
         }

         return null;
      }

      public bool SupportsStabilization( object ui )
      {
         return !( ui is GUIContent );
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
               onTextStabilized( afterText.Trim() );
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
         if( Settings.IsShutdown ) return;

         try
         {
            CopyToClipboard();
            ResetThresholdTimerIfRequired();

            KickoffTranslations();
            FinishTranslations();

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
            Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An error occurred in Update callback. " + Environment.NewLine + e );
         }
      }

      // create this as a field instead of local var, to prevent new creation on EVERY game loop
      private readonly List<string> _kickedOff = new List<string>();

      private void KickoffTranslations()
      {
         foreach( var kvp in _unstartedJobs )
         {
            if( !AutoTranslateClient.HasAvailableClients ) break;

            var key = kvp.Key;
            var job = kvp.Value;
            _kickedOff.Add( key );

            // lets see if the text should still be translated before kicking anything off
            if( !job.AnyComponentsStillHasOriginalUntranslatedText() ) continue;

            StartCoroutine( AutoTranslateClient.TranslateByWWW( job.Keys.RelevantKey, Settings.FromLanguage, Settings.Language, translatedText =>
            {
               _consecutiveErrors = 0;

               if( Settings.ForceSplitTextAfterCharacters > 0 )
               {
                  translatedText = translatedText.SplitToLines( Settings.ForceSplitTextAfterCharacters, '\n', ' ', '　' );
               }


               job.TranslatedText = translatedText;

               if( !string.IsNullOrEmpty( translatedText ) )
               {
                  QueueNewTranslationForDisk( job.Keys, translatedText );

                  _completedJobs.Add( job );
               }
            },
            () =>
            {
               _consecutiveErrors++;

               if( !Settings.IsShutdown )
               {
                  if( _consecutiveErrors > Settings.MaxErrors )
                  {
                     if( AutoTranslateClient.Fallback() )
                     {
                        Console.WriteLine( "[XUnity.AutoTranslator][WARN]: More than 5 consecutive errors occurred. Entering fallback mode." );
                        _consecutiveErrors = 0;
                     }
                     else
                     {
                        Settings.IsShutdown = true;
                        Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: More than 5 consecutive errors occurred. Shutting down plugin." );
                     }
                  }
               }
            } ) );
         }

         for( int i = 0 ; i < _kickedOff.Count ; i++ )
         {
            _unstartedJobs.Remove( _kickedOff[ i ] );
         }

         _kickedOff.Clear();
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
                  var text = component.GetText().Trim();
                  if( text == job.Keys.OriginalKey )
                  {
                     var info = component.GetTranslationInfo( false );
                     SetTranslatedText( component, job.TranslatedText, info );
                  }
               }

               AddTranslation( job.Keys, job.TranslatedText );
            }
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
               var key = new TranslationKeys( info.OriginalText );
               if( TryGetTranslation( key, out string translatedText ) && !string.IsNullOrEmpty( translatedText ) )
               {
                  SetTranslatedText( kvp.Key, translatedText, info );
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

         if( _isInTranslatedMode )
         {
            // make sure we use the translated version of all texts
            foreach( var kvp in ObjectExtensions.GetAllRegisteredObjects() )
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
            foreach( var kvp in ObjectExtensions.GetAllRegisteredObjects() )
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
               Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: An error while copying text to clipboard. " + Environment.NewLine + e );
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
