using System.Threading.Tasks;
using FlowBlox.WebServices.Models;

namespace FlowBlox.Interfaces
{
    public interface IFlowBloxInstallerService
    {
        Task<InstallerUpdateInfo> GetLatestInstallerUpdateAsync(string manifestUrl);

        Task<string> DownloadInstallerAsync(string installerUrl, string updateDownloadDirectoryName);
    }
}
