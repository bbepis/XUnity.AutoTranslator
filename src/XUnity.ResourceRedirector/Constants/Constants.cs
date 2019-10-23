using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Constants;

namespace XUnity.ResourceRedirector.Constants
{
   internal static class EnvironmentEx
   {
      internal static readonly string LoweredCurrentDirectory = Paths.GameRoot.ToLowerInvariant();
   }
}
