using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Shim;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{
   internal class ProtocolTransactionHandle : CustomYieldInstructionShim
   {
      public ProtocolTransactionHandle()
      {
         StartTime = Time.realtimeSinceStartup;
      }

      public void SetCompleted( string result, string error )
      {
         IsCompleted = true;
         Result = result;
         Error = error;
      }

      public override bool keepWaiting => !IsCompleted;

      public float StartTime { get; set; }

      public string Result { get; set; }

      public string Error { get; set; }

      public bool IsCompleted { get; private set; } = false;

      public bool Succeeded => Error == null;
   }
}
