using System.IO;
using System.Text;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers.Zip;

namespace XZipper
{
   class Program
   {
      static void Main( string[] args )
      {
         if( File.Exists( args[ 1 ] ) )
         {
            File.Delete( args[ 1 ] );
         }

         using var zip = ZipArchive.CreateArchive();
         zip.AddAllFromDirectory( args[ 0 ] );
         zip.SaveTo( args[ 1 ], new ZipWriterOptions( CompressionType.Deflate )
         {
            ArchiveEncoding = new ArchiveEncoding()
            {
               Default = Encoding.UTF8,
               Password = Encoding.UTF8
            },
            CompressionLevel = (int)CompressionLevel.BestCompression,
            UseZip64 = false
         } );
      }
   }
}
