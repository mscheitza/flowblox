using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum CollectionQueryOperation
    {
        [Display(Name = "CollectionQueryOperation_First", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "CollectionQueryOperation_Last", ResourceType = typeof(FlowBloxTexts))]
        Last,
        [Display(Name = "CollectionQueryOperation_ElementAt", ResourceType = typeof(FlowBloxTexts))]
        ElementAt,
        [Display(Name = "CollectionQueryOperation_Minimum", ResourceType = typeof(FlowBloxTexts))]
        Minimum,
        [Display(Name = "CollectionQueryOperation_Maximum", ResourceType = typeof(FlowBloxTexts))]
        Maximum,
        [Display(Name = "CollectionQueryOperation_Average", ResourceType = typeof(FlowBloxTexts))]
        Average,
        [Display(Name = "CollectionQueryOperation_Any", ResourceType = typeof(FlowBloxTexts))]
        Any,
        [Display(Name = "CollectionQueryOperation_All", ResourceType = typeof(FlowBloxTexts))]
        All
    }
}
