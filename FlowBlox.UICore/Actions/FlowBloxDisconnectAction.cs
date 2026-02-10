using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.UICore.Actions
{
    public class FlowBloxDisconnectAction : FlowBloxBaseAction
    {
        public BaseFlowBlock From { get; set; }

        public BaseFlowBlock To { get; set; }

        public override void Undo()
        {
            // Rückgängig machen des Disconnects bedeutet, die Verbindung wiederherzustellen
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (!referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Add(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Undo();
        }

        public override void Invoke()
        {
            // Trennen der Verbindung
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Remove(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Invoke();
        }
    }
}