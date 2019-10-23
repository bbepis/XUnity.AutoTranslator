using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Simple facade for accessing files in the redirected directory, even if they are contained in a zip folder.
   /// </summary>
   public static class RedirectedDirectory
   {
      private static UnzippedDirectory UnzippedDirectory;

      private static void Initialize()
      {
         if( UnzippedDirectory == null )
         {
            UnzippedDirectory = new UnzippedDirectory(
              Settings.RedirectedResourcesPath,
              Settings.CacheMetadataForAllFiles );
         }
      }

      internal static void Uninitialize()
      {
         if( UnzippedDirectory != null )
         {
            UnzippedDirectory.Dispose();
            UnzippedDirectory = null;
         }
      }

      /// <summary>
      /// Gets all the files in the directory. May return multiple files with same name due to zip files. Unzipped files will be returned first.
      /// </summary>
      /// <param name="path">A standard file path, that MUST be point to a place in the redirected resource directory.</param>
      /// <param name="extensions">The extensions of the files to return. None if all files should be returned.</param>
      /// <returns>The redirected resources.</returns>
      public static IEnumerable<RedirectedResource> GetFilesInDirectory( string path, params string[] extensions )
      {
         Initialize();
         return UnzippedDirectory.GetFiles( path, extensions );
      }

      /// <summary>
      /// Gets the file at the specified path. May return multiple files due to zip files. Unzipped files will be returned first.
      /// </summary>
      /// <param name="path">A standard file path, that MUST be point to a place in the redirected resource directory.</param>
      /// <returns>The redirected resources.</returns>
      public static IEnumerable<RedirectedResource> GetFile( string path )
      {
         Initialize();
         return UnzippedDirectory.GetFile( path );
      }

      /// <summary>
      /// Checks if the directory exists in either the file system or in a zip file.
      /// </summary>
      /// <param name="path">A standard file path, that MUST be point to a place in the redirected resource directory.</param>
      /// <returns>A bool indicating if the directory exists.</returns>
      public static bool DirectoryExists( string path )
      {
         Initialize();
         return UnzippedDirectory.DirectoryExists( path );
      }

      /// <summary>
      /// Checks if the file exists in either the file system or in a zip file.
      /// </summary>
      /// <param name="path">A standard file path, that MUST be point to a place in the redirected resource directory.</param>
      /// <returns>A bool indicating if the file exists.</returns>
      public static bool FileExists( string path )
      {
         Initialize();
         return UnzippedDirectory.FileExists( path );
      }
   }
}
