using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Util.FlowBlocks
{
    internal class CommonFlowBlockResolver
    {
        public static BaseFlowBlock FindCommonFlowBlock(BaseFlowBlock startBlock)
        {
            if (startBlock == null)
                return null;

            var allPaths = new List<List<BaseFlowBlock>>();
            FindPaths(startBlock, new List<BaseFlowBlock>(), allPaths, new HashSet<BaseFlowBlock>());
            if (allPaths.Count == 0)
                return null;

            allPaths.ForEach(x => x.Remove(startBlock));
            return FindCommonFlowBlock(allPaths, startBlock.ReferencedFlowBlocks);
        }

        private static void FindPaths(
            BaseFlowBlock currentBlock,
            List<BaseFlowBlock> currentPath,
            List<List<BaseFlowBlock>> allPaths,
            HashSet<BaseFlowBlock> currentPathBlocks)
        {
            if (currentBlock == null)
                return;

            if (!currentPathBlocks.Add(currentBlock))
            {
                allPaths.Add(new List<BaseFlowBlock>(currentPath));
                return;
            }

            currentPath.Add(currentBlock);

            if (currentBlock.ReferencedFlowBlocks == null || currentBlock.ReferencedFlowBlocks.Count == 0)
            {
                allPaths.Add(new List<BaseFlowBlock>(currentPath));
            }
            else
            {
                foreach (var previousBlock in currentBlock.ReferencedFlowBlocks)
                {
                    FindPaths(
                        previousBlock,
                        new List<BaseFlowBlock>(currentPath),
                        allPaths,
                        new HashSet<BaseFlowBlock>(currentPathBlocks));
                }
            }
        }

        private static BaseFlowBlock FindCommonFlowBlock(List<List<BaseFlowBlock>> allPaths, IEnumerable<BaseFlowBlock> exceptFlowBlocks)
        {
            if (allPaths == null || allPaths.Count == 0)
                return null;

            HashSet<BaseFlowBlock> commonBlocks = new HashSet<BaseFlowBlock>(allPaths.First());
            foreach (var path in allPaths)
            {
                commonBlocks.IntersectWith(path);
            }
            return commonBlocks
                .Except(exceptFlowBlocks)
                .FirstOrDefault();
        }
    }
}