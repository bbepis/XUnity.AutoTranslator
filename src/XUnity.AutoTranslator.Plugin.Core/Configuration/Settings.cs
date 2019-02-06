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
      public static readonly string PluginFolder = "Translators";
      public static readonly int MaxMaxCharactersPerTranslation = 500;
      public static readonly string DefaultLanguage = "en";
      public static readonly string DefaultFromLanguage = "ja";
      public static readonly string EnglishLanguage = "en";
      public static readonly int MaxErrors = 5;
      public static readonly float ClipboardDebounceTime = 1f;
      public static readonly int MaxTranslationsBeforeShutdown = 8000;
      public static readonly int MaxUnstartedJobs = 3500;
      public static readonly float IncreaseBatchOperationsEvery = 30;
      public static readonly int MaximumStaggers = 6;
      public static readonly int PreviousTextStaggerCount = 3;
      public static readonly int MaximumConsecutiveFramesTranslated = 90;
      public static readonly int MaximumConsecutiveSecondsTranslated = 60;
      public static bool UsesWhitespaceBetweenWords = false;
      public static string ApplicationName;


      public static bool IsShutdown = false;
      public static bool IsShutdownFatal = false;
      public static int TranslationCount = 0;
      public static int MaxAvailableBatchOperations = 40;

      public static readonly float MaxTranslationsQueuedPerSecond = 5;
      public static readonly int MaxSecondsAboveTranslationThreshold = 30;
      public static readonly int TranslationQueueWatchWindow = 6;

      public static readonly int BatchSize = 10;

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
      public static WhitespaceHandlingStrategy WhitespaceRemovalStrategy;
      public static float? ResizeUILineSpacingScale;
      public static bool ForceUIResizing;
      public static string[] IgnoreTextStartingWith;
      public static HashSet<string> GameLogTextPaths;
      public static bool TextGetterCompatibilityMode;

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
         Config.Current.Preferences[ "Service" ][ "Endpoint" ].Value = id;
         Config.Current.SaveConfig();
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


         ServiceEndpoint = Config.Current.Preferences.GetOrDefault( "Service", "Endpoint", KnownEndpointNames.GoogleTranslate );

         Language = Config.Current.Preferences.GetOrDefault( "General", "Language", DefaultLanguage );
         FromLanguage = Config.Current.Preferences.GetOrDefault( "General", "FromLanguage", DefaultFromLanguage );

         TranslationDirectory = Config.Current.Preferences.GetOrDefault( "Files", "Directory", "Translation" );
         OutputFile = Config.Current.Preferences.GetOrDefault( "Files", "OutputFile", @"Translation\_AutoGeneratedTranslations.{lang}.txt" );

         EnableIMGUI = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableIMGUI", false );
         EnableUGUI = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUGUI", true );
         EnableNGUI = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableNGUI", true );
         EnableTextMeshPro = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableTextMeshPro", true );
         EnableUtage = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUtage", true );
         AllowPluginHookOverride = Config.Current.Preferences.GetOrDefault( "TextFrameworks", "AllowPluginHookOverride", true );

         Delay = Config.Current.Preferences.GetOrDefault( "Behaviour", "Delay", 0f );
         MaxCharactersPerTranslation = Config.Current.Preferences.GetOrDefault( "Behaviour", "MaxCharactersPerTranslation", 200 );
         IgnoreWhitespaceInDialogue = Config.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreWhitespaceInDialogue", true );
         IgnoreWhitespaceInNGUI = Config.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreWhitespaceInNGUI", true );
         MinDialogueChars = Config.Current.Preferences.GetOrDefault( "Behaviour", "MinDialogueChars", 20 );
         ForceSplitTextAfterCharacters = Config.Current.Preferences.GetOrDefault( "Behaviour", "ForceSplitTextAfterCharacters", 0 );
         CopyToClipboard = Config.Current.Preferences.GetOrDefault( "Behaviour", "CopyToClipboard", false );
         MaxClipboardCopyCharacters = Config.Current.Preferences.GetOrDefault( "Behaviour", "MaxClipboardCopyCharacters", 450 );
         EnableUIResizing = Config.Current.Preferences.GetOrDefault( "Behaviour", "EnableUIResizing", true );
         EnableBatching = Config.Current.Preferences.GetOrDefault( "Behaviour", "EnableBatching", true );
         TrimAllText = Config.Current.Preferences.GetOrDefault( "Behaviour", "TrimAllText", ClrTypes.AdvEngine == null );
         UseStaticTranslations = Config.Current.Preferences.GetOrDefault( "Behaviour", "UseStaticTranslations", true );
         OverrideFont = Config.Current.Preferences.GetOrDefault( "Behaviour", "OverrideFont", string.Empty );
         ResizeUILineSpacingScale = Config.Current.Preferences.GetOrDefault<float?>( "Behaviour", "ResizeUILineSpacingScale", null );
         ForceUIResizing = Config.Current.Preferences.GetOrDefault( "Behaviour", "ForceUIResizing", false );
         IgnoreTextStartingWith = Config.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreTextStartingWith", "\\u180e;" )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => x.UnescapeJson() ).ToArray() ?? new string[ 0 ];
         TextGetterCompatibilityMode = Config.Current.Preferences.GetOrDefault( "Behaviour", "TextGetterCompatibilityMode", false );
         GameLogTextPaths = Config.Current.Preferences.GetOrDefault( "Behaviour", "GameLogTextPaths", string.Empty )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
         GameLogTextPaths.RemoveWhere( x => !x.StartsWith( "/" ) ); // clean up to ensure no 'empty' entries
         WhitespaceRemovalStrategy = Config.Current.Preferences.GetOrDefault( "Behaviour", "WhitespaceRemovalStrategy", WhitespaceHandlingStrategy.TrimPerNewline );

         TextureDirectory = Config.Current.Preferences.GetOrDefault( "Texture", "TextureDirectory", @"Translation\Texture" );
         EnableTextureTranslation = Config.Current.Preferences.GetOrDefault( "Texture", "EnableTextureTranslation", false );
         EnableTextureDumping = Config.Current.Preferences.GetOrDefault( "Texture", "EnableTextureDumping", false );
         EnableTextureToggling = Config.Current.Preferences.GetOrDefault( "Texture", "EnableTextureToggling", false );
         EnableTextureScanOnSceneLoad = Config.Current.Preferences.GetOrDefault( "Texture", "EnableTextureScanOnSceneLoad", false );
         EnableSpriteRendererHooking = Config.Current.Preferences.GetOrDefault( "Texture", "EnableSpriteRendererHooking", false );
         LoadUnmodifiedTextures = Config.Current.Preferences.GetOrDefault( "Texture", "LoadUnmodifiedTextures", false );
         TextureHashGenerationStrategy = Config.Current.Preferences.GetOrDefault( "Texture", "TextureHashGenerationStrategy", TextureHashGenerationStrategy.FromImageName );

         if( MaxCharactersPerTranslation > MaxMaxCharactersPerTranslation )
         {
            Config.Current.Preferences[ "Behaviour" ][ "MaxCharactersPerTranslation" ].Value = MaxMaxCharactersPerTranslation.ToString( CultureInfo.InvariantCulture );
            MaxCharactersPerTranslation = MaxMaxCharactersPerTranslation;
         }

         UserAgent = Config.Current.Preferences.GetOrDefault( "Http", "UserAgent", string.Empty );

         EnablePrintHierarchy = Config.Current.Preferences.GetOrDefault( "Debug", "EnablePrintHierarchy", false );
         EnableConsole = Config.Current.Preferences.GetOrDefault( "Debug", "EnableConsole", false );
         EnableDebugLogs = Config.Current.Preferences.GetOrDefault( "Debug", "EnableLog", false );

         EnableMigrations = Config.Current.Preferences.GetOrDefault( "Migrations", "Enable", true );
         MigrationsTag = Config.Current.Preferences.GetOrDefault( "Migrations", "Tag", string.Empty );

         AutoTranslationsFilePath = Path.Combine( Config.Current.DataPath, OutputFile.Replace( "{lang}", Language ) ).Replace( "/", "\\" ).Parameterize();
         UsesWhitespaceBetweenWords = LanguageHelper.RequiresWhitespaceUponLineMerging( FromLanguage );

         if( EnableMigrations )
         {
            Migrate();
         }

         // update tag
         MigrationsTag = Config.Current.Preferences[ "Migrations" ][ "Tag" ].Value = PluginData.Version;

         Config.Current.SaveConfig();
      }
      
      private static void Migrate()
      {
      }
   }
}
