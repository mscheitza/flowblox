using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class EditUserPropertiesWindow : MetroWindow
    {
        public EditUserPropertiesWindow()
        {
            InitializeComponent();
        }

        public EditUserPropertiesWindow(
            FlowBloxWebApiService webApiService,
            string userToken,
            FbUserData user) : this()
        {
            DataContext = new EditUserPropertiesViewModel(this, webApiService, userToken, user);
        }
    }
}
