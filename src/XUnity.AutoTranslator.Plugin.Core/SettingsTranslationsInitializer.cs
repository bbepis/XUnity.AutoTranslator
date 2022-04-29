using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class SettingsTranslationsInitializer
   {
      public static void LoadTranslations()
      {
         Settings.Replacements.Clear();
         Settings.Preprocessors.Clear();

         Directory.CreateDirectory( Settings.TranslationsPath );
         var substitutionFile = new FileInfo( Settings.SubstitutionFilePath ).FullName;
         var preprocessorsFile = new FileInfo( Settings.PreprocessorsFilePath ).FullName;
         var postprocessorsFile = new FileInfo( Settings.PostprocessorsFilePath ).FullName;

         LoadTranslationsInFile( substitutionFile, true, false, false );
         LoadTranslationsInFile( preprocessorsFile, false, true, false );
         LoadTranslationsInFile( postprocessorsFile, false, false, true );
      }

      private static void LoadTranslationsInFile( string fullFileName, bool isSubstitutionFile, bool isPreprocessorFile, bool isPostprocessorsFile )
      {
         var fileExists = File.Exists( fullFileName );
         if( fileExists )
         {
            using( var stream = File.OpenRead( fullFileName ) )
            {
               LoadTranslationsInStream( stream, fullFileName, isSubstitutionFile, isPreprocessorFile, isPostprocessorsFile );
            }
         }
         else
         {
            var fi = new FileInfo( fullFileName );
            Directory.CreateDirectory( fi.Directory.FullName );

            using( var stream = File.Create( fullFileName ) )
            {
               stream.Write( new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3 ); // UTF-8 BOM
               stream.Close();
            }
         }
      }

      private static void LoadTranslationsInStream( Stream stream, string fullFileName, bool isSubstitutionFile, bool isPreprocessorFile, bool isPostProcessorFile )
      {
         if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( $"Loading texts: {fullFileName}." );

         var reader = new StreamReader( stream, Encoding.UTF8 );
         {
            var context = new TranslationFileLoadingContext();

            string[] translations = reader.ReadToEnd().Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string translatioOrDirective in translations )
            {
               if( context.IsApplicable() )
               {
                  try
                  {
                     var kvp = TextHelper.ReadTranslationLineAndDecode( translatioOrDirective );
                     if( kvp != null )
                     {
                        string key = kvp[ 0 ];
                        string value = kvp[ 1 ];

                        if( !string.IsNullOrEmpty( key ) )
                        {
                           if( isSubstitutionFile )
                           {
                              if( !string.IsNullOrEmpty( value ) )
                              {
                                 Settings.Replacements[ key ] = value;
                              }
                           }
                           else if( isPreprocessorFile )
                           {
                              Settings.Preprocessors[ key ] = value;
                           }
                           else if( isPostProcessorFile )
                           {
                              Settings.Postprocessors[ key ] = value;
                           }
                        }
                     }
                  }
                  catch( Exception e )
                  {
                     XuaLogger.AutoTranslator.Warn( e, $"An error occurred while reading translation: '{translatioOrDirective}'." );
                  }
               }
            }
         }
      }
   }
}
