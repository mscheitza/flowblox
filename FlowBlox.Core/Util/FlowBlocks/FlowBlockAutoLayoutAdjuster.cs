using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.ControlFlow;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Actions;
using FlowBlox.Core.Provider;
using System.Drawing;
using System.Diagnostics;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public sealed class FlowBlockAutoLayoutResult
    {
        public int TotalFlowBlocks { get; init; }
        public int UpdatedFlowBlocks { get; init; }
        public int ComponentsProcessed { get; init; }
    }

    public static class FlowBlockAutoLayoutAdjuster
    {
        [ThreadStatic]
        private static List<FlowBloxMoveAction> _recordedMoveActions;

        private static readonly bool EnableTrace = true;
        private const string TracePrefix = "[FlowBlockAutoLayout]";
        private const int DefaultOriginX = 60;
        private const int DefaultOriginY = 260;
        private const int DefaultBlockWidth = 328;
        private const int DefaultBlockHeight = 235;
        private const int HorizontalSpacing = 80;
        private const int VerticalSpacing = 40;
        private const int ComponentSpacing = 140;

        public static FlowBlockAutoLayoutResult AdjustCurrentRegistryLayout()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var flowBlocks = registry?.GetFlowBlocks()?.ToList() ?? new List<BaseFlowBlock>();
            return Adjust(flowBlocks);
        }

        public static FlowBlockAutoLayoutResult Adjust(IEnumerable<BaseFlowBlock> flowBlocks)
        {
            _recordedMoveActions = new List<FlowBloxMoveAction>();

            var blocks = (flowBlocks ?? Enumerable.Empty<BaseFlowBlock>())
                .Where(x => x != null)
                .Distinct()
                .ToList();

            TraceLine($"Adjust start. BlockCount={blocks.Count}");

            if (blocks.Count == 0)
            {
                return new FlowBlockAutoLayoutResult();
            }

            var blockSet = new HashSet<BaseFlowBlock>(blocks);
            var sizeMap = blocks.ToDictionary(x => x, GetEffectiveSize);
            var successors = blocks.ToDictionary(
                x => x,
                x => x.GetNextFlowBlocks().Where(blockSet.Contains).ToList());

            TraceBlocks(blocks, sizeMap, successors);

            var components = BuildComponents(blocks, successors);
            var orderedComponents = components
                .OrderByDescending(x => x.Any(b => b is StartFlowBlock))
                .ThenBy(x => x.Min(b => b.Location.Y))
                .ThenBy(x => x.Min(b => b.Location.X))
                .ToList();

            var locationMap = new Dictionary<BaseFlowBlock, Point>();
            var nextComponentTop = DetermineGlobalTop(blocks);
            var componentLeft = DetermineGlobalLeft(blocks);

            TraceLine($"Global bounds: Left={componentLeft}, Top={nextComponentTop}");

            foreach (var component in orderedComponents)
            {
                TraceLine($"Layout component start. Count={component.Count}, TopStart={nextComponentTop}, LeftStart={componentLeft}");
                var componentLocations = LayoutComponent(component, successors, sizeMap, componentLeft, nextComponentTop);
                foreach (var kv in componentLocations)
                    locationMap[kv.Key] = kv.Value;

                var componentBottom = component
                    .Select(x => componentLocations[x].Y + sizeMap[x].Height)
                    .DefaultIfEmpty(nextComponentTop)
                    .Max();

                TraceLine($"Layout component end. Bottom={componentBottom}, NextComponentTop={componentBottom + ComponentSpacing}");

                nextComponentTop = componentBottom + ComponentSpacing;
            }

            ShiftLayoutBelowVerticalOrigin(locationMap);

            var updated = 0;
            foreach (var block in blocks)
            {
                if (!locationMap.TryGetValue(block, out var location))
                    continue;

                if (block.Location != location)
                {
                    TraceLine($"Apply location: Block={BlockName(block)}, Old=({block.Location.X},{block.Location.Y}), New=({location.X},{location.Y})");
                    _recordedMoveActions.Add(new FlowBloxMoveAction
                    {
                        FlowBlock = block,
                        From = block.Location,
                        To = location
                    });
                    block.Location = location;
                    updated++;
                }
            }

            return new FlowBlockAutoLayoutResult
            {
                TotalFlowBlocks = blocks.Count,
                UpdatedFlowBlocks = updated,
                ComponentsProcessed = orderedComponents.Count
            };
        }

        public static IReadOnlyList<FlowBloxMoveAction> GetRecordedMoveActions()
        {
            return _recordedMoveActions?.ToList() ?? new List<FlowBloxMoveAction>();
        }

        private static List<List<BaseFlowBlock>> BuildComponents(
            List<BaseFlowBlock> blocks,
            Dictionary<BaseFlowBlock, List<BaseFlowBlock>> successors)
        {
            var components = new List<List<BaseFlowBlock>>();
            var visited = new HashSet<BaseFlowBlock>();
            var blockSet = blocks.ToHashSet();

            foreach (var start in blocks)
            {
                if (!visited.Add(start))
                    continue;

                var queue = new Queue<BaseFlowBlock>();
                var component = new List<BaseFlowBlock>();
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);

                    foreach (var next in successors[current])
                    {
                        if (visited.Add(next))
                            queue.Enqueue(next);
                    }

                    foreach (var prev in current.ReferencedFlowBlocks.Where(blockSet.Contains))
                    {
                        if (visited.Add(prev))
                            queue.Enqueue(prev);
                    }
                }

                components.Add(component);
            }

            return components;
        }

        private static Dictionary<BaseFlowBlock, Point> LayoutComponent(
            List<BaseFlowBlock> component,
            Dictionary<BaseFlowBlock, List<BaseFlowBlock>> successors,
            Dictionary<BaseFlowBlock, Size> sizeMap,
            int componentLeft,
            int componentTop)
        {
            var componentSet = new HashSet<BaseFlowBlock>(component);
            var predecessors = component.ToDictionary(
                block => block,
                block => block.ReferencedFlowBlocks.Where(componentSet.Contains).ToList());
            var roots = GetRoots(component, predecessors);
            var rootSet = roots.ToHashSet();
            var pendingInputs = predecessors.ToDictionary(pair => pair.Key, pair => pair.Value.Count);
            var ready = new Queue<BaseFlowBlock>(roots);
            var scheduled = roots.ToHashSet();
            var executionOrder = new List<BaseFlowBlock>();

            // Release a shared successor only after every incoming path has finished.
            // Cycles have no dependency order; anchor the remaining cycle and ignore its
            // backward edges during measurement and placement.
            while (executionOrder.Count < component.Count)
            {
                if (ready.Count == 0)
                {
                    var cycleRoot = component.First(block => !scheduled.Contains(block));
                    rootSet.Add(cycleRoot);
                    scheduled.Add(cycleRoot);
                    ready.Enqueue(cycleRoot);
                    TraceLine($"Cycle anchor: Block={BlockName(cycleRoot)}");
                }

                var block = ready.Dequeue();
                executionOrder.Add(block);
                foreach (var next in successors[block])
                {
                    pendingInputs[next]--;
                    if (pendingInputs[next] == 0 && scheduled.Add(next))
                        ready.Enqueue(next);
                }
            }

            var orderIndices = executionOrder
                .Select((block, index) => (block, index))
                .ToDictionary(pair => pair.block, pair => pair.index);
            var pathChildren = component.ToDictionary(
                block => block,
                block => successors[block].Where(next =>
                    predecessors[next].Count == 1 && !rootSet.Contains(next) &&
                    orderIndices[next] > orderIndices[block]).ToList());
            var pathBounds = new Dictionary<BaseFlowBlock, (double Top, double Bottom)>();
            var childCenterOffsets = new Dictionary<BaseFlowBlock, double>();

            // Measure each separate path once, stopping before a shared successor.
            // Bounds are relative to the block center and include nested branches.
            for (var index = executionOrder.Count - 1; index >= 0; index--)
            {
                var block = executionOrder[index];
                var children = pathChildren[block];
                var top = -sizeMap[block].Height / 2.0;
                var bottom = sizeMap[block].Height / 2.0;
                var totalHeight = children.Sum(child => pathBounds[child].Bottom - pathBounds[child].Top)
                    + Math.Max(0, children.Count - 1) * VerticalSpacing;
                var cursor = -totalHeight / 2.0;

                foreach (var child in children)
                {
                    var bounds = pathBounds[child];
                    var centerOffset = children.Count == 1 ? 0 : cursor - bounds.Top;
                    if (children.Count == 1 && successors[block].Count == 1 && child is RecursiveTargetFlowBlockBase)
                        centerOffset = sizeMap[block].Height / 2.0 + VerticalSpacing + sizeMap[child].Height / 2.0;

                    childCenterOffsets[child] = centerOffset;
                    top = Math.Min(top, centerOffset + bounds.Top);
                    bottom = Math.Max(bottom, centerOffset + bounds.Bottom);
                    cursor += bounds.Bottom - bounds.Top + VerticalSpacing;
                }

                pathBounds[block] = (top, bottom);
                TraceLine($"Path bounds: Block={BlockName(block)}, TopOffset={top:F2}, BottomOffset={bottom:F2}, ChildCount={children.Count}");
            }

            var locations = new Dictionary<BaseFlowBlock, Point>();
            var rootCursorY = componentTop;
            foreach (var block in executionOrder)
            {
                var size = sizeMap[block];
                if (rootSet.Contains(block))
                {
                    var source = block.Location;
                    var x = source.X > 0 ? source.X : componentLeft;
                    var y = source.Y > 0 ? source.Y : rootCursorY;
                    locations[block] = new Point(Math.Max(0, x), Math.Max(0, y));
                    rootCursorY += (int)Math.Ceiling(pathBounds[block].Bottom - pathBounds[block].Top) + VerticalSpacing;
                    TraceLine($"Root placement: Block={BlockName(block)}, Final=({locations[block].X},{locations[block].Y})");
                    continue;
                }

                var inputs = predecessors[block].Where(locations.ContainsKey).ToList();
                var center = inputs.Average(input => GetCenterY(locations[input], sizeMap[input]));
                if (inputs.Count == 1 && childCenterOffsets.TryGetValue(block, out var offset))
                    center += offset;
                var nextX = inputs.Max(input => locations[input].X + sizeMap[input].Width + HorizontalSpacing);
                if (inputs.Count == 1 && successors[inputs[0]].Count == 1 && block is RecursiveTargetFlowBlockBase)
                    nextX = locations[inputs[0]].X;

                locations[block] = new Point(nextX, (int)Math.Round(center - size.Height / 2.0));
                TraceLine($"Path placement: Block={BlockName(block)}, InputCount={inputs.Count}, Final=({locations[block].X},{locations[block].Y})");
            }

            return locations;
        }

        private static List<BaseFlowBlock> GetRoots(
            List<BaseFlowBlock> component,
            Dictionary<BaseFlowBlock, List<BaseFlowBlock>> predecessors)
        {
            var starts = component
                .Where(x => x is StartFlowBlock)
                .OrderBy(x => x.Location.Y)
                .ThenBy(x => x.Location.X)
                .ToList();

            var roots = component
                .Where(x => predecessors[x].Count == 0)
                .OrderBy(x => x.Location.Y)
                .ThenBy(x => x.Location.X)
                .ToList();

            var ordered = new List<BaseFlowBlock>();
            foreach (var start in starts)
            {
                if (!ordered.Contains(start))
                    ordered.Add(start);
            }

            foreach (var root in roots)
            {
                if (!ordered.Contains(root))
                    ordered.Add(root);
            }

            if (ordered.Count == 0 && component.Count > 0)
                ordered.Add(component.OrderBy(x => x.Location.Y).ThenBy(x => x.Location.X).First());

            return ordered;
        }

        private static Size GetEffectiveSize(BaseFlowBlock flowBlock)
        {
            var width = flowBlock.Size.Width > 0 ? flowBlock.Size.Width : DefaultBlockWidth;
            var height = flowBlock.Size.Height > 0 ? flowBlock.Size.Height : DefaultBlockHeight;
            return new Size(width, height);
        }

        private static int DetermineGlobalTop(List<BaseFlowBlock> flowBlocks)
        {
            var minY = flowBlocks
                .Select(x => x.Location.Y)
                .Where(x => x > 0)
                .DefaultIfEmpty(DefaultOriginY)
                .Min();

            return Math.Max(0, minY);
        }

        private static int DetermineGlobalLeft(List<BaseFlowBlock> flowBlocks)
        {
            var minX = flowBlocks
                .Select(x => x.Location.X)
                .Where(x => x > 0)
                .DefaultIfEmpty(DefaultOriginX)
                .Min();

            return Math.Max(0, minX);
        }

        private static double GetCenterY(Point location, Size size)
        {
            return location.Y + (size.Height / 2.0);
        }

        private static void ShiftLayoutBelowVerticalOrigin(Dictionary<BaseFlowBlock, Point> locations)
        {
            var minY = locations.Values.Select(x => x.Y).DefaultIfEmpty(0).Min();
            if (minY >= 0)
                return;

            var offsetY = -minY + VerticalSpacing;
            foreach (var block in locations.Keys.ToList())
            {
                var location = locations[block];
                locations[block] = new Point(location.X, location.Y + offsetY);
            }

            TraceLine($"Shifted complete layout down by {offsetY} because MinY was {minY}.");
        }

        private static void TraceBlocks(
            List<BaseFlowBlock> blocks,
            Dictionary<BaseFlowBlock, Size> sizeMap,
            Dictionary<BaseFlowBlock, List<BaseFlowBlock>> successors)
        {
            foreach (var block in blocks.OrderBy(x => x.Location.Y).ThenBy(x => x.Location.X))
            {
                var size = sizeMap[block];
                var next = successors.TryGetValue(block, out var nextFlowBlocks)
                    ? string.Join(", ", nextFlowBlocks.Select(BlockName))
                    : string.Empty;
                var referenced = string.Join(", ", block.ReferencedFlowBlocks.Select(BlockName));

                TraceLine(
                    $"Block snapshot: Block={BlockName(block)}, Loc=({block.Location.X},{block.Location.Y}), Size=({size.Width},{size.Height}), Next=[{next}], Referenced=[{referenced}]");
            }
        }

        private static string BlockName(BaseFlowBlock block)
        {
            return block?.Name ?? block?.GetType().Name ?? "<null>";
        }

        private static void TraceLine(string message)
        {
            if (!EnableTrace)
                return;

            Trace.WriteLine($"{TracePrefix} {message}");
        }

    }
}
