using ADV;
using BepInEx;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.ResourceRedirector;

namespace KoikatsuTextResourceRedirector
{
   public class ScenarioDataResourceRedirector : AssetLoadedHandlerBase<ScenarioData>
   {
      protected override string CalculateModificationFilePath( ScenarioData asset, IAssetOrResourceLoadedContext context )
      {
         return context.GetPreferredFilePathWithCustomFileName( @"BepInEx\translation", asset, "translation.txt" )
            .Replace( @"abdata\", "" )
            .Replace( ".unity3d", "" );
      }

      protected override bool DumpAsset( string calculatedModificationPath, ScenarioData asset, IAssetOrResourceLoadedContext context )
      {
         var cache = new SimpleTextTranslationCache( calculatedModificationPath, false );

         foreach( var param in asset.list )
         {
            if( param.Command == Command.Text )
            {
               for( int i = 0; i < param.Args.Length; i++ )
               {
                  var key = param.Args[ i ];

                  if( !string.IsNullOrEmpty( key ) && LanguageHelper.IsTranslatable( key ) )
                  {
                     cache.AddTranslationToCache( key, key );
                  }
               }
            }
         }

         return true;
      }

      protected override bool ReplaceOrUpdateAsset( string calculatedModificationPath, ref ScenarioData asset, IAssetOrResourceLoadedContext context )
      {
         var cache = new SimpleTextTranslationCache( calculatedModificationPath, true );

         foreach( var param in asset.list )
         {
            if( param.Command == Command.Text )
            {
               for( int i = 0; i < param.Args.Length; i++ )
               {
                  var key = param.Args[ i ];
                  if( !string.IsNullOrEmpty( key ) )
                  {
                     if( cache.TryGetTranslation( key, true, out var translated ) )
                     {
                        param.Args[ i ] = translated;
                     }
                     else if( IsDumpingEnabled && LanguageHelper.IsTranslatable( key ) )
                     {
                        cache.AddTranslationToCache( key, key );
                     }
                  }
               }
            }
         }

         return true;
      }

      protected override bool ShouldHandleAsset( ScenarioData asset, IAssetOrResourceLoadedContext context )
      {
         return !context.HasReferenceBeenRedirectedBefore( asset );
      }
   }
}
