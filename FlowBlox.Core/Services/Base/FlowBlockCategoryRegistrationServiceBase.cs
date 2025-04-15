using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;

namespace FlowBlox.Core.Services.Base
{
    public abstract class FlowBlockCategoryRegistrationServiceBase : IFlowBloxCategoryRegistrationService
    {
        public abstract IEnumerable<FlowBlockCategory> GetAllCategoriesInModule();

        public void Register()
        {
            foreach (var category in GetAllCategoriesInModule())
            {
                FlowBlockCategory.Register(category);
            }
        }

        public void Unregister()
        {
            foreach (var category in GetAllCategoriesInModule())
            {
                FlowBlockCategory.Unregister(category);
            }
        }
    }
}
