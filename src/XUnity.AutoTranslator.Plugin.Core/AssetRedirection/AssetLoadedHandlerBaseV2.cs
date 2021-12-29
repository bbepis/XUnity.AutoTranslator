using System;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Base implementation of resource redirect handler that takes care of the plumbing for a
   /// resource redirector that is interested in either updating or dumping redirected resources.
   /// </summary>
   /// <typeparam name="TAsset">The type of asset being redirected.</typeparam>
   public abstract class AssetLoadedHandlerBaseV2<TAsset>
      where TAsset : UnityEngine.Object
   {
      /// <summary>
      /// Constructs the resource redirection handler.
      /// </summary>
      public AssetLoadedHandlerBaseV2()
      {
         ResourceRedirection.RegisterAssetLoadedHook( HookBehaviour.OneCallbackPerResourceLoaded, 0, HandleAsset );
         ResourceRedirection.RegisterResourceLoadedHook( HookBehaviour.OneCallbackPerResourceLoaded, 0, HandleResource );
      }

      /// <summary>
      /// Gets or sets a boolean indicating if the calculated modification path should be checked
      /// for a file or directory.
      /// </summary>
      protected bool CheckDirectory { get; set; }

      private void HandleAsset( AssetLoadedContext context ) => Handle( context );

      private void HandleResource( ResourceLoadedContext context ) => Handle( context );

      private void Handle( IAssetOrResourceLoadedContext context )
      {
         if( context.Asset.TryCastTo<TAsset>( out var castedAsset ) && ShouldHandleAsset( castedAsset, context ) )
         {
            var unqiuePath = context.GetUniqueFileSystemAssetPath( castedAsset );
            var modificationFilePath = CalculateModificationFilePath( castedAsset, context );
            if( ( CheckDirectory && RedirectedDirectory.DirectoryExists( modificationFilePath ) ) || ( !CheckDirectory && RedirectedDirectory.FileExists( modificationFilePath ) ) ) 
            {
               try
               {
                  bool handled = ReplaceOrUpdateAsset( modificationFilePath, ref castedAsset, context );
                  if( handled )
                  {
                     if( !Settings.EnableSilentMode )
                        XuaLogger.AutoTranslator.Debug( $"Replaced or updated resource file: '{unqiuePath}'." );
                  }
                  else
                  {
                     if( !Settings.EnableSilentMode )
                        XuaLogger.AutoTranslator.Debug( $"Did not replace or update resource file: '{unqiuePath}'." );
                  }

                  context.Complete(
                     skipRemainingPostfixes: handled );
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Error( e, $"An error occurred while replacing or updating resource file: '{unqiuePath}'." );
               }
            }
            else if( AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled )
            {
               try
               {
                  bool handled = DumpAsset( modificationFilePath, castedAsset, context );
                  if( handled )
                  {
                     if( !Settings.EnableSilentMode )
                        XuaLogger.AutoTranslator.Debug( $"Dumped resource file: '{unqiuePath}'." );
                  }
                  else
                  {
                     if( !Settings.EnableSilentMode )
                        XuaLogger.AutoTranslator.Debug( $"Did not dump resource file: '{unqiuePath}'." );
                  }

                  context.Complete(
                     skipRemainingPostfixes: handled );
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Error( e, $"An error occurred while dumping resource file: '{unqiuePath}'." );
               }
            }

            if( !UnityObjectReferenceComparer.Default.Equals( castedAsset, context.Asset ) )
            {
               context.Asset = castedAsset;
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
