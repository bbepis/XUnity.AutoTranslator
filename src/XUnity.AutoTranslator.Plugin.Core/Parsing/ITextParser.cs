namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal interface ITextParser
   {
      ParserResult Parse( string input );

      bool CanApply( object ui );
   }
}
