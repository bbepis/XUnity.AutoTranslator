using BepInEx;

namespace KoikatsuTextResourceRedirector
{
    [BepInDependency("gravydevsupreme.xunity.autotranslator", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GUID: "gravydevsupreme.koikatsu.textresourceredirector", Name: "KoikatsuTextResourceRedirector", Version: "1.0.0")]
    public class KoikatsuTextResourceRedirectorPlugin : BaseUnityPlugin
    {
        private ExcelDataResourceRedirector _excelRedirector;
        private ScenarioDataResourceRedirector _scenarioRedirector;

        void Awake()
        {
            _excelRedirector = new ExcelDataResourceRedirector();
            _scenarioRedirector = new ScenarioDataResourceRedirector();

            // disable thy self!
            enabled = false;
        }
    }
}
