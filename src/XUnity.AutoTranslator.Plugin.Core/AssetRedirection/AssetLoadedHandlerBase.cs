using System;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Base implementation of resource redirect handler that takes care of the plumming for a
   /// resource redirector that is interested in either updating or dumping redirected resources.
   /// </summary>
   /// <typeparam name="TAsset">The type of asset being redirected.</typeparam>
   public abstract class AssetLoadedHandlerBase<TAsset>
      where TAsset : UnityEngine.Object
   {
      /// <summary>
      /// Constructs the resource redirection handler.
      /// </summary>
      public AssetLoadedHandlerBase()
      {
         ResourceRedirection.RegisterAssetLoadedHook( HandleAsset );
         ResourceRedirection.RegisterResourceLoadedHook( HandleResource );
      }

      /// <summary>
      /// Gets a bool indicating if resource dumping is enabled in the Auto Translator.
      /// </summary>
      protected bool IsDumpingEnabled => Settings.EnableDumping;

      private void HandleAsset( AssetLoadedContext context ) => Handle( context );

      private void HandleResource( ResourceLoadedContext context ) => Handle( context );

      private void Handle( IAssetOrResourceLoadedContext context )
      {
         var assets = context.Assets;
         if( assets != null )
         {
            for( int i = 0; i < assets.Length; i++ )
            {
               var asset = assets[ i ];
               if( asset is TAsset castedAsset && ShouldHandleAsset( castedAsset, context ) )
               {
                  var modificationFilePath = CalculateModificationFilePath( castedAsset, context );
                  if( File.Exists( modificationFilePath ) ) // IO, ewww!
                  {
                     try
                     {
                        context.Handled = ReplaceOrUpdateAsset( modificationFilePath, ref castedAsset, context );
                        if( context.Handled )
                        {
                           XuaLogger.AutoTranslator.Debug( $"Replaced resource file: '{modificationFilePath}'." );
                        }
                        else
                        {
                           XuaLogger.AutoTranslator.Debug( $"Did not replace resource file: '{modificationFilePath}'." );
                        }
                     }
                     catch( Exception e )
                     {
                        XuaLogger.AutoTranslator.Error( e, $"An error occurred while loading resource file: '{modificationFilePath}'." );
                     }
                  }
                  else if( Settings.EnableDumping )
                  {
                     try
                     {
                        context.Handled = DumpAsset( modificationFilePath, castedAsset, context );
                        if( context.Handled )
                        {
                           XuaLogger.AutoTranslator.Debug( $"Dumped resource file: '{modificationFilePath}'." );
                        }
                        else
                        {
                           XuaLogger.AutoTranslator.Debug( $"Did not dump resource file: '{modificationFilePath}'." );
                        }
                     }
                     catch( Exception e )
                     {
                        XuaLogger.AutoTranslator.Error( e, $"An error occurred while dumping resource file: '{modificationFilePath}'." );
                     }
                  }

                  assets[ i ] = castedAsset;
               }
            }
         }
      }

      /// <summary>
      /// Method invoked when an asset should be updated or replaced.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="asset">The asset to be updated or replaced.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool ReplaceOrUpdateAsset( string calculatedModificationPath, ref TAsset asset, IAssetOrResourceLoadedContext context );

      /// <summary>
      /// Method invoked when an asset should be dumped.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="asset">The asset to be updated or replaced.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool DumpAsset( string calculatedModificationPath, TAsset asset, IAssetOrResourceLoadedContext context );

      /// <summary>
      /// Method invoked when a new resource event is fired to calculate a unique path for the resource.
      /// </summary>
      /// <param name="asset">The asset to be updated or replaced.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A string uniquely representing a path for the redirected resource.</returns>
      protected abstract string CalculateModificationFilePath( TAsset asset, IAssetOrResourceLoadedContext context );

      /// <summary>
      /// Method to be invoked to indicate if the asset should be handled or not.
      /// </summary>
      /// <param name="asset">The asset to be updated or replaced.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the asset should be handled.</returns>
      protected abstract bool ShouldHandleAsset( TAsset asset, IAssetOrResourceLoadedContext context );
   }
}
