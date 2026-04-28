using FlowBlox.Core.Models.FlowBlocks.Base;
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

        private static readonly bool EnableTrace = false;
        private const string TracePrefix = "[FlowBlockAutoLayout]";
        private const int DefaultOriginX = 60;
        private const int DefaultOriginY = 260;
        private const int DefaultBlockWidth = 328;
        private const int DefaultBlockHeight = 235;
        private const int HorizontalSpacing = 80;
        private const int VerticalSpacing = 40;
        private const int ComponentSpacing = 140;
        private const int RelaxationPasses = 3;

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

                    foreach (var prev in current.ReferencedFlowBlocks.Where(x => blocks.Contains(x)))
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
                x => x,
                x => x.ReferencedFlowBlocks.Where(componentSet.Contains).ToList());

            var locations = new Dictionary<BaseFlowBlock, Point>();
            var roots = GetRoots(component, predecessors);

            var rootCursorY = componentTop;
            foreach (var root in roots)
            {
                var rootSize = sizeMap[root];
                var sourceLocation = root.Location;

                var x = sourceLocation.X > 0 ? sourceLocation.X : componentLeft;
                var y = sourceLocation.Y > 0 ? sourceLocation.Y : rootCursorY;

                locations[root] = new Point(Math.Max(0, x), Math.Max(0, y));
                TraceLine($"Root placement: Block={BlockName(root)}, Source=({sourceLocation.X},{sourceLocation.Y}), Final=({locations[root].X},{locations[root].Y}), Size=({rootSize.Width},{rootSize.Height})");
                rootCursorY += rootSize.Height + VerticalSpacing;
            }

            foreach (var block in component.Where(x => !locations.ContainsKey(x)))
            {
                var size = sizeMap[block];
                locations[block] = new Point(componentLeft, rootCursorY);
                rootCursorY += size.Height + VerticalSpacing;
            }

            for (var pass = 0; pass < RelaxationPasses; pass++)
            {
                TraceLine($"Relaxation pass start: Pass={pass + 1}/{RelaxationPasses}");
                var centerCandidates = component.ToDictionary(
                    x => x,
                    _ => new List<double>());

                foreach (var parent in component)
                {
                    if (!successors.TryGetValue(parent, out var children))
                        continue;

                    var orderedChildren = children.Where(componentSet.Contains).ToList();
                    if (orderedChildren.Count == 0)
                        continue;

                    var parentLocation = locations[parent];
                    var parentSize = sizeMap[parent];
                    var parentCenter = GetCenterY(parentLocation, parentSize);
                    var nextX = parentLocation.X + parentSize.Width + HorizontalSpacing;

                    // Use actual child heights so centered groups remain stable even when blocks differ in size.
                    var stackedHeight = orderedChildren
                        .Select(child => sizeMap[child].Height)
                        .Sum() + (Math.Max(0, orderedChildren.Count - 1) * VerticalSpacing);
                    var currentTop = parentCenter - (stackedHeight / 2.0);
                    var rangeTop = currentTop;
                    var rangeBottom = currentTop + stackedHeight;

                    TraceLine(
                        $"Parent center calc: Block={BlockName(parent)}, ParentLoc=({parentLocation.X},{parentLocation.Y}), ParentSize=({parentSize.Width},{parentSize.Height}), ParentCenterY={parentCenter:F2}, ChildCount={orderedChildren.Count}, StackedHeight={stackedHeight:F2}, RangeTop={rangeTop:F2}, RangeBottom={rangeBottom:F2}, NextX={nextX}");

                    for (var i = 0; i < orderedChildren.Count; i++)
                    {
                        var child = orderedChildren[i];
                        var childLocation = locations[child];
                        var childSize = sizeMap[child];
                        var targetCenter = currentTop + (childSize.Height / 2.0);

                        centerCandidates[child].Add(targetCenter);
                        ApplyCenterAverageIfNotRoot(child, roots, centerCandidates, sizeMap, locations);
                        TraceLine(
                            $"Child target center: Parent={BlockName(parent)}, Child={BlockName(child)}, ChildIndex={i}, ChildLoc=({childLocation.X},{childLocation.Y}), ChildSize=({childSize.Width},{childSize.Height}), TargetCenterY={targetCenter:F2}");
                        currentTop += childSize.Height + VerticalSpacing;

                        if (childLocation.X < nextX)
                            locations[child] = new Point(nextX, childLocation.Y);

                        if (predecessors[child].Count > 1)
                        {
                            var predecessorCenter = predecessors[child]
                                .Select(p => GetCenterY(locations[p], sizeMap[p]))
                                .Average();
                            centerCandidates[child].Add(predecessorCenter);
                            ApplyCenterAverageIfNotRoot(child, roots, centerCandidates, sizeMap, locations);
                            var predecessorsText = string.Join(", ", predecessors[child].Select(BlockName));
                            TraceLine(
                                $"Child predecessor center: Child={BlockName(child)}, Predecessors=[{predecessorsText}], PredecessorCenterY={predecessorCenter:F2}");
                        }
                    }
                }

                foreach (var block in component)
                {
                    if (roots.Contains(block))
                        continue;

                    var blockSize = sizeMap[block];
                    var candidates = centerCandidates[block];
                    if (candidates.Count == 0)
                        continue;

                    var avgCenter = candidates.Average();
                    var targetTop = (int)Math.Round(avgCenter - (blockSize.Height / 2.0));
                    var current = locations[block];
                    TraceLine(
                        $"Block center apply: Block={BlockName(block)}, CurrentTop={current.Y}, SizeH={blockSize.Height}, CandidateCount={candidates.Count}, Candidates=[{string.Join(", ", candidates.Select(x => x.ToString("F2")))}], AvgCenterY={avgCenter:F2}, TargetTopRaw={targetTop}, TargetTopClamped={Math.Max(0, targetTop)}");
                    locations[block] = new Point(current.X, Math.Max(0, targetTop));
                }

                TraceLine($"Relaxation pass end: Pass={pass + 1}/{RelaxationPasses}");
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

        private static void ApplyCenterAverageIfNotRoot(
            BaseFlowBlock block,
            List<BaseFlowBlock> roots,
            Dictionary<BaseFlowBlock, List<double>> centerCandidates,
            Dictionary<BaseFlowBlock, Size> sizeMap,
            Dictionary<BaseFlowBlock, Point> locations)
        {
            if (roots.Contains(block))
                return;

            var candidates = centerCandidates[block];
            if (candidates.Count == 0)
                return;

            var blockSize = sizeMap[block];
            var avgCenter = candidates.Average();
            var targetTop = (int)Math.Round(avgCenter - (blockSize.Height / 2.0));
            var current = locations[block];
            locations[block] = new Point(current.X, Math.Max(0, targetTop));
        }

        private static void TraceLine(string message)
        {
            if (!EnableTrace)
                return;

            Trace.WriteLine($"{TracePrefix} {message}");
        }

    }
}
