using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.UICore.PopUp.Views;
using System.Windows;

namespace FlowBlox.UICore.PopUp.Provider
{
    public abstract class ComponentPopupProviderBase<TTarget> : IComponentPopupProvider
    {
        private bool _showOnceBecauseOptionWasMissing;

        public Type TargetType => typeof(TTarget);

        public abstract string OptionKey { get; }

        protected abstract string WindowTitle { get; }

        protected abstract IReadOnlyList<ComponentPopupItem> CreateItems(TTarget target);

        protected void SetOptionWasMissingAtInitialization(bool wasMissing)
        {
            _showOnceBecauseOptionWasMissing = wasMissing;
        }

        public bool CanShowFor(object target)
        {
            return target is TTarget;
        }

        public void ShowIfEnabled(object target, Window owner = null)
        {
            if (target is not TTarget typedTarget)
                return;

            var options = FlowBloxOptions.GetOptionInstance();
            var option = EnsureOption(options);
            if (!option.GetValueBoolean() && !_showOnceBecauseOptionWasMissing)
                return;

            var items = CreateItems(typedTarget);
            if (items.Count == 0)
                return;

            var window = new ComponentPopupWindow(WindowTitle, items)
            {
                Owner = owner ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive),
                WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            };

            window.ShowDialog();

            option.Value = window.ShowAgain ? bool.TrueString : bool.FalseString;
            _showOnceBecauseOptionWasMissing = false;
            options.Save();
        }

        private OptionElement EnsureOption(FlowBloxOptions options)
        {
            var option = options.GetOption(OptionKey);
            if (option != null)
                return option;

            option = new OptionElement(
                OptionKey,
                bool.FalseString,
                "Controls whether this component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean);

            options.OptionCollection[OptionKey] = option;
            options.Save();
            return option;
        }
    }
}
