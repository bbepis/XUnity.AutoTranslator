using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Protocol message supplied to the external process to indicate
   /// a translation is requested.
   /// </summary>
   public class TranslationRequest : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "1";

      /// <summary>
      /// Gets or sets the source language.
      /// </summary>
      public string SourceLanguage { get; set; }

      /// <summary>
      /// Gets or sets the destination language.
      /// </summary>
      public string DestinationLanguage { get; set; }

      /// <summary>
      /// Gets or sets the untranslated text.
      /// </summary>
      public string[] UntranslatedTexts { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         SourceLanguage = reader.ReadLine();
         DestinationLanguage = reader.ReadLine();
         var count = int.Parse( reader.ReadLine(), CultureInfo.InvariantCulture );
         var untranslatedTexts = new string[ count ];
         for( int i = 0 ; i < count ; i++ )
         {
            var encodedUntranslatedText = reader.ReadLine();
            var untranslatedText = Encoding.UTF8.GetString( Convert.FromBase64String( encodedUntranslatedText ) );
            untranslatedTexts[ i ] = untranslatedText;
         }
         UntranslatedTexts = untranslatedTexts;
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.WriteLine( SourceLanguage );
         writer.WriteLine( DestinationLanguage );
         writer.WriteLine( UntranslatedTexts.Length.ToString( CultureInfo.InvariantCulture ) );
         foreach( var untranslatedText in UntranslatedTexts )
         {
            var encodedUntranslatedText = Convert.ToBase64String( Encoding.UTF8.GetBytes( untranslatedText ), Base64FormattingOptions.None );
            writer.WriteLine( encodedUntranslatedText );
         }
      }
   }
}
