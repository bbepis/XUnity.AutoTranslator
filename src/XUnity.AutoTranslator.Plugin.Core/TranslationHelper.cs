using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Class used to map redirected resource translations to a source file.
   /// </summary>
   public static class TranslationHelper
   {
      private static Dictionary<string, HashSet<string>> _registrations = new Dictionary<string, HashSet<string>>();

      private static List<string> LookupRegistration( IEnumerable<UntranslatedText> keys )
      {
         HashSet<string> results = new HashSet<string>();
         HashSet<string> partResult;
         bool result;

         foreach( var key in keys )
         {
            // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
            result = _registrations.TryGetValue( key.TemplatedOriginal_Text, out partResult );
            if( result )
            {
               results.AddRange( partResult );
            }

            // lookup original minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_ExternallyTrimmed = 'What are you \ndoing here, {{A}}?'

               result = _registrations.TryGetValue( key.TemplatedOriginal_Text_ExternallyTrimmed, out partResult );
               if( result )
               {
                  results.AddRange( partResult );
               }
            }

            // lookup internally trimmed
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'

               result = _registrations.TryGetValue( key.TemplatedOriginal_Text_InternallyTrimmed, out partResult );
               if( result )
               {
                  results.AddRange( partResult );
               }
            }

            // lookup internally trimmed minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_FullyTrimmed = 'What are you doing here, {{A}}?'

               result = _registrations.TryGetValue( key.TemplatedOriginal_Text_FullyTrimmed, out partResult );
               if( result )
               {
                  results.AddRange( partResult );
               }
            }
         }

         var list = results.ToList();
         list.Sort();

         return list;
      }

      internal static void DisplayTranslationInfo( string originalText, string translatedText )
      {
         if( !Settings.EnableTranslationHelper ) return;

         var key = CreateTextKey( originalText );
         var keys = new List<UntranslatedText> { key };
         if( translatedText != null )
         {
            var translatedKey = CreateTextKey( translatedText );
            keys.Add( translatedKey );
         }

         var files = LookupRegistration( keys );
         if( files.Count > 0 )
         {
            var builder = new StringBuilder();
            builder.AppendLine();
            if( translatedText != null )
            {
               builder.Append( "For the text \"" ).Append( originalText ).AppendLine( "\", which was translated to \"" + translatedText + "\" found the following potential source files:" );
            }
            else
            {
               builder.Append( "For the text \"" ).Append( originalText ).AppendLine( "\" found the following potential source files:" );
            }
            foreach( var file in files )
            {
               builder.Append( "  " ).AppendLine( file );
            }

            XuaLogger.AutoTranslator.Info( builder.ToString() );
         }
      }

      /// <summary>
      /// Register the text as originating from the specified path.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="virtualFilePath"></param>
      public static void RegisterRedirectedResourceTextToPath( string text, string virtualFilePath )
      {
         if( !Settings.EnableTranslationHelper ) return;

         var key = CreateTextKey( text );

         AssociateTextKeyWithFile( key, virtualFilePath );
      }

      private static UntranslatedText CreateTextKey( string text )
      {
         var translatable = LanguageHelper.IsTranslatable( text );
         if( translatable )
         {
            return new UntranslatedText( text, false, true, Settings.FromLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
         }
         else
         {
            return new UntranslatedText( text, false, true, Settings.ToLanguageUsesWhitespaceBetweenWords, true, Settings.TemplateAllNumberAway );
         }
      }

      private static void AssociateTextKeyWithFile( UntranslatedText key, string virtualFilePath )
      {
         AssociateTextWithFile( key.TemplatedOriginal_Text, virtualFilePath );
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
         {
            AssociateTextWithFile( key.TemplatedOriginal_Text_ExternallyTrimmed, virtualFilePath );
         }
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
         {
            AssociateTextWithFile( key.TemplatedOriginal_Text_InternallyTrimmed, virtualFilePath );
         }
         if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
         {
            AssociateTextWithFile( key.TemplatedOriginal_Text_FullyTrimmed, virtualFilePath );
         }
      }

      private static void AssociateTextWithFile( string text, string virtualFilePath )
      {
         if( !_registrations.TryGetValue( text, out var set ) )
         {
            set = new HashSet<string>();
            _registrations[ text ] = set;
         }

         set.Add( virtualFilePath );
      }
   }
}
