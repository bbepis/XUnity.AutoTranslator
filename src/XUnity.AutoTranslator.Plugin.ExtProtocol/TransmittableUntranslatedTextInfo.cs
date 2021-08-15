using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Class representing an untranslated text and the contextual information surrounding it.
   /// </summary>
   public class TransmittableUntranslatedTextInfo
   {
      /// <summary>
      /// Constructs a transmittable untranslated text info.
      /// </summary>
      /// <param name="contextBefore"></param>
      /// <param name="untranslatedText"></param>
      /// <param name="contextAfter"></param>
      public TransmittableUntranslatedTextInfo( string[] contextBefore, string untranslatedText, string[] contextAfter )
      {
         ContextBefore = contextBefore;
         UntranslatedText = untranslatedText;
         ContextAfter = contextAfter;
      }

      /// <summary>
      /// Constructs a transmittable untranslated text info.
      /// </summary>
      public TransmittableUntranslatedTextInfo()
      {

      }

      /// <summary>
      /// The contextual untranslated text that preceeds the untranslated text.
      /// </summary>
      public string[] ContextBefore { get; set; }

      /// <summary>
      /// The untranslated text that should be translated.
      /// </summary>
      public string UntranslatedText { get; set; }

      /// <summary>
      /// The contextual untranslated text that comes after the untranslated text.
      /// </summary>
      public string[] ContextAfter { get; set; }

      internal void Encode( TextWriter writer )
      {
         writer.WriteLine( ( ContextBefore?.Length ?? 0 ).ToString( CultureInfo.InvariantCulture ) );
         if( ContextBefore != null )
         {
            foreach( var contextBefore in ContextBefore )
            {
               var encodedContextBefore = Convert.ToBase64String( Encoding.UTF8.GetBytes( contextBefore ), Base64FormattingOptions.None );
               writer.WriteLine( encodedContextBefore );
            }
         }

         writer.WriteLine( ( ContextAfter?.Length ?? 0 ).ToString( CultureInfo.InvariantCulture ) );
         if( ContextAfter != null )
         {
            foreach( var contextAfter in ContextAfter )
            {
               var encodedContextAfter = Convert.ToBase64String( Encoding.UTF8.GetBytes( contextAfter ), Base64FormattingOptions.None );
               writer.WriteLine( encodedContextAfter );
            }
         }

         var untranslatedText = Convert.ToBase64String( Encoding.UTF8.GetBytes( UntranslatedText ), Base64FormattingOptions.None );
         writer.WriteLine( untranslatedText );
      }

      internal void Decode( TextReader reader )
      {
         var contextBeforeCount = int.Parse( reader.ReadLine(), CultureInfo.InvariantCulture );
         var contextBefore = new string[ contextBeforeCount ];
         for( int i = 0; i < contextBeforeCount; i++ )
         {
            var base64 = reader.ReadLine();
            var line = Encoding.UTF8.GetString( Convert.FromBase64String( base64 ) );
            contextBefore[ i ] = line;
         }
         ContextBefore = contextBefore;

         var contextAfterCount = int.Parse( reader.ReadLine(), CultureInfo.InvariantCulture );
         var contextAfter = new string[ contextAfterCount ];
         for( int i = 0; i < contextAfterCount; i++ )
         {
            var base64 = reader.ReadLine();
            var line = Encoding.UTF8.GetString( Convert.FromBase64String( base64 ) );
            contextAfter[ i ] = line;
         }
         ContextAfter = contextAfter;

         var encodedUntranslatedText = reader.ReadLine();
         var untranslatedText = Encoding.UTF8.GetString( Convert.FromBase64String( encodedUntranslatedText ) );
         UntranslatedText = untranslatedText;
      }
   }
}
