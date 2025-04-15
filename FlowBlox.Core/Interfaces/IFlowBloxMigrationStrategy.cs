using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxMigrationStrategy
    {
        Type ComponentType { get; }

        Version Version { get; }

        void Migrate(JObject json);
    }
}
