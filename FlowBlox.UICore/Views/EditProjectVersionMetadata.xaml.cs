using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class EditProjectVersionMetadataWindow : MetroWindow
    {
        public EditProjectVersionMetadataWindow(
            MetroWindow owner,
            FlowBloxWebApiService flowBloxWebApiService,
            string userToken,
            FbProjectResult selectedProject,
            FbProjectVersionResult selectedVersion)
        {
            InitializeComponent();

            DataContext = new EditProjectVersionMetadataViewModel(
                this,
                flowBloxWebApiService,
                userToken,
                selectedProject,
                selectedVersion);
        }
    }
}
