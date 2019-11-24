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
      private string _path;

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

      public string Path
      {
         get
         {
            return _path;
         }
         set
         {
            if( _path != value )
            {
               _path = value;
               _normalizedPath = null;
            }
         }
      }
   }
}
