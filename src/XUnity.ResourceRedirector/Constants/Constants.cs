using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Constants;
using XUnity.Common.Shims;

namespace XUnity.ResourceRedirector.Constants
{
   internal static class EnvironmentEx
   {
      internal static readonly string LoweredCurrentDirectory = PathsHelper.Instance.GameRoot.ToLowerInvariant();
   }
}
