using System.Windows;

namespace FlowBlox.UICore.PopUp.Provider
{
    public interface IComponentPopupProvider
    {
        Type TargetType { get; }

        string OptionKey { get; }

        bool CanShowFor(object target);

        void ShowIfEnabled(object target, Window owner = null);
    }
}
