using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   internal class TextAssetExtensionData
   {
      private string _text;
      private byte[] _data;

      public Encoding Encoding { get; set; }

      public byte[] Data
      {
         get
         {
            if( _data == null && _text != null )
            {
               var stream = new MemoryStream();
               using( var writer = new StreamWriter( stream, Encoding ?? System.Text.Encoding.UTF8 ) )
               {
                  writer.Write( _text );
                  writer.Flush();

                  _data = stream.ToArray();
               }
            }

            return _data;
         }
         set
         {
            _data = value;
         }
      }

      public string Text
      {
         get
         {
            if( _text == null && _data != null )
            {
               var stream = new MemoryStream( _data );
               using( var reader = new StreamReader( stream, Encoding ?? System.Text.Encoding.UTF8 ) )
               {
                  _text = reader.ReadToEnd();
               }
            }

            return _text;
         }
         set
         {
            _text = value;
         }
      }
   }
}
