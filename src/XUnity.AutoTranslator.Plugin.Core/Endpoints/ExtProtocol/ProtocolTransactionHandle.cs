using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{
   internal class ProtocolTransactionHandle : CustomYieldInstructionShim
   {
      public ProtocolTransactionHandle()
      {
         StartTime = TimeSupport.Time.realtimeSinceStartup;
      }

      public void SetCompleted( string[] translatedTexts, string error, StatusCode statusCode )
      {
         IsCompleted = true;
         Results = translatedTexts;
         Error = error;
      }

      public override bool keepWaiting => !IsCompleted;

      public float StartTime { get; set; }

      public string[] Results { get; set; }

      public string Error { get; set; }

      public StatusCode StatusCode { get; set; }

      public bool IsCompleted { get; private set; } = false;

      public bool Succeeded => Error == null;
   }
}
