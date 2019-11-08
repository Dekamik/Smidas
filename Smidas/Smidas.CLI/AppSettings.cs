using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.CLI
{
    public class AppSettings
    {
        public string ChromeDriverPath { get; set; }

        public string[] Blacklist { get; set; }
    }
}
