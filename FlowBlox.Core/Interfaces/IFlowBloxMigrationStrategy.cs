using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxMigrationStrategy
    {
        Type ComponentType { get; }

        Version Version { get; }

        void Migrate(JObject json);
    }
}
