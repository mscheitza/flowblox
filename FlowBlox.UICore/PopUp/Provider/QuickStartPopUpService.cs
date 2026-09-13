namespace FlowBlox.UICore.PopUp.Provider
{
    public class QuickStartPopUpService : IQuickStartPopUpService
    {
        private readonly IReadOnlyList<IQuickStartPopUpProvider> _providers;

        public QuickStartPopUpService(IEnumerable<IQuickStartPopUpProvider> providers)
        {
            _providers = providers?.ToList() ?? [];
        }

        public bool ShowFor(object target, System.Windows.Window owner = null)
        {
            if (target == null)
                return false;

            var targetType = target.GetType();
            var provider = _providers
                .Where(x => x.CanShowFor(target))
                .OrderByDescending(x => GetInheritanceDistance(targetType, x.TargetType))
                .FirstOrDefault();

            if (provider == null)
                return false;

            provider.ShowIfEnabled(target, owner);
            return true;
        }

        public bool ShowFor<TTarget>(TTarget target, System.Windows.Window owner = null)
            => ShowFor((object)target, owner);

        private static int GetInheritanceDistance(Type targetType, Type providerTargetType)
        {
            var distance = 0;
            var current = targetType;

            while (current != null)
            {
                if (current == providerTargetType)
                    return int.MaxValue - distance;

                current = current.BaseType;
                distance++;
            }

            return providerTargetType.IsAssignableFrom(targetType) ? 0 : int.MinValue;
        }
    }
}
