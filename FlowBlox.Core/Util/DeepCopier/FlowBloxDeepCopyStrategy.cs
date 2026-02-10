using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Util.DeepCopier
{
    public class FlowBloxDeepCopyStrategy : IDeepCopyStrategy
    {
        private static FlowBloxDeepCopyStrategy _instance;

        private FlowBloxDeepCopyStrategy()
        {
        }

        public static FlowBloxDeepCopyStrategy Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlowBloxDeepCopyStrategy();
                return _instance;
            }
        }

        public List<DeepCopyAction> GetDeepCopyActions(object target)
        {
            return new List<DeepCopyAction>()
            {
                new DeepCopyAction() 
                { 
                    PropertyType = typeof(IManagedObject), 
                    Action = DeepCopyActions.CopyReference, 
                    ExceptCondition = managedObject => ManagedObject_ExceptCondition(managedObject, target) 
                },
                new DeepCopyAction() 
                { 
                    DeclaringType = typeof(BaseFlowBlock), 
                    Name = nameof(BaseFlowBlock.ReferencedFlowBlocks), 
                    Action = DeepCopyActions.CopyReference
                },
                new DeepCopyAction() 
                { 
                    PropertyType = typeof(BaseFlowBlock),
                    Action = DeepCopyActions.CopyReference,
                    ExceptCondition = managedObject => BaseFlowBlock_ExceptCondition(managedObject, target) 
                }
            };
        }

        private bool BaseFlowBlock_ExceptCondition(object flowBlock, object target)
        {
            if (flowBlock == target)
                return true;

            return false;
        }

        private bool ManagedObject_ExceptCondition(object managedObject, object target)
        {
            if (managedObject == target)
                return true;

            if (target is BaseFlowBlock)
                return ((BaseFlowBlock)target).IsManaged((IManagedObject)managedObject);

            return false;
        }
    }
}
