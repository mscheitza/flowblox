using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class CreateProjectVersionWindow : MetroWindow
    {
        public CreateProjectVersionWindow(
            MetroWindow owner,
            FlowBloxWebApiService flowBloxWebApiService,
            string userToken,
            FbProjectResult selectedProject)
        {
            InitializeComponent();

            DataContext = new CreateProjectVersionViewModel(
                this,
                flowBloxWebApiService,
                userToken,
                selectedProject);
        }
    }
}
