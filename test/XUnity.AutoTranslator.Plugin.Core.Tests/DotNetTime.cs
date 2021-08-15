using System.Diagnostics;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class DotNetTime : ITime
   {
      private Stopwatch _stopwatch;
      private int _frameCount;

      public DotNetTime()
      {
         _stopwatch = Stopwatch.StartNew();
      }

      public float realtimeSinceStartup => (float)_stopwatch.Elapsed.TotalSeconds;

      public int frameCount => ++_frameCount;
   }
}
