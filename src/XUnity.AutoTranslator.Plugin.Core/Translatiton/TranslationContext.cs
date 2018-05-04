//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace XUnity.AutoTranslator.Plugin.Core.Translatiton
//{
//   public class TranslationContext
//   {
//      private Dictionary<string, string> _staticTranslations;
//      private Dictionary<string, string> _dynamicTranslations;
//      private Dictionary<string, HashSet<string>> _variables;

//      public TranslationContext()
//      {
//         _staticTranslations = new Dictionary<string, string>();
//         _dynamicTranslations = new Dictionary<string, string>();
//         _variables = new Dictionary<string, HashSet<string>>();
//      }

//      public void AddVariable( string name, string value )
//      {
//         if( !_variables.TryGetValue( name, out var values ) )
//         {
//            values = new HashSet<string>();
//         }

//         values.Add( value );
//      }
//   }

//   public class Translation
//   {
//      public string Key { get; set; }

//      public string Expression { get; set; }
//   }

//   public enum DirectiveType
//   {
//      SetVariable,
//      UnsetVariable
//   }

//   public class TranslationDirective
//   {
//      public TranslationDirective( string directive )
//      {

//      }
//   }

//   public class RangeValue
//   {

//   }
//   public class TranslationReader : IDisposable
//   {
//      private readonly TextReader _reader;

//      private string _activeLine;
//      private List<TranslationDirective> _activeDirectives;

//      public TranslationReader( TextReader reader )
//      {
//         _reader = reader;
//      }

//      public Translation Read()
//      {

//      }
//   }
//}
