using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util.DeepCopier
{
    public class DynamicDeepCopier : DeepCopierBase<object>
    {
        private Dictionary<Type, Type> _copyTypeMap;

        public DynamicDeepCopier(List<DeepCopyAction> deepCopyActions): base(deepCopyActions) { }

        public DynamicDeepCopier(Dictionary<Type, Type> copyTypeMap = null) : base()
        {
            this._copyTypeMap = copyTypeMap;
        }

        protected override Type GetCopyType(Type sourceType)
        {
            if (_copyTypeMap == null)
                return sourceType;

            if (_copyTypeMap.ContainsKey(sourceType))
                return _copyTypeMap[sourceType];

            return sourceType;
        }
    }
}
