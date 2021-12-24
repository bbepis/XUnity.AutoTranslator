using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Accessor for the public API surface to interact with translations provided by the Auto Translator Plugin.
   /// </summary>
   public static class AutoTranslator
   {
      internal static IInternalTranslator Internal => AutoTranslationPlugin.Current;

      /// <summary>
      /// Gets the translator instance.
      /// </summary>
      public static ITranslator Default => AutoTranslationPlugin.Current;
   }
}
