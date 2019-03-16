using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
   internal static class Settings
   {
      // cannot be changed
      public static readonly int MaxFailuresForSameTextPerEndpoint = 3;
      public static readonly string PluginFolder = "Translators";
      public static readonly int MaxMaxCharactersPerTranslation = 500;
      public static readonly string DefaultLanguage = "en";
      public static readonly string DefaultFromLanguage = "ja";
      public static readonly string EnglishLanguage = "en";
      public static readonly string Romaji = "romaji";
      public static readonly int MaxErrors = 5;
      public static readonly float ClipboardDebounceTime = 1f;
      public static readonly int MaxTranslationsBeforeShutdown = 8000;
      public static readonly int MaxUnstartedJobs = 4000;
      public static readonly float IncreaseBatchOperationsEvery = 30;
      public static readonly int MaximumStaggers = 6;
      public static readonly int PreviousTextStaggerCount = 3;
      public static readonly int MaximumConsecutiveFramesTranslated = 90;
      public static readonly int MaximumConsecutiveSecondsTranslated = 60;
      public static bool UsesWhitespaceBetweenWords = false;
      public static string ApplicationName;
      public static float Timeout = 50.0f;


      public static bool IsShutdown = false;
      public static bool IsShutdownFatal = false;
      public static int TranslationCount = 0;
      public static int MaxAvailableBatchOperations = 50;

      public static readonly float MaxTranslationsQueuedPerSecond = 5;
      public static readonly int MaxSecondsAboveTranslationThreshold = 30;
      public static readonly int TranslationQueueWatchWindow = 6;

      // can be changed
      public static string ServiceEndpoint;
      public static string Language;
      public static string FromLanguage;
      public static string OutputFile;
      public static string TranslationDirectory;
      public static float Delay;
      public static int MaxCharactersPerTranslation;
      public static bool EnablePrintHierarchy;
      public static bool EnableConsole;
      public static bool EnableDebugLogs;
      public static string AutoTranslationsFilePath;
      public static bool EnableIMGUI;
      public static bool EnableUGUI;
      public static bool EnableNGUI;
      public static bool EnableTextMeshPro;
      public static bool EnableUtage;
      public static bool AllowPluginHookOverride;
      public static bool IgnoreWhitespaceInDialogue;
      public static bool IgnoreWhitespaceInNGUI;
      public static int MinDialogueChars;
      public static int ForceSplitTextAfterCharacters;
      public static bool EnableMigrations;
      public static string MigrationsTag;
      public static bool EnableBatching;
      public static bool TrimAllText;
      public static bool EnableUIResizing;
      public static bool UseStaticTranslations;
      public static string OverrideFont;
      public static string UserAgent;
      public static bool DisableCertificateValidation;
      public static WhitespaceHandlingStrategy WhitespaceRemovalStrategy;
      public static float? ResizeUILineSpacingScale;
      public static bool ForceUIResizing;
      public static string[] IgnoreTextStartingWith;
      public static HashSet<string> GameLogTextPaths;
      public static bool TextGetterCompatibilityMode;
      public static TextPostProcessing RomajiPostProcessing;
      public static TextPostProcessing TranslationPostProcessing;
      public static bool EnableExperimentalHooks;

      public static string TextureDirectory;
      public static bool EnableTextureTranslation;
      public static bool EnableTextureDumping;
      public static bool EnableTextureToggling;
      public static bool EnableTextureScanOnSceneLoad;
      public static bool EnableSpriteRendererHooking;
      public static bool LoadUnmodifiedTextures;
      public static TextureHashGenerationStrategy TextureHashGenerationStrategy;

      public static bool CopyToClipboard;
      public static int MaxClipboardCopyCharacters;

      public static void SetEndpoint( string id )
      {
         ServiceEndpoint = id;
         PluginEnvironment.Current.Preferences[ "Service" ][ "Endpoint" ].Value = id;
         PluginEnvironment.Current.SaveConfig();
      }

      public static void Configure()
      {
         try
         {
            ApplicationName = Path.GetFileNameWithoutExtension( ApplicationInformation.StartupPath );
         }
         catch( Exception e )
         {
            ApplicationName = "Unknown";
            XuaLogger.Current.Error( e, "An error occurred while getting application name." );
         }


         ServiceEndpoint = PluginEnvironment.Current.Preferences.GetOrDefault( "Service", "Endpoint", KnownTranslateEndpointNames.GoogleTranslate );

         Language = string.Intern( PluginEnvironment.Current.Preferences.GetOrDefault( "General", "Language", DefaultLanguage ) );
         FromLanguage = string.Intern( PluginEnvironment.Current.Preferences.GetOrDefault( "General", "FromLanguage", DefaultFromLanguage ) );

         TranslationDirectory = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "Directory", "Translation" );
         OutputFile = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "OutputFile", @"Translation\_AutoGeneratedTranslations.{lang}.txt" );

         EnableIMGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableIMGUI", false );
         EnableUGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUGUI", true );
         EnableNGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableNGUI", true );
         EnableTextMeshPro = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableTextMeshPro", true );
         EnableUtage = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUtage", true );
         AllowPluginHookOverride = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "AllowPluginHookOverride", true );

         Delay = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "Delay", 0f );
         MaxCharactersPerTranslation = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MaxCharactersPerTranslation", 200 );
         IgnoreWhitespaceInDialogue = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreWhitespaceInDialogue", true );
         IgnoreWhitespaceInNGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreWhitespaceInNGUI", true );
         MinDialogueChars = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MinDialogueChars", 20 );
         ForceSplitTextAfterCharacters = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ForceSplitTextAfterCharacters", 0 );
         CopyToClipboard = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CopyToClipboard", false );
         MaxClipboardCopyCharacters = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MaxClipboardCopyCharacters", 450 );
         EnableUIResizing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableUIResizing", true );
         EnableBatching = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableBatching", true );
         TrimAllText = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TrimAllText", ClrTypes.AdvEngine == null );
         UseStaticTranslations = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "UseStaticTranslations", true );
         OverrideFont = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OverrideFont", string.Empty );
         ResizeUILineSpacingScale = PluginEnvironment.Current.Preferences.GetOrDefault<float?>( "Behaviour", "ResizeUILineSpacingScale", null );
         ForceUIResizing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ForceUIResizing", false );
         IgnoreTextStartingWith = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreTextStartingWith", "\\u180e;" )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => JsonHelper.Unescape( x ) ).ToArray() ?? new string[ 0 ];
         TextGetterCompatibilityMode = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TextGetterCompatibilityMode", false );
         GameLogTextPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "GameLogTextPaths", string.Empty )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
         GameLogTextPaths.RemoveWhere( x => !x.StartsWith( "/" ) ); // clean up to ensure no 'empty' entries
         WhitespaceRemovalStrategy = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "WhitespaceRemovalStrategy", WhitespaceHandlingStrategy.TrimPerNewline );
         RomajiPostProcessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "RomajiPostProcessing", TextPostProcessing.ReplaceMacronWithCircumflex | TextPostProcessing.RemoveApostrophes );
         TranslationPostProcessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TranslationPostProcessing", TextPostProcessing.ReplaceMacronWithCircumflex );
         EnableExperimentalHooks = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableExperimentalHooks", false );

         TextureDirectory = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "TextureDirectory", @"Translation\Texture" );
         EnableTextureTranslation = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureTranslation", false );
         EnableTextureDumping = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureDumping", false );
         EnableTextureToggling = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureToggling", false );
         EnableTextureScanOnSceneLoad = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureScanOnSceneLoad", false );
         EnableSpriteRendererHooking = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableSpriteRendererHooking", false );
         LoadUnmodifiedTextures = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "LoadUnmodifiedTextures", false );
         TextureHashGenerationStrategy = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "TextureHashGenerationStrategy", TextureHashGenerationStrategy.FromImageName );
         
         if( MaxCharactersPerTranslation > MaxMaxCharactersPerTranslation )
         {
            PluginEnvironment.Current.Preferences[ "Behaviour" ][ "MaxCharactersPerTranslation" ].Value = MaxMaxCharactersPerTranslation.ToString( CultureInfo.InvariantCulture );
            MaxCharactersPerTranslation = MaxMaxCharactersPerTranslation;
         }

         UserAgent = PluginEnvironment.Current.Preferences.GetOrDefault( "Http", "UserAgent", string.Empty );
         DisableCertificateValidation = PluginEnvironment.Current.Preferences.GetOrDefault( "Http", "DisableCertificateValidation", false );

         EnablePrintHierarchy = PluginEnvironment.Current.Preferences.GetOrDefault( "Debug", "EnablePrintHierarchy", false );
         EnableConsole = PluginEnvironment.Current.Preferences.GetOrDefault( "Debug", "EnableConsole", false );
         EnableDebugLogs = PluginEnvironment.Current.Preferences.GetOrDefault( "Debug", "EnableLog", false );

         EnableMigrations = PluginEnvironment.Current.Preferences.GetOrDefault( "Migrations", "Enable", true );
         MigrationsTag = PluginEnvironment.Current.Preferences.GetOrDefault( "Migrations", "Tag", string.Empty );

         AutoTranslationsFilePath = Path.Combine( PluginEnvironment.Current.DataPath, OutputFile.Replace( "{lang}", Language ) ).Replace( "/", "\\" ).Parameterize();
         UsesWhitespaceBetweenWords = LanguageHelper.RequiresWhitespaceUponLineMerging( FromLanguage );

         if( EnableMigrations )
         {
            Migrate();
         }

         // update tag
         MigrationsTag = PluginEnvironment.Current.Preferences[ "Migrations" ][ "Tag" ].Value = PluginData.Version;

         PluginEnvironment.Current.SaveConfig();
      }
      
      private static void Migrate()
      {
      }
   }
}
