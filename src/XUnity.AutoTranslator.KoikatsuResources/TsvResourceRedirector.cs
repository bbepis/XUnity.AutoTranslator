using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.ResourceRedirector;

namespace KoikatsuTextResourceRedirector
{
   /// <summary>
   /// All credits to https://github.com/GeBo1 for implementation.
   /// </summary>
   class TsvResourceRedirector : TextAssetLoadedHandlerBase
   {
      private readonly TextAssetHelper textAssetHelper = new TextAssetHelper( new string[] { "\r\n", "\r", "\n" }, new string[] { "\t" } );

      public TsvResourceRedirector()
      {
         CheckDirectory = true;
      }

      protected override string CalculateModificationFilePath( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return context.GetPreferredFilePathWithCustomFileName( asset, null )
            .Replace( ".unity3d", "" );
      }

      public override TextAndEncoding TranslateTextAsset( string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         if( !textAssetHelper.IsTable( asset ) )
         {
            return null;
         }

         var defaultTranslationFile = Path.Combine( calculatedModificationPath, "translation.txt" );
         var redirectedResources = RedirectedDirectory.GetFilesInDirectory( calculatedModificationPath, ".txt" );
         var streams = redirectedResources.Select( x => x.OpenStream() );
         var cache = new SimpleTextTranslationCache(
            outputFile: defaultTranslationFile,
            inputStreams: streams,
            allowTranslationOverride: false,
            closeStreams: true );

         return Translate( cache, calculatedModificationPath, ref asset );
      }

      private TextAndEncoding Translate( SimpleTextTranslationCache cache, string calculatedModificationPath, ref TextAsset textAsset )
      {
         string TranslateCell( string cellText )
         {
            if( cache.TryGetTranslation( cellText, false, out string newText ) )
            {
               TranslationHelper.RegisterRedirectedResourceTextToPath( newText, calculatedModificationPath );
               return newText;
            }
            else
            {

               if( !string.IsNullOrEmpty( cellText ) && LanguageHelper.IsTranslatable( cellText ) )
               {
                  TranslationHelper.RegisterRedirectedResourceTextToPath( cellText, calculatedModificationPath );

                  if( AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled )
                  {
                     cache.AddTranslationToCache( cellText, cellText );
                  }
               }
            }
            return null;
         }

         string result = textAssetHelper.ProcessTable( textAsset, TranslateCell, out TextAssetHelper.TableResult tableResult );

         //Logger.Log( BepinLogLevel.Debug, $"{this.GetType()}: {tableResult.RowsUpdated}/{tableResult.Rows} rows updated" );
         if( tableResult.RowsUpdated > 0 )
         {
            return new TextAndEncoding( result, Encoding.UTF8 );
         }

         return null;
      }


      protected override bool DumpAsset( string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         if( !textAssetHelper.IsTable( asset ) )
         {
            return false;
         }

         var defaultTranslationFile = Path.Combine( calculatedModificationPath, "translation.txt" );
         var redirectedResources = RedirectedDirectory.GetFilesInDirectory( calculatedModificationPath, ".txt" );
         var streams = redirectedResources.Select( x => x.OpenStream() );
         var cache = new SimpleTextTranslationCache(
            outputFile: defaultTranslationFile,
            inputStreams: streams,
            allowTranslationOverride: false,
            closeStreams: true );

         bool DumpCell( string cellText )
         {
            if( !string.IsNullOrEmpty( cellText ) && LanguageHelper.IsTranslatable( cellText ) )
            {
               TranslationHelper.RegisterRedirectedResourceTextToPath( cellText, calculatedModificationPath );

               cache.AddTranslationToCache( cellText, cellText );
               return true;
            }
            return false;
         }

         return textAssetHelper.ActOnCells( asset, DumpCell, out TextAssetHelper.TableResult tableResult );
      }

      protected override bool ShouldHandleAsset( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return !context.HasReferenceBeenRedirectedBefore( asset );
      }
   }
}
