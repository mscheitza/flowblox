using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using Models.FlowBlocks.AI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer
{
    public abstract class TokenizerBase : ManagedObject
    {
        protected TokenizerBase()
        {
            this.TokenTranslations = new ObservableCollection<TokenTranslation>();
        }

        [Display(Name = "TokenizerBase_TokenTranslations", Description = "TokenizerBase_TokenTranslations_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public ObservableCollection<TokenTranslation> TokenTranslations { get; set; }

        public abstract void Initialize();

        public abstract void DisposeTokenizer();

        protected string TranslateOut(string decodedText)
        {
            if (TokenTranslations == null || TokenTranslations.Count == 0)
                return decodedText;

            foreach (var translation in TokenTranslations)
            {
                if (string.IsNullOrEmpty(translation.SourceCharacter))
                    continue;

                string destination = translation.DestinationCharacter;

                if (string.IsNullOrEmpty(destination))
                {
                    destination = translation.SpecialCharacter switch
                    {
                        SpecialCharacter.LineBreak => "\n",
                        SpecialCharacter.Tab => "\t",
                        SpecialCharacter.Space => " ",
                        SpecialCharacter.CarriageReturn => "\r",
                        _ => null
                    };
                }

                if (!string.IsNullOrEmpty(destination))
                    decodedText = decodedText.Replace(translation.SourceCharacter, destination);
            }

            return decodedText;
        }

        protected string TranslateIn(string inputText)
        {
            if (TokenTranslations == null || TokenTranslations.Count == 0)
                return inputText;

            foreach (var translation in TokenTranslations)
            {
                if (string.IsNullOrEmpty(translation.SourceCharacter))
                    continue;

                string source = translation.SourceCharacter;

                string destination = translation.DestinationCharacter;
                if (string.IsNullOrEmpty(destination))
                {
                    destination = translation.SpecialCharacter switch
                    {
                        SpecialCharacter.LineBreak => "\n",
                        SpecialCharacter.Tab => "\t",
                        SpecialCharacter.Space => " ",
                        SpecialCharacter.CarriageReturn => "\r",
                        _ => null
                    };
                }

                if (!string.IsNullOrEmpty(destination))
                    inputText = inputText.Replace(destination, source);
            }

            return inputText;
        }
    }
}