using System;
using System.IO;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Translation package for a stream representing the standard text translation format in UTF-8.
   /// </summary>
   public sealed class StreamTranslationPackage : IDisposable
   {
      private Stream _cachedStream;

      /// <summary>
      /// Constructs the translation package.
      /// </summary>
      /// <param name="name">The name to be displayed when it is loaded.</param>
      /// <param name="stream">The stream to be loaded. The stream represents a standard text translation file in UTF-8 format.</param>
      /// <param name="allowMultipleIterations">A bool indicating if the enumerable can be iterated multiple times (due translation reload).</param>
      public StreamTranslationPackage( string name, Stream stream, bool allowMultipleIterations )
      {
         if( allowMultipleIterations && !stream.CanSeek )
         {
            throw new ArgumentException( "Cannot iterate a non-seekable stream multiple times.", nameof( allowMultipleIterations ) );
         }
         Name = name;
         Stream = stream;
         AllowMultipleIterations = allowMultipleIterations;
      }

      /// <summary>
      /// Gets the name of the the package.
      /// </summary>
      public string Name { get; }
      private Stream Stream { get; set; }
      private bool AllowMultipleIterations { get; }

      internal Stream GetReadableStream()
      {
         if( !AllowMultipleIterations )
         {
            if( _cachedStream == null )
            {
               _cachedStream = new MemoryStream( Stream.ReadFully( 0 ) );
               Stream.Dispose();
               Stream = null;
            }
            return _cachedStream;
         }
         return Stream;
      }

      #region IDisposable Support
      private bool disposedValue = false; // To detect redundant calls

      private void Dispose( bool disposing )
      {
         if( !disposedValue )
         {
            if( disposing )
            {
               Stream?.Dispose();
            }

            Stream = null;
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
         }
      }

      // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
      // ~StreamTranslationPackage()
      // {
      //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      //   Dispose(false);
      // }

      // This code added to correctly implement the disposable pattern.
      /// <summary>
      /// Disposes the translation package.
      /// </summary>
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
