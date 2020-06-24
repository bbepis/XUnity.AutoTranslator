using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   /// <summary>
   /// Extension methods to calculate preferred resources paths for redirected resources.
   /// </summary>
   public static class AssetLoadedContextExtensions
   {
      /// <summary>
      /// Gets the default resource and preferred path to store the redirected resource at.
      /// </summary>
      /// <param name="context"></param>
      /// <param name="asset"></param>
      /// <param name="extension"></param>
      /// <returns></returns>
      public static string GetPreferredFilePath( this IAssetOrResourceLoadedContext context, UnityEngine.Object asset, string extension )
      {
         string parentDirectory;
         if( context is AssetLoadedContext )
         {
            parentDirectory = "assets";
         }
         else if( context is ResourceLoadedContext )
         {
            parentDirectory = "resources";
         }
         else
         {
            throw new ArgumentException( "context" );
         }

         return Path.Combine( Path.Combine( Settings.RedirectedResourcesPath, parentDirectory ), context.GetUniqueFileSystemAssetPath( asset ) ) + extension;
      }

      /// <summary>
      /// Gets the default resource and preferred path to store the redirected resource at.
      /// </summary>
      /// <param name="context"></param>
      /// <param name="parentDirectory"></param>
      /// <param name="asset"></param>
      /// <param name="extension"></param>
      /// <returns></returns>
      public static string GetPreferredFilePath( this IAssetOrResourceLoadedContext context, string parentDirectory, UnityEngine.Object asset, string extension )
      {
         return Path.Combine( parentDirectory, context.GetUniqueFileSystemAssetPath( asset ) ) + extension;
      }

      /// <summary>
      /// Gets the preferred path to store the resource at if a specific file name is required.
      /// </summary>
      /// <param name="context"></param>
      /// <param name="asset"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public static string GetPreferredFilePathWithCustomFileName( this IAssetOrResourceLoadedContext context, UnityEngine.Object asset, string fileName )
      {
         string parentDirectory;
         if( context is AssetLoadedContext )
         {
            parentDirectory = "assets";
         }
         else if( context is ResourceLoadedContext )
         {
            parentDirectory = "resources";
         }
         else
         {
            throw new ArgumentException( "context" );
         }

         var path = Path.Combine( Path.Combine( Settings.RedirectedResourcesPath, parentDirectory ), context.GetUniqueFileSystemAssetPath( asset ) );
         if( fileName != null )
         {
            path = Path.Combine( path, fileName );
         }
         return path;
      }

      /// <summary>
      /// Gets the preferred path to store the resource at if a specific file name is required.
      /// </summary>
      /// <param name="context"></param>
      /// <param name="parentDirectory"></param>
      /// <param name="asset"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public static string GetPreferredFilePathWithCustomFileName( this IAssetOrResourceLoadedContext context, string parentDirectory, UnityEngine.Object asset, string fileName )
      {
         var path = Path.Combine( parentDirectory, context.GetUniqueFileSystemAssetPath( asset ) );
         if( fileName != null )
         {
            path = Path.Combine( path, fileName );
         }
         return path;
      }
   }
}
