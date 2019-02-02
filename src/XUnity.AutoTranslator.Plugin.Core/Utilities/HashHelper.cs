using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class HashHelper
   {
      private static readonly SHA1Managed SHA1 = new SHA1Managed();
      private static readonly uint[] Lookup32 = CreateLookup32();


      public static string Compute( byte[] data )
      {
         var hash = SHA1.ComputeHash( data );
         var hex = ByteArrayToHexViaLookup32( hash );
         return hex.Substring( 0, 10 );
      }

      private static uint[] CreateLookup32()
      {
         var result = new uint[ 256 ];
         for( int i = 0 ; i < 256 ; i++ )
         {
            string s = i.ToString( "X2" );
            result[ i ] = ( (uint)s[ 0 ] ) + ( (uint)s[ 1 ] << 16 );
         }
         return result;
      }

      private static string ByteArrayToHexViaLookup32( byte[] bytes )
      {
         var lookup32 = Lookup32;
         var result = new char[ bytes.Length * 2 ];
         for( int i = 0 ; i < bytes.Length ; i++ )
         {
            var val = lookup32[ bytes[ i ] ];
            result[ 2 * i ] = (char)val;
            result[ 2 * i + 1 ] = (char)( val >> 16 );
         }
         return new string( result );
      }
   }
}
