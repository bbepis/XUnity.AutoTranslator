namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Accessor for the public API surface to interact with translations provided by the Auto Translator Plugin.
   /// </summary>
   public static class AutoTranslator
   {
      private static IInternalTranslator _internalTranslator;

      internal static void SetTranslator( IInternalTranslator translator )
      {
         _internalTranslator = translator;
      }

      internal static IInternalTranslator Internal => _internalTranslator;

      /// <summary>
      /// Gets the translator instance.
      /// </summary>
      public static ITranslator Default => _internalTranslator;
   }
}
