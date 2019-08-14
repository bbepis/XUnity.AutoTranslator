using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   /// <summary>
   /// Extension methods to calculate preferred resources paths for redirected resources.
   /// </summary>
   internal static class RedirectionContextExtensions
   {
      /// <summary>
      /// Gets the default resource and preferred path to store the redirected resource at.
      /// </summary>
      /// <typeparam name="TAsset"></typeparam>
      /// <param name="context"></param>
      /// <param name="extension"></param>
      /// <returns></returns>
      public static string GetPreferredFilePath<TAsset>( this IRedirectionContext<TAsset> context, string extension )
         where TAsset : UnityEngine.Object
      {
         return Path.Combine( Settings.RedirectedResourcesPath, context.UniqueFileSystemAssetPath ) + extension;
      }

      /// <summary>
      /// Gets the preferred path to store the resource at if a specific file name is required.
      /// </summary>
      /// <typeparam name="TAsset"></typeparam>
      /// <param name="context"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public static string GetPreferredFilePathWithCustomFileName<TAsset>( this IRedirectionContext<TAsset> context, string fileName )
         where TAsset : UnityEngine.Object
      {
         return Path.Combine( Path.Combine( Settings.RedirectedResourcesPath, context.UniqueFileSystemAssetPath ), fileName );
      }
   }
}
