using System.IO;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   internal class TextAssetResourceRedirectHandler : ResourceRedirectHandlerBase<TextAsset>
   {
      protected override string CalculateModificationFilePath( IRedirectionContext<TextAsset> context )
      {
         return context.GetPreferredFilePath( ".txt" );
      }

      protected override bool DumpAsset( string calculatedModificationPath, IRedirectionContext<TextAsset> context )
      {
         Directory.CreateDirectory( new FileInfo( calculatedModificationPath ).Directory.FullName );
         File.WriteAllBytes( calculatedModificationPath, context.Asset.bytes );

         return true;
      }

      protected override bool ReplaceOrUpdateAsset( string calculatedModificationPath, IRedirectionContext<TextAsset> context )
      {
         var data = File.ReadAllBytes( calculatedModificationPath );
         var text = Encoding.UTF8.GetString( data );
         
         var ext = context.Asset.GetOrCreateExtensionData<TextAssetExtensionData>();
         ext.Data = data;
         ext.Text = text;

         return true;
      }
   }
}
