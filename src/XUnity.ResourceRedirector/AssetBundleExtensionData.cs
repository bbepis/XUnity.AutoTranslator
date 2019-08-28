using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector
{
   internal class AssetBundleExtensionData
   {
      private string _normalizedPath;

      public string NormalizedPath
      {
         get
         {
            if( Path != null && _normalizedPath == null )
            {
               _normalizedPath = Path.ToLowerInvariant().MakeRelativePath( EnvironmentEx.LoweredCurrentDirectory );
            }
            return _normalizedPath;
         }
      }

      public string Path { get; set; }
   }
}
