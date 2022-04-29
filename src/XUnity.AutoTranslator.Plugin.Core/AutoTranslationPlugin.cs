using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ExIni;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using System.Diagnostics;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web.Internal;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.Common.Constants;
using XUnity.ResourceRedirector;
using XUnity.Common.Extensions;
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.AutoTranslator.Plugin.Core.UI;
using XUnity.AutoTranslator.Plugin.Core.Fonts;

#if MANAGED
using MonoMod.RuntimeDetour;
#endif

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Main plugin class for the AutoTranslator.
   /// </summary>
   public class AutoTranslationPlugin :
#if MANAGED
      MonoBehaviour,
#endif
      IMonoBehaviour,
      IInternalTranslator,
      ITranslationRegistry
   {
      /// <summary>
      /// Allow the instance to be accessed statically, as only one will exist.
      /// </summary>
      internal static AutoTranslationPlugin Current;

      private bool _hasResizedCurrentComponentDuringDiscovery;

      internal XuaWindow MainWindow;
      internal TranslationAggregatorWindow TranslationAggregatorWindow;
      internal TranslationAggregatorOptionsWindow TranslationAggregatorOptionsWindow;
      internal TranslationManager TranslationManager;
      internal TextTranslationCache TextCache;
      internal Dictionary<string, TextTranslationCache> PluginTextCaches = new Dictionary<string, TextTranslationCache>( StringComparer.OrdinalIgnoreCase );
      internal TextureTranslationCache TextureCache;
      internal UIResizeCache ResizeCache;
      internal SpamChecker SpamChecker;
      private Dictionary<string, UntranslatedText> CachedKeys = new Dictionary<string, UntranslatedText>( StringComparer.Ordinal );

      private List<Action<ComponentTranslationContext>> _shouldIgnore = new List<Action<ComponentTranslationContext>>();

      /// <summary>
      /// Keeps track of things to copy to clipboard.
      /// </summary>
      private List<string> _textsToCopyToClipboardOrdered = new List<string>();
      private HashSet<string> _textsToCopyToClipboard = new HashSet<string>();
      private float _clipboardUpdated = 0.0f;

      /// <summary>
      /// Texts currently being scheduled for translation by 'immediate' components.
      /// </summary>
      private HashSet<string> _immediatelyTranslating = new HashSet<string>();

      private bool _isInTranslatedMode = true;
      private bool _textHooksEnabled = true;

      private float _batchOperationSecondCounter = 0;

      private bool _hasValidOverrideFont = false;
      private bool _hasOverridenFont = false;
      private bool _initialized = false;
      private bool _started = false;
      private bool _temporarilyDisabled = false;
      private string _requireSpriteRendererCheckCausedBy = null;
      private int _lastSpriteUpdateFrame = -1;
      private bool _isCalledFromSceneManager = false;
      private bool _translationReloadRequest = false;
      private bool _hasUiBeenSetup = false;

      /// <summary>
      /// Initialized the plugin.
      /// </summary>
      public void Initialize()
      {
         // Setup 'singleton'
         Current = this;

         Paths.Initialize();

         // because we only use harmony/MonoMod through reflection due to
         // version compatibility issues, we call this method to
         // ensure that it is loaded before we attempt to obtain
         // various harmony/MonoMod classes through reflection
         HarmonyLoader.Load();

         // Setup configuration
         Settings.Configure();

         // Setup console, if enabled
         DebugConsole.Enable();

         InitializeHarmonyDetourBridge();

         InitializeTextTranslationCaches();

         // Setup hooks
         HooksSetup.InstallTextHooks();
         HooksSetup.InstallImageHooks();
         HooksSetup.InstallSpriteRendererHooks();
         HooksSetup.InstallTextGetterCompatHooks();
         HooksSetup.InstallComponentBasedPluginTranslationHooks();

         TextureCache = new TextureTranslationCache();
         TextureCache.TextureTranslationFileChanged += TextureCache_TextureTranslationFileChanged;
         ResizeCache = new UIResizeCache();
         TranslationManager = new TranslationManager();
         TranslationManager.JobCompleted += OnJobCompleted;
         TranslationManager.JobFailed += OnJobFailed;
         TranslationManager.InitializeEndpoints();
         SpamChecker = new SpamChecker( TranslationManager );

         // WORKAROUND: Initialize text parsers with delegate indicating if text should be translated
         UnityTextParsers.Initialize();

         // resource redirectors
         InitializeResourceRedirector();

         // validate configuration
         ValidateConfiguration();

         // enable scene scan
         EnableSceneLoadScan();

         // load fallback fonts
         LoadFallbackFont();

         // load all translations from files
         LoadTranslations( false );

         XuaLogger.AutoTranslator.Info( $"Loaded XUnity.AutoTranslator into Unity [{Application.unityVersion}] game." );
      }

      private static void LoadFallbackFont()
      {
         try
         {
            if( !string.IsNullOrEmpty( Settings.FallbackFontTextMeshPro ) )
            {
               var font = FontCache.GetOrCreateFallbackFontTextMeshPro();
               if( UnityTypes.TMP_Settings_Properties.FallbackFontAssets == null )
               {
                  XuaLogger.AutoTranslator.Info( $"Cannot use fallback font because it is not supported in this version." );
                  return;
               }

               if( font == null )
               {
                  XuaLogger.AutoTranslator.Warn( $"Could not load fallback font for TextMesh Pro: " + Settings.FallbackFontTextMeshPro );
                  return;
               }

#if MANAGED
               var fallbacks = (IList)UnityTypes.TMP_Settings_Properties.FallbackFontAssets.Get( null );
#else
               var fallbacksObj = (Il2CppSystem.Object)UnityTypes.TMP_Settings_Properties.FallbackFontAssets.Get( null );
               fallbacksObj.TryCastTo<Il2CppSystem.Collections.IList>( out var fallbacks );
#endif
               fallbacks.Add( font );

               XuaLogger.AutoTranslator.Info( $"Loaded fallback font for TextMesh Pro: " + Settings.FallbackFontTextMeshPro );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while trying to load fallback font for TextMesh Pro." );
         }
      }

      private static void InitializeHarmonyDetourBridge()
      {
         try
         {
            if( Settings.InitializeHarmonyDetourBridge )
            {
               InitializeHarmonyDetourBridgeSafe();
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while initializing harmony detour bridge." );
         }
      }

      private static void InitializeHarmonyDetourBridgeSafe()
      {
#if MANAGED
         HarmonyDetourBridge.Init();
#endif
      }

      private void InitializeTextTranslationCaches()
      {
         try
         {
            TextCache = new TextTranslationCache();
            TextCache.TextTranslationFileChanged += TextCache_TextTranslationFileChanged;

            var path = Path.Combine( Settings.TranslationsPath, "plugins" );
            var directory = new DirectoryInfo( path );
            if( directory.Exists )
            {
               foreach( var dir in directory.GetDirectories() )
               {
                  var cache = new TextTranslationCache( dir );
                  PluginTextCaches.Add( dir.Name, cache );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while initializing text translation caches." );
         }
      }

      private void TextCache_TextTranslationFileChanged()
      {
         _translationReloadRequest = true;
      }

      private void TextureCache_TextureTranslationFileChanged()
      {
         _translationReloadRequest = true;
      }

      private static void EnableLogAllLoadedResources()
      {
         ResourceRedirection.LogAllLoadedResources = true;
      }

      private void EnableTextAssetLoadedHandler()
      {
         new TextAssetLoadedHandler();
      }

      private void InitializeResourceRedirector()
      {
         try
         {
            if( Settings.LogAllLoadedResources )
            {
               EnableLogAllLoadedResources();
            }

            if( Settings.EnableTextAssetRedirector )
            {
               EnableTextAssetLoadedHandler();
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while initializing resource redirectors." );
         }
      }

      private void InitializeGUI()
      {
         DisableAutoTranslator();
         try
         {
            if( !_hasUiBeenSetup )
            {
               _hasUiBeenSetup = true;

               MainWindow = new XuaWindow( CreateXuaViewModel() );

               var vm = CreateTranslationAggregatorViewModel();
               TranslationAggregatorWindow = new TranslationAggregatorWindow( vm );
               TranslationAggregatorOptionsWindow = new TranslationAggregatorOptionsWindow( vm );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up UI." );
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
                  ToggleTranslation, () => _isInTranslatedMode ),
               new ToggleViewModel(
                  " Silent Logging",
                  "<b>SILENT</b>\nThe plugin will not print out success messages to the log in relation to translations.",
                  "<b>VERBOSE</b>\nThe plugin will print out success messages to the log in relation to translations.",
                  ToggleSilentMode, () => Settings.EnableSilentMode ),
               new ToggleViewModel(
                  " Translation Aggregator",
                  "<b>SHOWN</b>\nThe translation aggregator window is shown.",
                  "<b>HIDDEN</b>\nThe translation aggregator window is not shown.",
                  ToggleTranslationAggregator, () => TranslationAggregatorWindow != null && TranslationAggregatorWindow.IsShown ),
            },
            new DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager>(
               "----",
               "<b>SELECT TRANSLATOR</b>\nNo translator is currently selected, which means no new translations will be performed. Please select one from the dropdown.",
               "----",
               "<b>UNSELECT TRANSLATOR</b>\nThis will unselect the current translator, which means no new translations will be performed.",
               TranslationManager.AllEndpoints.Select( x => new TranslatorDropdownOptionViewModel( false, () => x == TranslationManager.CurrentEndpoint, x ) ).ToList(),
               OnEndpointSelected
            ),
            new DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager>(
               "----",
               "<b>SELECT FALLBACK TRANSLATOR</b>\nNo fallback translator is currently selected, which means if the primary translator fails no translation will be provided for the failing text. Please select one from the dropdown.",
               "----",
               "<b>UNSELECT FALLBACK TRANSLATOR</b>\nThis will unselect the current fallback translator.",
               TranslationManager.AllEndpoints.Select( x => new TranslatorDropdownOptionViewModel( true, () => x == TranslationManager.FallbackEndpoint, x ) ).ToList(),
               OnFallbackEndpointSelected
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

      private void ToggleTranslationAggregator()
      {
         if( TranslationAggregatorWindow != null )
         {
            TranslationAggregatorWindow.IsShown = !TranslationAggregatorWindow.IsShown;
         }
      }

      private void ToggleSilentMode()
      {
         Settings.SetSlientMode( !Settings.EnableSilentMode );
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

      private void ValidateConfiguration()
      {
         // check if font is supported
         try
         {
            if( !string.IsNullOrEmpty( Settings.OverrideFont ) )
            {
               var available = GetSupportedFonts();
               if( available == null )
               {
                  XuaLogger.AutoTranslator.Warn( $"Unable to validate OverrideFont validity due to shimmed APIs." );
               }
               else if( !available.Contains( Settings.OverrideFont ) )
               {
                  XuaLogger.AutoTranslator.Error( $"The specified override font is not available. Available fonts: " + string.Join( ", ", available ) );
                  Settings.OverrideFont = null;
               }
               else
               {
                  _hasValidOverrideFont = true;
               }
            }

            // Fonts & Materials/ARIAL SDF
            // Fonts & Materials/LiberationSans SDF
            if( !string.IsNullOrEmpty( Settings.OverrideFontTextMeshPro ) )
            {
               _hasValidOverrideFont = true;
            }

            _hasOverridenFont = _hasValidOverrideFont;
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while checking supported fonts." );
         }
      }

      internal static string[] GetSupportedFonts()
      {
         try
         {
            return FontHelper.GetOSInstalledFontNames();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "Unable to retrieve OS installed fonts." );
            return null;
         }
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

               if( TranslationManager.CurrentEndpoint == TranslationManager.FallbackEndpoint )
               {
                  XuaLogger.AutoTranslator.Warn( "Cannot use same fallback endpoint as primary." );
               }
            }

            Settings.SetEndpoint( TranslationManager.CurrentEndpoint?.Endpoint.Id );

            XuaLogger.AutoTranslator.Info( $"Set translator endpoint to '{TranslationManager.CurrentEndpoint?.Endpoint.Id}'." );
         }
      }

      private void OnFallbackEndpointSelected( TranslationEndpointManager endpoint )
      {
         if( TranslationManager.FallbackEndpoint != endpoint )
         {
            TranslationManager.FallbackEndpoint = endpoint;

            Settings.SetFallback( TranslationManager.FallbackEndpoint?.Endpoint.Id );

            XuaLogger.AutoTranslator.Info( $"Set fallback endpoint to '{TranslationManager.FallbackEndpoint?.Endpoint.Id}'." );

            if( TranslationManager.CurrentEndpoint != null && TranslationManager.CurrentEndpoint == TranslationManager.FallbackEndpoint )
            {
               XuaLogger.AutoTranslator.Warn( "Cannot use same fallback endpoint as primary." );
            }
         }
      }

      private void EnableSceneLoadScan()
      {
         try
         {
            XuaLogger.AutoTranslator.Debug( "Probing whether OnLevelWasLoaded or SceneManager is supported in this version of Unity. Any warnings related to OnLevelWasLoaded coming from Unity can safely be ignored." );
            if( UnityFeatures.SupportsSceneManager )
            {
               TranslationScopeHelper.RegisterSceneLoadCallback( OnLevelWasLoadedFromSceneManager );
               XuaLogger.AutoTranslator.Debug( "SceneManager is supported in this version of Unity." );
            }
            else
            {
               XuaLogger.AutoTranslator.Debug( "SceneManager is not supported in this version of Unity. Falling back to OnLevelWasLoaded and Application level API." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while settings up scene-load scans." );
         }
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
         if( !UnityFeatures.SupportsSceneManager || ( UnityFeatures.SupportsSceneManager && _isCalledFromSceneManager ) )
         {
            if( Settings.EnableTextureScanOnSceneLoad && ( Settings.EnableTextureDumping || Settings.EnableTextureTranslation ) )
            {
               XuaLogger.AutoTranslator.Info( "Performing texture lookup during scene load..." );
               var startTime = Time.realtimeSinceStartup;

               ManualHookForTextures();

               var endTime = Time.realtimeSinceStartup;
               XuaLogger.AutoTranslator.Info( $"Finished texture lookup (took {Math.Round( endTime - startTime, 2 )} seconds)" );
            }
         }
      }

      /// <summary>
      /// Loads the translations found in Translation.{lang}.txt
      /// </summary>
      private void LoadTranslations( bool reload )
      {
         ResizeCache.LoadResizeCommandsInFiles();

         SettingsTranslationsInitializer.LoadTranslations();
         TextCache.LoadTranslationFiles();

         if( reload )
         {
            var dict = new Dictionary<string, DirectoryInfo>( StringComparer.OrdinalIgnoreCase );
            var path = Path.Combine( Settings.TranslationsPath, "plugins" );
            var directory = new DirectoryInfo( path );
            if( directory.Exists )
            {
               foreach( var dir in directory.GetDirectories() )
               {
                  dict.Add( dir.Name, dir );
               }
            }

            foreach( var pluginCache in PluginTextCaches )
            {
               pluginCache.Value.LoadTranslationFiles();
               dict.Remove( pluginCache.Key );
            }

            // we need to hook any newly created folders
            foreach( var kvp in dict )
            {
               var assemblyName = kvp.Value.Name;

               var cache = new TextTranslationCache( kvp.Value );
               PluginTextCaches.Add( assemblyName, cache );
               cache.LoadTranslationFiles();

               var assembly = AppDomain.CurrentDomain
                  .GetAssemblies()
                  .FirstOrDefault( x => x.GetName().Name.Equals( assemblyName, StringComparison.OrdinalIgnoreCase ) );

               if( assembly != null )
               {
                  HooksSetup.InstallIMGUIBasedPluginTranslationHooks( assembly, true );
               }
            }
            HooksSetup.InstallComponentBasedPluginTranslationHooks();
         }
         else
         {
            foreach( var pluginCache in PluginTextCaches )
            {
               pluginCache.Value.LoadTranslationFiles();
            }
         }

         TextureCache.LoadTranslationFiles();
      }

      private void CreateTranslationJobFor(
         TranslationEndpointManager endpoint,
         object ui,
         UntranslatedText key,
         InternalTranslationResult translationResult,
         ParserTranslationContext context,
         bool checkOtherEndpoints,
         bool checkSpam,
         bool saveResultGlobally,
         bool isTranslatable,
         bool allowFallback,
         UntranslatedTextInfo untranslatedTextContext )
      {
         var added = endpoint.EnqueueTranslation( ui, key, translationResult, context, untranslatedTextContext, checkOtherEndpoints, saveResultGlobally, isTranslatable, allowFallback );
         if( added != null && isTranslatable && checkSpam && !( endpoint.Endpoint is PassthroughTranslateEndpoint ) )
         {
            SpamChecker.PerformChecks( key.TemplatedOriginal_Text_FullyTrimmed, endpoint );
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

                  var spriteRenderers = ComponentHelper.FindObjectsOfType<SpriteRenderer>();
                  foreach( var sr in spriteRenderers )
                  {
                     // simulate a hook
                     Texture2D _ = null;
                     Hook_ImageChangedOnComponent( sr, ref _, false, false );
                  }

                  var end = Time.realtimeSinceStartup;
                  var delta = Math.Round( end - start, 2 );

                  XuaLogger.AutoTranslator.Debug( $"Update SpriteRenderers caused by {_requireSpriteRendererCheckCausedBy} component (took " + delta + " seconds)" );
               }
               finally
               {
                  _requireSpriteRendererCheckCausedBy = null;
               }
            }
         }
      }

      private void QueueNewUntranslatedForClipboard( string untranslatedText )
      {
         if( Settings.CopyToClipboard && UnityFeatures.SupportsClipboard )
         {
            // Perhaps this should be the original, but that might have unexpected consequences for external tools??
            if( !_textsToCopyToClipboard.Contains( untranslatedText ) )
            {
               _textsToCopyToClipboard.Add( untranslatedText );
               _textsToCopyToClipboardOrdered.Add( untranslatedText );

               _clipboardUpdated = Time.realtimeSinceStartup;
            }
         }
      }

      internal string Hook_TextChanged_WithResult( object ui, string text, bool onEnable )
      {
         try
         {
            string result = null;
            if( _textHooksEnabled && !_temporarilyDisabled )
            {
               try
               {
                  var info = ui.GetOrCreateTextTranslationInfo();
                  var isComponentActive = DiscoverComponent( ui, info );

                  if( onEnable && info != null && CallOrigin.TextCache != null )
                  {
                     info.TextCache = CallOrigin.TextCache;
                  }

                  CallOrigin.ExpectsTextToBeReturned = true;

                  result = TranslateOrQueueWebJob( ui, text, isComponentActive, info );
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Warn( e, "An unexpected error occurred." );
               }
               finally
               {
                  _hasResizedCurrentComponentDuringDiscovery = false;
               }
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
            try
            {
               var info = ui.GetOrCreateTextTranslationInfo();
               var isComponentActive = DiscoverComponent( ui, info );

               if( onEnable && info != null && CallOrigin.TextCache != null )
               {
                  info.TextCache = CallOrigin.TextCache;
               }

               //XuaLogger.AutoTranslator.Warn( ui.GetText( info ) );

               TranslateOrQueueWebJob( ui, null, isComponentActive, info );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An unexpected error occurred." );
            }
            finally
            {
               _hasResizedCurrentComponentDuringDiscovery = false;
            }
         }

         if( onEnable )
         {
            CheckSpriteRenderer( ui );
         }
      }

      internal void Hook_ImageChangedOnComponent( object source, ref Texture2D texture, bool isPrefixHooked, bool onEnable )
      {
         if( !CallOrigin.ImageHooksEnabled ) return;
         if( !source.IsKnownImageType() ) return;

         Sprite _ = null;
         HandleImage( source, ref _, ref texture, isPrefixHooked );

         if( onEnable )
         {
            CheckSpriteRenderer( source );
         }
      }

      internal void Hook_ImageChangedOnComponent( object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked, bool onEnable )
      {
         if( !CallOrigin.ImageHooksEnabled ) return;
         if( !source.IsKnownImageType() ) return;

         HandleImage( source, ref sprite, ref texture, isPrefixHooked );

         if( onEnable )
         {
            CheckSpriteRenderer( source );
         }
      }

      internal void Hook_ImageChanged( ref Texture2D texture, bool isPrefixHooked )
      {
         if( !CallOrigin.ImageHooksEnabled ) return;
         if( texture == null ) return;

         Sprite _ = null;
         HandleImage( null, ref _, ref texture, isPrefixHooked );
      }

      private bool DiscoverComponent( object ui, TextTranslationInfo info )
      {
         if( info == null ) return true;

         try
         {
            var isComponentActive = ui.IsComponentActive();
            if( ( _hasValidOverrideFont || Settings.ForceUIResizing ) && isComponentActive )
            {
               if( _hasValidOverrideFont )
               {
                  if( _hasOverridenFont )
                  {
                     info.ChangeFont( ui );
                  }
                  else
                  {
                     info.UnchangeFont( ui );
                  }
               }

               if( Settings.ForceUIResizing )
               {
                  info.ResizeUI( ui, ResizeCache );
                  _hasResizedCurrentComponentDuringDiscovery = true;
               }

               return true;
            }
            return isComponentActive;
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while handling the UI discovery." );
         }

         return false;
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

               if( Settings.EnableTextPathLogging )
               {
                  var path = ui.GetPath();
                  if( path != null )
                  {
                     var scope = TranslationScopeHelper.GetScope( ui );
                     XuaLogger.AutoTranslator.Info( $"Setting text on '{ui.GetType().FullName}' to '{text}'" );
                     XuaLogger.AutoTranslator.Info( "Path : " + path );
                     XuaLogger.AutoTranslator.Info( "Level: " + scope );
                  }
               }

               if( !_hasResizedCurrentComponentDuringDiscovery && info != null && ( Settings.EnableUIResizing || Settings.ForceUIResizing ) )
               {
                  if( isTranslated || Settings.ForceUIResizing )
                  {
                     info.ResizeUI( ui, ResizeCache );
                  }
                  else
                  {
                     info.UnresizeUI( ui );
                  }
               }

               // NGUI only behaves if you set the text after the resize behaviour
               ui.SetText( text, info );

               info?.ResetScrollIn( ui );

               if( info.GetIsKnownTextComponent() && originalText != null && ui != null && !ui.IsSpammingComponent() )
               {
                  if( _isInTranslatedMode && isTranslated )
                     TranslationHelper.DisplayTranslationInfo( originalText, text );

                  QueueNewUntranslatedForClipboard( originalText );

                  if( TranslationAggregatorWindow != null )
                  {
                     TranslationAggregatorWindow.OnNewTranslationAdded( originalText, text );
                  }
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
               XuaLogger.AutoTranslator.Error( e, "An error occurred while setting text on a component." );
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

      private string TranslateOrQueueWebJob( object ui, string text, bool ignoreComponentState, TextTranslationInfo info )
      {
         var tc = CallOrigin.GetTextCache( info, TextCache );

         if( info != null && info.IsStabilizingText == true )
         {
            return TranslateImmediate( ui, text, info, ignoreComponentState, tc );
         }

         return TranslateOrQueueWebJobImmediate(
            ui,
            text,
            TranslationScopes.None,
            info,
            info.GetSupportsStabilization(),
            ignoreComponentState,
            false,
            tc.AllowGeneratingNewTranslations,
            tc,
            null,
            null );
      }

      private void HandleImage( object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked )
      {
         if( Settings.EnableTextureDumping )
         {
            try
            {
               DumpTexture( source, texture );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while dumping texture." );
            }
         }

         if( Settings.EnableTextureTranslation )
         {
            try
            {
               TranslateTexture( source, ref sprite, ref texture, isPrefixHooked, null );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while translating texture." );
            }
         }
      }

      private void TranslateTexture( object ui, ref Sprite sprite, TextureReloadContext context )
      {
         if( ui.TryCastTo<Texture2D>( out var texture2d ) )
         {
            TranslateTexture( null, ref sprite, ref texture2d, false, context );
         }
         else
         {
            Texture2D _ = null;
            TranslateTexture( ui, ref sprite, ref _, false, context );
         }
      }

      private void TranslateTexture( object ui, TextureReloadContext context )
      {
         Sprite __ = null;
         if( ui.TryCastTo<Texture2D>( out var texture2d ) )
         {
            TranslateTexture( null, ref __, ref texture2d, false, context );
         }
         else
         {
            Texture2D _ = null;
            TranslateTexture( ui, ref __, ref _, false, context );
         }
      }

      private void TranslateTexture( object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked, TextureReloadContext context )
      {
         try
         {
            CallOrigin.ImageHooksEnabled = false;

            var previousTextureValue = texture;
            texture = texture ?? source.GetTexture();
            if( texture == null ) return;

            var tti = texture.GetOrCreateTextureTranslationInfo();
            var iti = source.GetOrCreateImageTranslationInfo( texture );
            var key = tti.GetKey();
            if( string.IsNullOrEmpty( key ) ) return;

            bool hasContext = context != null;
            bool forceReload = false;
            bool changedImage = false;
            if( hasContext )
            {
               forceReload = context.RegisterTextureInContextAndDetermineWhetherToReload( texture );
            }

            if( Settings.EnableLegacyTextureLoading
               && Settings.EnableSpriteRendererHooking
               && iti?.IsTranslated == true
               && source.TryCastTo<SpriteRenderer>( out var sr ) )
            {

               var originalTexture = tti.Original.Target;
               var translatedTexture = tti.Translated;
               if( Equals( texture, originalTexture ) && tti.IsTranslated )
               {
                  // if the texture is the original, we update the sprite
                  if( tti.TranslatedSprite != null )
                  {
                     if( isPrefixHooked )
                     {
                        if( sprite != null )
                        {
                           sprite = tti.TranslatedSprite;
                        }
                     }
                     else
                     {
                        sr.sprite = tti.TranslatedSprite;
                     }
                  }
               }
               else if( Equals( texture, translatedTexture ) ) // can only happen if && tti.IsTranslated
               {
                  // if the texture is the translated, we do not need to do anything
               }
               else
               {
                  // if the texture is neither the original or the translated, we must reset

                  iti.Reset( texture );

                  if( tti.IsTranslated )
                  {
                     if( isPrefixHooked && sprite != null && tti.TranslatedSprite != null )
                     {
                        sprite = tti.TranslatedSprite;
                     }
                  }
               }
            }

            if( TextureCache.TryGetTranslatedImage( key, out var newData, out var translatedImage ) )
            {
               if( _isInTranslatedMode )
               {
                  var isCompatible = texture.IsCompatible( translatedImage.ImageFormat );

                  // handle texture
                  if( !tti.IsTranslated || forceReload )
                  {
                     try
                     {
                        if( Settings.EnableLegacyTextureLoading || !isCompatible )
                        {
                           tti.CreateTranslatedTexture( newData, translatedImage.ImageFormat );
                           changedImage = true;
                        }
                        else
                        {
                           texture.LoadImageEx( newData, translatedImage.ImageFormat, null );
                           changedImage = true;
                        }
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
                           if( Settings.EnableLegacyTextureLoading || !isCompatible )
                           {
                              var newSprite = source.SetTexture( tti.Translated, sprite, isPrefixHooked );
                              if( newSprite != null )
                              {
                                 tti.TranslatedSprite = newSprite;
                                 if( isPrefixHooked && sprite != null )
                                 {
                                    sprite = newSprite;
                                 }
                              }
                           }

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
               var originalData = tti.GetOriginalData();
               if( originalData != null )
               {
                  if( tti.IsTranslated )
                  {
                     try
                     {
                        if( Settings.EnableLegacyTextureLoading ) // original data is always compatible (PNG)
                        {
                           // we just need to ensure we set/change the reference
                           tti.CreateOriginalTexture();
                           changedImage = true;
                        }
                        else
                        {
                           texture.LoadImageEx( originalData, ImageFormat.PNG, null );
                           changedImage = true;
                        }
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
                           var original = tti.Original.Target;
                           if( Settings.EnableLegacyTextureLoading && original != null )
                           {
                              source.SetTexture( original, null, isPrefixHooked );
                           }

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
               var originalData = tti.GetOriginalData();
               if( originalData != null )
               {
                  // handle texture
                  if( tti.IsTranslated )
                  {
                     try
                     {
                        if( Settings.EnableLegacyTextureLoading ) // original data is always compatible (PNG)
                        {
                           // we just need to ensure we set/change the reference
                           tti.CreateOriginalTexture();
                           changedImage = true;
                        }
                        else
                        {
                           texture.LoadImageEx( originalData, ImageFormat.PNG, null );
                           changedImage = true;
                        }
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
                           var original = tti.Original.Target;
                           if( Settings.EnableLegacyTextureLoading && original != null )
                           {
                              source.SetTexture( original, null, isPrefixHooked );
                           }

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

            if( previousTextureValue == null )
            {
               texture = null;
            }
            else if( tti.UsingReplacedTexture )
            {
               if( tti.IsTranslated )
               {
                  var translated = tti.Translated;
                  if( translated != null )
                  {
                     texture = translated;
                  }
               }
               else
               {
                  var original = tti.Original.Target;
                  if( original != null )
                  {
                     texture = original;
                  }
               }
            }
            else
            {
               texture = previousTextureValue;
            }

            if( forceReload && changedImage )
            {
               XuaLogger.AutoTranslator.Info( $"Reloaded texture: {texture.name} ({key})." );
            }
         }
         finally
         {
            CallOrigin.ImageHooksEnabled = true;
         }
      }

      private void DumpTexture( object source, Texture2D texture )
      {
         try
         {
            CallOrigin.ImageHooksEnabled = false;

            texture = texture ?? source.GetTexture();
            if( texture == null ) return;

            var info = texture.GetOrCreateTextureTranslationInfo();
            if( info.IsDumped ) return;

            try
            {
               if( ShouldTranslate( texture ) )
               {
                  var key = info.GetKey();
                  if( string.IsNullOrEmpty( key ) ) return;

                  if( !TextureCache.IsImageRegistered( key ) )
                  {
                     var name = texture.GetTextureName( "Unnamed" );

                     var originalData = info.GetOrCreateOriginalData();
                     TextureCache.RegisterImageFromData( name, key, originalData );
                  }
               }
            }
            finally
            {
               info.IsDumped = true;
            }
         }
         finally
         {
            CallOrigin.ImageHooksEnabled = true;
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

      private string TranslateImmediate( object ui, string text, TextTranslationInfo info, bool ignoreComponentState, IReadOnlyTextTranslationCache tc )
      {
         if( info != null && info.IsCurrentlySettingText )
            return null;

         text = text ?? ui.GetText( info );

         // Get the trimmed text
         string originalText = text;

         // this only happens if the game sets the text of a component to our translation
         if( info?.IsTranslated == true && originalText == info.TranslatedText )
         {
            return null;
         }

         bool shouldIgnore = false;
         if( info != null )
         {
            info.Reset( originalText );
            shouldIgnore = info.ShouldIgnore;
         }

         if( text.IsNullOrWhiteSpace() )
            return null;

         if( CheckAndFixRedirected( ui, text, info ) )
            return null;

         var scope = TranslationScopeHelper.GetScope( ui );
         if( !shouldIgnore && tc.IsTranslatable( text, false, scope ) && ( ignoreComponentState || ui.IsComponentActive() ) )
         {
            //var textKey = new TranslationKey( ui, text, !ui.SupportsStabilization(), false );
            var isSpammer = ui.IsSpammingComponent();
            var textKey = GetCacheKey( text, isSpammer );

            // potentially shortcircuit if fully templated
            if( ( textKey.IsTemplated && !tc.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
               var isPartial = tc.IsPartial( textKey.TemplatedOriginal_Text, scope );
               SetTranslatedText( ui, untemplatedTranslation, !isPartial ? originalText : null, info );
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our cache, simply load it and set text
            string translation;
            if( tc.TryGetTranslation( textKey, false, false, scope, out translation ) )
            {
               var untemplatedTranslation = textKey.Untemplate( translation );
               var isPartial = tc.IsPartial( textKey.TemplatedOriginal_Text, scope );
               SetTranslatedText( ui, untemplatedTranslation, !isPartial ? originalText : null, info );
               return untemplatedTranslation;
            }
            else
            {
               if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
               {
                  var result = UnityTextParsers.GameLogTextParser.Parse( text, scope, tc );
                  if( result != null )
                  {
                     var isTranslatable = LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text );
                     translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, isTranslatable || Settings.OutputUntranslatableText, tc, null );
                     if( translation != null )
                     {
                        var isPartial = tc.IsPartial( textKey.TemplatedOriginal_Text, scope );
                        SetTranslatedText( ui, translation, null, info );
                        return translation;
                     }
                  }
               }
            }
         }

         return null;
      }

      #region IInternalTranslator

      private ComponentTranslationContext InvokeOnTranslatingCallback( object textComponent, string untranslatedText, TextTranslationInfo info )
      {
         var len = _shouldIgnore.Count;
         if( info != null && !info.IsCurrentlySettingText && len > 0 )
         {
            try
            {
               var context = new ComponentTranslationContext( textComponent, untranslatedText );

               info.IsCurrentlySettingText = true;

               for( int i = 0; i < len; i++ )
               {
                  _shouldIgnore[ i ]( context );
                  if( context.Behaviour != ComponentTranslationBehaviour.Default )
                  {
                     return context;
                  }

               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred during a on-translating callback." );
            }
            finally
            {
               info.IsCurrentlySettingText = false;
            }

         }
         return null;
      }

      void ITranslator.IgnoreTextComponent( object textComponent )
      {
         var info = textComponent.GetOrCreateTextTranslationInfo();
         if( info != null )
         {
            info.ShouldIgnore = true;
         }
      }

      void ITranslator.UnignoreTextComponent( object textComponent )
      {
         var info = textComponent.GetOrCreateTextTranslationInfo();
         if( info != null )
         {
            info.ShouldIgnore = false;
         }
      }

      void ITranslator.RegisterOnTranslatingCallback( Action<ComponentTranslationContext> shouldIgnore )
      {
         _shouldIgnore.Add( shouldIgnore );
      }

      void ITranslator.UnregisterOnTranslatingCallback( Action<ComponentTranslationContext> shouldIgnore )
      {
         _shouldIgnore.Remove( shouldIgnore );
      }

      void IInternalTranslator.TranslateAsync( TranslationEndpointManager endpoint, string untranslatedText, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, TranslationScopes.None, endpoint, null, onCompleted, false, true, false, null );
      }

      void ITranslator.TranslateAsync( string untranslatedText, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, TranslationScopes.None, TranslationManager.CurrentEndpoint, null, onCompleted, true, true, true, null );
      }

      void ITranslator.TranslateAsync( string untranslatedText, int scope, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, scope, TranslationManager.CurrentEndpoint, null, onCompleted, true, true, true, null );
      }

      bool ITranslator.TryTranslate( string text, out string translatedText )
      {
         return TryTranslate( text, TranslationScopes.None, out translatedText );
      }

      bool ITranslator.TryTranslate( string text, int scope, out string translatedText )
      {
         return TryTranslate( text, scope, out translatedText );
      }

      private bool TryTranslate( string text, int scope, out string translatedText )
      {
         if( scope == TranslationScopes.None )
         {
            scope = TranslationScopeHelper.GetScope( null );
         }

         if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) )
         {
            var textKey = GetCacheKey( text, false );

            // potentially shortcircuit if fully templated
            if( ( textKey.IsTemplated && !TextCache.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
               translatedText = untemplatedTranslation;
               return true;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TextCache.TryGetTranslation( textKey, true, false, scope, out translation ) )
            {
               translatedText = translation;
               return true;
            }
            else
            {
               var parserResult = UnityTextParsers.RegexSplittingTextParser.Parse( text, scope, TextCache ) ?? UnityTextParsers.RichTextParser.Parse( text, scope );
               if( parserResult != null )
               {
                  translatedText = TranslateByParserResult( null, parserResult, scope, null, false, true, false, null );
                  return translatedText != null;
               }
            }
         }

         translatedText = null;
         return false;
      }

      #endregion

      private InternalTranslationResult Translate(
         string text,
         int scope,
         TranslationEndpointManager endpoint,
         ParserTranslationContext context,
         Action<TranslationResult> onCompleted,
         bool isGlobal,
         bool allowStartTranslateImmediate,
         bool allowFallback,
         UntranslatedTextInfo untranslatedTextContext )
      {
         var result = new InternalTranslationResult( isGlobal, onCompleted );
         if( isGlobal )
         {
            if( scope == TranslationScopes.None && context == null )
            {
               scope = TranslationScopeHelper.GetScope( null );
            }

            if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) )
            {
               var textKey = GetCacheKey( text, false );

               // potentially shortcircuit if fully templated
               if( ( textKey.IsTemplated && !TextCache.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
               {
                  var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
                  result.SetCompleted( untemplatedTranslation );
                  return result;
               }

               // if we already have translation loaded in our _translatios dictionary, simply load it and set text
               string translation;
               if( TextCache.TryGetTranslation( textKey, true, false, scope, out translation ) )
               {
                  result.SetCompleted( textKey.Untemplate( translation ) );
                  return result;
               }
               else
               {
                  if( context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion )
                  {
                     var parserResult = UnityTextParsers.RegexSplittingTextParser.Parse( text, scope, TextCache );
                     if( parserResult != null )
                     {
                        translation = TranslateByParserResult( endpoint, parserResult, scope, result, allowStartTranslateImmediate, result.IsGlobal, allowFallback, context );
                        if( translation != null )
                        {
                           result.SetCompleted( translation );
                        }

                        // return because result will be completed later by recursive call

                        return result;
                     }

                     if( !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                     {
                        parserResult = UnityTextParsers.RichTextParser.Parse( text, scope );
                        if( parserResult != null )
                        {
                           translation = TranslateByParserResult( endpoint, parserResult, scope, result, allowStartTranslateImmediate, result.IsGlobal, allowFallback, context );
                           if( translation != null )
                           {
                              result.SetCompleted( translation );
                           }

                           // return because result will be completed later by recursive call

                           return result;
                        }
                     }
                  }
                  else
                  {
                     if( Settings.MaxTextParserRecursion != 1 )
                     {
                        XuaLogger.AutoTranslator.Warn( "Attempted to exceed maximum allowed levels of text parsing recursion!" );
                     }
                  }

                  var isTranslatable = LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text );
                  if( !isTranslatable && !Settings.OutputUntranslatableText && !textKey.IsTemplated )
                  {
                     result.SetCompleted( text );
                  }
                  else if( Settings.IsShutdown )
                  {
                     result.SetErrorWithMessage( "The plugin is shutdown." );
                  }
                  else if( endpoint == null )
                  {
                     result.SetErrorWithMessage( "No translator is selected." );
                  }
                  else if( endpoint.HasFailedDueToConsecutiveErrors )
                  {
                     result.SetErrorWithMessage( "The translation endpoint is shutdown." );
                  }
                  else if( !allowStartTranslateImmediate )
                  {
                     result.SetErrorWithMessage( "Could not resolve a translation at this time." );
                  }
                  else if( IsBelowMaxLength( text ) || endpoint == TranslationManager.PassthroughEndpoint )
                  {
                     CreateTranslationJobFor( endpoint, null, textKey, result, context, true, true, true, isTranslatable, allowFallback, untranslatedTextContext );
                  }
                  else if( Settings.OutputTooLongText )
                  {
                     CreateTranslationJobFor( TranslationManager.PassthroughEndpoint, null, textKey, result, context, true, true, true, isTranslatable, false, untranslatedTextContext );
                  }
                  else
                  {
                     result.SetErrorWithMessage( "The provided text exceeds the maximum length." );
                  }

                  return result;
               }
            }
            else
            {
               result.SetErrorWithMessage( $"The provided text ({text}) cannot be translated." );
            }
         }
         else
         {
            if( endpoint == null )
            {
               result.SetErrorWithMessage( "No translator is selected." );
            }
            else if( !text.IsNullOrWhiteSpace() && endpoint.IsTranslatable( text ) )
            {
               var textKey = GetCacheKey( text, false );
               if( textKey.IsTemplated && !endpoint.IsTranslatable( textKey.TemplatedOriginal_Text ) )
               {
                  result.SetErrorWithMessage( "This text is already considered a translation for something else." );
                  return result;
               }

               // potentially shortcircuit if fully templated
               if( textKey.IsOnlyTemplate )
               {
                  var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
                  result.SetCompleted( untemplatedTranslation );
                  return result;
               }

               // if we already have translation loaded in our _translatios dictionary, simply load it and set text
               string translation;
               if( endpoint.TryGetTranslation( textKey, out translation ) )
               {
                  result.SetCompleted( textKey.Untemplate( translation ) );
                  return result;
               }
               else
               {
                  if( context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion )
                  {
                     var parserResult = UnityTextParsers.RegexSplittingTextParser.Parse( text, TranslationScopes.None, TextCache );
                     if( parserResult != null )
                     {
                        translation = TranslateByParserResult( endpoint, parserResult, TranslationScopes.None, result, allowStartTranslateImmediate, result.IsGlobal, allowFallback, context );
                        if( translation != null )
                        {
                           result.SetCompleted( translation );
                        }

                        // return because result will be completed later by recursive call

                        return result;
                     }

                     if( !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                     {
                        parserResult = UnityTextParsers.RichTextParser.Parse( text, TranslationScopes.None );
                        if( parserResult != null )
                        {
                           translation = TranslateByParserResult( endpoint, parserResult, TranslationScopes.None, result, allowStartTranslateImmediate, result.IsGlobal, allowFallback, context );
                           if( translation != null )
                           {
                              result.SetCompleted( translation );
                           }

                           // return because result will be completed later by recursive call

                           return result;
                        }
                     }
                  }
                  else
                  {
                     if( Settings.MaxTextParserRecursion != 1 )
                     {
                        XuaLogger.AutoTranslator.Warn( "Attempted to exceed maximum allowed levels of text parsing recursion!" );
                     }
                  }

                  var isTranslatable = LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text );
                  if( !isTranslatable && !Settings.OutputUntranslatableText && !textKey.IsTemplated )
                  {
                     result.SetCompleted( text );
                  }
                  else if( Settings.IsShutdown )
                  {
                     result.SetErrorWithMessage( "The plugin is shutdown." );
                  }
                  else if( endpoint.HasFailedDueToConsecutiveErrors )
                  {
                     result.SetErrorWithMessage( "The translation endpoint is shutdown." );
                  }
                  else if( !allowStartTranslateImmediate )
                  {
                     result.SetErrorWithMessage( "Could not resolve a translation at this time." );
                  }
                  else if( IsBelowMaxLength( text ) || endpoint == TranslationManager.PassthroughEndpoint )
                  {
                     CreateTranslationJobFor( endpoint, null, textKey, result, context, true, true, true, isTranslatable, allowFallback, untranslatedTextContext );
                  }
                  else if( Settings.OutputTooLongText )
                  {
                     CreateTranslationJobFor( TranslationManager.PassthroughEndpoint, null, textKey, result, context, true, true, true, isTranslatable, false, untranslatedTextContext );
                  }
                  else
                  {
                     result.SetErrorWithMessage( "The provided text exceeds the maximum length." );
                  }

                  return result;
               }
            }
            else
            {
               result.SetErrorWithMessage( $"The provided text ({text}) cannot be translated." );
            }
         }

         return result;
      }

      private string TranslateByParserResult(
         TranslationEndpointManager endpoint,
         ParserResult result,
         int scope,
         InternalTranslationResult translationResult,
         bool allowStartTranslateImmediate,
         bool isGlobal,
         bool allowFallback,
         ParserTranslationContext parentContext )
      {
         var allowPartial = endpoint == null && result.AllowPartialTranslation;
         var context = new ParserTranslationContext( null, endpoint, translationResult, result, parentContext );
         if( isGlobal )
         {
            // attempt to lookup ALL strings immediately; return result if possible; queue operations
            var translation = result.GetTranslationFromParts( untranslatedTextInfoPart =>
            {
               var untranslatedTextPart = untranslatedTextInfoPart.UntranslatedText;
               if( !untranslatedTextPart.IsNullOrWhiteSpace() && TextCache.IsTranslatable( untranslatedTextPart, true, scope ) )
               {
                  var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
                  if( TextCache.IsTranslatable( textKey.TemplatedOriginal_Text, true, scope ) )
                  {
                     string partTranslation;
                     if( TextCache.TryGetTranslation( textKey, false, true, scope, out partTranslation ) )
                     {
                        return textKey.Untemplate( partTranslation ) ?? string.Empty;
                     }
                     else if( ( !Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text ) && !textKey.IsTemplated ) || textKey.IsOnlyTemplate )
                     {
                        return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                     }
                     else
                     {
                        // incomplete, must start job
                        var partResult = Translate( untranslatedTextPart, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate, allowFallback, untranslatedTextInfoPart );
                        if( partResult.TranslatedText != null )
                        {
                           return textKey.Untemplate( partResult.TranslatedText ) ?? string.Empty;
                        }
                        else if( allowPartial )
                        {
                           return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                        }
                     }
                  }
                  else
                  {
                     // the template itself does not require a translation, which means the untranslated template equals the translated text
                     return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                  }
               }
               else
               {
                  // the value will do
                  return untranslatedTextPart ?? string.Empty;
               }

               return null;
            } );


            //if( translation != null && context.CachedCombinedResult() )
            //{
            //   TextCache.AddTranslationToCache( context.Result.OriginalText, translation, result.PersistCombinedResult, TranslationType.Full, TranslationScopes.None );
            //   context.Endpoint.AddTranslationToCache( context.Result.OriginalText, translation );
            //}

            return translation;
         }
         else
         {
            if( endpoint == null )
            {
               return null;
            }

            var translation = result.GetTranslationFromParts( untranslatedTextInfoPart =>
            {
               var untranslatedTextPart = untranslatedTextInfoPart.UntranslatedText;
               if( !untranslatedTextPart.IsNullOrWhiteSpace() && endpoint.IsTranslatable( untranslatedTextPart ) )
               {
                  var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
                  if( endpoint.IsTranslatable( textKey.TemplatedOriginal_Text ) )
                  {
                     string partTranslation;
                     if( endpoint.TryGetTranslation( textKey, out partTranslation ) )
                     {
                        return textKey.Untemplate( partTranslation ) ?? string.Empty;
                     }
                     else if( ( !Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text ) && !textKey.IsTemplated ) || textKey.IsOnlyTemplate )
                     {
                        return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                     }
                     else
                     {
                        // incomplete, must start job
                        var partResult = Translate( untranslatedTextPart, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate, allowFallback, untranslatedTextInfoPart );
                        if( partResult.TranslatedText != null )
                        {
                           return textKey.Untemplate( partResult.TranslatedText ) ?? string.Empty;
                        }
                        else if( allowPartial )
                        {
                           return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                        }
                     }
                  }
                  else
                  {
                     // the template itself does not require a translation, which means the untranslated template equals the translated text
                     return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                  }
               }
               else
               {
                  // the value will do
                  return untranslatedTextPart ?? string.Empty;
               }

               return null;
            } );


            //if( translation != null && context.CachedCombinedResult() )
            //{
            //   context.Endpoint.AddTranslationToCache( context.Result.OriginalText, translation );
            //}

            return translation;
         }
      }

      private bool CheckAndFixRedirected( object ui, string text, TextTranslationInfo info )
      {
         if( Settings.RedirectedResourceDetectionStrategy == RedirectedResourceDetection.None || !LanguageHelper.HasRedirectedTexts ) return false;

         if( info != null && _textHooksEnabled && !info.IsCurrentlySettingText )
         {
            if( text.IsRedirected() )
            {
               info.IsCurrentlySettingText = true;
               _textHooksEnabled = false;

               try
               {
                  var fixedText = text.FixRedirected();
                  info.RedirectedTranslations.Add( fixedText );

                  ui.SetText( fixedText, info );

                  return true;
               }
               finally
               {
                  _textHooksEnabled = true;
                  info.IsCurrentlySettingText = false;
               }
            }
            else if( info.RedirectedTranslations.Contains( text ) )
            {
               return true;
            }
         }

         return false;
      }

      /// <summary>
      /// Translates the string of a UI  text or queues it up to be translated
      /// by the HTTP translation service.
      /// </summary>
      private string TranslateOrQueueWebJobImmediate(
         object ui, string text, int scope, TextTranslationInfo info,
         bool allowStabilizationOnTextComponent, bool ignoreComponentState,
         bool allowStartTranslationImmediate, bool allowStartTranslationLater,
         IReadOnlyTextTranslationCache tc, UntranslatedTextInfo untranslatedTextContext,
         ParserTranslationContext context )
      {
         if( info != null && info.IsCurrentlySettingText )
            return null;

         text = text ?? ui.GetText( info );

         // this only happens if the game sets the text of a component to our translation
         if( info?.IsTranslated == true && text == info.TranslatedText )
         {
            return null;
         }

         bool shouldIgnore = false;
         if( info != null )
         {
            info.Reset( text );
            shouldIgnore = info.ShouldIgnore;
         }

         if( scope == TranslationScopes.None && context == null )
         {
            scope = TranslationScopeHelper.GetScope( ui );
         }

         if( text.IsNullOrWhiteSpace() )
            return null;

         if( context == null )
         {
            if( CheckAndFixRedirected( ui, text, info ) )
               return null;
         }

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !shouldIgnore && ( ignoreComponentState || ui.IsComponentActive() ) )
         {
            if( context == null && info != null )
            {
               var callbackContext = InvokeOnTranslatingCallback( ui, text, info );
               if( callbackContext != null )
               {
                  switch( callbackContext.Behaviour )
                  {
                     case ComponentTranslationBehaviour.OverrideTranslatedText:
                        var translatedText = callbackContext.OverriddenTranslatedText;
                        if( translatedText != null )
                        {
                           SetTranslatedText( ui, translatedText, text, info );
                        }
                        return translatedText;
                     case ComponentTranslationBehaviour.IgnoreComponent:
                        return null;
                     case ComponentTranslationBehaviour.Default:
                     default:
                        break;
                  }
               }
            }

            if( !tc.IsTranslatable( text, false, scope ) )
               return null;

            var isSpammer = ui.IsSpammingComponent();
            if( isSpammer && !IsBelowMaxLength( text ) ) return null; // avoid templating long strings every frame for IMGUI, important!

            // potentially shortcircuit if templated is a translation
            var textKey = GetCacheKey( text, isSpammer );

            // potentially shortcircuit if fully templated
            if( ( textKey.IsTemplated && !tc.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
               if( context == null )
               {
                  SetTranslatedText( ui, untemplatedTranslation, text, info );
               }
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( tc.TryGetTranslation( textKey, !isSpammer, false, scope, out translation ) )
            {
               var untemplatedTranslation = textKey.Untemplate( translation );
               if( context == null ) // never set text if operation is contextualized (only a part translation)
               {
                  var isPartial = tc.IsPartial( textKey.TemplatedOriginal_Text, scope );
                  SetTranslatedText( ui, untemplatedTranslation, !isPartial ? text : null, info );
               }
               return untemplatedTranslation;
            }
            else
            {
               var isTranslatable = LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text );
               if( !isSpammer )
               {
                  if( context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion )
                  {
                     if( UnityTextParsers.GameLogTextParser.CanApply( ui ) && context == null ) // only at the first layer!
                     {
                        var result = UnityTextParsers.GameLogTextParser.Parse( text, scope, tc );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, isTranslatable || Settings.OutputUntranslatableText, tc, context );
                           if( translation != null )
                           {
                              if( context == null )
                              {
                                 SetTranslatedText( ui, translation, null, info );
                              }
                              return translation;
                           }

                           if( context != null )
                           {
                              return null;
                           }
                        }
                     }
                     if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) )
                     {
                        var result = UnityTextParsers.RegexSplittingTextParser.Parse( text, scope, tc );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, isTranslatable || Settings.OutputUntranslatableText, tc, context );
                           if( translation != null )
                           {
                              if( context == null )
                              {
                                 SetTranslatedText( ui, translation, text, info );
                              }
                              return translation;
                           }

                           if( context != null )
                           {
                              return null;
                           }
                        }
                     }
                     if( UnityTextParsers.RichTextParser.CanApply( ui ) && !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                     {
                        var result = UnityTextParsers.RichTextParser.Parse( text, scope );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, isTranslatable || Settings.OutputUntranslatableText, tc, context );
                           if( translation != null )
                           {
                              if( context == null )
                              {
                                 SetTranslatedText( ui, translation, text, info );
                              }
                              return translation;
                           }

                           if( context != null )
                           {
                              return null;
                           }
                        }
                     }
                  }
                  else
                  {
                     if( Settings.MaxTextParserRecursion != 1 )
                     {
                        XuaLogger.AutoTranslator.Warn( "Attempted to exceed maximum allowed levels of text parsing recursion!" );
                     }
                  }
               }

               if( !isTranslatable && !Settings.OutputUntranslatableText && ( !textKey.IsTemplated || isSpammer ) )
               {
                  if( _isInTranslatedMode && !isSpammer )
                     TranslationHelper.DisplayTranslationInfo( text, null );

                  // FIXME: SET TEXT? Set it to the same? Only impact is RESIZE behaviour!
                  return text;
               }
               else
               {
                  TranslateByEndpoint( ui, text, textKey, isTranslatable, isSpammer, scope, info, allowStabilizationOnTextComponent, allowStartTranslationImmediate, allowStartTranslationLater, tc, untranslatedTextContext, context );
               }
            }
         }

         return null;
      }

      private void TranslateByEndpoint(
         object ui, string text, UntranslatedText textKey, bool isTranslatable,
         bool isSpammer, int scope, TextTranslationInfo info, bool allowStabilizationOnTextComponent,
         bool allowStartTranslationImmediate, bool allowStartTranslationLater,
         IReadOnlyTextTranslationCache tc, UntranslatedTextInfo untranslatedTextContext,
         ParserTranslationContext context )
      {
         string translation;

         var endpoint = GetTranslationEndpoint( context, true );
         if( allowStartTranslationImmediate )
         {
            // Lets try not to spam a service that might not be there...
            if( endpoint != null )
            {
               if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
               {
                  if( IsBelowMaxLength( text ) || endpoint == TranslationManager.PassthroughEndpoint )
                  {
                     CreateTranslationJobFor( endpoint, ui, textKey, null, context, true, true, true, isTranslatable, true, untranslatedTextContext );
                  }
                  else if( Settings.OutputTooLongText )
                  {
                     CreateTranslationJobFor( TranslationManager.PassthroughEndpoint, ui, textKey, null, context, true, true, true, isTranslatable, false, untranslatedTextContext );
                  }
               }
            }
         }
         else if( allowStabilizationOnTextComponent && allowStartTranslationLater && context == null ) // never stabilize a text that is contextualized or that does not support stabilization
         {
            // if we dont know what text to translate it to, we need to figure it out.
            // this might take a while, so add the UI text component to the ongoing operations
            // list, so we dont start multiple operations for it, as its text might be constantly
            // changing.
            info.IsStabilizingText = true;

            // start a coroutine, that will execute once the string of the UI text has stopped
            // changing. For all texts except 'story' texts, this will add a delay for exactly 
            // 0.5s to the translation. This is barely noticable.
            //
            // on the other hand, for 'story' texts, this will take the time that it takes
            // for the text to stop 'scrolling' in.
            try
            {

               Action<string> onTextStabilized = stabilizedText =>
               {
                  info.IsStabilizingText = false;

                  text = stabilizedText;

                  // This means it has been translated through "ImmediateTranslate" function
                  if( info?.IsTranslated == true ) return;

                  info?.Reset( text );

                  if( stabilizedText.IsNullOrWhiteSpace() )
                     return;

                  if( context == null )
                  {
                     if( CheckAndFixRedirected( ui, text, info ) )
                        return;
                  }

                  if( tc.IsTranslatable( stabilizedText, false, scope ) )
                  {
                     // potentially shortcircuit if templated is a translation
                     var stabilizedTextKey = GetCacheKey( stabilizedText, false );

                     // potentially shortcircuit if fully templated
                     if( ( stabilizedTextKey.IsTemplated && !tc.IsTranslatable( stabilizedTextKey.TemplatedOriginal_Text, false, scope ) ) || stabilizedTextKey.IsOnlyTemplate )
                     {
                        var untemplatedTranslation = stabilizedTextKey.Untemplate( stabilizedTextKey.TemplatedOriginal_Text );
                        SetTranslatedText( ui, untemplatedTranslation, text, info );
                        return;
                     }

                     // once the text has stabilized, attempt to look it up
                     if( tc.TryGetTranslation( stabilizedTextKey, true, false, scope, out translation ) )
                     {
                        var isPartial = tc.IsPartial( stabilizedTextKey.TemplatedOriginal_Text, scope );
                        SetTranslatedText( ui, stabilizedTextKey.Untemplate( translation ), !isPartial ? text : null, info );
                     }
                     else
                     {
                        // PREMISE: context should ALWAYS be null inside this method! That means we initialize the first layer of text parser recursion from here

                        var isStabilizedTranslatable = LanguageHelper.IsTranslatable( stabilizedTextKey.TemplatedOriginal_Text );
                        if( UnityTextParsers.GameLogTextParser.CanApply( ui ) && context == null )
                        {
                           var result = UnityTextParsers.GameLogTextParser.Parse( stabilizedText, scope, tc );
                           if( result != null )
                           {
                              var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, isStabilizedTranslatable || Settings.OutputUntranslatableText, tc, context );
                              if( translatedText != null && context == null )
                              {
                                 SetTranslatedText( ui, translatedText, null, info );
                              }
                              return;
                           }
                        }
                        if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) )
                        {
                           var result = UnityTextParsers.RegexSplittingTextParser.Parse( stabilizedText, scope, tc );
                           if( result != null )
                           {
                              var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, isStabilizedTranslatable || Settings.OutputUntranslatableText, tc, context );
                              if( translatedText != null && context == null )
                              {
                                 SetTranslatedText( ui, translatedText, text, info );
                              }
                              return;
                           }
                        }
                        if( UnityTextParsers.RichTextParser.CanApply( ui ) && !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                        {
                           var result = UnityTextParsers.RichTextParser.Parse( stabilizedText, scope );
                           if( result != null )
                           {
                              var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, isStabilizedTranslatable || Settings.OutputUntranslatableText, tc, context );
                              if( translatedText != null && context == null )
                              {
                                 SetTranslatedText( ui, translatedText, text, info );
                              }
                              return;
                           }
                        }

                        if( !isStabilizedTranslatable && !Settings.OutputUntranslatableText && !stabilizedTextKey.IsTemplated )
                        {
                           if( _isInTranslatedMode && !isSpammer )
                              TranslationHelper.DisplayTranslationInfo( text, null );

                           // FIXME: SET TEXT? Set it to the same? Only impact is RESIZE behaviour!
                        }
                        else
                        {
                           // Lets try not to spam a service that might not be there...
                           if( endpoint != null )
                           {
                              if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                              {
                                 if( IsBelowMaxLength( text ) || endpoint == TranslationManager.PassthroughEndpoint )
                                 {
                                    CreateTranslationJobFor( endpoint, ui, stabilizedTextKey, null, context, true, true, true, isStabilizedTranslatable, true, untranslatedTextContext );
                                 }
                                 else if( Settings.OutputTooLongText )
                                 {
                                    CreateTranslationJobFor( TranslationManager.PassthroughEndpoint, ui, stabilizedTextKey, null, context, true, true, true, isStabilizedTranslatable, false, untranslatedTextContext );
                                 }
                              }
                           }
                        }
                     }
                  }
               };

               if( endpoint?.Endpoint is PassthroughTranslateEndpoint )
               {
                  onTextStabilized( text );
               }
               else
               {
                  var delay = endpoint?.TranslationDelay ?? Settings.DefaultTranslationDelay;
                  var retries = endpoint?.MaxRetries ?? Settings.DefaultMaxRetries;

                  CoroutineHelper.Start(
                     WaitForTextStablization(
                        ui: ui,
                        info: info,
                        delay: delay, // 0.9 second to prevent '1 second tickers' from getting translated
                        maxTries: retries, // 60 tries, about 1 minute
                        currentTries: 0,
                        onMaxTriesExceeded: () =>
                        {
                           info.IsStabilizingText = false;
                        },
                        onTextStabilized: onTextStabilized ) );
               }
            }
            catch( Exception )
            {
               info.IsStabilizingText = false;
            }
         }
         else if( allowStartTranslationLater ) // this should only be called for immediate UI translation, theoreticalaly
         {
            var delay = endpoint?.TranslationDelay ?? Settings.DefaultTranslationDelay;

            CoroutineHelper.Start(
               WaitForTextStablization(
                  textKey: textKey,
                  delay: delay,
                  onTextStabilized: () =>
                  {
                     // if we already have translation loaded in our _translatios dictionary, simply load it and set text
                     string translation;
                     if( tc.TryGetTranslation( textKey, true, false, scope, out translation ) )
                     {
                        // no need to do anything !
                     }
                     else
                     {
                        // Lets try not to spam a service that might not be there...
                        var endpoint = GetTranslationEndpoint( context, true );
                        if( endpoint != null )
                        {
                           // once the text has stabilized, attempt to look it up
                           if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                           {
                              if( IsBelowMaxLength( text ) || endpoint == TranslationManager.PassthroughEndpoint )
                              {
                                 CreateTranslationJobFor( endpoint, ui, textKey, null, context, true, true, true, isTranslatable, true, untranslatedTextContext );
                              }
                              else if( Settings.OutputTooLongText )
                              {
                                 CreateTranslationJobFor( TranslationManager.PassthroughEndpoint, ui, textKey, null, context, true, true, true, isTranslatable, false, untranslatedTextContext );
                              }
                           }
                        }
                     }
                  } ) );
         }
      }

      private TranslationEndpointManager GetTranslationEndpoint( ParserTranslationContext context, bool allowFallback )
      {
         var endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
         if( allowFallback && endpoint != null && endpoint.HasFailedDueToConsecutiveErrors && TranslationManager.IsFallbackAvailableFor( endpoint ) )
         {
            XuaLogger.AutoTranslator.Warn( "Falling back to fallback translator in order to perform translation." );

            endpoint = TranslationManager.FallbackEndpoint;
         }
         return endpoint;
      }

      private string TranslateOrQueueWebJobImmediateByParserResult( object ui, ParserResult result, int scope, bool allowStartTranslationImmediate, bool allowStartTranslationLater, bool allowImmediateCaching, IReadOnlyTextTranslationCache tc, ParserTranslationContext parentContext )
      {
         // attempt to lookup ALL strings immediately; return result if possible; queue operations
         var allowPartial = TranslationManager.CurrentEndpoint == null && result.AllowPartialTranslation;
         var context = new ParserTranslationContext( ui, TranslationManager.CurrentEndpoint, null, result, parentContext );

         var translation = result.GetTranslationFromParts( untranslatedTextInfoPart =>
         {
            var untranslatedTextPart = untranslatedTextInfoPart.UntranslatedText;
            if( !untranslatedTextPart.IsNullOrWhiteSpace() && tc.IsTranslatable( untranslatedTextPart, true, scope ) )
            {
               var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
               if( tc.IsTranslatable( textKey.TemplatedOriginal_Text, true, scope ) )
               {
#warning Why is using a regex here not allowed?
                  string partTranslation;
                  if( tc.TryGetTranslation( textKey, false, true, scope, out partTranslation ) )
                  {
                     return textKey.Untemplate( partTranslation ) ?? string.Empty;
                  }
                  else if( ( !Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text ) && !textKey.IsTemplated ) || textKey.IsOnlyTemplate )
                  {
                     return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                  }
                  else
                  {
                     partTranslation = TranslateOrQueueWebJobImmediate( ui, untranslatedTextPart, scope, null, false, true, allowStartTranslationImmediate, allowStartTranslationLater, tc, untranslatedTextInfoPart, context );
                     if( partTranslation != null )
                     {
                        return textKey.Untemplate( partTranslation ) ?? string.Empty;
                     }
                     else if( allowPartial )
                     {
                        return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
                     }
                  }
               }
               else
               {
                  // the template itself does not require a translation, which means the untranslated template equals the translated text
                  return textKey.Untemplate( textKey.TemplatedOriginal_Text ) ?? string.Empty;
               }
            }
            else
            {
               // the value will do
               return untranslatedTextPart ?? string.Empty;
            }

            return null;
         } );

         try
         {
            if( Settings.CacheParsedTranslations && allowImmediateCaching && parentContext == null && translation != null && context.CachedCombinedResult() )
            {
               if( !Settings.EnableSilentMode )
                  XuaLogger.AutoTranslator.Debug( $"Parsed translation cached: '{context.Result.OriginalText}' => '{translation}'" );

               TextCache.AddTranslationToCache( context.Result.OriginalText, translation, false, TranslationType.Full, scope );
               context.Endpoint.AddTranslationToCache( context.Result.OriginalText, translation );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while attempting to cache a parsed translation." );
         }

         return translation;
      }

      /// <summary>
      /// Utility method that allows me to wait to call an action, until
      /// the text has stopped changing. This is important for 'story'
      /// mode text, which 'scrolls' into place slowly.
      /// </summary>
      private IEnumerator WaitForTextStablization( object ui, TextTranslationInfo info, float delay, int maxTries, int currentTries, Action<string> onTextStabilized, Action onMaxTriesExceeded )
      {
         yield return null; // wait a single frame to allow any external plugins to complete their hooking logic

         bool succeeded = false;
         while( currentTries < maxTries ) // shortcircuit
         {
            var beforeText = ui.GetText( info );

            var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( delay );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               float start = Time.realtimeSinceStartup;
               var end = start + delay;
               while( Time.realtimeSinceStartup < end )
               {
                  yield return null;
               }
            }

            var afterText = ui.GetText( info );

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
      /// Utility method that allows waiating to call an action, until
      /// the text has stopped changing. This is important for 'story'
      /// mode text, which 'scrolls' into place slowly. This version is
      /// for global text, where the component cannot tell us if the text
      /// has changed itself.
      /// </summary>
      private IEnumerator WaitForTextStablization( UntranslatedText textKey, float delay, Action onTextStabilized, Action onFailed = null )
      {
         var text = textKey.TemplatedOriginal_Text_FullyTrimmed;

         if( !_immediatelyTranslating.Contains( text ) )
         {
            _immediatelyTranslating.Add( text );
            try
            {
               var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( delay );
               if( instruction != null )
               {
                  yield return instruction;
               }
               else
               {
                  float start = Time.realtimeSinceStartup;
                  var end = start + delay;
                  while( Time.realtimeSinceStartup < end )
                  {
                     yield return null;
                  }
               }

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
               XuaLogger.AutoTranslator.Error( e, "An unexpected error occurred during plugin initialization." );
            }
         }
      }

      private TextTranslationCache GetTextCacheFor( string assemblyName )
      {
         if( !PluginTextCaches.TryGetValue( assemblyName, out var cache ) )
         {
            cache = new TextTranslationCache( assemblyName );
            PluginTextCaches[ assemblyName ] = cache;
         }
         return cache;
      }

      void ITranslationRegistry.RegisterPluginSpecificTranslations( Assembly assembly, StreamTranslationPackage package )
      {
         var cache = GetTextCacheFor( assembly.GetName().Name );
         cache.RegisterPackage( package );
         cache.LoadTranslationFiles();

         HooksSetup.InstallComponentBasedPluginTranslationHooks();
         HooksSetup.InstallIMGUIBasedPluginTranslationHooks( assembly, true );
      }

      void ITranslationRegistry.RegisterPluginSpecificTranslations( Assembly assembly, KeyValuePairTranslationPackage package )
      {
         var cache = GetTextCacheFor( assembly.GetName().Name );
         cache.RegisterPackage( package );
         cache.LoadTranslationFiles();

         HooksSetup.InstallComponentBasedPluginTranslationHooks();
         HooksSetup.InstallIMGUIBasedPluginTranslationHooks( assembly, true );
      }

      void ITranslationRegistry.EnablePluginTranslationFallback( Assembly assembly )
      {
         var cache = GetTextCacheFor( assembly.GetName().Name );
         cache.AllowFallback = true;
         cache.DefaultAllowFallback = true;

         HooksSetup.InstallComponentBasedPluginTranslationHooks();
         HooksSetup.InstallIMGUIBasedPluginTranslationHooks( assembly, true );
      }

      IEnumerator HookLoadedPlugins()
      {
         yield return null;

         if( PluginTextCaches.Count == 0 )
         {
            XuaLogger.AutoTranslator.Info( "Skipping plugin scan because no plugin-specific translations has been registered." );
            yield break;
         }
         else
         {
            XuaLogger.AutoTranslator.Info( "Scanning for plugins to hook for translations..." );
         }

         var gameDataPath = Application.dataPath.UseCorrectDirectorySeparators();
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();
         foreach( var assembly in assemblies )
         {
            try
            {
               if( assembly.FullName.StartsWith( "XUnity" ) )
                  continue;

               if( assembly.ManifestModule.GetType().FullName.Contains( "Emit" ) )
                  continue;

               var location = assembly.Location.UseCorrectDirectorySeparators();
               if( !location.StartsWith( gameDataPath, StringComparison.OrdinalIgnoreCase ) && PluginTextCaches.TryGetValue( assembly.GetName().Name, out _ ) )
               {
                  HooksSetup.InstallIMGUIBasedPluginTranslationHooks( assembly, false );
               }
            }
            catch( Exception e1 )
            {
               XuaLogger.AutoTranslator.Warn( e1, "An error occurred while scanning assembly: " + assembly.FullName );
            }
         }
      }

      public void Start()
      {
         if( !_started )
         {
            _started = true;

            // Ensure awake is called. This will not be called when the plugin is loaded by MelonLoader
            Awake();

            try
            {
               CoroutineHelper.Start( HookLoadedPlugins() );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An unexpected error occurred during plugin start." );
            }
         }
      }

      private static bool _inputSupported = true;
      private void HandleInputSafe()
      {
         if( _inputSupported )
         {
            try
            {
               HandleInput();
            }
            catch( TypeLoadException e )
            {
               _inputSupported = false;
               XuaLogger.AutoTranslator.Warn( e, "Input API is not available!" );
            }
         }
      }

      private void HandleInput()
      {
         var isAltPressed = Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt );
         if( isAltPressed )
         {
            var isCtrlPressed = Input.GetKey( KeyCode.LeftControl );

            if( Input.GetKeyDown( KeyCode.T ) )
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
               ToggleTranslationAggregator();
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
               else if( Input.GetKeyDown( KeyCode.Keypad7 ) )
               {
                  PrintSceneInformation();
               }
               else if( Input.GetKeyDown( KeyCode.Keypad6 ) )
               {
                  PrintObjects();
               }
            }
         }
      }

      public void Update()
      {
         try
         {
            var frameCount = Time.frameCount;

            TranslationManager.Update();

            if( frameCount % 36000 == 0 && CachedKeys.Count > 0 )
            {
               CachedKeys.Clear();
            }

            if( UnityFeatures.SupportsClipboard )
            {
               CopyToClipboard();
            }

            if( !Settings.IsShutdown )
            {
               EnableAutoTranslator();
               SpamChecker.Update();

               UpdateSpriteRenderers();
               IncrementBatchOperations();
               KickoffTranslations();

               TranslationAggregatorWindow?.Update();
            }

            if( _translationReloadRequest )
            {
               _translationReloadRequest = false;
               ReloadTranslations();
            }

            // perform this check every 100 frames!
            if( frameCount % 100 == 0
               && TranslationManager.OngoingTranslations == 0
               && TranslationManager.UnstartedTranslations == 0 )
            {
               ConnectionTrackingWebClient.CheckServicePoints();
            }

            HandleInputSafe();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred in Update callback. " );
         }
      }

      private void PrintSceneInformation()
      {
         var sli = new SceneLoadInformation();

         XuaLogger.AutoTranslator.Info( "Active Scene: " + sli.ActiveScene.Name + " (" + sli.ActiveScene.Id + ")" );

         XuaLogger.AutoTranslator.Info( "Loaded Scenes:" );
         for( int i = 0; i < sli.LoadedScenes.Count; i++ )
         {
            var si = sli.LoadedScenes[ i ];
            XuaLogger.AutoTranslator.Info( i + ": " + si.Name + " (" + si.Id + ")" );
         }
      }

      public void OnGUI()
      {
         // initialize ui
         InitializeGUI();

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
                  XuaLogger.AutoTranslator.Error( e, "An error occurred in XUnity.AutoTranslator UI. Disabling the UI." );

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
                  XuaLogger.AutoTranslator.Error( e, "An error occurred in Translation Aggregator UI. Disabling the UI." );

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
                  XuaLogger.AutoTranslator.Error( e, "An error occurred in Translation Aggregator Options UI. Disabling the UI." );

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

         XuaLogger.AutoTranslator.Info( "Rebooted Auto Translator." );
      }

      private void KickoffTranslations()
      {
         TranslationManager.KickoffTranslations();
      }

      private void OnJobFailed( TranslationJob job )
      {
         foreach( var translationResult in job.TranslationResults )
         {
            translationResult.Item.SetErrorWithMessage( job.ErrorMessage ?? "Unknown error." );
         }

         foreach( var context in job.Contexts )
         {
            var translationResult = context.TranslationResult;
            if( translationResult != null )
            {
               // are all jobs within this context completed? If so, we can set the text
               if( context.Jobs.Any( x => x.State == TranslationJobState.Failed ) )
               {
                  translationResult.SetErrorWithMessage( job.ErrorMessage ?? "Unknown error." );
               }
            }
         }
      }

      private void OnJobCompleted( TranslationJob job )
      {
         var translationType = job.TranslationType;
         var shouldPersist = job.ShouldPersistTranslation;

         if( job.Key.IsTemplated && Settings.GenerateStaticSubstitutionTranslations )
         {
            bool addedTemplatedTranslation = false;

            // make sure case of spamming component is handled (templated numbers!)
            if( job.Key.IsFromSpammingComponent )
            {
               addedTemplatedTranslation = true;

               if( job.SaveResultGlobally )
               {
                  TextCache.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersist, translationType, TranslationScopes.None );
               }
               job.Endpoint.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText );
            }

            foreach( var keyAndComponent in job.Components )
            {
               var key = keyAndComponent.Key;

               if( key.IsFromSpammingComponent ) // not theoretically possible because spamming components are not added as a component
               {
                  if( addedTemplatedTranslation ) continue; // no reason to do this more than once

                  addedTemplatedTranslation = true;
                  if( job.SaveResultGlobally )
                  {
                     TextCache.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersist, translationType, TranslationScopes.None );
                  }
                  job.Endpoint.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText );
               }
               else
               {
                  var untemplatedTranslatableText = key.Untemplate( key.TemplatedOriginal_Text );
                  var untemplatedTranslatedText = key.Untemplate( job.TranslatedText );
                  if( job.SaveResultGlobally )
                  {
                     TextCache.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText, shouldPersist, translationType, TranslationScopes.None );
                  }
                  job.Endpoint.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText );
               }
            }

            // also handle storing translations for 'TranslationResults'
            foreach( var translationResult in job.TranslationResults )
            {
               var key = translationResult.Key;

               if( key.IsFromSpammingComponent ) // not theoretically possible because translation results are never considered "spamming"
               {
                  if( addedTemplatedTranslation ) continue; // no reason to do this more than once

                  addedTemplatedTranslation = true;
                  if( job.SaveResultGlobally )
                  {
                     TextCache.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersist, translationType, TranslationScopes.None );
                  }
                  job.Endpoint.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText );
               }
               else
               {
                  var untemplatedTranslatableText = key.Untemplate( key.TemplatedOriginal_Text );
                  var untemplatedTranslatedText = key.Untemplate( job.TranslatedText );
                  if( job.SaveResultGlobally )
                  {
                     TextCache.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText, shouldPersist, translationType, TranslationScopes.None );
                  }
                  job.Endpoint.AddTranslationToCache( untemplatedTranslatableText, untemplatedTranslatedText );
               }
            }
         }
         else
         {
            if( job.SaveResultGlobally )
            {
               TextCache.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersist, translationType, TranslationScopes.None );
            }
            job.Endpoint.AddTranslationToCache( job.Key.TemplatedOriginal_Text, job.TranslatedText );
         }

         // fix translation results directly on jobs
         foreach( var translationResult in job.TranslationResults )
         {
            if( !string.IsNullOrEmpty( job.TranslatedText ) )
            {
               translationResult.Item.SetCompleted( translationResult.Key.Untemplate( job.TranslatedText ) );
            }
            else
            {
               translationResult.Item.SetEmptyResponse();
            }
         }

         foreach( var keyAndComponent in job.Components )
         {
            var component = keyAndComponent.Item;
            var key = keyAndComponent.Key;

            // update the original text, but only if it has not been chaanged already for some reason (could be other translator plugin or game itself)
            try
            {
               var info = component.GetOrCreateTextTranslationInfo();
               var text = component.GetText( info );
               if( text == key.Original_Text )
               {
                  if( !string.IsNullOrEmpty( job.TranslatedText ) )
                  {
                     SetTranslatedText( component, key.Untemplate( job.TranslatedText ), key.Original_Text, info );
                  }
               }
            }
            catch( NullReferenceException )
            {
               // might fail if compoent is no longer associated to game
            }
         }

         // handle each context
         foreach( var contextJobWasCompletedOn in job.Contexts )
         {
            // traverse up to the parent and check all jobs in this and children has succeeded!
            var context = contextJobWasCompletedOn.GetAncestorContext();

            // are all jobs within this context completed? If so, we can set the text
            if( context.HasAllJobsCompleted() )
            {
               try
               {
                  var info = context.Component.GetOrCreateTextTranslationInfo();
                  var text = context.Component.GetText( info );
                  var result = context.Result;

                  string translatedText;
                  if( context.TranslationResult == null )
                  {
                     translatedText = TranslateOrQueueWebJobImmediateByParserResult( context.Component, result, TranslationScopes.None, false, false, false, TextCache, null );
                  }
                  else
                  {
                     translatedText = TranslateByParserResult( context.Endpoint, result, TranslationScopes.None, null, false, context.TranslationResult.IsGlobal, false, null );
                  }

                  if( !string.IsNullOrEmpty( translatedText ) )
                  {
                     if( context.CachedCombinedResult() )
                     {
                        if( job.SaveResultGlobally )
                        {
                           TextCache.AddTranslationToCache( context.Result.OriginalText, translatedText, context.PersistCombinedResult(), TranslationType.Full, TranslationScopes.None );
                        }
                        job.Endpoint.AddTranslationToCache( context.Result.OriginalText, translatedText );
                     }

                     if( text == result.OriginalText )
                     {
                        SetTranslatedText( context.Component, translatedText, context.Result.OriginalText, info );
                     }

                     if( context.TranslationResult != null )
                     {
                        context.TranslationResult.SetCompleted( translatedText );
                     }
                  }
                  else
                  {
                     if( context.TranslationResult != null )
                     {
                        context.TranslationResult.SetEmptyResponse();
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
               XuaLogger.AutoTranslator.Error( $"Maximum translations ({Settings.MaxTranslationsBeforeShutdown}) per session reached. Shutting plugin down." );

               TranslationManager.ClearAllJobs();
            }
         }
      }

      private UntranslatedText GetCacheKey( string originalText, bool isFromSpammingComponent )
      {
         if( isFromSpammingComponent && CachedKeys.Count < Settings.MaxImguiKeyCacheCount )
         {
            if( !CachedKeys.TryGetValue( originalText, out var key ) )
            {
               key = new UntranslatedText( originalText, isFromSpammingComponent, !isFromSpammingComponent, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
               CachedKeys.Add( originalText, key );
            }

            return key;
         }

         return new UntranslatedText( originalText, isFromSpammingComponent, !isFromSpammingComponent, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
      }

      private void ReloadTranslations()
      {
         try
         {
            TextCache.PruneMainTranslationFile();

            LoadTranslations( true );

            var context = new TextureReloadContext();
            foreach( var kvp in ExtensionDataHelper.GetAllRegisteredObjects() )
            {
               var ui = kvp.Key;
               try
               {
                  var tti = kvp.Value as TextTranslationInfo;

                  if( tti.GetIsKnownTextComponent() && ui.IsComponentActive() )
                  {
                     var scope = TranslationScopeHelper.GetScope( ui );

                     if( tti != null && !tti.OriginalText.IsNullOrWhiteSpace() )
                     {
                        bool updated = false;
                        var originalText = tti.OriginalText;
                        try
                        {
                           var key = GetCacheKey( originalText, false );
                           if( TextCache.TryGetTranslation( key, true, false, scope, out string translatedText ) )
                           {
                              tti.UnresizeUI( ui );
                              SetTranslatedText( kvp.Key, key.Untemplate( translatedText ), null, tti ); // no need to untemplatize the translated text
                              updated = true;
                              continue;
                           }
                           else
                           {
                              if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
                              {
                                 var result = UnityTextParsers.GameLogTextParser.Parse( originalText, scope, TextCache );
                                 if( result != null )
                                 {
                                    var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, false, TextCache, null );
                                    if( translation != null )
                                    {
                                       tti.UnresizeUI( ui );
                                       SetTranslatedText( ui, translation, null, tti );
                                       updated = true;
                                       continue;
                                    }
                                 }
                              }
                              if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) )
                              {
                                 var result = UnityTextParsers.RegexSplittingTextParser.Parse( originalText, scope, TextCache );
                                 if( result != null )
                                 {
                                    var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, false, TextCache, null );
                                    if( translation != null )
                                    {
                                       tti.UnresizeUI( ui );
                                       SetTranslatedText( ui, translation, null, tti );
                                       updated = true;
                                       continue;
                                    }
                                 }
                              }
                              if( UnityTextParsers.RichTextParser.CanApply( ui ) )
                              {
                                 var result = UnityTextParsers.RichTextParser.Parse( originalText, scope );
                                 if( result != null )
                                 {
                                    var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, false, TextCache, null );
                                    if( translation != null )
                                    {
                                       tti.UnresizeUI( ui );
                                       SetTranslatedText( ui, translation, null, tti );
                                       updated = true;
                                       continue;
                                    }
                                 }
                              }
                           }
                        }
                        finally
                        {
                           if( !updated )
                           {
                              SetText( ui, tti.OriginalText, false, null, tti );
                              Hook_TextChanged( ui, false );
                           }
                        }
                     }
                  }

                  if( Settings.EnableTextureTranslation && ( ui is Texture2D || ui.IsKnownImageType() ) )
                  {
                     TranslateTexture( ui, context );
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ExtensionDataHelper.Remove( ui );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while reloading translations." );
         }
      }

      private void ToggleFont()
      {
         if( _hasValidOverrideFont )
         {
            _hasOverridenFont = !_hasOverridenFont;

            var objects = ExtensionDataHelper.GetAllRegisteredObjects();
            XuaLogger.AutoTranslator.Info( $"Toggling fonts of {objects.Count} objects." );

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
                        if( ui.IsComponentActive() )
                        {
                           tti.ChangeFont( ui );
                        }
                     }
                     catch( Exception )
                     {
                        // not super pretty, no...
                        ExtensionDataHelper.Remove( ui );
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
                  if( tti != null )
                  {
                     var ui = kvp.Key;
                     try
                     {
                        if( ui.IsComponentActive() )
                        {
                           tti.UnchangeFont( ui );
                        }
                     }
                     catch( Exception )
                     {
                        // not super pretty, no...
                        ExtensionDataHelper.Remove( ui );
                     }
                  }
               }
            }
         }
      }

      private void ToggleTranslation()
      {
         _isInTranslatedMode = !_isInTranslatedMode;
         var objects = ExtensionDataHelper.GetAllRegisteredObjects();

         XuaLogger.AutoTranslator.Info( $"Toggling translations of {objects.Count} objects." );

         if( _isInTranslatedMode )
         {
            // make sure we use the translated version of all texts
            foreach( var kvp in objects )
            {
               var ui = kvp.Key;
               try
               {
                  var tti = kvp.Value as TextTranslationInfo;
                  if( tti.GetIsKnownTextComponent() && ui.IsComponentActive() )
                  {
                     if( tti != null && tti.IsTranslated )
                     {
                        SetText( ui, tti.TranslatedText, true, null, tti );
                     }
                  }

                  if( Settings.EnableTextureTranslation && Settings.EnableTextureToggling && ( ui is Texture2D || ui.IsKnownImageType() ) )
                  {
                     TranslateTexture( ui, null );
                  }
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ExtensionDataHelper.Remove( ui );
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
                  var tti = kvp.Value as TextTranslationInfo;
                  if( tti.GetIsKnownTextComponent() && ui.IsComponentActive() )
                  {
                     if( tti != null && tti.IsTranslated )
                     {
                        SetText( ui, tti.OriginalText, false, null, tti );
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
                  ExtensionDataHelper.Remove( ui );
               }
            }
         }
      }

      private void CopyToClipboard()
      {
         if( Settings.CopyToClipboard
            && _textsToCopyToClipboardOrdered.Count > 0
            && ( _textsToCopyToClipboardOrdered.Count > 5 || Time.realtimeSinceStartup - _clipboardUpdated > Settings.ClipboardDebounceTime ) )
         {
            try
            {
               ClipboardHelper.CopyToClipboard( _textsToCopyToClipboardOrdered, Settings.MaxClipboardCopyCharacters );
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

         using( var stream = File.Open( Path.Combine( Paths.GameRoot, "hierarchy.txt" ), FileMode.Create ) )
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
            var textures = ComponentHelper.FindObjectsOfType<Texture2D>();
            foreach( var texture in textures )
            {
               Texture2D t = texture;
               Hook_ImageChanged( ref t, false );
            }

            //// scan all components and set dirty
            //var components = ComponentHelper.FindObjectsOfType<Component>();
            //foreach( var component in components )
            //{
            //   component.SetAllDirtyEx();
            //}
         }
      }

      private IEnumerable<GameObject> GetAllRoots()
      {
         var objects = ComponentHelper.FindObjectsOfType<GameObject>();
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
            var layer =
#if MANAGED
               LayerMask.LayerToName( obj.layer );
#elif IL2CPP
               obj.layer.ToString();
#endif
            var components = string.Join( ", ", obj.GetComponents<Component>().Select( x =>
            {
               string output = null;
               var type = x?.GetType();
               if( type != null )
               {
                  output = type.Name;

                  var info = x.GetOrCreateTextTranslationInfo();
                  var text = x.GetText( info );
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
               var textComponent = component.CreateDerivedProxyIfRequiredAndPossible();
               if( textComponent != null )
               {
                  Hook_TextChanged( textComponent, false );
               }

               if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
               {
                  if( component.IsKnownImageType() )
                  {
                     Texture2D _ = null;
                     Hook_ImageChangedOnComponent( component, ref _, false, false );
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

      internal bool IsTemporarilyDisabled()
      {
         return _temporarilyDisabled;
      }

      void OnDestroy()
      {
         try
         {
            RedirectedDirectory.Uninitialize();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while uninitializing redirected directory cache." );
         }

         try
         {
            TextCache.Dispose();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while disposing translation text cache." );
         }

         try
         {
            TextureCache.Dispose();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while disposing translation texture cache." );
         }

         foreach( var ce in TranslationManager.AllEndpoints )
         {
            try
            {
               if( ce.Endpoint is IDisposable disposable ) disposable.Dispose();
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while disposing endpoint." );
            }
         }
      }
   }
}
