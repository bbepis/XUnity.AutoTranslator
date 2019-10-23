using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   public static class RedirectedDirectory
   {
      private static readonly UnzippedDirectory UnzippedDirectory = new UnzippedDirectory(
         Settings.RedirectedResourcesPath,
         Settings.CacheMetadataForAllFiles );

      public static IEnumerable<RedirectedResource> GetFiles( string path, params string[] extensions )
      {
         return UnzippedDirectory.GetFiles( path, extensions );
      }

      public static IEnumerable<RedirectedResource> GetFile( string path )
      {
         return UnzippedDirectory.GetFile( path );
      }

      public static bool DirectoryExists( string path )
      {
         return UnzippedDirectory.DirectoryExists( path );
      }

      public static bool FileExists( string path )
      {
         return UnzippedDirectory.FileExists( path );
      }
   }
}
