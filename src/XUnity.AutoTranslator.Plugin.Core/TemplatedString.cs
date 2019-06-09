using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TemplatedString
   {
      public TemplatedString( string template, Dictionary<string, string> arguments )
      {
         Template = template;
         Arguments = arguments;
      }

      public string Template { get; private set; }

      public Dictionary<string, string> Arguments { get; private set; }

      public string Untemplate( string text )
      {
         foreach( var kvp in Arguments )
         {
            text = text.Replace( kvp.Key, kvp.Value );
         }
         return text;
      }

      public string PrepareUntranslatedText( string untranslatedText )
      {
         foreach( var kvp in Arguments )
         {
            var key = kvp.Key;
            var translatorFriendlyKey = CreateTranslatorFriendlyKey( key );

            untranslatedText = untranslatedText.Replace( key, translatorFriendlyKey );
         }
         return untranslatedText;
      }

      public string FixTranslatedText( string translatedText )
      {
         foreach( var kvp in Arguments )
         {
            var key = kvp.Key;
            var translatorFriendlyKey = CreateTranslatorFriendlyKey( key );
            translatedText = ReplaceApproximateMatches( translatedText, translatorFriendlyKey, key );
         }
         return translatedText;
      }

      public static string CreateTranslatorFriendlyKey( string key )
      {
         var c = key[ 2 ];
         var translatorFriendlyKey = "ZM" + (char)( c + 2 ) + "Z";
         return translatorFriendlyKey;
      }

      public static string ReplaceApproximateMatches( string translatedText, string translatorFriendlyKey, string key )
      {
         var cidx = 0;
         var startIdx = 0;

         for( int i = 0; i < translatedText.Length; i++ )
         {
            var c = translatedText[ i ];
            if( c == ' ' || c == '　' ) continue;

            if( c == translatorFriendlyKey[ cidx ] )
            {
               if( cidx == 0 )
               {
                  startIdx = i;
               }

               cidx++;
            }
            else
            {
               cidx = 0;
               startIdx = 0;
            }

            if( cidx == translatorFriendlyKey.Length )
            {
               int endIdx = i + 1;

               var lengthOfKey = endIdx - startIdx;
               var diff = lengthOfKey - key.Length;

               translatedText = translatedText.Remove( startIdx, lengthOfKey ).Insert( startIdx, key );

               i -= diff;

               cidx = 0;
               startIdx = 0;
            }
         }

         return translatedText;
      }
   }
}
