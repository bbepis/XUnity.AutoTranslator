using System.Collections.Generic;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// A text and the encoding it is supposed to be stored with.
   /// </summary>
   public class TextAndEncoding
   {
      /// <summary>Constructs a new text with encoding.</summary>
      /// <param name="text"/>
      /// <param name="encoding">Encoding to be use when auto-encoding <c>Text</c> into <c>Bytes</c> (pass <c>null</c> to disable auto-encoding)</param>
      /// <param name="bytes"/>
      public TextAndEncoding( string text, Encoding encoding, IEnumerable<byte> bytes = null )
      {
         Text = text;
         Encoding = encoding;
         Bytes = bytes;
      }

      /// <summary>Constructs a new text with encoding.</summary>
      /// <param name="bytes"/>
      public TextAndEncoding( IEnumerable<byte> bytes ) : this( null, null, bytes ) { }

      /// <summary>
      /// Gets the text.
      /// </summary>
      public string Text { get; }

      /// <summary>
      ///   <para>Gets the encoding used when storing <c>Text</c> into <c>Bytes</c>.</para>
      ///   <para><c>Bytes</c> will be left untouched if it set directly or if <c>Encoding</c> is <c>null</c>.</para>
      /// </summary>
      public Encoding Encoding { get; }

      /// <summary>
      /// Gets the bytes.
      /// </summary>
      public IEnumerable<byte> Bytes { get;  }
   }
}
