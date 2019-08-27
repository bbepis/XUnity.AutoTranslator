using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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
         var ascii = Encoding.ASCII.GetString( assetBundleData, 0, Mathf.Min( 1024, assetBundleData.Length - 4 ) );

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
   }
}
