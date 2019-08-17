namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Shared interface between AssetLoadedContext and ResourceLoadedContext.
   /// </summary>
   public interface IAssetOrResourceLoadedContext
   {
      /// <summary>
      /// Gets a bool indicating if this resource has already been redirected before.
      /// </summary>
      bool HasReferenceBeenRedirectedBefore( UnityEngine.Object asset );

      /// <summary>
      /// Gets a file system path for the specfic asset that should be unique.
      /// </summary>
      /// <param name="asset"></param>
      /// <returns></returns>
      string GetUniqueFileSystemAssetPath( UnityEngine.Object asset );

      /// <summary>
      /// Gets the loaded assets. Override individual indices to change the asset reference that will be loaded.
      /// </summary>
      UnityEngine.Object[] Assets { get; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      bool Handled { get; set; }
   }
}
