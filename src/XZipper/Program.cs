using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.IO;

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

         using( ZipFile zip = new ZipFile( args[ 1 ] ) )
         {
            zip.CompressionLevel = CompressionLevel.BestCompression;

            zip.AddDirectory( args[ 0 ] );
            zip.Save();
         }
      }
   }
}
