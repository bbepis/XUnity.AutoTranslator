using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.ResourceRedirector.Examples
{
   class TextureReplacementPlugin
   {
      void Awake()
      {
         ResourceRedirection.RegisterAssetLoadedHook( HookBehaviour.OneCallbackPerResourceLoaded, AssetLoaded );
      }

      public void AssetLoaded( AssetLoadedContext context )
      {
         if( context.Asset is Texture2D texture2d ) // also acts as a null check
         {
            // TODO: Modify, replace or dump the texture

            context.Handled = true;
            context.Asset = texture2d; // only need to update the reference if you created a new texture
         }
      }
   }

   class AssetBundleRedirectorPlugin
   {
      void Awake()
      {
         ResourceRedirection.RegisterAssetBundleLoadingHook( AssetBundleLoading );
         ResourceRedirection.RegisterAsyncAssetBundleLoadingHook( AsyncAssetBundleLoading );
      }

      public void AssetBundleLoading( AssetBundleLoadingContext context )
      {
         if( !File.Exists( context.OriginalParameters.Path ) )
         {
            // the game is trying to load a path that does not exist, lets redirect to our own resources

            // obtain different resource path
            var normalizedPath = context.GetNormalizedPath();
            var modFolderPath = Path.Combine( "mods", normalizedPath );

            // if the path exists, let's load that instead
            if( File.Exists( modFolderPath ) )
            {
               var bundle = AssetBundle.LoadFromFile( modFolderPath );

               context.Handled = true;
               context.Bundle = bundle;
            }
         }
      }

      public void AsyncAssetBundleLoading( AsyncAssetBundleLoadingContext context )
      {
         if( !File.Exists( context.OriginalParameters.Path ) )
         {
            // the game is trying to load a path that does not exist, lets redirect to our own resources

            // obtain different resource path
            var normalizedPath = context.GetNormalizedPath();
            var modFolderPath = Path.Combine( "mods", normalizedPath );

            // if the path exists, let's load that instead
            if( File.Exists( modFolderPath ) )
            {
               var request = AssetBundle.LoadFromFileAsync( modFolderPath );

               context.Handled = true;
               context.Request = request;
            }
         }
      }
   }
}
