namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Status code.
   /// </summary>
   public enum StatusCode
   {
      /// <summary>
      /// OK status.
      /// </summary>
      OK = 0,

      /// <summary>
      /// Failure caused by blocking from external service.
      /// </summary>
      Blocked = 1,

      /// <summary>
      /// Unknown status.
      /// </summary>
      Unknown = 1000,
   }
}
