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
         Text = text;
         Encoding = encoding;
      }

      /// <summary>
      /// Constructs a text with encoding from a byte array.
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="encoding"></param>
      public TextAndEncoding( byte[] bytes, Encoding encoding )
      {
         Bytes = bytes;
         Encoding = encoding;
      }

      /// <summary>
      /// Constructs a TextAndEncoding representing a binary piece of data with
      /// no encoding.
      /// </summary>
      /// <param name="bytes"></param>
      public TextAndEncoding( byte[] bytes )
      {
         Bytes = bytes;
      }

      /// <summary>
      /// Gets the text.
      /// </summary>
      public string Text { get; }

      /// <summary>
      /// Gets the bytes.
      /// </summary>
      public byte[] Bytes { get; }

      /// <summary>
      /// Gets the encoding the text is supposed to be stored with.
      /// </summary>
      public Encoding Encoding { get; }
   }
}
