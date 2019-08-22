using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.ResourceRedirector.Constants
{
   internal static class EnvironmentEx
   {
      internal static readonly string LoweredCurrentDirectory = Environment.CurrentDirectory.ToLowerInvariant();
   }
}
