using System;
using System.Globalization;
using System.IO;
using XUnity.Common.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the ResourceLoaded hook.
   /// </summary>
   public class ResourceLoadedContext : IAssetOrResourceLoadedContext
   {
      internal ResourceLoadedContext( ResourceLoadedParameters parameters, UnityEngine.Object[] assets )
      {
         Parameters = parameters;
         Assets = assets;
      }

      /// <summary>
      /// Gets a bool indicating if this resource has already been redirected before.
      /// </summary>
      public bool HasReferenceBeenRedirectedBefore( UnityEngine.Object asset )
      {
         return asset.GetExtensionData<ResourceExtensionData>()?.HasBeenRedirected == true;
      }

      /// <summary>
      /// Gets a file system path for the specfic asset that should be unique.
      /// </summary>
      /// <param name="asset"></param>
      /// <returns></returns>
      public string GetUniqueFileSystemAssetPath( UnityEngine.Object asset )
      {
         var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
         if( ext.FullFileSystemAssetPath == null )
         {
            string path = string.Empty;

            if( !string.IsNullOrEmpty( Parameters.Path ) )
            {
               path = Parameters.Path.ToLowerInvariant();
            }

            if( Parameters.LoadType == ResourceLoadType.LoadByType )
            {
               var assetName = asset.name;
               if( !string.IsNullOrEmpty( assetName ) )
               {
                  path = Path.Combine( path, assetName.ToLowerInvariant() );
               }
               else
               {
                  string suffix = null;
                  if( Assets.Length > 1 )
                  {
                     var idx = Array.IndexOf( Assets, asset );
                     if( idx == -1 )
                     {
                        suffix = "_with_unknown_index";
                     }
                     else
                     {
                        suffix = "_" + idx.ToString( CultureInfo.InvariantCulture );
                     }
                  }

                  path = Path.Combine( path, "unnamed_asset" + suffix );
               }
            }


            path = path.UseCorrectDirectorySeparators();

            ext.FullFileSystemAssetPath = path;
         }

         return ext.FullFileSystemAssetPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset/resource load should be called.
      /// </summary>
      /// <param name="skipRemainingPostfixes">Indicate if any other hooks should be skipped.</param>
      public void Complete( bool skipRemainingPostfixes = true )
      {
         SkipRemainingPostfixes = skipRemainingPostfixes;
      }

      /// <summary>
      /// Disables recursive calls if you make an asset/asset bundle load call
      /// from within your callback. If you want to prevent recursion this should
      /// be called before you load the asset/asset bundle.
      /// </summary>
      public void DisableRecursion()
      {
         ResourceRedirection.RecursionEnabled = false;
      }

      /// <summary>
      /// Gets the parameters the asset load call was called with.
      /// </summary>
      public ResourceLoadedParameters Parameters { get; }

      /// <summary>
      /// Gets the loaded assets. Override individual indices to change the asset reference that will be loaded.
      ///
      /// Consider using this if the load type is 'LoadByType' and you subscribed with 'OneCallbackPerLoadCall'.
      /// </summary>
      public UnityEngine.Object[] Assets { get; set; }

      /// <summary>
      /// Gets the loaded asset. This is simply equal to the first index of the Assets property, with some
      /// additional null guards to prevent NullReferenceExceptions when using it.
      /// </summary>
      public UnityEngine.Object Asset
      {
         get
         {
            if( Assets == null || Assets.Length < 1 )
            {
               return null;
            }
            return Assets[ 0 ];
         }
         set
         {
            if( Assets == null || Assets.Length < 1 )
            {
               Assets = new UnityEngine.Object[ 1 ];
            }
            Assets[ 0 ] = value;
         }
      }

      internal bool SkipRemainingPostfixes { get; set; }
   }
}
