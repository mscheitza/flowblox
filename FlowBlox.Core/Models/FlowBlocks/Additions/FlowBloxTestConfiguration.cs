using CsvHelper;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Enums;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name = "FlowBloxTestConfiguration", ResourceType = typeof(FlowBloxTexts))]
    public class FlowBloxTestConfiguration : INotifyPropertyChanged
    {
        private FieldElement _fieldElement;
        private FlowBloxTestConfigurationSelectionMode _selectionMode;
        private List<ExpectationCondition> _expectationConditions;
        private string _userInput;
        private int? _index;

        public FieldElement FieldElement
        {
            get => _fieldElement;
            set
            {
                if (_fieldElement != value)
                {
                    _fieldElement = value;
                    OnPropertyChanged();
                }
            }
        }

        public FlowBloxTestConfigurationSelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (_selectionMode != value)
                {
                    _selectionMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsIndexVisible));
                }
            }
        }

        [Display(Name = "FlowBloxTestConfiguration_ExpectationConditions", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.GridView)]
        public List<ExpectationCondition> ExpectationConditions
        {
            get => _expectationConditions;
            set
            {
                if (_expectationConditions != value)
                {
                    _expectationConditions = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserInput
        {
            get => _userInput;
            set
            {
                if (_userInput != value)
                {
                    _userInput = value;
                    OnPropertyChanged();
                }
            }
        }


        public int? Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsIndexVisible => SelectionMode == FlowBloxTestConfigurationSelectionMode.Index;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
