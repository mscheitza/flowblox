using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Reflection;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public enum AssociatedFlowBlockResolutionSource
    {
        None,
        Property,
        Path
    }

    public sealed class AssociatedFlowBlockResolutionResult
    {
        public static readonly AssociatedFlowBlockResolutionResult NotResolved =
            new(null, AssociatedFlowBlockResolutionSource.None);

        public AssociatedFlowBlockResolutionResult(BaseFlowBlock? flowBlock, AssociatedFlowBlockResolutionSource source)
        {
            FlowBlock = flowBlock;
            Source = source;
        }

        public BaseFlowBlock? FlowBlock { get; }
        public AssociatedFlowBlockResolutionSource Source { get; }
        public bool Resolved => FlowBlock != null;
    }

    public static class AssociatedFlowBlockResolver
    {
        private static readonly MethodInfo? _getPreviousFlowBlockOnPathMethod = typeof(BaseFlowBlock).GetMethod(
            GlobalConstants.GetPreviousFlowBlockOnPathMethodName,
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(BaseFlowBlock), typeof(Type[])],
            null);

        public static IEnumerable<PropertyInfo> GetResolvableProperties(BaseFlowBlock flowBlock)
        {
            ArgumentNullException.ThrowIfNull(flowBlock);

            return flowBlock.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => typeof(BaseFlowBlock).IsAssignableFrom(property.PropertyType))
                .Where(property => property.GetCustomAttribute<AssociatedFlowBlockResolvableAttribute>() != null);
        }

        public static AssociatedFlowBlockResolutionResult Resolve(BaseFlowBlock flowBlock, PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(flowBlock);
            ArgumentNullException.ThrowIfNull(property);

            if (!typeof(BaseFlowBlock).IsAssignableFrom(property.PropertyType))
                throw new InvalidOperationException($"Property '{property.Name}' is not assignable to {nameof(BaseFlowBlock)}.");

            var directValue = property.GetValue(flowBlock) as BaseFlowBlock;
            if (directValue != null)
                return new AssociatedFlowBlockResolutionResult(directValue, AssociatedFlowBlockResolutionSource.Property);

            if (_getPreviousFlowBlockOnPathMethod == null)
                return AssociatedFlowBlockResolutionResult.NotResolved;

            var pathResolved = _getPreviousFlowBlockOnPathMethod.Invoke(
                flowBlock,
                new object[] { flowBlock, new[] { property.PropertyType } }) as BaseFlowBlock;

            if (pathResolved != null)
                return new AssociatedFlowBlockResolutionResult(pathResolved, AssociatedFlowBlockResolutionSource.Path);

            return AssociatedFlowBlockResolutionResult.NotResolved;
        }

        public static AssociatedFlowBlockResolutionResult Resolve(BaseFlowBlock flowBlock, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(flowBlock);
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            var property = flowBlock.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' was not found on flow block '{flowBlock.Name}'.");

            if (!typeof(BaseFlowBlock).IsAssignableFrom(property.PropertyType))
                throw new InvalidOperationException($"Property '{property.Name}' is not a BaseFlowBlock reference property.");

            if (property.GetCustomAttribute<AssociatedFlowBlockResolvableAttribute>() == null)
                throw new InvalidOperationException($"Property '{property.Name}' is not marked with AssociatedFlowBlockResolvableAttribute.");

            return Resolve(flowBlock, property);
        }
    }
}
