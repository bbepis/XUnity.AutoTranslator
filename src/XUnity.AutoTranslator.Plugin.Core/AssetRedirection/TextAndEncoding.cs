using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// A text and the encoding it is supposed to be stored with.
   /// </summary>
   public class TextAndEncoding
   {
      /// <summary>
      /// Constructs a new text with encoding.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="encoding"></param>
      public TextAndEncoding( string text, Encoding encoding )
      {
         Encoding = encoding ?? Encoding.UTF8;
         if (text is null)
         {
            Bytes = null;
         }
         else
         {
            var stream = new MemoryStream();
            using( var writer = new StreamWriter( stream, Encoding ) )
            {
               writer.Write( text );
               writer.Flush();

               Bytes = stream.ToArray();
            }
         }
         roundTripText = true;
      }

      /// <summary>
      /// Constructs a new text with encoding.
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="encoding"></param>
      public TextAndEncoding( byte[] bytes, Encoding encoding )
      {
         Bytes = bytes;
         Encoding = encoding ?? Encoding.UTF8;
         try
         {
            roundTripText = Bytes != null && Bytes == Encoding.GetBytes( Encoding.GetString( Bytes ) );
         }
         catch ( System.ArgumentException )
         {
            roundTripText = false;
         }
      }

      /// <summary>
      /// Gets the text.
      /// </summary>
      public string Text
      {
         get
         { 
            if (roundTripText && Bytes?.Length > 0)
            {
               try
               {
                  return Encoding.GetString( Bytes );
               }
               catch( DecoderFallbackException ) { }
            }
            return null;
         }
      }

      /// <summary>
      /// Gets the bytes.
      /// </summary>
      public byte[] Bytes { get; }

      /// <summary>
      /// Gets the encoding the text is supposed to be stored with.
      /// </summary>
      public Encoding Encoding { get; }

      private readonly bool roundTripText = false;
   }
}
