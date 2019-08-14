using System;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   /// <summary>
   /// Base implementation of resource redirect handler that takes care of the plumming for a
   /// resource redirector that is interested in either updating of dumping redirected resources.
   /// </summary>
   /// <typeparam name="TAsset">The type of asset being redirected.</typeparam>
   internal abstract class ResourceRedirectHandlerBase<TAsset> : IResourceRedirectHandler<TAsset>
      where TAsset : UnityEngine.Object
   {
      public ResourceRedirectHandlerBase()
      {
         ResourceRedirectionManager<TAsset>.AssetLoading += Handle;
      }

      /// <summary>
      /// Method to be invoked whenever a resource of the specified type is redirected.
      /// </summary>
      /// <param name="context">A context containing all relevant information of the resource redirection.</param>
      public void Handle( IRedirectionContext<TAsset> context )
      {
         if( context.HasBeenRedirectedBefore )
         {
            context.Handled = true;
            return;
         }

         var modificationFilePath = CalculateModificationFilePath( context );
         if( Settings.RedirectedFiles.Contains( modificationFilePath ) )
         {
            try
            {
               context.Handled = ReplaceOrUpdateAsset( modificationFilePath, context );
               if( context.Handled )
               {
                  XuaLogger.Default.Debug( $"Replaced resource file: '{modificationFilePath}'." );
               }
               else
               {
                  XuaLogger.Default.Debug( $"Did not replace resource file: '{modificationFilePath}'." );
               }
            }
            catch( Exception e )
            {
               XuaLogger.Default.Error( e, $"An error occurred while loading resource file: '{modificationFilePath}'." );
            }
         }
         else if( Settings.EnableDumping )
         {
            try
            {
               context.Handled = DumpAsset( modificationFilePath, context );
               if( context.Handled )
               {
                  XuaLogger.Default.Debug( $"Dumped resource file: '{modificationFilePath}'." );
               }
               else
               {
                  XuaLogger.Default.Debug( $"Did not dump resource file: '{modificationFilePath}'." );
               }
            }
            catch( Exception e )
            {
               XuaLogger.Default.Error( e, $"An error occurred while dumping resource file: '{modificationFilePath}'." );
            }
         }
      }

      /// <summary>
      /// Method invoked when an asset should be updated or replaced.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool ReplaceOrUpdateAsset( string calculatedModificationPath, IRedirectionContext<TAsset> context );

      /// <summary>
      /// Method invoked when an asset should be dumped.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool DumpAsset( string calculatedModificationPath, IRedirectionContext<TAsset> context );

      /// <summary>
      /// Method invoked when a new resource event is fired to calculate a unique path for the resource.
      /// </summary>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A string uniquely representing a path for the redirected resource.</returns>
      protected abstract string CalculateModificationFilePath( IRedirectionContext<TAsset> context );
   }
}
