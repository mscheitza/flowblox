using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.CommandLine
{
    public class Options
    {
        [Option('p', "project", Required = true, HelpText = "Set path to project file.")]
        public string ProjectFile { get; set; }
    }
}
