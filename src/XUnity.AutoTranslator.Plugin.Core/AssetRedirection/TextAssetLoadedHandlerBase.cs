using System.IO;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Base class for handling text assets. Specifically made for TextAsset because that resource
   /// class can be difficult to work with since it only has property getters and no setters.
   /// </summary>
   public abstract class TextAssetLoadedHandlerBase : AssetLoadedHandlerBaseV2<TextAsset>
   {
      /// <summary>
      /// Default constructor.
      /// </summary>
      public TextAssetLoadedHandlerBase()
      {
         HooksSetup.InstallTextAssetHooks();
      }

      /// <summary>
      /// Primary method that is supposed to perform the translation of the text asset. Instead of actually modifying the
      /// TextAsset, the text and it's suggested encoding should be returned from the method and the auto translator
      /// will ensure that this text/data is returned when requested by the game. Return null to not handle.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="asset">The text asset with the original content.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A TextAndEncoding indicating the text that should replace with TextAsset. Return null to not handle.</returns>
      public abstract TextAndEncoding TranslateTextAsset( string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context );

      /// <summary>
      /// Method invoked when an asset should be updated or replaced.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="asset">The asset to be updated or replaced.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected override sealed bool ReplaceOrUpdateAsset( string calculatedModificationPath, ref TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         var info = TranslateTextAsset( calculatedModificationPath, asset, context );

         if( info != null )
         {
            var ext = asset.GetOrCreateExtensionData<TextAssetExtensionData>();
            ext.Encoding = info.Encoding;
            ext.Text = info.Text;
            ext.Data = info.Bytes;

            return true;
         }

         return false;
      }
   }
}
