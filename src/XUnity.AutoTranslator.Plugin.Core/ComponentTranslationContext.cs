namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Callback-context object used when this plugin intends to attempt a translation on a component.
   /// </summary>
   public class ComponentTranslationContext
   {
      internal ComponentTranslationContext( object component, string originalText )
      {
         Component = component;
         OriginalText = originalText;
      }

      /// <summary>
      /// Gets the text component in question. Often unity-related object such as Text or TextMeshProUGUI.
      /// </summary>
      public object Component { get; }

      /// <summary>
      /// Gets the original text that was in the text component (as retrieved through the components text property).
      /// </summary>
      public string OriginalText { get; }

      /// <summary>
      /// Gets the overriden translated text if set through the OverrideTranslatedText method.
      /// </summary>
      public string OverriddenTranslatedText { get; private set; }

      internal ComponentTranslationBehaviour Behaviour { get; private set; }

      /// <summary>
      /// Resets to default behaviour, such that no overriding or ignoring happens.
      /// </summary>
      public void ResetBehaviour()
      {
         Behaviour = ComponentTranslationBehaviour.Default;
         OverriddenTranslatedText = null;
      }

      /// <summary>
      /// Overrides the translation with the specified text.
      /// </summary>
      /// <param name="translation"></param>
      public void OverrideTranslatedText( string translation )
      {
         Behaviour = ComponentTranslationBehaviour.OverrideTranslatedText;
         OverriddenTranslatedText = translation;
      }

      /// <summary>
      /// Ignores the component.
      /// </summary>
      public void IgnoreComponent()
      {
         Behaviour = ComponentTranslationBehaviour.IgnoreComponent;
         OverriddenTranslatedText = null;
      }
   }
}
