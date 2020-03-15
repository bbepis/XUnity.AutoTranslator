using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   internal class UnzippedDirectory : IDisposable
   {
      private static readonly char[] PathSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
      private static readonly string _loweredCurrentDirectory = Paths.GameRoot.ToLowerInvariant();
      private readonly string _root;
      private readonly bool _cacheNormalFiles;
      private DirectoryEntry _rootDirectory;
      private List<ZipFile> _allZipFiles;

      public UnzippedDirectory( string root, bool cacheNormalFiles )
      {
         _cacheNormalFiles = cacheNormalFiles;
         _allZipFiles = new List<ZipFile>();

         Directory.CreateDirectory( root );

         if( Path.IsPathRooted( root ) )
         {
            _root = root.ToLowerInvariant();
         }
         else
         {
            _root = Path.Combine( _loweredCurrentDirectory, root.ToLowerInvariant() );
         }
         Initialize();
      }

      public IEnumerable<RedirectedResource> GetFiles( string path, params string[] extensions )
      {
         if( !_cacheNormalFiles )
         {
            if( Directory.Exists( path ) )
            {
               var noExtensions = extensions == null || extensions.Length == 0;
               var files = Directory.GetFiles( path, "*", SearchOption.TopDirectoryOnly );
               foreach( var file in files )
               {
                  if( noExtensions || extensions.Any( x => file.EndsWith( x, StringComparison.OrdinalIgnoreCase ) ) )
                  {
                     yield return new RedirectedResource( file );
                  }
               }
            }
         }

         if( _rootDirectory != null )
         {
            path = path.ToLowerInvariant();
            if( !Path.IsPathRooted( path ) )
            {
               path = Path.Combine( _loweredCurrentDirectory, path );
            }
            path = path.MakeRelativePath( _root );

            var entries = _rootDirectory.GetFiles( path, null, true )
               .OrderBy( x => x.IsZipped )
               .ThenBy( x => x.ContainerFile )
               .ThenBy( x => x.FullPath )
               .ToList();

            foreach( var entry in entries )
            {
               if( entry.IsZipped )
               {
                  yield return new RedirectedResource( () => entry.ZipFile.GetInputStream( entry.ZipEntry ), entry.ContainerFile, entry.FullPath );
               }
               else
               {
                  yield return new RedirectedResource( entry.FileName );
               }
            }
         }
      }

      public IEnumerable<RedirectedResource> GetFile( string path )
      {
         if( !_cacheNormalFiles )
         {
            if( File.Exists( path ) )
            {
               yield return new RedirectedResource( path );
            }
         }

         if( _rootDirectory != null )
         {
            path = path.ToLowerInvariant();
            if( !Path.IsPathRooted( path ) )
            {
               path = Path.Combine( _loweredCurrentDirectory, path );
            }
            path = path.MakeRelativePath( _root );

            var entries = _rootDirectory.GetFiles( path, null, false )
               .OrderBy( x => x.IsZipped )
               .ThenBy( x => x.ContainerFile )
               .ThenBy( x => x.FullPath );

            foreach( var entry in entries )
            {
               if( entry.IsZipped )
               {
                  yield return new RedirectedResource( () => entry.ZipFile.GetInputStream( entry.ZipEntry ), entry.ContainerFile, entry.FullPath );
               }
               else
               {
                  yield return new RedirectedResource( entry.FileName );
               }
            }
         }
      }

      public bool DirectoryExists( string path )
      {
         var originalPath = path;

         var exists = false;
         if( _rootDirectory != null )
         {
            path = path.ToLowerInvariant();
            if( !Path.IsPathRooted( path ) )
            {
               path = Path.Combine( _loweredCurrentDirectory, path );
            }
            path = path.MakeRelativePath( _root );

            exists = _rootDirectory.DirectoryExists( path );
         }

         return exists || ( !_cacheNormalFiles && Directory.Exists( originalPath ) );
      }

      public bool FileExists( string path )
      {
         var originalPath = path;

         var exists = false;
         if( _rootDirectory != null )
         {
            path = path.ToLowerInvariant();
            if( !Path.IsPathRooted( path ) )
            {
               path = Path.Combine( _loweredCurrentDirectory, path );
            }
            path = path.MakeRelativePath( _root );

            exists = _rootDirectory.FileExists( path );
         }

         return exists || ( !_cacheNormalFiles && File.Exists( originalPath ) );
      }

      private void Initialize()
      {
         if( Directory.Exists( _root ) )
         {
            var files = Directory.GetFiles( _root, "*", SearchOption.AllDirectories );

            if( files.Length > 0 )
            {
               _rootDirectory = new DirectoryEntry();
            }

            foreach( var file in files )
            {
               var isZip = file.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase );

               if( isZip || ( _cacheNormalFiles && !isZip ) )
               {
                  var current = _rootDirectory;
                  var relativePath = file.ToLowerInvariant().MakeRelativePath( _root );
                  var parts = relativePath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries );

                  for( int i = 0; i < parts.Length; i++ )
                  {
                     var part = parts[ i ];

                     if( i == parts.Length - 1 )
                     {
                        if( part.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) )
                        {
                           // this is the zip file, read the metadata!
                           var start = current;

                           var zf = new ZipFile( file );
                           _allZipFiles.Add( zf );

                           foreach( ZipEntry entry in zf )
                           {
                              current = start;
                              var internalPath = entry.Name.UseCorrectDirectorySeparators().ToLowerInvariant();
                              var internalParts = internalPath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries );
                              for( int j = 0; j < internalParts.Length; j++ )
                              {
                                 var internalPart = internalParts[ j ];
                                 if( j == internalParts.Length - 1 )
                                 {
                                    if( entry.IsFile )
                                    {
                                       current.AddFile( internalPart, new FileEntry( internalPath, file, zf, entry ) );
                                    }
                                    else
                                    {
                                       current = current.GetOrCreateDirectory( internalPart );
                                    }
                                 }
                                 else
                                 {
                                    current = current.GetOrCreateDirectory( internalPart );
                                 }
                              }

                           }
                        }
                        else
                        {
                           current.AddFile( part, new FileEntry( file ) );
                        }
                     }
                     else
                     {
                        current = current.GetOrCreateDirectory( part );
                     }
                  }
               }
            }
         }
      }

      private class FileEntry
      {
         private string _fullPath;

         public FileEntry( string fileName, string containerFile, ZipFile zipFile, ZipEntry zipEntry )
         {
            FileName = fileName;
            ContainerFile = containerFile;
            ZipFile = zipFile;
            ZipEntry = zipEntry;
         }

         public FileEntry( string fileName )
         {
            FileName = fileName;
         }

         public string FileName { get; }

         public string ContainerFile { get; }

         public ZipFile ZipFile { get; }

         public ZipEntry ZipEntry { get; }

         public bool IsZipped => ContainerFile != null;

         public string FullPath
         {
            get
            {
               if( _fullPath == null )
               {
                  if( ContainerFile != null )
                  {
                     _fullPath = Path.Combine( ContainerFile, FileName );
                  }
                  else
                  {
                     _fullPath = FileName;
                  }
               }
               return _fullPath;
            }
         }
      }

      private class DirectoryEntry
      {
         private Dictionary<string, DirectoryEntry> _directories = new Dictionary<string, DirectoryEntry>( StringComparer.OrdinalIgnoreCase );
         private Dictionary<string, List<FileEntry>> _files = new Dictionary<string, List<FileEntry>>( StringComparer.OrdinalIgnoreCase );

         public DirectoryEntry GetOrCreateDirectory( string name )
         {
            if( !_directories.TryGetValue( name, out var zpr ) )
            {
               zpr = new DirectoryEntry();
               _directories.Add( name, zpr );
            }

            return zpr;
         }

         public DirectoryEntry GetDirectory( string name )
         {
            _directories.TryGetValue( name, out var zpr );
            return zpr;
         }

         public void AddFile( string name, FileEntry entry )
         {
            if( !_files.TryGetValue( name, out var files ) )
            {
               files = new List<FileEntry>();
               _files.Add( name, files );
            }
            files.Add( entry );
         }

         public List<FileEntry> GetFiles( string fullPath, string[] extensions, bool findAllByExtensionInLastDirectory )
         {
            var entries = new List<FileEntry>();
            var parts = fullPath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries );

            FillEntries( parts, 0, extensions, findAllByExtensionInLastDirectory, entries );

            return entries;
         }

         public bool DirectoryExists( string fullPath )
         {
            var current = this;

            var parts = fullPath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < parts.Length; i++ )
            {
               var part = parts[ i ];

               current = current.GetDirectory( part );

               if( current == null ) return false;
            }

            return true;
         }

         public bool FileExists( string fullPath )
         {
            var current = this;

            var parts = fullPath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < parts.Length; i++ )
            {
               var part = parts[ i ];

               if( i == parts.Length - 1 )
               {
                  return current._files.ContainsKey( part );
               }
               else
               {
                  current = current.GetDirectory( part );

                  if( current == null ) return false;
               }
            }

            return true;
         }

         private void FillEntries( string[] parts, int index, string[] extensions, bool findAllByExtensionInLastDirectory, List<FileEntry> entries )
         {
            if( index < parts.Length )
            {
               var part = parts[ index ];
               if( !findAllByExtensionInLastDirectory && index == parts.Length - 1 )
               {
                  // find the files with part name....
                  if( _files.TryGetValue( part, out var files ) )
                  {
                     entries.AddRange( files );
                  }
               }
               else
               {
                  if( _directories.TryGetValue( part, out var subPath ) )
                  {
                     subPath.FillEntries( parts, index + 1, extensions, findAllByExtensionInLastDirectory, entries );
                  }
               }
            }
            else if( findAllByExtensionInLastDirectory )
            {
               if( extensions == null || extensions.Length == 0 )
               {
                  foreach( var kvp in _files )
                  {
                     entries.AddRange( kvp.Value );
                  }
               }
               else
               {
                  foreach( var kvp in _files )
                  {
                     var fileName = kvp.Key;
                     if( extensions.Any( x => fileName.EndsWith( x, StringComparison.OrdinalIgnoreCase ) ) )
                     {
                        entries.AddRange( kvp.Value );
                     }
                  }
               }
            }
         }
      }

      #region IDisposable Support
      private bool disposedValue = false; // To detect redundant calls

      protected virtual void Dispose( bool disposing )
      {
         if( !disposedValue )
         {
            if( disposing )
            {
               // TODO: dispose managed state (managed objects).
            }

            foreach( var zf in _allZipFiles )
            {
               zf.Close();
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
         }
      }

      // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
      // ~UnzippedDirectory() {
      //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      //   Dispose(false);
      // }

      // This code added to correctly implement the disposable pattern.
      public void Dispose()
      {
         // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
         Dispose( true );
         // TODO: uncomment the following line if the finalizer is overridden above.
         // GC.SuppressFinalize(this);
      }
      #endregion
   }
}
