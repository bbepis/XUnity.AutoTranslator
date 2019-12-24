using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Representation of a redirected resource that has a stream of data that can be opened.
   /// </summary>
   public class RedirectedResource
   {
      private readonly Func<Stream> _streamFactory;

      internal RedirectedResource( Func<Stream> streamFactory, string containerFile, string fullName )
      {
         _streamFactory = streamFactory;
         IsZipped = containerFile != null;
         ContainerFile = containerFile;
         FullName = fullName;
      }

      internal RedirectedResource( string fullName )
      {
         FullName = fullName;
         _streamFactory = () => File.OpenRead( FullName );
      }

      /// <summary>
      /// Gets a bool indicating if this redirected resource was zipped.
      /// </summary>
      public bool IsZipped { get; }

      /// <summary>
      /// If the redirected resource was zipped, this is the container file.
      /// </summary>
      public string ContainerFile { get; }

      /// <summary>
      /// Gets the full name of the file.
      /// </summary>
      public string FullName { get; }

      /// <summary>
      /// Opens a stream to the redirected resource.
      /// </summary>
      /// <returns></returns>
      public Stream OpenStream()
      {
         return _streamFactory();
      }
   }
}
