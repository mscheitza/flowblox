using System.Windows;

namespace FlowBlox.UICore.PopUp.Provider
{
    public interface IComponentPopupProvider
    {
        Type TargetType { get; }

        ComponentPopupEvent PopupEvent { get; }

        string OptionKey { get; }

        bool CanShowFor(object target, ComponentPopupEvent popupEvent);

        bool Show(object target, Window owner = null, bool force = false);
    }
}
