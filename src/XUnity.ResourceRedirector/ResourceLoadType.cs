namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum representing the different ways a resource may be loaded.
   /// </summary>
   public enum ResourceLoadType
   {
      /// <summary>
      /// Indicates that this call is loading all assets of a specific type (below a specific path) in the Resources API.
      /// </summary>
      LoadByType,

      /// <summary>
      /// Indicates that this call is loading a single named asset in the Resources API.
      /// </summary>
      LoadNamed,

      /// <summary>
      /// Indicates that this call is loading a single named built-in asset in the Resources API.
      /// </summary>
      LoadNamedBuiltIn
   }
}
