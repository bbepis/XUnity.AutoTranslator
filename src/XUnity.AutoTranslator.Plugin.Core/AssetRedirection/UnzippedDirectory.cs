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
   internal class UnzippedDirectory
   {
      private static readonly string _loweredCurrentDirectory = Paths.GameRoot.ToLowerInvariant();
      private readonly string _root;
      private readonly bool _cacheNormalFiles;
      private PathReference _rootZipDir;

      public UnzippedDirectory( string root, bool cacheNormalFiles )
      {
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
         _cacheNormalFiles = cacheNormalFiles;
      }

      public IEnumerable<RedirectedResource> GetFiles( string path, params string[] extensions )
      {
         if( !_cacheNormalFiles )
         {
            if( extensions == null || extensions.Length == 0 )
            {
               var files = Directory.GetFiles( path, "*", SearchOption.TopDirectoryOnly );
               foreach( var file in files )
               {
                  yield return new RedirectedResource( () => File.OpenRead( file ), null, file );
               }
            }
            else
            {
               foreach( var extension in extensions )
               {
                  var files = Directory.GetFiles( path, "*" + extension, SearchOption.TopDirectoryOnly );
                  foreach( var file in files )
                  {
                     yield return new RedirectedResource( () => File.OpenRead( file ), null, file );
                  }
               }
            }
         }

         if( _rootZipDir != null )
         {
            var fullPath = Path.Combine( _loweredCurrentDirectory, path.ToLowerInvariant() )
               .MakeRelativePath( _root );

            var entries = _rootZipDir.GetEntries( fullPath, extensions, true )
               .OrderBy( x => x.IsZipped )
               .ThenBy( x => x.FullPath );

            foreach( var entry in entries )
            {
               if( entry.IsZipped )
               {
                  yield return new RedirectedResource( () => new StoredZipEntryStream( File.OpenRead( entry.ContainerFile ), entry.Offset, entry.Size ), entry.ContainerFile, entry.FullPath );
               }
               else
               {
                  yield return new RedirectedResource( () => File.OpenRead( entry.FileName ), null, entry.FullPath );
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
               yield return new RedirectedResource( () => File.OpenRead( path ), null, path );
            }
         }

         if( _rootZipDir != null )
         {
            var fullPath = Path.Combine( _loweredCurrentDirectory, path.ToLowerInvariant() )
               .MakeRelativePath( _root );

            var entries = _rootZipDir.GetEntries( fullPath, null, false )
               .OrderBy( x => x.IsZipped )
               .ThenBy( x => x.FullPath );

            foreach( var entry in entries )
            {
               if( entry.IsZipped )
               {
                  yield return new RedirectedResource( () => new StoredZipEntryStream( File.OpenRead( entry.ContainerFile ), entry.Offset, entry.Size ), entry.ContainerFile, entry.FullPath );
               }
               else
               {
                  yield return new RedirectedResource( () => File.OpenRead( entry.FileName ), null, entry.FullPath );
               }
            }
         }
      }

      public bool DirectoryExists( string path )
      {
         var exists = false;
         if( _rootZipDir != null )
         {
            var fullPath = Path.Combine( _loweredCurrentDirectory, path.ToLowerInvariant() )
               .MakeRelativePath( _root );

            exists = _rootZipDir.DirectoryExists( fullPath );
         }

         return exists || ( !_cacheNormalFiles && Directory.Exists( path ) );
      }

      public bool FileExists( string path )
      {
         var exists = false;
         if( _rootZipDir != null )
         {
            var fullPath = Path.Combine( _loweredCurrentDirectory, path.ToLowerInvariant() )
               .MakeRelativePath( _root );

            exists = _rootZipDir.FileExists( fullPath );
         }

         return exists || ( !_cacheNormalFiles && File.Exists( path ) );
      }

      private void Initialize()
      {
         var fullPath = Path.Combine( _loweredCurrentDirectory, _root );
         var files = Directory.GetFiles( fullPath, "*", SearchOption.AllDirectories );

         if( files.Length > 0 )
         {
            _rootZipDir = new PathReference();
         }

         foreach( var file in files )
         {
            if( !_cacheNormalFiles && !file.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) )
            {
               // skip any files that are not .zip files if we do not cache them all
               continue;
            }

            var current = _rootZipDir;
            var relativePath = file.ToLowerInvariant().MakeRelativePath( _root );
            var parts = relativePath.Split( new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );

            for( int i = 0; i < parts.Length; i++ )
            {
               var part = parts[ i ];

               if( i == parts.Length - 1 )
               {
                  if( part.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) )
                  {
                     // this is the zip file, read the metadata!
                     part = Path.GetFileNameWithoutExtension( part );
                     current = current.GetOrCreateChildPath( part );
                     var start = current;

                     var zf = new ZipFile( file );
                     var initialOffset = (long)zf.GetType().GetField( "offsetOfFirstEntry", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( zf );
                     foreach( ZipEntry entry in zf )
                     {
                        if( entry.CompressionMethod != CompressionMethod.Stored )
                        {
                           XuaLogger.AutoTranslator.Warn( "Found .zip file in RedirectedResources directory that is not using 'Stored' compression: " + file );
                           break;
                        }

                        current = start;
                        var internalPath = entry.Name.Replace( '/', '\\' ).ToLowerInvariant();
                        var internalParts = internalPath.Split( new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );
                        for( int j = 0; j < internalParts.Length; j++ )
                        {
                           var internalPart = internalParts[ j ];
                           if( j == internalParts.Length - 1 )
                           {
                              if( entry.IsFile )
                              {
                                 current.AddFile( internalPart, new FileEntry( internalPath, file, initialOffset + entry.Offset, entry.Size ) );
                              }
                              else
                              {
                                 current = current.GetOrCreateChildPath( internalPart );
                              }
                           }
                           else
                           {
                              current = current.GetOrCreateChildPath( internalPart );
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
                  current = current.GetOrCreateChildPath( part );
               }
            }
         }

      }

      private class FileEntry
      {
         private string _fullPath;

         public FileEntry( string fileName, string containerFile, long offset, long size )
         {
            FileName = fileName;
            ContainerFile = containerFile;
            Offset = offset;
            Size = size;
         }

         public FileEntry( string fileName )
         {
            FileName = fileName;
         }

         public long Offset { get; }

         public long Size { get; }

         public string FileName { get; }

         public string ContainerFile { get; }

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

      private class PathReference
      {
         private Dictionary<string, PathReference> _subPaths = new Dictionary<string, PathReference>( StringComparer.OrdinalIgnoreCase );
         private Dictionary<string, List<FileEntry>> _files = new Dictionary<string, List<FileEntry>>( StringComparer.OrdinalIgnoreCase );

         public PathReference GetOrCreateChildPath( string name )
         {
            if( !_subPaths.TryGetValue( name, out var zpr ) )
            {
               zpr = new PathReference();
               _subPaths.Add( name, zpr );
            }

            return zpr;
         }

         public PathReference GetChildPath( string name )
         {
            _subPaths.TryGetValue( name, out var zpr );
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

         public List<FileEntry> GetEntries( string fullPath, string[] extensions, bool findAllByExtensionInLastDirectory )
         {
            var entries = new List<FileEntry>();
            var parts = fullPath.Split( new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );

            FillEntries( parts, 0, extensions, findAllByExtensionInLastDirectory, entries );

            return entries;
         }

         public bool DirectoryExists( string fullPath )
         {
            var current = this;

            var parts = fullPath.Split( new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < parts.Length; i++ )
            {
               var part = parts[ i ];

               current = current.GetChildPath( part );

               if( current == null ) return false;
            }

            return true;
         }

         public bool FileExists( string fullPath )
         {
            var current = this;

            var parts = fullPath.Split( new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < parts.Length; i++ )
            {
               var part = parts[ i ];

               if( i == parts.Length - 1 )
               {
                  return current._files.ContainsKey( part );
               }
               else
               {
                  current = current.GetChildPath( part );

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
               if( _subPaths.TryGetValue( part, out var subPath ) )
               {
                  subPath.FillEntries( parts, index + 1, extensions, findAllByExtensionInLastDirectory, entries );
               }

               // find the files with part name....
               if( _files.TryGetValue( part, out var files ) )
               {
                  entries.AddRange( files );
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

      private class StoredZipEntryStream : Stream
      {
         private Stream baseStream;
         private readonly long length;
         private long position;

         public StoredZipEntryStream( Stream baseStream, long offset, long length )
         {
            if( baseStream == null ) throw new ArgumentNullException( "baseStream" );
            if( !baseStream.CanRead ) throw new ArgumentException( "can't read base stream" );
            if( offset < 0 ) throw new ArgumentOutOfRangeException( "offset" );

            this.baseStream = baseStream;
            this.length = length;

            offset += 26;

            const int BUFFER_SIZE = 512;
            byte[] buffer = new byte[ BUFFER_SIZE ];
            if( baseStream.CanSeek )
            {
               baseStream.Seek( offset, SeekOrigin.Begin );
            }
            else
            { // read it manually...
               while( offset > 0 )
               {
                  int read = baseStream.Read( buffer, 0, offset < BUFFER_SIZE ? (int)offset : BUFFER_SIZE );
                  offset -= read;
               }
            }

            var fileNameLength = ReadLeShort();
            var extraFieldLength = ReadLeShort();

            var extraOffset = fileNameLength + extraFieldLength;
            if( baseStream.CanSeek )
            {
               baseStream.Seek( extraOffset, SeekOrigin.Current );
            }
            else
            { // read it manually...
               while( extraOffset > 0 )
               {
                  int read = baseStream.Read( buffer, 0, extraOffset < BUFFER_SIZE ? (int)extraOffset : BUFFER_SIZE );
                  extraOffset -= read;
               }
            }
         }

         private int ReadLeShort()
         {
            return ( baseStream.ReadByte() | ( baseStream.ReadByte() << 8 ) );
         }

         public override int Read( byte[] buffer, int offset, int count )
         {
            CheckDisposed();
            long remaining = length - position;
            if( remaining <= 0 ) return 0;
            if( remaining < count ) count = (int)remaining;
            int read = baseStream.Read( buffer, offset, count );
            position += read;
            return read;
         }
         private void CheckDisposed()
         {
            if( baseStream == null ) throw new ObjectDisposedException( GetType().Name );
         }
         public override long Length
         {
            get { CheckDisposed(); return length; }
         }
         public override bool CanRead
         {
            get { CheckDisposed(); return true; }
         }
         public override bool CanWrite
         {
            get { CheckDisposed(); return false; }
         }
         public override bool CanSeek
         {
            get { CheckDisposed(); return false; }
         }
         public override long Position
         {
            get
            {
               CheckDisposed();
               return position;
            }
            set { throw new NotSupportedException(); }
         }
         public override long Seek( long offset, SeekOrigin origin )
         {
            throw new NotSupportedException();
         }
         public override void SetLength( long value )
         {
            throw new NotSupportedException();
         }
         public override void Flush()
         {
            CheckDisposed(); baseStream.Flush();
         }
         protected override void Dispose( bool disposing )
         {
            base.Dispose( disposing );
            if( disposing )
            {
               if( baseStream != null )
               {
                  try { baseStream.Dispose(); }
                  catch { }
                  baseStream = null;
               }
            }
         }
         public override void Write( byte[] buffer, int offset, int count )
         {
            throw new NotImplementedException();
         }
      }
   }
}
