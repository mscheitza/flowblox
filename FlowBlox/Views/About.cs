using FlowBlox.Core;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace FlowBlox.Views
{
    internal partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            string productName = Application.ProductName;

            string productVersion = FlowBloxVersionHelper.GetDisplayVersion(Application.ProductVersion);

            string description = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;
            string copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

            labelTitle.Text = productName;
            labelVersion.Text = $"Version {productVersion}";
            labelDescription.Text = string.IsNullOrWhiteSpace(description)
                                    ? "FlowBlox – Open Source Visual Data Flow IDE"
                                    : description;

            string copyrightOrDefault = string.IsNullOrWhiteSpace(copyright)
                ? $"© {DateTime.Now.Year} Marcel Scheitza and contributors. Licensed under MIT."
                : copyright;

            copyrightOrDefault = System.Text.RegularExpressions.Regex.Replace(
                copyrightOrDefault,
                @"(©\s*\d{4})\s+",
                "$1" + Environment.NewLine
            );

            copyrightOrDefault = System.Text.RegularExpressions.Regex.Replace(
                copyrightOrDefault,
                @"\.\s+",
                "." + Environment.NewLine
            );

            labelCopyright.Text = copyrightOrDefault;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
