namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Interface providing the update/start callback of monobehaiour. Intended for IL2CPP usage.
   /// </summary>
   public interface IMonoBehaviour : IMonoBehaviour_Update
   {
      /// <summary>
      /// Start callback
      /// </summary>
      void Start();

      /// <summary>
      /// OnGUI callback
      /// </summary>
      void OnGUI();
   }
}
