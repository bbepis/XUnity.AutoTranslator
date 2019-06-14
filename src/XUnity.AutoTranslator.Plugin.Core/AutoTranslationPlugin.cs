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
using System.Globalization;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using System.Diagnostics;
using XUnity.AutoTranslator.Plugin.Core.UI;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web.Internal;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Main plugin class for the AutoTranslator.
   /// </summary>
   public class AutoTranslationPlugin : MonoBehaviour, ITranslator
   {
      /// <summary>
      /// Allow the instance to be accessed statically, as only one will exist.
      /// </summary>
      internal static AutoTranslationPlugin Current;

      internal XuaWindow MainWindow;
      internal TranslationAggregatorWindow TranslationAggregatorWindow;
      internal TranslationAggregatorOptionsWindow TranslationAggregatorOptionsWindow;
      internal TranslationManager TranslationManager;
      internal TextTranslationCache TextCache;
      internal TextureTranslationCache TextureCache;
      internal SpamChecker SpamChecker;

      /// <summary>
      /// Keeps track of things to copy to clipboard.
      /// </summary>
      private List<string> _textsToCopyToClipboardOrdered = new List<string>();
      private HashSet<string> _textsToCopyToClipboard = new HashSet<string>();
      private float _clipboardUpdated = 0.0f;

      /// <summary>
      /// This is a hash set that contains all Text components that is currently being worked on by
      /// the translation plugin.
      /// </summary>
      private HashSet<object> _ongoingOperations = new HashSet<object>();

      /// <summary>
      /// Texts currently being scheduled for translation by 'immediate' components.
      /// </summary>
      private HashSet<string> _immediatelyTranslating = new HashSet<string>();

      private bool _isInTranslatedMode = true;
      private bool _textHooksEnabled = true;
      private bool _imageHooksEnabled = true;

      private float _batchOperationSecondCounter = 0;

      private bool _hasValidOverrideFont = false;
      private bool _hasOverridenFont = false;
      private bool _initialized = false;
      private bool _temporarilyDisabled = false;
      private string _requireSpriteRendererCheckCausedBy = null;
      private int _lastSpriteUpdateFrame = -1;
      private bool _isCalledFromSceneManager = false;

      /// <summary>
      /// Initialized the plugin.
      /// </summary>
      public void Initialize()
      {
         // Setup 'singleton'
         Current = this;

         // because we only use harmony through reflection due to
         // version compatibility issues, we call this method to
         // ensure that harmony is loaded before we attempt to obtain
         // various harmony classes through reflection
         HarmonyLoader.Load();

         // Setup logger, if it was not already initialized by a plugin-version
         if( XuaLogger.Current == null )
         {
            XuaLogger.Current = new ConsoleLogger();
         }

         // Setup configuration
         Settings.Configure();

         // Setup console, if enabled
         DebugConsole.Enable();

         // Setup hooks
         HooksSetup.InstallTextHooks();
         HooksSetup.InstallImageHooks();
         HooksSetup.InstallTextGetterCompatHooks();

         TextCache = new TextTranslationCache();
         TextureCache = new TextureTranslationCache();
         TranslationManager = new TranslationManager();
         TranslationManager.JobCompleted += OnJobCompleted;
         TranslationManager.JobFailed += OnJobFailed;
         TranslationManager.InitializeEndpoints( gameObject );
         SpamChecker = new SpamChecker( TranslationManager );

         // WORKAROUND: Initialize text parsers with delegate indicating if text should be translated
         UnityTextParsers.Initialize( text => TextCache.IsTranslatable( text ) && IsBelowMaxLength( text ) );

         // validate configuration
         ValidateConfiguration();

         // enable scene scan
         EnableSceneLoadScan();

         // load all translations from files
         LoadTranslations();

         // start maintenance thread
         StartMaintenance();

         // initialize ui
         InitializeGUI();

         XuaLogger.Current.Info( $"Loaded XUnity.AutoTranslator into Unity [{Application.unityVersion}] game." );
      }

      private void InitializeGUI()
      {
         try
         {
            DisableAutoTranslator();

            MainWindow = new XuaWindow( CreateXuaViewModel() );
            
            var vm = CreateTranslationAggregatorViewModel();
            TranslationAggregatorWindow = new TranslationAggregatorWindow( vm );
            TranslationAggregatorOptionsWindow = new TranslationAggregatorOptionsWindow( vm );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up UI." );
         }
         finally
         {
            EnableAutoTranslator();
         }
      }

      private TranslationAggregatorViewModel CreateTranslationAggregatorViewModel()
      {
         return new TranslationAggregatorViewModel( TranslationManager );
      }

      private XuaViewModel CreateXuaViewModel()
      {
         return new XuaViewModel(
            new List<ToggleViewModel>
            {
               new ToggleViewModel(
                  " Translated",
                  "<b>TRANSLATED</b>\nThe plugin currently displays translated texts. Disabling this does not mean the plugin will no longer perform translations, just that they will not be displayed.",
                  "<b>NOT TRANSLATED</b>\nThe plugin currently displays untranslated texts.",
                  ToggleTranslation, () => _isInTranslatedMode )
            },
            new DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager>(
               TranslationManager.AllEndpoints.Select( x => new TranslatorDropdownOptionViewModel( () => x == TranslationManager.CurrentEndpoint, x ) ).ToList(),
               OnEndpointSelected
            ),
            new List<ButtonViewModel>
            {
               new ButtonViewModel( "Reboot", "<b>REBOOT PLUGIN</b>\nReboots the plugin if it has been shutdown. This only works if the plugin was shut down due to consequtive errors towards the translation endpoint.", RebootPlugin, null ),
               new ButtonViewModel( "Reload", "<b>RELOAD TRANSLATION</b>\nReloads all translation text files and texture files from disk.", ReloadTranslations, null ),
               new ButtonViewModel( "Hook", "<b>MANUAL HOOK</b>\nTraverses the unity object tree for looking for anything that can be translated. Performs a translation if something is found.", ManualHook, null )
            },
            new List<LabelViewModel>
            {
               new LabelViewModel( "Version: ", () => PluginData.Version ),
               new LabelViewModel( "Plugin status: ", () => Settings.IsShutdown ? "Shutdown" : "Running" ),
               new LabelViewModel( "Translator status: ", GetCurrentEndpointStatus ),
               new LabelViewModel( "Running translations: ", () => $"{(TranslationManager.OngoingTranslations)}" ),
               new LabelViewModel( "Served translations: ", () => $"{Settings.TranslationCount} / {Settings.MaxTranslationsBeforeShutdown}" ),
               new LabelViewModel( "Queued translations: ", () => $"{(TranslationManager.UnstartedTranslations)} / {Settings.MaxUnstartedJobs}" ),
               new LabelViewModel( "Error'ed translations: ", () => $"{TranslationManager.CurrentEndpoint?.ConsecutiveErrors ?? 0} / {Settings.MaxErrors}"  ),
            } );
      }

      private string GetCurrentEndpointStatus()
      {
         var endpoint = TranslationManager.CurrentEndpoint;
         if( endpoint == null )
         {
            return "Not selected";
         }
         else if( endpoint.HasFailedDueToConsecutiveErrors )
         {
            return "Shutdown";
         }
         return "Running";
      }

      private void StartMaintenance()
      {
         // start a thread that will periodically removed unused references
         var t1 = new Thread( MaintenanceLoop );
         t1.IsBackground = true;
         t1.Start();
      }

      private void ValidateConfiguration()
      {
         // check if language is supported
         if( !LanguageHelper.IsFromLanguageSupported( Settings.FromLanguage ) )
         {
            XuaLogger.Current.Error( $"The plugin has been configured to use the 'FromLanguage={Settings.FromLanguage}'. This language is not supported. Shutting plugin down." );

            Settings.IsShutdown = true;
         }

         // check if font is supported
         try
         {
            if( !string.IsNullOrEmpty( Settings.OverrideFont ) )
            {
               var available = GetSupportedFonts();
               if( !available.Contains( Settings.OverrideFont ) )
               {
                  XuaLogger.Current.Error( $"The specified override font is not available. Available fonts: " + string.Join( ", ", available ) );
                  Settings.OverrideFont = null;
               }
               else
               {
                  _hasValidOverrideFont = true;
               }

               _hasOverridenFont = _hasValidOverrideFont;
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while checking supported fonts." );
         }
      }

      internal static string[] GetSupportedFonts()
      {
         return Font.GetOSInstalledFontNames();
      }

      private void OnEndpointSelected( TranslationEndpointManager endpoint )
      {
         if( TranslationManager.CurrentEndpoint != endpoint )
         {
            TranslationManager.CurrentEndpoint = endpoint;

            if( TranslationManager.CurrentEndpoint != null )
            {
               if( !Settings.IsShutdown )
               {
                  if( TranslationManager.CurrentEndpoint.HasFailedDueToConsecutiveErrors )
                  {
                     RebootPlugin();
                  }
                  ManualHook();
               }
            }

            Settings.SetEndpoint( TranslationManager.CurrentEndpoint?.Endpoint.Id );

            XuaLogger.Current.Info( $"Set translator endpoint to '{TranslationManager.CurrentEndpoint?.Endpoint.Id}'." );
         }
      }

      private void MaintenanceLoop( object state )
      {
         int i = 0;

         while( true )
         {
            // run every 1 minutes
            if( i++ % 12 == 0 )
            {
               try
               {
                  ObjectReferenceMapper.Cull();
               }
               catch( Exception e )
               {
                  XuaLogger.Current.Error( e, "An unexpected error occurred while removing GC'ed resources." );
               }
            }

            // run every 5 seconds
            try
            {
               TextCache.SaveNewTranslationsToDisk();
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An error occurred while saving translations to disk." );
            }

            Thread.Sleep( 1000 * 5 );
         }
      }

      private void EnableSceneLoadScan()
      {
         try
         {
            XuaLogger.Current.Info( "Probing whether OnLevelWasLoaded or SceneManager is supported in this version of Unity. Any warnings related to OnLevelWasLoaded coming from Unity can safely be ignored." );
            if( Features.SupportsSceneManager )
            {
               EnableSceneLoadScanInternal();
               XuaLogger.Current.Info( "SceneManager is supported in this version of Unity." );
            }
            else
            {
               XuaLogger.Current.Info( "SceneManager is not supported in this version of Unity. Falling back to OnLevelWasLoaded and Application level API." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while settings up scene-load scans." );
         }
      }

      private void EnableSceneLoadScanInternal()
      {
         // do this in a different class to avoid having an anonymous method with references to the "Scene" class
         SceneManagerLoader.EnableSceneLoadScanInternal( this );
      }

      internal void OnLevelWasLoadedFromSceneManager( int id )
      {
         try
         {
            _isCalledFromSceneManager = true;
            OnLevelWasLoaded( id );
         }
         finally
         {
            _isCalledFromSceneManager = false;
         }
      }

      private void OnLevelWasLoaded( int id )
      {
         if( !Features.SupportsSceneManager || ( Features.SupportsSceneManager && _isCalledFromSceneManager ) )
         {
            if( Settings.EnableTextureScanOnSceneLoad && ( Settings.EnableTextureDumping || Settings.EnableTextureTranslation ) )
            {
               XuaLogger.Current.Info( "Performing texture lookup during scene load..." );
               var startTime = Time.realtimeSinceStartup;

               ManualHookForTextures();

               var endTime = Time.realtimeSinceStartup;
               XuaLogger.Current.Info( $"Finished texture lookup (took {Math.Round( endTime - startTime, 2 )} seconds)" );
            }
         }
      }

      /// <summary>
      /// Loads the translations found in Translation.{lang}.txt
      /// </summary>
      private void LoadTranslations()
      {
         TextCache.LoadTranslationFiles();
         TextureCache.LoadTranslationFiles();
      }

      private void CreateTranslationJobFor( TranslationEndpointManager endpoint, object ui, UntranslatedText key, TranslationResult translationResult, ParserTranslationContext context )
      {
         var added = endpoint.EnqueueTranslation( ui, key, translationResult, context );
         if( added && translationResult == null )
         {
            SpamChecker.PerformChecks( key.TrimmedTranslatableText );
         }
      }

      private void IncrementBatchOperations()
      {
         _batchOperationSecondCounter += Time.deltaTime;

         if( _batchOperationSecondCounter > Settings.IncreaseBatchOperationsEvery )
         {
            var endpoints = TranslationManager.ConfiguredEndpoints;
            foreach( var endpoint in endpoints )
            {
               if( endpoint.AvailableBatchOperations < Settings.MaxAvailableBatchOperations )
               {
                  endpoint.AvailableBatchOperations++;
               }
            }

            _batchOperationSecondCounter = 0;
         }
      }

      private void UpdateSpriteRenderers()
      {
         if( Settings.EnableSpriteRendererHooking && ( Settings.EnableTextureTranslation || Settings.EnableTextureDumping ) )
         {
            if( _requireSpriteRendererCheckCausedBy != null )
            {
               try
               {
                  var start = Time.realtimeSinceStartup;

                  var spriteRenderers = GameObject.FindObjectsOfType<SpriteRenderer>();
                  foreach( var sr in spriteRenderers )
                  {
                     // simulate a hook
                     Hook_ImageChangedOnComponent( sr, null, false, false );
                  }

                  var end = Time.realtimeSinceStartup;
                  var delta = Math.Round( end - start, 2 );

                  XuaLogger.Current.Debug( $"Update SpriteRenderers caused by {_requireSpriteRendererCheckCausedBy} component (took " + delta + " seconds)" );
               }
               finally
               {
                  _requireSpriteRendererCheckCausedBy = null;
               }
            }
         }
      }

      private void QueueNewUntranslatedForClipboard( UntranslatedText key )
      {
         if( Settings.CopyToClipboard && Features.SupportsClipboard )
         {
            if( !_textsToCopyToClipboard.Contains( key.TrimmedTranslatableText ) )
            {
               _textsToCopyToClipboard.Add( key.TrimmedTranslatableText );
               _textsToCopyToClipboardOrdered.Add( key.TrimmedTranslatableText );

               _clipboardUpdated = Time.realtimeSinceStartup;
            }
         }
      }

      internal string ExternalHook_TextChanged_WithResult( object ui, string text )
      {
         if( !ui.IsKnownTextType() ) return null;

         try
         {
            CallOrigin.ExpectsTextToBeReturned = true;

            if( _textHooksEnabled && !_temporarilyDisabled )
            {
               return TranslateOrQueueWebJob( ui, text, true );
            }
            return null;
         }
         finally
         {
            CallOrigin.ExpectsTextToBeReturned = false;
         }
      }

      internal string Hook_TextChanged_WithResult( object ui, string text, bool onEnable )
      {
         try
         {
            CallOrigin.ExpectsTextToBeReturned = true;

            string result = null;
            if( _textHooksEnabled && !_temporarilyDisabled )
            {
               result = TranslateOrQueueWebJob( ui, text, false );
            }

            if( onEnable )
            {
               CheckSpriteRenderer( ui );
            }
            return result;
         }
         finally
         {
            CallOrigin.ExpectsTextToBeReturned = false;
         }
      }

      internal void Hook_TextChanged( object ui, bool onEnable )
      {
         if( _textHooksEnabled && !_temporarilyDisabled )
         {
            TranslateOrQueueWebJob( ui, null, false );
         }

         if( onEnable )
         {
            CheckSpriteRenderer( ui );
         }
      }

      internal void Hook_ImageChangedOnComponent( object source, Texture2D texture, bool isPrefixHooked, bool onEnable )
      {
         if( !_imageHooksEnabled ) return;
         if( !source.IsKnownImageType() ) return;

         HandleImage( source, texture, isPrefixHooked );

         if( onEnable )
         {
            CheckSpriteRenderer( source );
         }
      }

      internal void Hook_ImageChanged( Texture2D texture, bool isPrefixHooked )
      {
         if( !_imageHooksEnabled ) return;
         if( texture == null ) return;

         HandleImage( null, texture, isPrefixHooked );
      }

      internal void Hook_HandleComponent( object ui )
      {
         if( _hasValidOverrideFont )
         {
            var info = ui.GetOrCreateTextTranslationInfo();
            if( _hasOverridenFont )
            {
               info?.ChangeFont( ui );
            }
            else
            {
               info?.UnchangeFont( ui );
            }
         }

         if( Settings.ForceUIResizing )
         {
            var info = ui.GetOrCreateTextTranslationInfo();
            if( info?.IsCurrentlySettingText == false )
            {
               // force UI resizing is highly problematic for NGUI because text should somehow
               // be set after changing "resize" properties... brilliant stuff
               if( ui.GetType() != ClrTypes.UILabel )
               {
                  info?.ResizeUI( ui );
               }
            }
         }
      }

      private void CheckSpriteRenderer( object ui )
      {
         if( Settings.EnableSpriteRendererHooking )
         {
            var currentFrame = Time.frameCount;
            var lastFrame = currentFrame - 1;

            if( lastFrame != _lastSpriteUpdateFrame && currentFrame != _lastSpriteUpdateFrame )
            {
               _requireSpriteRendererCheckCausedBy = ui?.GetType().Name;
            }
            _lastSpriteUpdateFrame = currentFrame;
         }
      }

      internal void SetTranslatedText( object ui, string translatedText, string originalText, TextTranslationInfo info )
      {
         info?.SetTranslatedText( translatedText );

         if( _isInTranslatedMode && !CallOrigin.ExpectsTextToBeReturned )
         {
            SetText( ui, translatedText, true, originalText, info );
         }
      }

      /// <summary>
      /// Sets the text of a UI  text, while ensuring this will not fire a text changed event.
      /// </summary>
      private void SetText( object ui, string text, bool isTranslated, string originalText, TextTranslationInfo info )
      {
         if( !info?.IsCurrentlySettingText ?? true )
         {
            try
            {
               _textHooksEnabled = false;

               if( info != null )
               {
                  info.IsCurrentlySettingText = true;
               }

               if( Settings.EnableUIResizing || Settings.ForceUIResizing )
               {
                  if( isTranslated || Settings.ForceUIResizing )
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

               if( originalText != null && TranslationAggregatorWindow != null && info != null && !ui.IsSpammingComponent() )
               {
                  TranslationAggregatorWindow.OnNewTranslationAdded( originalText, text );
               }
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
               XuaLogger.Current.Error( e, "An error occurred while setting text on a component." );
            }
            finally
            {
               _textHooksEnabled = true;

               if( info != null )
               {
                  info.IsCurrentlySettingText = false;
               }
            }
         }
      }

      private bool IsBelowMaxLength( string str )
      {
         return str.Length <= Settings.MaxCharactersPerTranslation;
      }

      private bool IsBelowMaxLengthStrict( string str )
      {
         return str.Length <= ( Settings.MaxCharactersPerTranslation / 2 );
      }

      private bool ShouldTranslateTextComponent( object ui, bool ignoreComponentState )
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

            if( !ignoreComponentState )
            {
               var behaviour = component as Behaviour;
               if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
               {
                  return false;
               }
            }

            var inputField = go.GetFirstComponentInSelfOrAncestor( ClrTypes.InputField )
               ?? go.GetFirstComponentInSelfOrAncestor( ClrTypes.TMP_InputField )
               ?? go.GetFirstComponentInSelfOrAncestor( ClrTypes.UIInput );

            return inputField == null;
         }

         return true;
      }

      private string TranslateOrQueueWebJob( object ui, string text, bool ignoreComponentState )
      {
         var info = ui.GetOrCreateTextTranslationInfo();

         if( _ongoingOperations.Contains( ui ) )
         {
            return TranslateImmediate( ui, text, info, ignoreComponentState );
         }

         var supportsStabilization = ui.SupportsStabilization();
         if( Settings.Delay == 0 || !supportsStabilization )
         {
            return TranslateOrQueueWebJobImmediate( ui, text, info, supportsStabilization, ignoreComponentState );
         }
         else
         {
            StartCoroutine(
               DelayForSeconds( Settings.Delay, () =>
               {
                  TranslateOrQueueWebJobImmediate( ui, text, info, supportsStabilization, ignoreComponentState );
               } ) );
         }

         return null;
      }

      private static bool IsCurrentlySetting( TextTranslationInfo info )
      {
         if( info == null ) return false;

         return info.IsCurrentlySettingText;
      }

      private void HandleImage( object source, Texture2D texture, bool isPrefixHooked )
      {
         if( Settings.EnableTextureDumping )
         {
            try
            {
               DumpTexture( source, texture );
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An error occurred while dumping texture." );
            }
         }

         if( Settings.EnableTextureTranslation )
         {
            try
            {
               TranslateTexture( source, texture, isPrefixHooked, null );
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An error occurred while translating texture." );
            }
         }
      }

      private void TranslateTexture( object ui, TextureReloadContext context )
      {
         if( ui is Texture2D texture2d )
         {
            TranslateTexture( null, texture2d, false, context );
         }
         else
         {
            TranslateTexture( ui, null, false, context );
         }
      }

      private void TranslateTexture( object source, Texture2D texture, bool isPrefixHooked, TextureReloadContext context )
      {
         try
         {
            _imageHooksEnabled = false;

            texture = texture ?? source.GetTexture();
            if( texture == null ) return;

            var tti = texture.GetOrCreateTextureTranslationInfo();
            var iti = source.GetOrCreateImageTranslationInfo();
            var key = tti.GetKey( texture );
            if( string.IsNullOrEmpty( key ) ) return;

            bool hasContext = context != null;
            bool forceReload = false;
            if( hasContext )
            {
               forceReload = context.RegisterTextureInContextAndDetermineWhetherToReload( texture );
            }

            if( TextureCache.TryGetTranslatedImage( key, out var newData ) )
            {
               if( _isInTranslatedMode )
               {
                  // handle texture
                  if( !tti.IsTranslated || forceReload )
                  {
                     try
                     {
                        texture.LoadImageEx( newData, tti.IsNonReadable( texture ) );
                     }
                     finally
                     {
                        tti.IsTranslated = true;
                     }
                  }

                  // handle containing component
                  if( iti != null )
                  {
                     if( !iti.IsTranslated || hasContext )
                     {
                        try
                        {
                           if( !isPrefixHooked )
                           {
                              source.SetAllDirtyEx();
                           }
                        }
                        finally
                        {
                           iti.IsTranslated = true;
                        }
                     }
                  }
               }
            }
            else
            {
               // if we cannot find the texture, and the texture is considered translated... hmmm someone has removed a file

               // handle texture
               var originalData = tti.GetOriginalData( texture );
               if( originalData != null )
               {
                  if( tti.IsTranslated )
                  {
                     try
                     {
                        texture.LoadImageEx( originalData, tti.IsNonReadable( texture ) );
                     }
                     finally
                     {
                        tti.IsTranslated = true;
                     }
                  }

                  // handle containing component
                  if( iti != null )
                  {
                     if( iti.IsTranslated )
                     {
                        try
                        {
                           if( !isPrefixHooked )
                           {
                              source.SetAllDirtyEx();
                           }
                        }
                        finally
                        {
                           iti.IsTranslated = true;
                        }
                     }
                  }
               }
            }

            if( !_isInTranslatedMode )
            {
               var originalData = tti.GetOriginalData( texture );
               if( originalData != null )
               {
                  // handle texture
                  if( tti.IsTranslated )
                  {
                     try
                     {
                        texture.LoadImageEx( originalData, tti.IsNonReadable( texture ) );
                     }
                     finally
                     {
                        tti.IsTranslated = false;
                     }
                  }

                  // handle containing component
                  if( iti != null )
                  {
                     if( iti.IsTranslated )
                     {
                        try
                        {
                           if( !isPrefixHooked )
                           {
                              source.SetAllDirtyEx();
                           }
                        }
                        finally
                        {
                           iti.IsTranslated = false;
                        }
                     }
                  }
               }
            }

            if( forceReload )
            {
               XuaLogger.Current.Info( $"Reloaded texture: {texture.name} ({key})." );
            }
         }
         finally
         {
            _imageHooksEnabled = true;
         }
      }

      private void DumpTexture( object source, Texture2D texture )
      {
         try
         {
            _imageHooksEnabled = false;

            texture = texture ?? source.GetTexture();
            if( texture == null ) return;

            var info = texture.GetOrCreateTextureTranslationInfo();
            if( info.HasDumpedAlternativeTexture ) return;

            try
            {
               if( ShouldTranslate( texture ) )
               {
                  var key = info.GetKey( texture );
                  if( string.IsNullOrEmpty( key ) ) return;

                  if( !TextureCache.IsImageRegistered( key ) )
                  {
                     var name = texture.GetTextureName();

                     var originalData = info.GetOrCreateOriginalData( texture );
                     TextureCache.RegisterImageFromData( name, key, originalData );
                  }
               }
            }
            finally
            {
               info.HasDumpedAlternativeTexture = true;
            }
         }
         finally
         {
            _imageHooksEnabled = true;
         }
      }

      private bool ShouldTranslate( Texture2D texture )
      {
         // convert to int so engine versions that does not have specific enums still work
         var format = (int)texture.format;

         // 1 = Alpha8
         // 9 = R16
         // 63 = R8
         return format != 1
            && format != 9
            && format != 63;
      }

      internal void RenameTextureWithKey( string name, string key, string newKey )
      {
         TextureCache.RenameFileWithKey( name, key, newKey );
      }

      private string TranslateImmediate( object ui, string text, TextTranslationInfo info, bool ignoreComponentState )
      {
         // Get the trimmed text
         string originalText = text;

         text = text ?? ui.GetText();

         if( !string.IsNullOrEmpty( text ) && TextCache.IsTranslatable( text ) && ShouldTranslateTextComponent( ui, ignoreComponentState ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( originalText );

            //var textKey = new TranslationKey( ui, text, !ui.SupportsStabilization(), false );
            var isSpammer = ui.IsSpammingComponent();
            var textKey = GetCacheKey( ui, text, isSpammer, false );

            // potentially shortcircuit if fully templated
            if( text != textKey.TemplatedOriginalText && !TextCache.IsTranslatable( textKey.TemplatedOriginalText ) )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginalText );
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TextCache.TryGetTranslation( textKey, false, out translation ) )
            {
               if( !string.IsNullOrEmpty( translation ) )
               {
                  var untemplatedTranslation = textKey.Untemplate( translation );
                  SetTranslatedText( ui, untemplatedTranslation, originalText, info );
                  return untemplatedTranslation;
               }
            }
            else
            {
               if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
               {
                  var result = UnityTextParsers.GameLogTextParser.Parse( text );
                  if( result.Succeeded )
                  {
                     translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, false );
                     if( translation != null )
                     {
                        SetTranslatedText( ui, translation, originalText, info );
                        return translation;
                     }
                  }
               }
            }
         }

         return null;
      }

      // "public" endpoint to perform translations
      TranslationResult ITranslator.Translate( TranslationEndpointManager endpoint, string untranslatedText )
      {
         return Translate( untranslatedText, endpoint, null );
      }

      private TranslationResult Translate( string text, TranslationEndpointManager endpoint, ParserTranslationContext context )
      {
         var result = new TranslationResult();

         // Ensure that we actually want to translate this text and its owning UI element.
         if( !string.IsNullOrEmpty( text ) && endpoint.IsTranslatable( text ) && IsBelowMaxLength( text ) )
         {
            var textKey = GetCacheKey( null, text, false, context != null );

            // potentially shortcircuit if fully templated
            if( text != textKey.TemplatedOriginalText && !endpoint.IsTranslatable( textKey.TemplatedOriginalText ) )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginalText );
               result.SetCompleted( untemplatedTranslation, true );
               return result;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( endpoint.TryGetTranslation( textKey, out translation ) )
            {
               if( !string.IsNullOrEmpty( translation ) )
               {
                  result.SetCompleted( textKey.Untemplate( translation ), true );
               }
               else
               {
                  result.SetEmptyResponse( true );
               }
               return result;
            }
            else
            {
               if( context == null )
               {
                  var parserResult = UnityTextParsers.RichTextParser.Parse( text );
                  if( parserResult.Succeeded )
                  {
                     translation = TranslateByParserResult( endpoint, parserResult, result, true );
                     if( translation != null )
                     {
                        result.SetCompleted( translation, true );
                     }

                     // return because result will be completed later by recursive call

                     return result;
                  }
               }

               if( Settings.IsShutdown )
               {
                  result.SetErrorWithMessage( "The plugin is shutdown.", true );
               }
               else if( endpoint.HasFailedDueToConsecutiveErrors )
               {
                  result.SetErrorWithMessage( "The translation endpoint is shutdown.", true );
               }
               else
               {
                  CreateTranslationJobFor( endpoint, null, textKey, result, context );
               }

               return result;
            }
         }
         else
         {
            result.SetErrorWithMessage( $"The provided text ({text}) cannot be translated.", true );

            return result;
         }
      }

      private string TranslateByParserResult( TranslationEndpointManager endpoint, ParserResult result, TranslationResult translationResult, bool allowStartJob )
      {
         Dictionary<string, string> translations = new Dictionary<string, string>();

         // attempt to lookup ALL strings immediately; return result if possible; queue operations
         foreach( var kvp in result.Arguments )
         {
            var variableName = kvp.Key;
            var untranslatedTextPart = kvp.Value;
            if( !string.IsNullOrEmpty( untranslatedTextPart ) && endpoint.IsTranslatable( untranslatedTextPart ) && IsBelowMaxLength( untranslatedTextPart ) )
            {
               var textKey = new UntranslatedText( untranslatedTextPart, false, false );
               if( endpoint.IsTranslatable( textKey.TemplatedOriginalText ) )
               {
                  string partTranslation;
                  if( endpoint.TryGetTranslation( textKey, out partTranslation ) )
                  {
                     translations.Add( variableName, textKey.Untemplate( partTranslation ) );
                  }
                  else if( allowStartJob )
                  {
                     // incomplete, must start job
                     var context = new ParserTranslationContext( null, endpoint, translationResult, result );
                     Translate( untranslatedTextPart, endpoint, context );
                  }
               }
               else
               {
                  // the template itself does not require a translation, which means the untranslated template equals the translated text
                  translations.Add( variableName, textKey.Untemplate( textKey.TemplatedOriginalText ) );
               }
            }
            else
            {
               // the value will do
               translations.Add( variableName, untranslatedTextPart );
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
      /// Translates the string of a UI  text or queues it up to be translated
      /// by the HTTP translation service.
      /// </summary>
      private string TranslateOrQueueWebJobImmediate( object ui, string text, TextTranslationInfo info, bool supportsStabilization, bool ignoreComponentState, ParserTranslationContext context = null )
      {
         text = text ?? ui.GetText();

         // make sure text exists
         var originalText = text;

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !string.IsNullOrEmpty( text ) && TextCache.IsTranslatable( text ) && ShouldTranslateTextComponent( ui, ignoreComponentState ) && !IsCurrentlySetting( info ) )
         {
            info?.Reset( originalText );
            var isSpammer = ui.IsSpammingComponent();
            if( isSpammer && !IsBelowMaxLength( text ) ) return null; // avoid templating long strings every frame for IMGUI, important!

            var textKey = GetCacheKey( ui, text, isSpammer, context != null );

            // potentially shortcircuit if fully templated
            if( text != textKey.TemplatedOriginalText && !TextCache.IsTranslatable( textKey.TemplatedOriginalText ) )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginalText );
               if( context == null )
               {
                  SetTranslatedText( ui, untemplatedTranslation, originalText, info );
               }
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TextCache.TryGetTranslation( textKey, !isSpammer, out translation ) )
            {
               if( context == null && !isSpammer )
               {
                  QueueNewUntranslatedForClipboard( textKey );
               }

               if( !string.IsNullOrEmpty( translation ) )
               {
                  var untemplatedTranslation = textKey.Untemplate( translation );
                  if( context == null ) // never set text if operation is contextualized (only a part translation)
                  {
                     SetTranslatedText( ui, untemplatedTranslation, originalText, info );
                  }
                  return untemplatedTranslation;
               }
            }
            else
            {
               if( context == null )
               {
                  if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
                  {
                     var result = UnityTextParsers.GameLogTextParser.Parse( text );
                     if( result.Succeeded )
                     {
                        translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, false );
                        if( translation != null )
                        {
                           SetTranslatedText( ui, translation, originalText, info );
                           return translation;
                        }
                     }
                  }
                  else if( UnityTextParsers.RichTextParser.CanApply( ui ) && IsBelowMaxLength( text ) )
                  {
                     var result = UnityTextParsers.RichTextParser.Parse( text );
                     if( result.Succeeded )
                     {
                        //var isWhitelisted = ui.IsWhitelistedForImmediateRichTextTranslation();

                        translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, false );
                        if( translation != null )
                        {
                           SetTranslatedText( ui, translation, originalText, info );
                           return translation;
                        }
                        //else if( isWhitelisted )
                        //{
                        //   return null;
                        //}
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
                           delay: 0.9f, // 0.9 second to prevent '1 second tickers' from getting translated
                           maxTries: 60, // 50 tries, about 1 minute
                           currentTries: 0,
                           onMaxTriesExceeded: () =>
                           {
                              _ongoingOperations.Remove( ui );
                           },
                           onTextStabilized: stabilizedText =>
                           {
                              _ongoingOperations.Remove( ui );

                              originalText = stabilizedText;

                              if( !string.IsNullOrEmpty( stabilizedText ) && TextCache.IsTranslatable( stabilizedText ) )
                              {
                                 var stabilizedTextKey = GetCacheKey( ui, stabilizedText, false, false );

                                 // potentially shortcircuit if fully templated
                                 if( stabilizedText != stabilizedTextKey.TemplatedOriginalText && !TextCache.IsTranslatable( stabilizedTextKey.TemplatedOriginalText ) )
                                 {
                                    var untemplatedTranslation = stabilizedTextKey.Untemplate( stabilizedTextKey.TemplatedOriginalText );
                                    SetTranslatedText( ui, untemplatedTranslation, originalText, info );
                                    return;
                                 }

                                 QueueNewUntranslatedForClipboard( stabilizedTextKey );

                                 info?.Reset( originalText );

                                 // once the text has stabilized, attempt to look it up
                                 if( TextCache.TryGetTranslation( stabilizedTextKey, true, out translation ) )
                                 {
                                    if( !string.IsNullOrEmpty( translation ) )
                                    {
                                       SetTranslatedText( ui, stabilizedTextKey.Untemplate( translation ), originalText, info );
                                    }
                                 }
                                 else
                                 {
                                    if( context == null )
                                    {
                                       if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
                                       {
                                          var result = UnityTextParsers.GameLogTextParser.Parse( stabilizedText );
                                          if( result.Succeeded )
                                          {
                                             var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, true );
                                             if( translatedText != null )
                                             {
                                                SetTranslatedText( ui, translatedText, originalText, info );
                                             }
                                             return;
                                          }
                                       }
                                       else if( UnityTextParsers.RichTextParser.CanApply( ui ) && IsBelowMaxLength( stabilizedText ) )
                                       {
                                          var result = UnityTextParsers.RichTextParser.Parse( stabilizedText );
                                          if( result.Succeeded )
                                          {
                                             var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, true );
                                             if( translatedText != null )
                                             {
                                                SetTranslatedText( ui, translatedText, originalText, info );
                                             }
                                             return;
                                          }
                                       }
                                    }

                                    // Lets try not to spam a service that might not be there...
                                    var endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
                                    if( endpoint != null )
                                    {
                                       if( IsBelowMaxLength( stabilizedText ) )
                                       {
                                          if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                                          {
                                             CreateTranslationJobFor( endpoint, ui, stabilizedTextKey, null, context );
                                          }
                                       }
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
               else if( !isSpammer || ( isSpammer && IsBelowMaxLengthStrict( text ) ) )
               {
                  if( context != null )
                  {
                     // if there is a context, this is a part-translation, which means it is not a candidate for scrolling-in text
                     var endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
                     if( endpoint != null )
                     {
                        if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                        {
                           // once the text has stabilized, attempt to look it up
                           CreateTranslationJobFor( endpoint, ui, textKey, null, context );
                        }
                     }
                  }
                  else
                  {
                     StartCoroutine(
                        WaitForTextStablization(
                           textKey: textKey,
                           delay: 0.9f,
                           onTextStabilized: () =>
                           {
                              // Lets try not to spam a service that might not be there...
                              var endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
                              if( endpoint != null )
                              {
                                 // once the text has stabilized, attempt to look it up
                                 if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                                 {
                                    if( !TextCache.TryGetTranslation( textKey, true, out translation ) )
                                    {
                                       CreateTranslationJobFor( endpoint, ui, textKey, null, context );
                                    }
                                 }
                              }
                           } ) );
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
            var variableName = kvp.Key;
            var untranslatedTextPart = kvp.Value;
            if( !string.IsNullOrEmpty( untranslatedTextPart ) && TextCache.IsTranslatable( untranslatedTextPart ) && IsBelowMaxLength( untranslatedTextPart ) )
            {
               var textKey = new UntranslatedText( untranslatedTextPart, false, false );
               if( TextCache.IsTranslatable( textKey.TemplatedOriginalText ) )
               {
                  string partTranslation;
                  if( TextCache.TryGetTranslation( textKey, false, out partTranslation ) )
                  {
                     translations.Add( variableName, textKey.Untemplate( partTranslation ) );
                  }
                  else if( allowStartJob )
                  {
                     // incomplete, must start job
                     var context = new ParserTranslationContext( ui, TranslationManager.CurrentEndpoint, null, result );
                     TranslateOrQueueWebJobImmediate( ui, untranslatedTextPart, null, false, true, context );
                  }
               }
               else
               {
                  // the template itself does not require a translation, which means the untranslated template equals the translated text
                  translations.Add( variableName, textKey.Untemplate( textKey.TemplatedOriginalText ) );
               }
            }
            else
            {
               // the value will do
               translations.Add( variableName, untranslatedTextPart );
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
      private IEnumerator WaitForTextStablization( object ui, float delay, int maxTries, int currentTries, Action<string> onTextStabilized, Action onMaxTriesExceeded )
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
               onTextStabilized( afterText );
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

      /// <summary>
      /// Utility method that allows me to wait to call an action, until
      /// the text has stopped changing. This is important for 'story'
      /// mode text, which 'scrolls' into place slowly. This version is
      /// for global text, where the component cannot tell us if the text
      /// has changed itself.
      /// </summary>
      private IEnumerator WaitForTextStablization( UntranslatedText textKey, float delay, Action onTextStabilized, Action onFailed = null )
      {
         var text = textKey.TrimmedTranslatableText;

         if( !_immediatelyTranslating.Contains( text ) )
         {
            _immediatelyTranslating.Add( text );
            try
            {
               yield return new WaitForSeconds( delay );

               bool succeeded = true;
               foreach( var otherImmediatelyTranslating in _immediatelyTranslating )
               {
                  if( text != otherImmediatelyTranslating )
                  {
                     if( text.RemindsOf( otherImmediatelyTranslating ) )
                     {
                        succeeded = false;
                        break;
                     }
                  }
               }

               if( succeeded )
               {
                  onTextStabilized();
               }
               else
               {
                  onFailed?.Invoke();
               }
            }
            finally
            {
               _immediatelyTranslating.Remove( text );
            }
         }
      }

      private IEnumerator DelayForSeconds( float delay, Action onContinue )
      {
         yield return new WaitForSeconds( delay );
         onContinue();
      }

      void Awake()
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
               XuaLogger.Current.Error( e, "An unexpected error occurred during plugin initialization." );
            }
         }
      }

      void Start()
      {
         try
         {
            // this is delayed to ensure other plugins has had a chance to startup
            HooksSetup.InstallOverrideTextHooks();
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An unexpected error occurred during plugin start." );
         }
      }

      void Update()
      {
         try
         {
            if( Features.SupportsClipboard )
            {
               CopyToClipboard();
            }

            if( !Settings.IsShutdown )
            {
               SpamChecker.Update();

               UpdateSpriteRenderers();
               IncrementBatchOperations();
               KickoffTranslations();

               TranslationAggregatorWindow?.Update();
            }

            // perform this check every 100 frames!
            if( Time.frameCount % 100 == 0
               && TranslationManager.OngoingTranslations == 0
               && TranslationManager.UnstartedTranslations == 0 )
            {
               ConnectionTrackingWebClient.CheckServicePoints();
            }

            if( Input.anyKey )
            {
               var isAltPressed = Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt );

               if( isAltPressed )
               {
                  var isCtrlPressed = Input.GetKey( KeyCode.LeftControl );

                  if( Settings.EnablePrintHierarchy && Input.GetKeyDown( KeyCode.Y ) )
                  {
                     PrintObjects();
                  }
                  else if( Input.GetKeyDown( KeyCode.T ) )
                  {
                     ToggleTranslation();
                  }
                  else if( Input.GetKeyDown( KeyCode.F ) )
                  {
                     ToggleFont();
                  }
                  else if( Input.GetKeyDown( KeyCode.R ) )
                  {
                     ReloadTranslations();
                  }
                  else if( Input.GetKeyDown( KeyCode.U ) )
                  {
                     ManualHook();
                  }
                  else if( Input.GetKeyDown( KeyCode.Q ) )
                  {
                     RebootPlugin();
                  }
                  //else if( Input.GetKeyDown( KeyCode.B ) )
                  //{
                  //   ConnectionTrackingWebClient.CloseServicePoints();
                  //}
                  else if( Input.GetKeyDown( KeyCode.Alpha0 ) || Input.GetKeyDown( KeyCode.Keypad0 ) )
                  {
                     if( MainWindow != null )
                     {
                        MainWindow.IsShown = !MainWindow.IsShown;
                     }
                  }
                  else if( Input.GetKeyDown( KeyCode.Alpha1 ) || Input.GetKeyDown( KeyCode.Keypad1 ) )
                  {
                     if( TranslationAggregatorWindow != null )
                     {
                        TranslationAggregatorWindow.IsShown = !TranslationAggregatorWindow.IsShown;
                     }
                  }
                  else if( isCtrlPressed )
                  {
                     if( Input.GetKeyDown( KeyCode.Keypad9 ) )
                     {
                        Settings.SimulateError = !Settings.SimulateError;
                     }
                     else if( Input.GetKeyDown( KeyCode.Keypad8 ) )
                     {
                        Settings.SimulateDelayedError = !Settings.SimulateDelayedError;
                     }
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred in Update callback. " );
         }
      }

      void OnGUI()
      {
         try
         {
            DisableAutoTranslator();

            if( MainWindow != null )
            {
               try
               {
                  if( MainWindow.IsShown ) MainWindow.OnGUI();
               }
               catch( Exception e )
               {
                  XuaLogger.Current.Error( e, "An error occurred in XUnity.AutoTranslator UI. Disabling the UI." );

                  MainWindow = null;
               }
            }

            if( TranslationAggregatorWindow != null )
            {
               try
               {
                  if( TranslationAggregatorWindow.IsShown ) TranslationAggregatorWindow.OnGUI();
               }
               catch( Exception e )
               {
                  XuaLogger.Current.Error( e, "An error occurred in Translation Aggregator UI. Disabling the UI." );

                  TranslationAggregatorWindow = null;
               }
            }

            if( TranslationAggregatorOptionsWindow != null )
            {
               try
               {
                  if( TranslationAggregatorOptionsWindow.IsShown ) TranslationAggregatorOptionsWindow.OnGUI();
               }
               catch( Exception e )
               {
                  XuaLogger.Current.Error( e, "An error occurred in Translation Aggregator Options UI. Disabling the UI." );

                  TranslationAggregatorOptionsWindow = null;
               }
            }
         }
         finally
         {
            EnableAutoTranslator();
         }
      }

      private void RebootPlugin()
      {
         var endpoints = TranslationManager.ConfiguredEndpoints;
         foreach( var endpoint in endpoints )
         {
            endpoint.ConsecutiveErrors = 0;
         }

         XuaLogger.Current.Info( "Rebooted Auto Translator." );
      }

      private void KickoffTranslations()
      {
         TranslationManager.KickoffTranslations();
      }

      private void OnJobFailed( TranslationJob job )
      {
         foreach( var translationResult in job.TranslationResults )
         {
            translationResult.Item.SetErrorWithMessage( job.ErrorMessage ?? "Unknown error.", false );
         }

         foreach( var context in job.Contexts )
         {
            var translationResult = context.TranslationResult;
            if( translationResult != null )
            {
               // are all jobs within this context completed? If so, we can set the text
               if( context.Jobs.Any( x => x.State == TranslationJobState.Failed ) )
               {
                  translationResult.SetErrorWithMessage( job.ErrorMessage ?? "Unknown error.", false );
               }
            }
         }
      }

      private void OnJobCompleted( TranslationJob job )
      {
         if( job.Key.IsTemplated && Settings.GenerateStaticSubstitutionTranslations && !job.Key.IsFromSpammingComponent )
         {
            foreach( var keyAndComponent in job.Components )
            {
               var key = keyAndComponent.Key;

               var untemplatedTranslatableText = key.Untemplate( key.TranslatableText );
               var untemplatedTranslatedText = key.Untemplate( job.TranslatedText );
               if( job.SaveResultGlobally )
               {
                  TextCache.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText );
               }
               job.Endpoint.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText );
            }
         }
         else
         {
            if( job.SaveResultGlobally )
            {
               TextCache.AddTranslationToCache( job.Key.TranslatableText, job.TranslatedText );
            }
            job.Endpoint.AddTranslationToCache( job.Key.TranslatableText, job.TranslatedText );
         }

         // fix translation results directly on jobs
         foreach( var translationResult in job.TranslationResults )
         {
            if( !string.IsNullOrEmpty( job.TranslatedText ) )
            {
               translationResult.Item.SetCompleted( translationResult.Key.Untemplate( job.TranslatedText ), false );
            }
            else
            {
               translationResult.Item.SetEmptyResponse( false );
            }
         }

         foreach( var keyAndComponent in job.Components )
         {
            var component = keyAndComponent.Item;
            var key = keyAndComponent.Key;

            // update the original text, but only if it has not been chaanged already for some reason (could be other translator plugin or game itself)
            try
            {
               var text = component.GetText();
               if( text == key.OriginalText )
               {
                  var info = component.GetOrCreateTextTranslationInfo();
                  if( !string.IsNullOrEmpty( job.TranslatedText ) )
                  {
                     SetTranslatedText( component, key.Untemplate( job.TranslatedText ), key.OriginalText, info );
                  }
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
                  var text = context.Component.GetText();
                  var result = context.Result;
                  Dictionary<string, string> translations = new Dictionary<string, string>();

                  string translatedText;
                  if( context.TranslationResult == null )
                  {
                     translatedText = TranslateOrQueueWebJobImmediateByParserResult( context.Component, result, false );
                  }
                  else
                  {
                     translatedText = TranslateByParserResult( context.Endpoint, result, null, false );
                  }

                  if( !string.IsNullOrEmpty( translatedText ) )
                  {
                     if( result.PersistCombinedResult )
                     {
                        if( job.SaveResultGlobally )
                        {
                           TextCache.AddTranslationToCache( context.Result.OriginalText, translatedText );
                        }
                        job.Endpoint.AddTranslationToCache( context.Result.OriginalText, translatedText );
                     }

                     if( text == result.OriginalText )
                     {
                        var info = context.Component.GetOrCreateTextTranslationInfo();
                        SetTranslatedText( context.Component, translatedText, context.Result.OriginalText, info );
                     }

                     if( context.TranslationResult != null )
                     {
                        context.TranslationResult.SetCompleted( translatedText, false );
                     }
                  }
                  else
                  {
                     if( context.TranslationResult != null )
                     {
                        context.TranslationResult.SetEmptyResponse( false );
                     }
                  }
               }
               catch( NullReferenceException )
               {
               }
            }
         }

         Settings.TranslationCount++;
         if( !Settings.IsShutdown )
         {
            if( Settings.TranslationCount > Settings.MaxTranslationsBeforeShutdown )
            {
               Settings.IsShutdown = true;
               XuaLogger.Current.Error( $"Maximum translations ({Settings.MaxTranslationsBeforeShutdown}) per session reached. Shutting plugin down." );

               TranslationManager.ClearAllJobs();
            }
         }
      }

      private static UntranslatedText GetCacheKey( object ui, string originalText, bool templatizeByNumbers, bool neverRemoveWhitespace )
      {
         var removeInternalWhitespace = !neverRemoveWhitespace
            && ( ( Settings.IgnoreWhitespaceInDialogue && originalText.Length > Settings.MinDialogueChars ) || ( Settings.IgnoreWhitespaceInNGUI && ui.IsNGUI() ) );

         return new UntranslatedText( originalText, templatizeByNumbers, removeInternalWhitespace );
      }

      private void ReloadTranslations()
      {
         LoadTranslations();

         var context = new TextureReloadContext();
         foreach( var kvp in ObjectReferenceMapper.GetAllRegisteredObjects() )
         {
            var ui = kvp.Key;
            try
            {
               if( ui is Component component )
               {
                  if( component.gameObject?.activeSelf ?? false )
                  {
                     var tti = kvp.Value as TextTranslationInfo;
                     var originalText = tti.OriginalText;
                     if( tti != null && !string.IsNullOrEmpty( originalText ) )
                     {
                        var key = GetCacheKey( kvp.Key, originalText, false, false );
                        if( TextCache.TryGetTranslation( key, true, out string translatedText ) && !string.IsNullOrEmpty( translatedText ) )
                        {
                           SetTranslatedText( kvp.Key, key.Untemplate( translatedText ), null, tti ); // no need to untemplatize the translated text
                        }
                        else if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
                        {
                           var result = UnityTextParsers.GameLogTextParser.Parse( originalText );
                           if( result.Succeeded )
                           {
                              var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, false );
                              if( translation != null )
                              {
                                 SetTranslatedText( ui, translation, null, tti );
                              }
                           }
                        }
                        else if( UnityTextParsers.RichTextParser.CanApply( ui ) && IsBelowMaxLength( originalText ) )
                        {
                           var result = UnityTextParsers.RichTextParser.Parse( originalText );
                           if( result.Succeeded )
                           {
                              var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, false );
                              if( translation != null )
                              {
                                 SetTranslatedText( ui, translation, null, tti );
                              }
                           }
                        }
                     }
                  }
               }

               if( Settings.EnableTextureTranslation )
               {
                  TranslateTexture( ui, context );
               }
            }
            catch( Exception )
            {
               // not super pretty, no...
               ObjectReferenceMapper.Remove( ui );
            }
         }
      }

      private void ToggleFont()
      {
         if( _hasValidOverrideFont )
         {
            _hasOverridenFont = !_hasOverridenFont;

            var objects = ObjectReferenceMapper.GetAllRegisteredObjects();
            XuaLogger.Current.Info( $"Toggling fonts of {objects.Count} objects." );

            if( _hasOverridenFont )
            {
               // make sure we use the translated version of all texts
               foreach( var kvp in objects )
               {
                  var tti = kvp.Value as TextTranslationInfo;
                  if( tti != null )
                  {
                     var ui = kvp.Key;
                     try
                     {
                        if( ( ui as Component )?.gameObject?.activeSelf ?? false )
                        {
                           tti?.ChangeFont( ui );
                        }
                     }
                     catch( Exception )
                     {
                        // not super pretty, no...
                        ObjectReferenceMapper.Remove( ui );
                     }
                  }
               }
            }
            else
            {
               // make sure we use the original version of all texts
               foreach( var kvp in objects )
               {
                  var tti = kvp.Value as TextTranslationInfo;
                  var ui = kvp.Key;
                  try
                  {
                     if( ( ui as Component )?.gameObject?.activeSelf ?? false )
                     {
                        tti?.UnchangeFont( ui );
                     }
                  }
                  catch( Exception )
                  {
                     // not super pretty, no...
                     ObjectReferenceMapper.Remove( ui );
                  }
               }
            }
         }
      }

      private void ToggleTranslation()
      {
         _isInTranslatedMode = !_isInTranslatedMode;
         var objects = ObjectReferenceMapper.GetAllRegisteredObjects();

         XuaLogger.Current.Info( $"Toggling translations of {objects.Count} objects." );

         if( _isInTranslatedMode )
         {
            // make sure we use the translated version of all texts
            foreach( var kvp in objects )
            {
               var ui = kvp.Key;
               try
               {
                  if( ui is Component component )
                  {
                     if( component.gameObject?.activeSelf ?? false )
                     {
                        var tti = kvp.Value as TextTranslationInfo;
                        if( tti != null && tti.IsTranslated )
                        {
                           SetText( ui, tti.TranslatedText, true, null, tti );
                        }
                     }
                  }

                  if( Settings.EnableTextureTranslation && Settings.EnableTextureToggling )
                  {
                     TranslateTexture( ui, null );
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ObjectReferenceMapper.Remove( ui );
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
                  if( ui is Component component )
                  {
                     if( component.gameObject?.activeSelf ?? false )
                     {
                        var tti = kvp.Value as TextTranslationInfo;
                        if( tti != null && tti.IsTranslated )
                        {
                           SetText( ui, tti.OriginalText, false, null, tti );
                        }
                     }
                  }

                  if( Settings.EnableTextureTranslation && Settings.EnableTextureToggling )
                  {
                     TranslateTexture( ui, null );
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ObjectReferenceMapper.Remove( ui );
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
               XuaLogger.Current.Error( e, "An error while copying text to clipboard." );
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
         ManualHookForComponents();
         ManualHookForTextures();
      }

      private void ManualHookForComponents()
      {
         foreach( var root in GetAllRoots() )
         {
            TraverseChildrenManualHook( root );
         }
      }

      private void ManualHookForTextures()
      {
         if( Settings.EnableTextureScanOnSceneLoad && ( Settings.EnableTextureTranslation || Settings.EnableTextureDumping ) )
         {
            // scan all textures and update
            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach( var texture in textures )
            {
               Hook_ImageChanged( texture, false );
            }

            //// scan all components and set dirty
            //var components = GameObject.FindObjectsOfType<Component>();
            //foreach( var component in components )
            //{
            //   component.SetAllDirtyEx();
            //}
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
            var components = string.Join( ", ", obj.GetComponents<Component>().Select( x =>
            {
               string output = null;
               var type = x?.GetType();
               if( type != null )
               {
                  output = type.Name;

                  var text = x.GetText();
                  if( !string.IsNullOrEmpty( text ) )
                  {
                     output += " (" + text + ")";
                  }
               }

               return output;
            } ).Where( x => x != null ).ToArray() );
            var line = string.Format( "{0,-50} {1,100}",
               identation + obj.name + " [" + layer + "]",
               components );

            writer.WriteLine( line );

            if( obj.transform != null )
            {
               for( int i = 0; i < obj.transform.childCount; i++ )
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
               if( component.IsKnownTextType() )
               {
                  Hook_TextChanged( component, false );
               }

               if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
               {
                  if( component.IsKnownImageType() )
                  {
                     Hook_ImageChangedOnComponent( component, null, false, false );
                  }
               }
            }

            if( obj.transform != null )
            {
               for( int i = 0; i < obj.transform.childCount; i++ )
               {
                  var child = obj.transform.GetChild( i );
                  TraverseChildrenManualHook( child.gameObject );
               }
            }
         }
      }

      /// <summary>
      /// Temporarily disables the Auto Translator functionality.
      /// </summary>
      public void DisableAutoTranslator()
      {
         _temporarilyDisabled = true;
      }

      /// <summary>
      /// Re-enables the Auto Translator functionality after
      /// having disabled it thorugh DisableAutoTranslator
      /// </summary>
      public void EnableAutoTranslator()
      {
         _temporarilyDisabled = false;
      }

      void OnApplicationQuit()
      {
         foreach( var ce in TranslationManager.AllEndpoints )
         {
            try
            {
               if( ce.Endpoint is IDisposable disposable ) disposable.Dispose();
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An error occurred while disposing endpoint." );
            }
         }
      }
   }

   internal static class SceneManagerLoader
   {
      public static void EnableSceneLoadScanInternal( AutoTranslationPlugin plugin )
      {
         // specified in own method, because of chance that this has changed through Unity lifetime
         SceneManager.sceneLoaded += ( arg1, arg2 ) => plugin.OnLevelWasLoadedFromSceneManager( arg1.buildIndex );
      }
   }
}
