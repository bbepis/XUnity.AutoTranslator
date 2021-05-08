using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class ZipFileExtensions
   {
      public static List<ZipEntry> GetEntries( this ZipFile zf )
      {
         var entries = new List<ZipEntry>();
         foreach( ZipEntry entry in zf )
         {
            entries.Add( entry );
         }
         return entries;
      }
   }
}
