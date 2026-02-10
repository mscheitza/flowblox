using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FlowBlox.Core.Attributes;

namespace FlowBlox.UICore.ViewModels
{
    public class NotificationOverrideItem
    {
        public Type EnumType { get; set; }
        public Enum EnumValue { get; set; }
        public string EnumDisplayName { get; set; }
        public NotificationType SelectedNotificationType { get; set; }
    }

    public class ManageNotificationOverridesViewModel
    {
        private readonly BaseFlowBlock _flowBlock;
        private readonly Window _ownerWindow;

        public ObservableCollection<NotificationOverrideItem> Items { get; }

        public ObservableCollection<NotificationType> NotificationTypes { get; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public ManageNotificationOverridesViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);

            Items = new ObservableCollection<NotificationOverrideItem>();
            NotificationTypes = new ObservableCollection<NotificationType>();
        }

        public ManageNotificationOverridesViewModel(BaseFlowBlock flowBlock, Window ownerWindow) : this()
        {
            _flowBlock = flowBlock;
            _ownerWindow = ownerWindow;

            LoadItems();
            LoadNotificationTypes();
        }

        private void LoadNotificationTypes()
        {
            this.NotificationTypes.AddRange(Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>());
        }

        private void LoadItems()
        {
            foreach (var enumType in _flowBlock.NotificationTypes)
            {
                foreach (Enum enumValue in Enum.GetValues(enumType))
                {
                    var displayName = enumValue.GetDisplayName();
                    var currentType = _flowBlock.GetCurrentNotificationType(enumValue);

                    Items.Add(new NotificationOverrideItem
                    {
                        EnumType = enumType,
                        EnumValue = enumValue,
                        EnumDisplayName = displayName,
                        SelectedNotificationType = currentType
                    });
                }
            }
        }

        private void Ok()
        {
            foreach (var item in Items)
            {
                _flowBlock.OverrideNotificationType(item.EnumValue, item.SelectedNotificationType);
            }

            _ownerWindow.Close();
        }

        private void Cancel()
        {
            _ownerWindow.Close();
        }
    }

    public class NotificationItem
    {
        public Type EnumType { get; set; }
        public Enum EnumValue { get; set; }
        public bool IsSuppressed { get; set; }
        public string EnumDisplayName { get; internal set; }
    }
}
