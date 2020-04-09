namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal interface ITextParser
   {
      ParserResult Parse( string input, int scope );
   }
}
