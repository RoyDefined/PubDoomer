using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Settings;

public static class SettingsStatics
{
    public static readonly string LocalSavesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PubDoomer");
}
