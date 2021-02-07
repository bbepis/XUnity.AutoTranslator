using System.Collections;
using System.Threading.Tasks;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public static class CoroutineSimulator
   {
      public static async Task StartAsync( IEnumerator coroutine )
      {
         while(coroutine.MoveNext())
         {
            if( coroutine.Current is IEnumerator innerCoroutine)
            {
               await StartAsync( innerCoroutine );
            }
            else
            {
               await Task.Delay( 1000 );
            }
         }
      }
   }
}
