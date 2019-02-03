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
      //public static bool DeleteUnmodifiedTextures;
      public static TextureHashGenerationStrategy TextureHashGenerationStrategy;

      public static bool CopyToClipboard;
      public static int MaxClipboardCopyCharacters;

      public static void Configure()
      {
         try
         {
            ApplicationName = Path.GetFileNameWithoutExtension( ApplicationInformation.StartupPath );
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while getting application name." );
         }


         ServiceEndpoint = Config.Current.Preferences[ "Service" ][ "Endpoint" ].GetOrDefault( KnownEndpointNames.GoogleTranslate, true );

         Language = Config.Current.Preferences[ "General" ][ "Language" ].GetOrDefault( DefaultLanguage );
         FromLanguage = Config.Current.Preferences[ "General" ][ "FromLanguage" ].GetOrDefault( DefaultFromLanguage, true );

         TranslationDirectory = Config.Current.Preferences[ "Files" ][ "Directory" ].GetOrDefault( @"Translation" );
         OutputFile = Config.Current.Preferences[ "Files" ][ "OutputFile" ].GetOrDefault( @"Translation\_AutoGeneratedTranslations.{lang}.txt" );

         EnableIMGUI = Config.Current.Preferences[ "TextFrameworks" ][ "EnableIMGUI" ].GetOrDefault( false );
         EnableUGUI = Config.Current.Preferences[ "TextFrameworks" ][ "EnableUGUI" ].GetOrDefault( true );
         EnableNGUI = Config.Current.Preferences[ "TextFrameworks" ][ "EnableNGUI" ].GetOrDefault( true );
         EnableTextMeshPro = Config.Current.Preferences[ "TextFrameworks" ][ "EnableTextMeshPro" ].GetOrDefault( true );
         EnableUtage = Config.Current.Preferences[ "TextFrameworks" ][ "EnableUtage" ].GetOrDefault( true );
         AllowPluginHookOverride = Config.Current.Preferences[ "TextFrameworks" ][ "AllowPluginHookOverride" ].GetOrDefault( true );

         Delay = Config.Current.Preferences[ "Behaviour" ][ "Delay" ].GetOrDefault( 0f );
         MaxCharactersPerTranslation = Config.Current.Preferences[ "Behaviour" ][ "MaxCharactersPerTranslation" ].GetOrDefault( 200 );
         IgnoreWhitespaceInDialogue = Config.Current.Preferences[ "Behaviour" ][ "IgnoreWhitespaceInDialogue" ].GetOrDefault( true );
         IgnoreWhitespaceInNGUI = Config.Current.Preferences[ "Behaviour" ][ "IgnoreWhitespaceInNGUI" ].GetOrDefault( true );
         MinDialogueChars = Config.Current.Preferences[ "Behaviour" ][ "MinDialogueChars" ].GetOrDefault( 20 );
         ForceSplitTextAfterCharacters = Config.Current.Preferences[ "Behaviour" ][ "ForceSplitTextAfterCharacters" ].GetOrDefault( 0 );
         CopyToClipboard = Config.Current.Preferences[ "Behaviour" ][ "CopyToClipboard" ].GetOrDefault( false );
         MaxClipboardCopyCharacters = Config.Current.Preferences[ "Behaviour" ][ "MaxClipboardCopyCharacters" ].GetOrDefault( 450 );
         EnableUIResizing = Config.Current.Preferences[ "Behaviour" ][ "EnableUIResizing" ].GetOrDefault( true );
         EnableBatching = Config.Current.Preferences[ "Behaviour" ][ "EnableBatching" ].GetOrDefault( true );
         TrimAllText = Config.Current.Preferences[ "Behaviour" ][ "TrimAllText" ].GetOrDefault( ClrTypes.AdvEngine == null );
         UseStaticTranslations = Config.Current.Preferences[ "Behaviour" ][ "UseStaticTranslations" ].GetOrDefault( true );
         OverrideFont = Config.Current.Preferences[ "Behaviour" ][ "OverrideFont" ].GetOrDefault( string.Empty );
         ResizeUILineSpacingScale = Config.Current.Preferences[ "Behaviour" ][ "ResizeUILineSpacingScale" ].GetOrDefault<float?>( null, true );
         ForceUIResizing = Config.Current.Preferences[ "Behaviour" ][ "ForceUIResizing" ].GetOrDefault( false );
         IgnoreTextStartingWith = Config.Current.Preferences[ "Behaviour" ][ "IgnoreTextStartingWith" ].GetOrDefault( "\\u180e;", true )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => x.UnescapeJson() ).ToArray() ?? new string[ 0 ];
         TextGetterCompatibilityMode = Config.Current.Preferences[ "Behaviour" ][ "TextGetterCompatibilityMode" ].GetOrDefault( false );
         GameLogTextPaths = Config.Current.Preferences[ "Behaviour" ][ "GameLogTextPaths" ].GetOrDefault( "", true )
            ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
         GameLogTextPaths.RemoveWhere( x => !x.StartsWith( "/" ) ); // clean up to ensure no 'empty' entries

         TextureDirectory = Config.Current.Preferences[ "Texture" ][ "TextureDirectory" ].GetOrDefault( @"Translation\Texture" );
         EnableTextureTranslation = Config.Current.Preferences[ "Texture" ][ "EnableTextureTranslation" ].GetOrDefault( false );
         EnableTextureDumping = Config.Current.Preferences[ "Texture" ][ "EnableTextureDumping" ].GetOrDefault( false );
         EnableTextureToggling = Config.Current.Preferences[ "Texture" ][ "EnableTextureToggling" ].GetOrDefault( false );
         EnableTextureScanOnSceneLoad = Config.Current.Preferences[ "Texture" ][ "EnableTextureScanOnSceneLoad" ].GetOrDefault( false );
         EnableSpriteRendererHooking = Config.Current.Preferences[ "Texture" ][ "EnableSpriteRendererHooking" ].GetOrDefault( false );
         LoadUnmodifiedTextures = Config.Current.Preferences[ "Texture" ][ "LoadUnmodifiedTextures" ].GetOrDefault( false );
         //DeleteUnmodifiedTextures = Config.Current.Preferences[ "Texture" ][ "DeleteUnmodifiedTextures" ].GetOrDefault( false );
         TextureHashGenerationStrategy = Config.Current.Preferences[ "Texture" ][ "TextureHashGenerationStrategy" ].GetOrDefault( TextureHashGenerationStrategy.FromImageName );

         if( MaxCharactersPerTranslation > MaxMaxCharactersPerTranslation )
         {
            Config.Current.Preferences[ "Behaviour" ][ "MaxCharactersPerTranslation" ].Value = MaxMaxCharactersPerTranslation.ToString( CultureInfo.InvariantCulture );
            MaxCharactersPerTranslation = MaxMaxCharactersPerTranslation;
         }

         // special handling because of enum parsing
         try
         {
            WhitespaceRemovalStrategy = Config.Current.Preferences[ "Behaviour" ][ "WhitespaceRemovalStrategy" ].GetOrDefault( WhitespaceHandlingStrategy.TrimPerNewline );
         }
         catch( Exception e )
         {
            WhitespaceRemovalStrategy = WhitespaceHandlingStrategy.TrimPerNewline;

            Logger.Current.Warn( e, "An error occurred while configuring 'WhitespaceRemovalStrategy'. Using default." );
         }

         UserAgent = Config.Current.Preferences[ "Http" ][ "UserAgent" ].GetOrDefault( string.Empty );

         EnablePrintHierarchy = Config.Current.Preferences[ "Debug" ][ "EnablePrintHierarchy" ].GetOrDefault( false );
         EnableConsole = Config.Current.Preferences[ "Debug" ][ "EnableConsole" ].GetOrDefault( false );
         EnableDebugLogs = Config.Current.Preferences[ "Debug" ][ "EnableLog" ].GetOrDefault( false );

         EnableMigrations = Config.Current.Preferences[ "Migrations" ][ "Enable" ].GetOrDefault( true );
         MigrationsTag = Config.Current.Preferences[ "Migrations" ][ "Tag" ].GetOrDefault( string.Empty );

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
