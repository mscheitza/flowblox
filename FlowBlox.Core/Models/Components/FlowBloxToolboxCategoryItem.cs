using FlowBlox.Core.Util.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Components
{
    public sealed class FlowBloxToolboxCategoryItem
    {
        public FlowBloxToolboxCategoryItem(string name, string? displayNameResourceKey = null, Type? displayNameResourceType = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayNameResourceKey = displayNameResourceKey;
            DisplayNameResourceType = displayNameResourceType;
        }

        public string Name { get; }

        public string? DisplayNameResourceKey { get; }

        public Type? DisplayNameResourceType { get; }

        public string DisplayName => GetDisplayName();

        public string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(DisplayNameResourceKey) && DisplayNameResourceType != null)
            {
                string? localizedName = FlowBloxResourceUtil.GetLocalizedString(DisplayNameResourceKey, DisplayNameResourceType);
                if (!string.IsNullOrWhiteSpace(localizedName))
                    return localizedName;
            }

            return Name;
        }
    }
}
