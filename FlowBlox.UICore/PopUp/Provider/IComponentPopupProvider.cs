using System.Windows;

namespace FlowBlox.UICore.PopUp.Provider
{
    public interface IComponentPopupProvider
    {
        Type TargetType { get; }

        ComponentPopupEvent PopupEvent { get; }

        string OptionKey { get; }

        bool CanShowFor(object target, ComponentPopupEvent popupEvent);

        void ShowIfEnabled(object target, Window owner = null);
    }
}
