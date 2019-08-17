using System.IO;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   internal class TextAssetLoadedHandler : AssetLoadedHandlerBase<TextAsset>
   {
      protected override string CalculateModificationFilePath( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return context.GetPreferredFilePath( asset, ".txt" );
      }

      protected override bool DumpAsset( string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         Directory.CreateDirectory( new FileInfo( calculatedModificationPath ).Directory.FullName );
         File.WriteAllBytes( calculatedModificationPath, asset.bytes );

         return true;
      }

      protected override bool ReplaceOrUpdateAsset( string calculatedModificationPath, ref TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         var data = File.ReadAllBytes( calculatedModificationPath );
         var text = Encoding.UTF8.GetString( data );
         
         var ext = asset.GetOrCreateExtensionData<TextAssetExtensionData>();
         ext.Data = data;
         ext.Text = text;

         return true;
      }

      protected override bool ShouldHandleAsset( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return !context.HasReferenceBeenRedirectedBefore( asset );
      }
   }
}
