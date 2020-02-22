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
      /// Gets the text.
      /// </summary>
      public string Text { get; }

      /// <summary>
      /// Gets the encoding the text is supposed to be stored with.
      /// </summary>
      public Encoding Encoding { get; }
   }
}
