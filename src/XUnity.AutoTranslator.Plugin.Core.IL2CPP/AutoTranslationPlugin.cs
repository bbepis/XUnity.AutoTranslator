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
using XUnity.Common.Extensions;
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.Common.Shims;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.AutoTranslator.Plugin.Shims;
using UnityEngine.Events;
using MelonLoader;
using XUnity.AutoTranslator.Plugin.Core;

namespace XUnity.AutoTranslator.Plugin.Core
{

   /// <summary>
   /// Main plugin class for the AutoTranslator.
   /// </summary>
   public class AutoTranslationPlugin : IInternalTranslator, IMonoBehaviour
   {
      /// <summary>
      /// Allow the instance to be accessed statically, as only one will exist.
      /// </summary>
      internal static AutoTranslationPlugin Current;

      internal TranslationManager TranslationManager;
      internal TextTranslationCache TextCache;
      internal TextureTranslationCache TextureCache;
      internal UIResizeCache ResizeCache;
      internal SpamChecker SpamChecker;

      /// <summary>
      /// Texts currently being scheduled for translation by 'immediate' components.
      /// </summary>
      private HashSet<string> _immediatelyTranslating = new HashSet<string>();

      private float _batchOperationSecondCounter = 0;

      private bool _isInTranslatedMode = true;
      private bool _textHooksEnabled = true;

      private bool _initialized = false;
      private bool _temporarilyDisabled = false;

      /// <summary>
      /// Initialized the plugin.
      /// </summary>
      public void Initialize()
      {
         // Setup 'singleton'
         Current = this;
         AutoTranslator.SetTranslator( this );

         // because we only use harmony/MonoMod through reflection due to
         // version compatibility issues, we call this method to
         // ensure that it is loaded before we attempt to obtain
         // various harmony/MonoMod classes through reflection
         HarmonyLoader.Load();

         // Setup configuration
         Settings.Configure();

         // Setup console, if enabled
         DebugConsole.Enable();

         // Setup hooks
         HooksSetup.InstallTextHooks();

         TextCache = new TextTranslationCache();
         TextureCache = new TextureTranslationCache();
         ResizeCache = new UIResizeCache();
         TranslationManager = new TranslationManager();
         TranslationManager.JobCompleted += OnJobCompleted;
         TranslationManager.JobFailed += OnJobFailed;
         TranslationManager.InitializeEndpoints();
         SpamChecker = new SpamChecker( TranslationManager );

         // WORKAROUND: Initialize text parsers with delegate indicating if text should be translated
         UnityTextParsers.Initialize();

         // validate configuration
         ValidateConfiguration();

         // load all translations from files
         LoadTranslations();

         XuaLogger.AutoTranslator.Info( $"Loaded XUnity.AutoTranslator into Unity [{Application.unityVersion}] game." );
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
         // check if language is supported
         if( !LanguageHelper.IsFromLanguageSupported( Settings.FromLanguage ) )
         {
            XuaLogger.AutoTranslator.Error( $"The plugin has been configured to use the 'FromLanguage={Settings.FromLanguage}'. This language is not supported. Shutting plugin down." );

            Settings.IsShutdown = true;
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
            }

            Settings.SetEndpoint( TranslationManager.CurrentEndpoint?.Endpoint.Id );

            XuaLogger.AutoTranslator.Info( $"Set translator endpoint to '{TranslationManager.CurrentEndpoint?.Endpoint.Id}'." );
         }
      }

      /// <summary>
      /// Loads the translations found in Translation.{lang}.txt
      /// </summary>
      private void LoadTranslations()
      {
         ResizeCache.LoadResizeCommandsInFiles();
         TextCache.LoadTranslationFiles();
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
         bool isTranslatable )
      {
         var added = endpoint.EnqueueTranslation( ui, key, translationResult, context, checkOtherEndpoints, saveResultGlobally, isTranslatable );
         if( added != null && isTranslatable && checkSpam && !( endpoint.Endpoint is PassthroughTranslateEndpoint ) )
         {
            SpamChecker.PerformChecks( key.TemplatedOriginal_Text_FullyTrimmed );
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

      internal string Hook_TextChanged_WithResult( ITextComponent ui, string text, bool onEnable )
      {
         string result = null;
         if( _textHooksEnabled && !_temporarilyDisabled )
         {
            var info = ui.GetOrCreateTextTranslationInfo();

            result = TranslateOrQueueWebJob( ui, text, false, info );
         }

         if( _isInTranslatedMode )
         {
            return result;
         }

         return null;
      }

      internal void Hook_TextChanged( ITextComponent ui, bool onEnable )
      {
         //XuaLogger.AutoTranslator.Error( ui.GetHashCode() + ": event!" );

         if( _textHooksEnabled && !_temporarilyDisabled )
         {
            var info = ui.GetOrCreateTextTranslationInfo();
            TranslateOrQueueWebJob( ui, null, false, info );
         }
      }

      internal void Hook_HandleComponent( ITextComponent ui )
      {
         try
         {
            if( Settings.ForceUIResizing )
            {
               var info = ui.GetOrCreateTextTranslationInfo();
               if( info?.IsCurrentlySettingText == false )
               {
                  info?.ResizeUI( ui, ResizeCache );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while handling the UI resize/font hooks." );
         }
      }

      internal void SetTranslatedText( ITextComponent ui, string translatedText, string originalText, TextTranslationInfo info )
      {
         info?.SetTranslatedText( translatedText );

         //XuaLogger.Current.Warn( "4: " + translatedText + " - " + ui.GetHashCode() );

         if( _isInTranslatedMode && !CallOrigin.ExpectsTextToBeReturned )
         {
            SetText( ui, translatedText, true, originalText, info );
         }
      }

      /// <summary>
      /// Sets the text of a UI  text, while ensuring this will not fire a text changed event.
      /// </summary>
      private void SetText( ITextComponent ui, string text, bool isTranslated, string originalText, TextTranslationInfo info )
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
                  var path = ui.GameObject.GetPath();
                  if( path != null )
                  {
                     var scope = TranslationScopeHelper.Instance.GetScope( ui );
                     XuaLogger.AutoTranslator.Info( $"Setting text on '{ui.GetType().FullName}' to '{text}'" );
                     XuaLogger.AutoTranslator.Info( "Path : " + path );
                     XuaLogger.AutoTranslator.Info( "Level: " + scope );
                  }
               }

               if( Settings.EnableUIResizing || Settings.ForceUIResizing )
               {
                  if( isTranslated || Settings.ForceUIResizing )
                  {
                     info?.ResizeUI( ui, ResizeCache );
                  }
                  else
                  {
                     info?.UnresizeUI( ui );
                  }
               }

               // NGUI only behaves if you set the text after the resize behaviour
               ui.SetText( text );

               //XuaLogger.AutoTranslator.Error( ui.GetHashCode() + ": " + ui.text );

               if( originalText != null && ui != null && !ui.IsSpammingComponent() )
               {
                  if( _isInTranslatedMode && isTranslated )
                     TranslationHelper.DisplayTranslationInfo( originalText, text );
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

      private string TranslateOrQueueWebJob( ITextComponent ui, string text, bool ignoreComponentState, TextTranslationInfo info )
      {
         if( info != null && info.IsStabilizingText == true )
         {
            return TranslateImmediate( ui, text, info, ignoreComponentState );
         }

         return TranslateOrQueueWebJobImmediate( ui, text, TranslationScopes.None, info, info.GetSupportsStabilization(), ignoreComponentState, false, true );
      }

      private static bool IsCurrentlySetting( TextTranslationInfo info )
      {
         if( info == null ) return false;

         return info.IsCurrentlySettingText;
      }

      private string TranslateImmediate( ITextComponent ui, string text, TextTranslationInfo info, bool ignoreComponentState )
      {
         text = text ?? ui.GetText();

         // Get the trimmed text
         string originalText = text;

         // this only happens if the game sets the text of a component to our translation
         if( info?.IsTranslated == true && originalText == info.TranslatedText )
         {
            return null;
         }

         info?.Reset( originalText );

         //XuaLogger.Current.Warn( "3: " + originalText + " - " + ui.GetHashCode() );

         var scope = TranslationScopeHelper.Instance.GetScope( ui );
         if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) && ui.ShouldTranslateTextComponent( ignoreComponentState ) && !IsCurrentlySetting( info ) )
         {
            //var textKey = new TranslationKey( ui, text, !ui.SupportsStabilization(), false );
            var isSpammer = ui.IsSpammingComponent();
            var textKey = GetCacheKey( ui, text, isSpammer );

            // potentially shortcircuit if fully templated
            if( ( textKey.IsTemplated && !TextCache.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
               var isPartial = TextCache.IsPartial( textKey.TemplatedOriginal_Text, scope );
               SetTranslatedText( ui, untemplatedTranslation, !isPartial ? originalText : null, info );
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our cache, simply load it and set text
            string translation;
            if( TextCache.TryGetTranslation( textKey, false, false, scope, out translation ) )
            {
               var untemplatedTranslation = textKey.Untemplate( translation );
               var isPartial = TextCache.IsPartial( textKey.TemplatedOriginal_Text, scope );
               SetTranslatedText( ui, untemplatedTranslation, !isPartial ? originalText : null, info );
               return untemplatedTranslation;
            }
            else
            {
               if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
               {
                  var result = UnityTextParsers.GameLogTextParser.Parse( text, scope, TextCache );
                  if( result != null )
                  {
                     translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, null );
                     if( translation != null )
                     {
                        var isPartial = TextCache.IsPartial( textKey.TemplatedOriginal_Text, scope );
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

      void IInternalTranslator.TranslateAsync( TranslationEndpointManager endpoint, string untranslatedText, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, TranslationScopes.None, endpoint, null, onCompleted, false, true );
      }

      void ITranslator.TranslateAsync( string untranslatedText, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, TranslationScopes.None, TranslationManager.CurrentEndpoint, null, onCompleted, true, true );
      }

      void ITranslator.TranslateAsync( string untranslatedText, int scope, Action<TranslationResult> onCompleted )
      {
         Translate( untranslatedText, scope, TranslationManager.CurrentEndpoint, null, onCompleted, true, true );
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
            scope = TranslationScopeHelper.Instance.GetScope( null );
         }

         if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) && IsBelowMaxLength( text ) )
         {
            var textKey = GetCacheKey( null, text, false );

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
                  translatedText = TranslateByParserResult( null, parserResult, scope, null, false, true, null );
                  return translatedText != null;
               }
            }
         }

         translatedText = null;
         return false;
      }

      #endregion

      private InternalTranslationResult Translate( string text, int scope, TranslationEndpointManager endpoint, ParserTranslationContext context, Action<TranslationResult> onCompleted, bool isGlobal, bool allowStartTranslateImmediate )
      {
         var result = new InternalTranslationResult( isGlobal, onCompleted );
         if( isGlobal )
         {
            if( scope == TranslationScopes.None && context == null )
            {
               scope = TranslationScopeHelper.Instance.GetScope( null );
            }

            if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) && IsBelowMaxLength( text ) )
            {
               var textKey = GetCacheKey( null, text, false );

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
                        translation = TranslateByParserResult( endpoint, parserResult, scope, result, allowStartTranslateImmediate, result.IsGlobal, context );
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
                           translation = TranslateByParserResult( endpoint, parserResult, scope, result, allowStartTranslateImmediate, result.IsGlobal, context );
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
                  else
                  {
                     CreateTranslationJobFor( endpoint, null, textKey, result, context, true, true, true, isTranslatable );
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
            else if( !text.IsNullOrWhiteSpace() && endpoint.IsTranslatable( text ) && IsBelowMaxLength( text ) )
            {
               var textKey = GetCacheKey( null, text, false );
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
                        translation = TranslateByParserResult( endpoint, parserResult, TranslationScopes.None, result, allowStartTranslateImmediate, result.IsGlobal, context );
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
                           translation = TranslateByParserResult( endpoint, parserResult, TranslationScopes.None, result, allowStartTranslateImmediate, result.IsGlobal, context );
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
                  else
                  {
                     CreateTranslationJobFor( endpoint, null, textKey, result, context, false, false, false, isTranslatable );
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

      private string TranslateByParserResult( TranslationEndpointManager endpoint, ParserResult result, int scope, InternalTranslationResult translationResult, bool allowStartTranslateImmediate, bool isGlobal, ParserTranslationContext parentContext )
      {
         var allowPartial = endpoint == null && result.AllowPartialTranslation;
         var context = new ParserTranslationContext( null, endpoint, translationResult, result, parentContext );
         if( isGlobal )
         {
            // attempt to lookup ALL strings immediately; return result if possible; queue operations
            var translation = result.GetTranslationFromParts( untranslatedTextPart =>
            {
               if( !untranslatedTextPart.IsNullOrWhiteSpace() && TextCache.IsTranslatable( untranslatedTextPart, true, scope ) && IsBelowMaxLength( untranslatedTextPart ) )
               {
                  var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords );
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
                        var partResult = Translate( untranslatedTextPart, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate );
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

            var translation = result.GetTranslationFromParts( untranslatedTextPart =>
            {
               if( !untranslatedTextPart.IsNullOrWhiteSpace() && endpoint.IsTranslatable( untranslatedTextPart ) && IsBelowMaxLength( untranslatedTextPart ) )
               {
                  var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords );
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
                        var partResult = Translate( untranslatedTextPart, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate );
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

      /// <summary>
      /// Translates the string of a UI  text or queues it up to be translated
      /// by the HTTP translation service.
      /// </summary>
      private string TranslateOrQueueWebJobImmediate(
         ITextComponent ui, string text, int scope, TextTranslationInfo info,
         bool allowStabilizationOnTextComponent, bool ignoreComponentState,
         bool allowStartTranslationImmediate, bool allowStartTranslationLater,
         ParserTranslationContext context = null )
      {
         text = text ?? ui.GetText();

         // make sure text exists
         var originalText = text;

         // this only happens if the game sets the text of a component to our translation
         if( info?.IsTranslated == true && originalText == info.TranslatedText )
         {
            return null;
         }

         info?.Reset( originalText );

         if( scope == TranslationScopes.None && context == null )
         {
            scope = TranslationScopeHelper.Instance.GetScope( ui );
         }

         // Ensure that we actually want to translate this text and its owning UI element. 
         if( !text.IsNullOrWhiteSpace() && TextCache.IsTranslatable( text, false, scope ) && ui.ShouldTranslateTextComponent( ignoreComponentState ) && !IsCurrentlySetting( info ) )
         {
            var isSpammer = ui.IsSpammingComponent();
            if( isSpammer && !IsBelowMaxLength( text ) ) return null; // avoid templating long strings every frame for IMGUI, important!

            // potentially shortcircuit if templated is a translation
            var textKey = GetCacheKey( ui, text, isSpammer );

            // potentially shortcircuit if fully templated
            if( ( textKey.IsTemplated && !TextCache.IsTranslatable( textKey.TemplatedOriginal_Text, false, scope ) ) || textKey.IsOnlyTemplate )
            {
               var untemplatedTranslation = textKey.Untemplate( textKey.TemplatedOriginal_Text );
               if( context == null )
               {
                  SetTranslatedText( ui, untemplatedTranslation, originalText, info );
               }
               return untemplatedTranslation;
            }

            // if we already have translation loaded in our _translatios dictionary, simply load it and set text
            string translation;
            if( TextCache.TryGetTranslation( textKey, !isSpammer, false, scope, out translation ) )
            {
               var untemplatedTranslation = textKey.Untemplate( translation );
               if( context == null ) // never set text if operation is contextualized (only a part translation)
               {
                  var isPartial = TextCache.IsPartial( textKey.TemplatedOriginal_Text, scope );
                  SetTranslatedText( ui, untemplatedTranslation, !isPartial ? originalText : null, info );
               }
               return untemplatedTranslation;
            }
            else
            {
               if( !isSpammer )
               {
                  if( context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion )
                  {
                     var isBelowMaxLength = IsBelowMaxLength( text );

                     if( UnityTextParsers.GameLogTextParser.CanApply( ui ) && context == null ) // only at the first layer!
                     {
                        var result = UnityTextParsers.GameLogTextParser.Parse( text, scope, TextCache );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, context );
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
                     if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) && isBelowMaxLength )
                     {
                        var result = UnityTextParsers.RegexSplittingTextParser.Parse( text, scope, TextCache );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, context );
                           if( translation != null )
                           {
                              if( context == null )
                              {
                                 SetTranslatedText( ui, translation, originalText, info );
                              }
                              return translation;
                           }

                           if( context != null )
                           {
                              return null;
                           }
                        }
                     }
                     if( UnityTextParsers.RichTextParser.CanApply( ui ) && isBelowMaxLength && !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                     {
                        var result = UnityTextParsers.RichTextParser.Parse( text, scope );
                        if( result != null )
                        {
                           translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, context );
                           if( translation != null )
                           {
                              if( context == null )
                              {
                                 SetTranslatedText( ui, translation, originalText, info );
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

               var isTranslatable = LanguageHelper.IsTranslatable( textKey.TemplatedOriginal_Text );
               if( !isTranslatable && !Settings.OutputUntranslatableText && ( !textKey.IsTemplated || isSpammer ) )
               {
                  if( _isInTranslatedMode && !isSpammer )
                     TranslationHelper.DisplayTranslationInfo( originalText, null );

                  // FIXME: SET TEXT? Set it to the same? Only impact is RESIZE behaviour!
                  return text;
               }
               else
               {
                  var endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
                  if( allowStartTranslationImmediate )
                  {
                     // Lets try not to spam a service that might not be there...
                     if( endpoint != null )
                     {
                        if( IsBelowMaxLength( text ) )
                        {
                           if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                           {
                              CreateTranslationJobFor( endpoint, ui, textKey, null, context, true, true, true, isTranslatable );
                           }
                        }
                     }

                     return null;
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

                           originalText = stabilizedText;

                           // This means it has been translated through "ImmediateTranslate" function
                           if( info?.IsTranslated == true ) return;

                           info?.Reset( originalText );

                           if( !stabilizedText.IsNullOrWhiteSpace() && TextCache.IsTranslatable( stabilizedText, false, scope ) )
                           {
                              // potentially shortcircuit if templated is a translation
                              var stabilizedTextKey = GetCacheKey( ui, stabilizedText, false );

                              // potentially shortcircuit if fully templated
                              if( ( stabilizedTextKey.IsTemplated && !TextCache.IsTranslatable( stabilizedTextKey.TemplatedOriginal_Text, false, scope ) ) || stabilizedTextKey.IsOnlyTemplate )
                              {
                                 var untemplatedTranslation = stabilizedTextKey.Untemplate( stabilizedTextKey.TemplatedOriginal_Text );
                                 SetTranslatedText( ui, untemplatedTranslation, originalText, info );
                                 return;
                              }

                              // once the text has stabilized, attempt to look it up
                              if( TextCache.TryGetTranslation( stabilizedTextKey, true, false, scope, out translation ) )
                              {
                                 var isPartial = TextCache.IsPartial( stabilizedTextKey.TemplatedOriginal_Text, scope );
                                 SetTranslatedText( ui, stabilizedTextKey.Untemplate( translation ), !isPartial ? originalText : null, info );
                              }
                              else
                              {
                                 // PREMISE: context should ALWAYS be null inside this method! That means we initialize the first layer of text parser recursion from here

                                 var isBelowMaxLength = IsBelowMaxLength( stabilizedText );
                                 if( UnityTextParsers.GameLogTextParser.CanApply( ui ) && context == null )
                                 {
                                    var result = UnityTextParsers.GameLogTextParser.Parse( stabilizedText, scope, TextCache );
                                    if( result != null )
                                    {
                                       var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, context );
                                       if( translatedText != null && context == null )
                                       {
                                          SetTranslatedText( ui, translatedText, null, info );
                                       }
                                       return;
                                    }
                                 }
                                 if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) && isBelowMaxLength )
                                 {
                                    var result = UnityTextParsers.RegexSplittingTextParser.Parse( stabilizedText, scope, TextCache );
                                    if( result != null )
                                    {
                                       var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, context );
                                       if( translatedText != null && context == null )
                                       {
                                          SetTranslatedText( ui, translatedText, originalText, info );
                                       }
                                       return;
                                    }
                                 }
                                 if( UnityTextParsers.RichTextParser.CanApply( ui ) && isBelowMaxLength && !context.HasBeenParsedBy( ParserResultOrigin.RichTextParser ) )
                                 {
                                    var result = UnityTextParsers.RichTextParser.Parse( stabilizedText, scope );
                                    if( result != null )
                                    {
                                       var translatedText = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, true, false, context );
                                       if( translatedText != null && context == null )
                                       {
                                          SetTranslatedText( ui, translatedText, originalText, info );
                                       }
                                       return;
                                    }
                                 }

                                 var isStabilizedTranslatable = LanguageHelper.IsTranslatable( stabilizedTextKey.TemplatedOriginal_Text );
                                 if( !isStabilizedTranslatable && !Settings.OutputUntranslatableText && !stabilizedTextKey.IsTemplated )
                                 {
                                    if( _isInTranslatedMode && !isSpammer )
                                       TranslationHelper.DisplayTranslationInfo( originalText, null );

                                    // FIXME: SET TEXT? Set it to the same? Only impact is RESIZE behaviour!
                                 }
                                 else
                                 {
                                    // Lets try not to spam a service that might not be there...
                                    if( endpoint != null )
                                    {
                                       if( IsBelowMaxLength( stabilizedText ) )
                                       {
                                          if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                                          {
                                             CreateTranslationJobFor( endpoint, ui, stabilizedTextKey, null, context, true, true, true, isStabilizedTranslatable );
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
                           CoroutineHelper.Instance.Start(
                              WaitForTextStablization(
                                 ui: ui,
                                 delay: 0.9f, // 0.9 second to prevent '1 second tickers' from getting translated
                                 maxTries: 60, // 50 tries, about 1 minute
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
                     CoroutineHelper.Instance.Start(
                        WaitForTextStablization(
                           textKey: textKey,
                           delay: 0.9f,
                           onTextStabilized: () =>
                           {
                              // if we already have translation loaded in our _translatios dictionary, simply load it and set text
                              if( TextCache.TryGetTranslation( textKey, !isSpammer, false, scope, out _ ) )
                              {
                                 // no need to do anything !
                              }
                              else
                              {
                                 // Lets try not to spam a service that might not be there...
                                 endpoint = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
                                 if( endpoint != null )
                                 {
                                    // once the text has stabilized, attempt to look it up
                                    if( !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors )
                                    {
                                       if( !TextCache.TryGetTranslation( textKey, true, false, scope, out _ ) )
                                       {
                                          CreateTranslationJobFor( endpoint, ui, textKey, null, context, true, true, true, isTranslatable );
                                       }
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

      private string TranslateOrQueueWebJobImmediateByParserResult( ITextComponent ui, ParserResult result, int scope, bool allowStartTranslationImmediate, bool allowStartTranslationLater, ParserTranslationContext parentContext )
      {
         // attempt to lookup ALL strings immediately; return result if possible; queue operations
         var allowPartial = TranslationManager.CurrentEndpoint == null && result.AllowPartialTranslation;
         var context = new ParserTranslationContext( ui, TranslationManager.CurrentEndpoint, null, result, parentContext );

         var translation = result.GetTranslationFromParts( untranslatedTextPart =>
         {
            if( !untranslatedTextPart.IsNullOrWhiteSpace() && TextCache.IsTranslatable( untranslatedTextPart, true, scope ) && IsBelowMaxLength( untranslatedTextPart ) )
            {
               var textKey = new UntranslatedText( untranslatedTextPart, false, false, Settings.FromLanguageUsesWhitespaceBetweenWords );
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
                     partTranslation = TranslateOrQueueWebJobImmediate( ui, untranslatedTextPart, scope, null, false, true, allowStartTranslationImmediate, allowStartTranslationLater, context );
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


         //try
         //{
         //   if( translation != null && context.CachedCombinedResult() )
         //   {
         //      XuaLogger.AutoTranslator.Debug( $"Cached: '{context.Result.OriginalText}' => '{translation}'" );

         //      TextCache.AddTranslationToCache( context.Result.OriginalText, translation, result.PersistCombinedResult, TranslationType.Full, scope );
         //      context.Endpoint.AddTranslationToCache( context.Result.OriginalText, translation );
         //   }
         //}
         //catch( Exception e )
         //{
         //   XuaLogger.AutoTranslator.Error( e, "What is going on?" );
         //}

         return translation;
      }

      /// <summary>
      /// Utility method that allows me to wait to call an action, until
      /// the text has stopped changing. This is important for 'story'
      /// mode text, which 'scrolls' into place slowly.
      /// </summary>
      private IEnumerator WaitForTextStablization( ITextComponent ui, float delay, int maxTries, int currentTries, Action<string> onTextStabilized, Action onMaxTriesExceeded )
      {
         yield return null;

         bool succeeded = false;
         while( currentTries < maxTries ) // shortcircuit
         {
            var beforeText = ui.GetText();

            var instruction = CoroutineHelper.Instance.CreateWaitForSecondsRealtime( delay );
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
               var instruction = CoroutineHelper.Instance.CreateWaitForSecondsRealtime( delay );
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

      public void OnApplicationStart()
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

      public void OnUpdate()
      {
         try
         {
            TranslationManager.Update();

            if( !Settings.IsShutdown )
            {
               EnableAutoTranslator();
               SpamChecker.Update();

               IncrementBatchOperations();
               KickoffTranslations();
            }

            // perform this check every 100 frames!
            if( Time.frameCount % 100 == 0
               && TranslationManager.OngoingTranslations == 0
               && TranslationManager.UnstartedTranslations == 0 )
            {
               ConnectionTrackingWebClient.CheckServicePoints();
            }

            var isAltPressed = Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt );

            if( isAltPressed )
            {
               var isCtrlPressed = Input.GetKey( KeyCode.LeftControl );

               if( Input.GetKeyDown( KeyCode.T ) )
               {
                  ToggleTranslation();
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
               //else if( Input.GetKeyDown( KeyCode.Alpha0 ) || Input.GetKeyDown( KeyCode.Keypad0 ) )
               //{
               //   if( MainWindow != null )
               //   {
               //      MainWindow.IsShown = !MainWindow.IsShown;
               //   }
               //}
               //else if( Input.GetKeyDown( KeyCode.Alpha1 ) || Input.GetKeyDown( KeyCode.Keypad1 ) )
               //{
               //   ToggleTranslationAggregator();
               //}
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
               }
            }
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
            var component = (ITextComponent)keyAndComponent.Item;
            var key = keyAndComponent.Key;

            // update the original text, but only if it has not been chaanged already for some reason (could be other translator plugin or game itself)
            try
            {
               var text = component.GetText();
               if( text == key.Original_Text )
               {
                  var info = component.GetOrCreateTextTranslationInfo();
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
            var component = (ITextComponent)context.Component;

            // are all jobs within this context completed? If so, we can set the text
            if( context.HasAllJobsCompleted() )
            {
               try
               {
                  var text = component.GetText();
                  var result = context.Result;

                  string translatedText;
                  if( context.TranslationResult == null )
                  {
                     translatedText = TranslateOrQueueWebJobImmediateByParserResult( (ITextComponent)context.Component, result, TranslationScopes.None, false, false, null );
                  }
                  else
                  {
                     translatedText = TranslateByParserResult( context.Endpoint, result, TranslationScopes.None, null, false, context.TranslationResult.IsGlobal, null );
                  }

                  if( !string.IsNullOrEmpty( translatedText ) )
                  {
                     if( result.CacheCombinedResult )
                     {
                        if( job.SaveResultGlobally )
                        {
                           TextCache.AddTranslationToCache( context.Result.OriginalText, translatedText, result.PersistCombinedResult, TranslationType.Full, TranslationScopes.None );
                        }
                        job.Endpoint.AddTranslationToCache( context.Result.OriginalText, translatedText );
                     }

                     if( text == result.OriginalText )
                     {
                        var info = component.GetOrCreateTextTranslationInfo();
                        SetTranslatedText( component, translatedText, context.Result.OriginalText, info );
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

      private static UntranslatedText GetCacheKey( object ui, string originalText, bool isFromSpammingComponent )
      {
         return new UntranslatedText( originalText, isFromSpammingComponent, !isFromSpammingComponent, Settings.FromLanguageUsesWhitespaceBetweenWords );
      }

      private void ReloadTranslations()
      {
         LoadTranslations();

         var context = new TextureReloadContext();
         foreach( var kvp in ExtensionDataHelper.GetAllRegisteredObjects() )
         {
            var ui = (ITextComponent)kvp.Key;
            try
            {
               var tti = kvp.Value as TextTranslationInfo;

               if( tti.GetIsKnownTextComponent() && ui.IsComponentActive() )
               {
                  var scope = TranslationScopeHelper.Instance.GetScope( ui );

                  if( tti != null && !tti.OriginalText.IsNullOrWhiteSpace() )
                  {
                     var originalText = tti.OriginalText;
                     var isBelowMaxLength = IsBelowMaxLength( originalText );
                     var key = GetCacheKey( kvp.Key, originalText, false );
                     if( TextCache.TryGetTranslation( key, true, false, scope, out string translatedText ) )
                     {
                        tti.UnresizeUI( ui );
                        SetTranslatedText( ui, key.Untemplate( translatedText ), null, tti ); // no need to untemplatize the translated text
                        continue;
                     }
                     else
                     {
                        if( UnityTextParsers.GameLogTextParser.CanApply( ui ) )
                        {
                           var result = UnityTextParsers.GameLogTextParser.Parse( originalText, scope, TextCache );
                           if( result != null )
                           {
                              var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, null );
                              if( translation != null )
                              {
                                 tti.UnresizeUI( ui );
                                 SetTranslatedText( ui, translation, null, tti );
                                 continue;
                              }
                           }
                        }
                        if( UnityTextParsers.RegexSplittingTextParser.CanApply( ui ) && isBelowMaxLength )
                        {
                           var result = UnityTextParsers.RegexSplittingTextParser.Parse( originalText, scope, TextCache );
                           if( result != null )
                           {
                              var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, null );
                              if( translation != null )
                              {
                                 tti.UnresizeUI( ui );
                                 SetTranslatedText( ui, translation, null, tti );
                                 continue;
                              }
                           }
                        }
                        if( UnityTextParsers.RichTextParser.CanApply( ui ) && isBelowMaxLength )
                        {
                           var result = UnityTextParsers.RichTextParser.Parse( originalText, scope );
                           if( result != null )
                           {
                              var translation = TranslateOrQueueWebJobImmediateByParserResult( ui, result, scope, false, false, null );
                              if( translation != null )
                              {
                                 tti.UnresizeUI( ui );
                                 SetTranslatedText( ui, translation, null, tti );
                                 continue;
                              }
                           }
                        }
                     }
                  }
               }
            }
            catch( Exception )
            {
               // not super pretty, no...
               ExtensionDataHelper.Remove( ui );
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
               var ui = (ITextComponent)kvp.Key;
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
               var ui = (ITextComponent)kvp.Key;
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
               }
               catch( Exception )
               {
                  // not super pretty, no...
                  ExtensionDataHelper.Remove( ui );
               }
            }
         }
      }

      private void ManualHook()
      {
         ManualHookForComponents();
      }

      private void ManualHookForComponents()
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

      private void TraverseChildrenManualHook( GameObject obj )
      {
         if( obj != null )
         {
            var components = obj.GetComponents<Component>();
            foreach( var component in components )
            {
               var textComponent = component.AsTextComponent();

               if( textComponent != null )
               {
                  Hook_TextChanged( textComponent, false );
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

      void OnApplicationQuit()
      {
         try
         {
            RedirectedDirectory.Uninitialize();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while uninitializing redirected directory cache." );
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
