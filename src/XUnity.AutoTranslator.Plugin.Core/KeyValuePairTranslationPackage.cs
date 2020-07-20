using System.Collections.Generic;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Translation package consisting of key-value pairs of strings.
   /// </summary>
   public class KeyValuePairTranslationPackage
   {
      private List<KeyValuePair<string, string>> _cachedEntries;

      /// <summary>
      /// Constructs the translation package.
      /// </summary>
      /// <param name="name">The name to be displayed when it is loaded.</param>
      /// <param name="entries">The entries to be loaded.</param>
      /// <param name="allowMultipleIterations">A bool indicating if the enumerable can be iterated multiple times (due translation reload).</param>
      public KeyValuePairTranslationPackage( string name, IEnumerable<KeyValuePair<string, string>> entries, bool allowMultipleIterations )
      {
         Name = name;
         Entries = entries;
         AllowMultipleIterations = allowMultipleIterations;
      }

      /// <summary>
      /// Gets the name of the the package.
      /// </summary>
      public string Name { get; }
      private IEnumerable<KeyValuePair<string, string>> Entries { get; }
      private bool AllowMultipleIterations { get; }

      internal IEnumerable<KeyValuePair<string, string>> GetIterableEntries()
      {
         if( !AllowMultipleIterations )
         {
            if( _cachedEntries == null )
            {
               _cachedEntries = Entries.ToList();
            }
            return _cachedEntries;
         }
         return Entries;
      }
   }
}
