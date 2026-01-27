using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBlox.UICore.Views
{
    public partial class EditProjectMetadataWindow : MetroWindow
    {
        public EditProjectMetadataWindow(
            Window owner,
            FlowBloxWebApiService webApiService,
            string userToken,
            FbProjectResult project)
        {
            InitializeComponent();

            Owner = owner;

            DataContext = new EditProjectMetadataViewModel(
                this,
                webApiService,
                userToken,
                project);
        }
    }
}
