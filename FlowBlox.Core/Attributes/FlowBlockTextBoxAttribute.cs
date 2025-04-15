using FlowBlox.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockTextBoxAttribute : Attribute
    {
        public bool MultiLine { get; set; }

        public char PasswordChar { get; set; }

        public bool IsCodingMode { get; set; }

        /// <summary>
        /// Optional syntax highlighting definition used by the AvalonEdit code editor.
        /// <para>
        /// This can be either:
        /// </para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>The name of a built-in highlighting definition (e.g., <c>"XML"</c>, <c>"C#"</c>, <c>"JavaScript"</c>).</description>
        ///   </item>
        ///   <item>
        ///     <description>The full resource name of an embedded <c>.xshd</c> file (e.g., <c>"FlowBlox.UICore.Resources.Highlighting.SQL.xshd"</c>).</description>
        ///   </item>
        /// </list>
        /// <para>
        /// Only applicable when <see cref="IsCodingMode"/> is set to <c>true</c>.
        /// </para>
        /// </summary>
        public string SyntaxHighlighting { get; set; }

        public FlowBlockTextBoxAttribute()
        {
            MultiLine = false;
            IsCodingMode = false;
        }
    }
}
