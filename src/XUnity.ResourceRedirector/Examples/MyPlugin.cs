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
         ResourceRedirection.RegisterAssetLoadedHook( AssetLoaded );
      }

      public void AssetLoaded( AssetLoadedContext context )
      {
         for( int i = 0; i < context.Assets.Length; i++ )
         {
            var asset = context.Assets[ i ];
            if( asset is Texture2D texture2d )
            {
               // TODO: Modify, replace or dump the texture

               context.Handled = true;
               context.Assets[ i ] = texture2d; // only need to update the reference if you created a new texture
            }
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

               context.Bundle = bundle;
               context.Handled = true;
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

               context.Request = request;
               context.Handled = true;
            }
         }
      }
   }
}
