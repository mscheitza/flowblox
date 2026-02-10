using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Util.FlowBlocks
{
    internal class CommonFlowBlockResolver
    {
        public static BaseFlowBlock FindCommonFlowBlock(BaseFlowBlock startBlock)
        {
            var allPaths = new List<List<BaseFlowBlock>>();
            FindPaths(startBlock, new List<BaseFlowBlock>(), allPaths);
            allPaths.ForEach(x => x.Remove(startBlock));
            return FindCommonFlowBlock(allPaths, startBlock.ReferencedFlowBlocks);
        }

        private static void FindPaths(BaseFlowBlock currentBlock, List<BaseFlowBlock> currentPath, List<List<BaseFlowBlock>> allPaths)
        {
            currentPath.Add(currentBlock);

            if (currentBlock.ReferencedFlowBlocks == null || currentBlock.ReferencedFlowBlocks.Count == 0)
            {
                allPaths.Add(new List<BaseFlowBlock>(currentPath));
            }
            else
            {
                foreach (var previousBlock in currentBlock.ReferencedFlowBlocks)
                {
                    FindPaths(previousBlock, new List<BaseFlowBlock>(currentPath), allPaths);
                }
            }
        }

        private static BaseFlowBlock FindCommonFlowBlock(List<List<BaseFlowBlock>> allPaths, IEnumerable<BaseFlowBlock> exceptFlowBlocks)
        {
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
