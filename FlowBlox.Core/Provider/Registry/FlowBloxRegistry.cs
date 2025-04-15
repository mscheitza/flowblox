using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Components.Modifier;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Data;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Events;
using Microsoft.Win32;
using static FlowBlox.Core.Models.Components.FieldElement;
using FlowBlox.Core.Constants;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.Provider.Registry
{
    public class FlowBloxRegistry
    {
        private HashSet<BaseFlowBlock> _flowBlocks;
        private HashSet<IManagedObject> _managedObjects;
        private Dictionary<IFlowBloxComponent, IFlowBloxComponent> _originalReferences;

        public delegate void ManagedObjectRemovedHandler(ManagedObjectRemovedEventArgs eventArgs);
        public delegate void ManagedObjectAddedHandler(ManagedObjectAddedEventArgs eventArgs);

        public event ManagedObjectRemovedHandler OnManagedObjectRemoved;
        public event ManagedObjectAddedHandler OnManagedObjectAdded;

        public FlowBloxRegistry()
        {
            _flowBlocks = new HashSet<BaseFlowBlock>();
            _managedObjects = new HashSet<IManagedObject>();
            _originalReferences = new Dictionary<IFlowBloxComponent, IFlowBloxComponent>();
        }

        public FlowBloxRegistry(FlowBloxRegistry copyFrom) : this()
        {
            _flowBlocks.AddRange(copyFrom.GetFlowBlocks());
            _managedObjects.AddRange(copyFrom.GetManagedObjects());
        }

        public List<BaseResultFlowBlock> GetResultFlowBlocks() => this.GetFlowBlocks().OfType<BaseResultFlowBlock>().ToList();
        
        public List<BaseFlowBlock> GetInputFlowBlocks()
        {
            return _flowBlocks
                .Where(x => x.GetInputCardinality() != FlowBlockCardinalities.None)
                .ToList();
        }

        public List<FieldElement> GetAllFields() => GetFieldElements().ToList();

        public List<FieldElement> GetRuntimeFields() => GetFieldElements().Where(x => !x.UserField).ToList();

        public List<FieldElement> GetUserFields(UserFieldTypes userFieldType = UserFieldTypes.None)
        {
            var userFields = GetFieldElements().Where(x => x.UserField);
            if (userFieldType == UserFieldTypes.None)
                return userFields.ToList();
            else
                return userFields.Where(x => x.UserFieldType == userFieldType).ToList();
        }

        private string GetNextName<T>(string prefix = null)
            where T : IFlowBloxComponent
        {
            return GetNextName(typeof(T), prefix);
        }

        private string GetNextName(Type type, string prefix = null)
        {
            prefix ??= type.Name;

            IEnumerable<IFlowBloxComponent> candidates;
            if (typeof(BaseFlowBlock).IsAssignableFrom(type))
            {
                candidates = _flowBlocks;
            }
            else if (typeof(IManagedObject).IsAssignableFrom(type))
            {
                candidates = _managedObjects;
            }
            else
            {
                throw new InvalidOperationException($"Cannot determine source collection for type {type.Name}.");
            }

            var existingNames = candidates
                .Where(obj => type == obj.GetType())
                .Select(obj => obj.Name)
                .Where(name => !string.IsNullOrEmpty(name) && name.StartsWith(prefix))
                .ToHashSet();

            for (int i = 0; i < int.MaxValue; i++)
            {
                string candidate = prefix + i;
                if (!existingNames.Contains(candidate))
                    return candidate;
            }

            throw new InvalidOperationException($"Unable to find a unique name with prefix '{prefix}' for type '{type.Name}'.");
        }

        private string GetNextName<T>(object prefix) 
            where T : IFlowBloxComponent
        {
            return GetNextName<T>(prefix?.ToString());
        }

        public FieldElement CreateUserField(UserFieldTypes userFieldType, string fieldName = "")
        {
            int numericNameSuffix = GetUserFields().Count;

            var fieldElement = new FieldElement()
            {
                UserField = true,
                UserFieldType = userFieldType,
                ListOfValues = new ObservableCollection<ValueItem>(),
                Name = !string.IsNullOrEmpty(fieldName) ?
                    fieldName :
                    GetNextName<FieldElement>(prefix: userFieldType)
            };

            fieldElement.OnAfterCreate();
            fieldElement.OnAfterLoad();

            _managedObjects.Add(fieldElement);

            return fieldElement;
        }

        public bool HasUserField(string fieldName)
        {
            string fullyQualifiedName = "$User::" + fieldName;
            if (GetFieldElements().Any(x => x.FullyQualifiedName == fullyQualifiedName))
            {
                return true;
            }
            return false;
        }

        public void RemoveField(string fullyQualifiedFieldName)
        {
            foreach(var fieldElement in _managedObjects
                .OfType<FieldElement>()
                .Where(x => x.FullyQualifiedName == fullyQualifiedFieldName))
            {
                RemoveManagedbject(fieldElement);
            }
        }

        public bool HasField(string fullyQualifiedFieldName)
        {
            return GetFieldElements().Any(x => x.FullyQualifiedName == fullyQualifiedFieldName);
        }

        public FieldElement GetFieldElementOrNull(string fullyQualifiedFieldName)
        {
            return GetFieldElements().FirstOrDefault(x => x.FullyQualifiedName == fullyQualifiedFieldName);
        }

        public FieldElement CreateField(BaseResultFlowBlock source, FieldNameGenerationMode nameGenerationMode)
        {
            var field = new FieldElement(source, nameGenerationMode);
            field.OnAfterCreate();
            field.OnAfterLoad();
            _managedObjects.Add(field);
            return field;
        }

        public BaseFlowBlock GetStartFlowBlock() => this.GetFlowBlocks().OfType<StartFlowBlock>().FirstOrDefault();

        public List<BaseFlowBlock> GetPreviousElements(BaseFlowBlock nextElement)
        {
            List<BaseFlowBlock> whereNextElementList = new List<BaseFlowBlock>();
            foreach (var element in this.GetFlowBlocks())
            {
                if (element.GetNextFlowBlocks().Contains(nextElement))
                    whereNextElementList.Add(element);
            }
            return whereNextElementList;
        }

        public IEnumerable<T> GetFlowBlocks<T>() where T : BaseFlowBlock => this.GetFlowBlocks().OfType<T>();

        public IEnumerable<BaseFlowBlock> GetFlowBlocks(Type type) => this.GetFlowBlocks().Where(x => type.IsAssignableFrom(x.GetType()));

        public IEnumerable<BaseFlowBlock> GetFlowBlocks() => _flowBlocks;

        public virtual IEnumerable<IManagedObject> GetManagedObjects() => _managedObjects;

        public virtual IEnumerable<T> GetManagedObjects<T>() where T : IManagedObject => _managedObjects.OfType<T>();

        public virtual IEnumerable<IManagedObject> GetManagedObjects(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeof(IManagedObject).IsAssignableFrom(type))
                throw new ArgumentException($"Der Typ {type.FullName} implementiert nicht IManagedObject.", nameof(type));

            return _managedObjects.Where(obj => type.IsInstanceOfType(obj));
        }

        public IEnumerable<FieldElement> GetFieldElements() => _managedObjects.OfType<FieldElement>();

        public void RegisterFlowBlock(BaseFlowBlock flowBlock)  
        {
            if (_flowBlocks.Contains(flowBlock))
                return;

            _flowBlocks.Add(flowBlock);

            // Register fields
            if (flowBlock is BaseResultFlowBlock)
            {
                var resultFlowBlock = (BaseResultFlowBlock)flowBlock;
                resultFlowBlock.Fields.ForEach(y => y.Source = resultFlowBlock);
                resultFlowBlock.Fields.ForEach(y => this.Register(y));
            }

            // Register managed objects (transitional)
            foreach (var pi in flowBlock.GetType().GetProperties()
                .Where(x => typeof(IManagedObject).IsAssignableFrom(x.PropertyType) || 
                            (x.PropertyType.IsGenericType && typeof(IEnumerable<IManagedObject>).IsAssignableFrom(x.PropertyType))))
            {
                var propertyValue = pi.GetValue(flowBlock);
                if (propertyValue == null)
                    continue;

                if (propertyValue is IManagedObject managedObject)
                {
                    this.Register(managedObject);
                }
                else if (propertyValue is IEnumerable<IManagedObject> managedObjects)
                {
                    foreach (var obj in managedObjects)
                    {
                        this.Register(obj);
                    }
                }
            }
        }

        public virtual void RegisterManagedObject(IManagedObject managedObject)
        {
            if (!_managedObjects.Contains(managedObject))
            {
                _managedObjects.Add(managedObject);
                OnManagedObjectAdded?.Invoke(new ManagedObjectAddedEventArgs(managedObject));
            }
                
        }

        public void RemoveFlowBlock(BaseFlowBlock flowBlock)
        {
            _flowBlocks.Remove(flowBlock);

            foreach(var managedObject in flowBlock.DefinedManagedObjects)
            {
                RemoveManagedbject(managedObject);
            }
        }

        public virtual void RemoveManagedbject(IManagedObject managedObject)
        {
            _managedObjects.Remove(managedObject);
            OnManagedObjectRemoved?.Invoke(new ManagedObjectRemovedEventArgs(managedObject));
        }

        public virtual void Register(object item)
        {
            if (item is BaseFlowBlock)
                RegisterFlowBlock((BaseFlowBlock)item);
            if (item is IManagedObject)
                RegisterManagedObject((IManagedObject)item);
        }

        public virtual void Unregister(object item)
        {
            if (item is BaseFlowBlock)
                RemoveFlowBlock((BaseFlowBlock)item);
            if (item is IManagedObject)
                RemoveManagedbject((IManagedObject)item);
        }

        public virtual void ReplaceManagedObjectCollection<T>(List<T> managedObjects) where T : IManagedObject
        {
            foreach(var toRemove in _managedObjects.OfType<T>())
                _managedObjects.Remove(toRemove);

            foreach (var toAdd in managedObjects)
                _managedObjects.Add(toAdd);
        }

        public virtual void ReplaceRef(BaseFlowBlock from, BaseFlowBlock to)
        {
            StoreOriginalRef(from, to);

            if (_flowBlocks.Contains(from))
            {
                _flowBlocks.Remove(from);
                _flowBlocks.Add(to);
            }
        }

        public virtual void ReplaceRef(IManagedObject from, IManagedObject to)
        {
            StoreOriginalRef(from, to);

            if (_managedObjects.Contains(from))
            {
                _managedObjects.Remove(from);
                _managedObjects.Add(to);
            }
        }

        private void StoreOriginalRef(IFlowBloxComponent from, IFlowBloxComponent to)
        {
            if (!_originalReferences.ContainsKey(from))
                _originalReferences[to] = from;
            else
                _originalReferences[to] = _originalReferences[from];
        }

        public IFlowBloxComponent GetOriginalRef(IFlowBloxComponent flowBloxComponent)
        {
            return _originalReferences.TryGetValue(flowBloxComponent, out var originalReference)
                ? originalReference
                : flowBloxComponent;
        }

        public T CreateFlowBlockUnregistered<T>() where T : BaseFlowBlock
        {
            return (T)CreateFlowBlockUnregistered(typeof(T));
        }

        public BaseFlowBlock CreateFlowBlockUnregistered(Type type)
        {
            var flowBlock  = (BaseFlowBlock)Activator.CreateInstance(type);
            flowBlock!.Name = flowBlock.NamePrefix;
            if (flowBlock.CreateNumericNameSuffix)
                flowBlock.Name = GetNextName(type, flowBlock.NamePrefix);
            return flowBlock;
        }

        public void PostProcessManagedObjectCreated(IManagedObject managedObject)
        {
            if (string.IsNullOrEmpty(managedObject.Name))
                managedObject.Name = GetNextName(managedObject.GetType());

            managedObject.OnAfterCreate();
            managedObject.OnAfterLoad();
        }

        public void PostProcessFlowBlockCreated(BaseFlowBlock flowBlock)
        {
            flowBlock.OnAfterCreate();
            flowBlock.OnAfterLoad();
        }
    }
}
