using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AssetBundleLoaded hook (both asynchronous and synchronous).
   /// </summary>
   public class AssetBundleLoadedContext
   {
      private string _normalizedPath;

      internal AssetBundleLoadedContext( AssetBundleLoadingParameters parameters, AssetBundle bundle )
      {
         Parameters = parameters;
         Bundle = bundle;
      }

      /// <summary>
      /// Gets a normalized path to the asset bundle that is:
      ///  * Relative to the current directory
      ///  * Lower-casing
      ///  * Uses '\' as separators.
      /// </summary>
      /// <returns></returns>
      public string GetNormalizedPath()
      {
         if( _normalizedPath == null && Parameters.Path != null )
         {
            _normalizedPath = Parameters.Path
               .ToLowerInvariant()
               .UseCorrectDirectorySeparators()
               .MakeRelativePath( EnvironmentEx.LoweredCurrentDirectory );
         }
         return _normalizedPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      /// <param name="skipRemainingPostfixes">Indicate if the remaining postfixes should be skipped.</param>
      public void Complete( bool skipRemainingPostfixes = true )
      {
         SkipRemainingPostfixes = skipRemainingPostfixes;
      }

      /// <summary>
      /// Disables recursive calls if you make an asset/asset bundle load call
      /// from within your callback. If you want to prevent recursion this should
      /// be called before you load the asset/asset bundle.
      /// </summary>
      public void DisableRecursion()
      {
         ResourceRedirection.RecursionEnabled = false;
      }

      /// <summary>
      /// Gets the parameters of the call.
      /// </summary>
      public AssetBundleLoadingParameters Parameters { get; }

      /// <summary>
      /// Gets or sets the AssetBundle being loaded.
      /// </summary>
      public AssetBundle Bundle { get; set; }

      internal bool SkipRemainingPostfixes { get; private set; }
   }
}
