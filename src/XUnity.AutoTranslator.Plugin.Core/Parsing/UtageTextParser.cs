namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public class UtageTextParser : UnityTextParserBase
   {
      public UtageTextParser()
      {
         AddIgnoredTag( "ruby" );
      }
   }
}
