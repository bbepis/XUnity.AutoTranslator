using BepInEx;

namespace KoikatsuTextResourceRedirector
{
   [BepInDependency( "gravydevsupreme.xunity.autotranslator", BepInDependency.DependencyFlags.HardDependency )]
   [BepInPlugin( GUID: "gravydevsupreme.koikatsu.textresourceredirector", Name: "KoikatsuTextResourceRedirector", Version: "1.1.1" )]
   public class KoikatsuTextResourceRedirectorPlugin : BaseUnityPlugin
   {
      private ExcelDataResourceRedirector _excelRedirector;
      private ScenarioDataResourceRedirector _scenarioRedirector;
      private TsvResourceRedirector _tsvRedirector;

      void Awake()
      {
         _excelRedirector = new ExcelDataResourceRedirector();
         _scenarioRedirector = new ScenarioDataResourceRedirector();
         _tsvRedirector = new TsvResourceRedirector();

         // disable thy self!
         enabled = false;
      }
   }
}
