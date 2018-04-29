using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExIni;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
   public interface IConfiguration
   {
      string DataPath { get; }

      IniFile Preferences { get; }

      void SaveConfig();

      IniFile ReloadConfig();
   }
}
