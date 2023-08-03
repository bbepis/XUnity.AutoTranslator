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

      public string FixTranslatedText( string translatedText, bool useTranslatorFriendlyArgs )
      {
         foreach( var kvp in Arguments )
         {
            var key = kvp.Key;
            var translatorFriendlyKey = useTranslatorFriendlyArgs ? CreateTranslatorFriendlyKey( key ) : key;
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
            var tfKeyMaxIndex = translatorFriendlyKey.Length - 1;
            var cidx = tfKeyMaxIndex;
            var endIndex = tfKeyMaxIndex;

            var maxIndex = translatedText.Length - 1;
            for (int i = maxIndex; i >= 0; i--)
            {
                var c = translatedText[i];
                if (c == ' ' || c == '　') continue;

                if ((c = char.ToUpperInvariant(c)) == char.ToUpperInvariant(translatorFriendlyKey[cidx])
                    || c == char.ToUpperInvariant(translatorFriendlyKey[cidx = tfKeyMaxIndex]))
                {
                    if (cidx == tfKeyMaxIndex) endIndex = i;

                    cidx--;
                }

                if (cidx >= 0) continue;

                var lengthOfFriendlyKeyPlusSpaces = (endIndex + 1) - i;
                translatedText = translatedText.Remove(i, lengthOfFriendlyKeyPlusSpaces).Insert(i, key);

                cidx = tfKeyMaxIndex;
            }

            return translatedText;
      }
   }
}
