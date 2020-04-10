using System.IO;
using System.Text;
using System.Xml.Schema;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   internal class TextAssetExtensionData
   {
      private byte[] _data;
      private bool roundTripText = false;
      private Encoding _encoding;

      public Encoding Encoding
      {
         get
         {
            return _encoding ?? Encoding.UTF8;
         }
         set
         {
            _encoding = value;
         }
      }

      public byte[] Data
      {
         get
         {
            return _data;
         }
         set
         {
            _data = value;
            try
            {
               roundTripText = _data != null && _data == Encoding.GetBytes( Encoding.GetString( _data ) );
            }
            catch( DecoderFallbackException )
            {
               roundTripText = false;
            }
         }
      }

      public string Text
      {
         get
         {
            if( roundTripText && _data?.Length > 0 )
            {
               try
               {
                  return Encoding.GetString( _data );
               }
               catch( DecoderFallbackException ) { }
            }
            return null;
         }
         set
         {
            roundTripText = true;
            if( value is null )
            {
               _data = null;
            }
            else
            {
               var stream = new MemoryStream();
               using( var writer = new StreamWriter( stream, Encoding ) )
               {
                  writer.Write( value );
                  writer.Flush();

                  _data = stream.ToArray();
               }
            }
         }
      }
   }
}
