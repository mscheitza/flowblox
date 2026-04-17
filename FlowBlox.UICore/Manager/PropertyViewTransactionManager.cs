using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.DependencyInjection;
using System.IO;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Actions;
using FlowBlox.UICore.Interfaces;

namespace FlowBlox.UICore.Manager
{
    public class PropertyViewTransactionManager
	{
		private DynamicDeepCopier _deepCopier;
		private FlowBloxRegistry _registry;
		private Dictionary<object, object> _refMappings = new Dictionary<object, object>();
        private IFlowBloxProjectComponentProvider _componentProvider;
        private string _transactionProtocolFilePath;

        public PropertyViewTransactionManager()
		{
			_deepCopier = new DynamicDeepCopier();
			_deepCopier.OnAfterCopy += _deepCopier_OnAfterCopy;
			_registry = FlowBloxRegistryProvider.GetRegistry();
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<IFlowBloxProjectComponentProvider>();
        }

		private void _deepCopier_OnAfterCopy(object source, object copy)
		{
			if (source is IManagedObject && copy is IManagedObject)
			{
				_refMappings[source] = copy;
				_registry.ReplaceRef((IManagedObject)source, (IManagedObject)copy);
			}

			if (source is BaseFlowBlock && copy is BaseFlowBlock)
			{
				_refMappings[source] = copy;
				_registry.ReplaceRef((BaseFlowBlock)source, (BaseFlowBlock)copy);
			}

			if (copy is IFlowBloxComponent)
			{
				((IFlowBloxComponent)copy).OnAfterLoad();
			}
		}

		public class OpenResult
		{
			public object TransientTarget { get; set; }
			public FlowBloxRegistry Registry { get; set; }
		}

		public OpenResult Open(object target, bool detached = false)
		{
			_registry = FlowBloxRegistryProvider.OpenTransaction(detached);
			_deepCopier.PropertyActions = FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(target);
			var transientTarget = _deepCopier.Copy(target);
			var protocol = _deepCopier.GetProtocol();
			StoreProtocol(target, protocol);

			return new OpenResult
			{
				TransientTarget = transientTarget,
				Registry = _registry
			};
		}

        /// <summary>
        /// Adds a new object to the transaction by converting it into a transient object.
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public object Append(object sourceObject)
        {
            if (sourceObject == null)
                throw new ArgumentNullException(nameof(sourceObject));

            var copiedObject = _deepCopier.Copy(sourceObject);
            var protocol = _deepCopier.GetProtocol();

			if (!string.IsNullOrWhiteSpace(_transactionProtocolFilePath))
				StoreProtocol(protocol, _transactionProtocolFilePath);

            return copiedObject;
        }

        public void Cancel()
		{
			FlowBloxRegistryProvider.CancelTransaction();
		}

		public void Commit(object target, object transientTarget)
		{
			// Create restore object
			var restoreDeepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(target));
			var restoreObject = restoreDeepCopier.Copy(target);

			// Copy changes to the target object
			_deepCopier.SpecialMode = DeepCopierSpecialMode.Recopy;
			_deepCopier.PropertyActions = FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(transientTarget);
			_deepCopier.Copy(transientTarget, target);
			var protocol = _deepCopier.GetProtocol();
			StoreProtocol(transientTarget, protocol, true);

			// Create repetition object
			var repetitionDeepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(target));
			var repetitionObject = repetitionDeepCopier.Copy(target);

			// Create and append change
			_componentProvider.GetCurrentChangelist().AddChange(new FlowBloxEditAction()
			{
				Target = target,
				RestoreObject = restoreObject,
				RepetitionObject = repetitionObject
			});

			// Committing the changes to the registry
			FlowBloxRegistryProvider.CommitTransaction();
		}

		private void StoreProtocol(string protocol, string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Property name must not be null, empty, or whitespace.", nameof(filePath));

            File.WriteAllText(filePath, protocol);
        }

		private void StoreProtocol(object target, string protocol, bool recopy = false)
		{
			var protocolDirectory = FlowBloxOptions.GetOptionInstance().GetOption("Paths.DeepCopierProtocolDir").Value;

			if (!Directory.Exists(protocolDirectory))
				Directory.CreateDirectory(protocolDirectory);

			string fileName = target.GetType().Name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + (recopy ? "_recopy" : "") + ".txt";
            _transactionProtocolFilePath = protocolDirectory + "\\" + fileName;
			StoreProtocol(protocol, _transactionProtocolFilePath);
        }
	}
}
