using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class AutoTranslatorSettings
   {
      public static string UserAgent => Settings.UserAgent;

      public static string SourceLanguage => Settings.FromLanguage;

      public static string DestinationLanguage => Settings.Language;
   }
}
