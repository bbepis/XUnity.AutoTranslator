using System;
using System.Globalization;
using System.IO;
using System.Linq;

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

      private string[] _untranslatedTexts;

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
      public string[] UntranslatedTexts => _untranslatedTexts ?? ( _untranslatedTexts = UntranslatedTextInfos.Select( x => x.UntranslatedText ).ToArray() );

      /// <summary>
      /// Gets or sets the untranslated text.
      /// </summary>
      public TransmittableUntranslatedTextInfo[] UntranslatedTextInfos { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         SourceLanguage = reader.ReadLine();
         DestinationLanguage = reader.ReadLine();
         var count = int.Parse( reader.ReadLine(), CultureInfo.InvariantCulture );
         var untranslatedTextInfos = new TransmittableUntranslatedTextInfo[ count ];
         for( int i = 0; i < count; i++ )
         {
            var untranslatedTextInfo = new TransmittableUntranslatedTextInfo();
            untranslatedTextInfo.Decode( reader );
            untranslatedTextInfos[ i ] = untranslatedTextInfo;
         }
         UntranslatedTextInfos = untranslatedTextInfos;
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.WriteLine( SourceLanguage );
         writer.WriteLine( DestinationLanguage );
         writer.WriteLine( UntranslatedTextInfos.Length.ToString( CultureInfo.InvariantCulture ) );
         foreach( var untranslatedText in UntranslatedTextInfos )
         {
            untranslatedText.Encode( writer );
         }
      }
   }
}
