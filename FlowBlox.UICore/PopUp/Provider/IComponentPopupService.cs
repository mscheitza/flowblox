using System.Windows;

namespace FlowBlox.UICore.PopUp.Provider
{
    public interface IComponentPopupService
    {
        bool ShowFor(object target, Window owner = null);

        bool ShowFor<TTarget>(TTarget target, Window owner = null);
    }
}
