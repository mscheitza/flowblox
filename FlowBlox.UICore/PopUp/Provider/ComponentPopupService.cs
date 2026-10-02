namespace FlowBlox.UICore.PopUp.Provider
{
    public class ComponentPopupService : IComponentPopupService
    {
        private readonly IReadOnlyList<IComponentPopupProvider> _providers;

        public ComponentPopupService(IEnumerable<IComponentPopupProvider> providers)
        {
            _providers = providers?.ToList() ?? [];
        }

        public bool ShowFor(object target, System.Windows.Window owner = null)
            => ShowFor(target, ComponentPopupEvent.Open, owner);

        public bool ShowFor(object target, ComponentPopupEvent popupEvent, System.Windows.Window owner = null)
        {
            var provider = FindProvider(target, popupEvent);

            if (provider == null)
                return false;

            return provider.Show(target, owner);
        }

        public bool ShowFor<TTarget>(TTarget target, System.Windows.Window owner = null)
            => ShowFor((object)target, owner);

        public bool ShowFor<TTarget>(TTarget target, ComponentPopupEvent popupEvent, System.Windows.Window owner = null)
            => ShowFor((object)target, popupEvent, owner);

        public bool HasProviderFor(object target, ComponentPopupEvent popupEvent = ComponentPopupEvent.Open)
            => FindProvider(target, popupEvent) != null;

        public bool ShowForForced(
            object target,
            ComponentPopupEvent popupEvent = ComponentPopupEvent.Open,
            System.Windows.Window owner = null)
        {
            var provider = FindProvider(target, popupEvent);
            return provider?.Show(target, owner, force: true) == true;
        }

        private IComponentPopupProvider FindProvider(object target, ComponentPopupEvent popupEvent)
        {
            if (target == null)
                return null;

            var targetType = target.GetType();
            return _providers
                .Where(x => x.CanShowFor(target, popupEvent))
                .OrderByDescending(x => GetInheritanceDistance(targetType, x.TargetType))
                .FirstOrDefault();
        }

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
