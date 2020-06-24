using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class CabHelper
   {
      private static string CreateRandomCab()
      {
         return "CAB-" + Guid.NewGuid().ToString( "N" );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="assetBundleData"></param>
      public static void RandomizeCab( byte[] assetBundleData )
      {
         var ascii = Encoding.ASCII.GetString( assetBundleData, 0, Math.Min( 1024, assetBundleData.Length - 4 ) );

         var origCabIndex = ascii.IndexOf( "CAB-", StringComparison.Ordinal );
         if( origCabIndex < 0 )
            return;

         var origCabLength = ascii.Substring( origCabIndex ).IndexOf( '\0' );
         if( origCabLength < 0 || origCabLength > 36 )
            return;

         var cab = CreateRandomCab();
         var cabBytes = Encoding.ASCII.GetBytes( cab );
         Buffer.BlockCopy( cabBytes, 36 - origCabLength, assetBundleData, origCabIndex, origCabLength );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="assetBundleData"></param>
      public static void RandomizeCabWithAnyLength( byte[] assetBundleData )
      {
         FindAndReplaceCab( "CAB-", 0, assetBundleData, 2048 );
      }

      private static void FindAndReplaceCab( string ansiStringToStartWith, byte byteToEndWith, byte[] data, int maxIterations = -1 )
      {
         var len = Math.Min( data.Length, maxIterations );
         if( len == -1 )
         {
            len = data.Length;
         }

         int pos = 0;
         char c;
         byte b;
         int searchLen = ansiStringToStartWith.Length;
         var newCab = Guid.NewGuid().ToString( "N" );
         int cabIdx = 0;

         for( int i = 0; i < len; i++ )
         {
            b = data[ i ];
            c = (char)b;

            if( pos == searchLen )
            {
               while( data[ i ] != byteToEndWith && i < len )
               {
                  if( cabIdx >= newCab.Length )
                  {
                     cabIdx = 0;
                     newCab = Guid.NewGuid().ToString( "N" );
                  }

                  data[ i++ ] = (byte)newCab[ cabIdx++ ];
               }

               break;
            }
            else if( c == ansiStringToStartWith[ pos ] )
            {
               pos++;
            }
            else
            {
               pos = 0;
            }
         }
      }
   }
}
