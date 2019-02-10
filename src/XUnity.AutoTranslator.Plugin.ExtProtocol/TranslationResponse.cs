using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Protocol message returned to a translator when a translation completed.
   /// </summary>
   public class TranslationResponse : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "2";

      /// <summary>
      /// Gets or sets the translated text.
      /// </summary>
      public string[] TranslatedTexts { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         var count = int.Parse( reader.ReadLine(), CultureInfo.InvariantCulture );
         var translatedTexts = new string[ count ];
         for( int i = 0 ; i < count ; i++ )
         {
            var encodedTranslatedText = reader.ReadLine();
            var translatedText = Encoding.UTF8.GetString( Convert.FromBase64String( encodedTranslatedText ) );
            translatedTexts[ i ] = translatedText;
         }
         TranslatedTexts = translatedTexts;
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.WriteLine( TranslatedTexts.Length.ToString( CultureInfo.InvariantCulture ) );
         foreach( var translatedText in TranslatedTexts )
         {
            var encodedTranslatedText = Convert.ToBase64String( Encoding.UTF8.GetBytes( translatedText ), Base64FormattingOptions.None );
            writer.WriteLine( encodedTranslatedText );
         }
      }
   }
}
