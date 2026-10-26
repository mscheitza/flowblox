using FlowBlox.Core;
using FlowBlox.Core.Models.Deletion;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    public partial class DeletionConfirmationWindow : MetroWindow
    {
        public DeletionConfirmationWindow(DeletionReport report)
        {
            InitializeComponent();
            DataContext = new ConfirmationViewModel(report);
        }

        private void Delete_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        private sealed class ConfirmationViewModel
        {
            public ConfirmationViewModel(DeletionReport report)
            {
                Entries = report.Dependencies.Select(CreateEntry).ToList();
            }

            public string Title => L("DeletionConfirmation_Title");
            public string HeaderTitle => L("DeletionConfirmation_Header");
            public string Description => L("DeletionConfirmation_Description");
            public string BackText => L("DeletionConfirmation_Back");
            public string DeleteText => L("DeletionConfirmation_Delete");
            public IReadOnlyList<ConfirmationEntry> Entries { get; }

            private static ConfirmationEntry CreateEntry(DeletionDependency dependency)
            {
                var status = dependency.ReviewStatus;
                var palette = GetPalette(status);
                var objectType = FlowBloxComponentHelper.GetDisplayName(dependency.ReferencingObject);
                var objectName = dependency.ReferencingObject.ToString();
                if (dependency.EditTarget != null &&
                    !ReferenceEquals(dependency.EditTarget, dependency.ReferencingObject))
                {
                    objectType = $"{FlowBloxComponentHelper.GetDisplayName(dependency.EditTarget)} \u2192 {objectType}";
                    objectName = $"{dependency.EditTarget} \u2192 {objectName}";
                }

                return new ConfirmationEntry
                {
                    ObjectType = objectType,
                    ObjectName = objectName,
                    PropertyLabel = L("DeletionConfirmation_Property"),
                    Property = dependency.Property.GetCustomAttributes(typeof(DisplayAttribute), true)
                        .OfType<DisplayAttribute>().FirstOrDefault()?.GetName() ?? dependency.Property.Name,
                    ImpactLabel = L("DeletionConfirmation_Impact"),
                    Impact = $"{L("DeletionConfirmation_Impact_" + dependency.Impact)}: " +
                             dependency.ReferencedObject,
                    Status = L("DeletionConfirmation_Status_" + status),
                    Icon = status == DeletionReviewStatus.Information
                        ? PackIconMaterialKind.InformationOutline
                        : status == DeletionReviewStatus.ReviewRecommended
                            ? PackIconMaterialKind.ClipboardTextSearchOutline
                            : PackIconMaterialKind.AlertOutline,
                    CardBackground = palette.CardBackground,
                    CardBorder = palette.CardBorder,
                    IconBackground = palette.IconBackground,
                    StatusBackground = palette.StatusBackground,
                    Accent = palette.Accent
                };
            }

            private static CardPalette GetPalette(DeletionReviewStatus status) => status switch
            {
                DeletionReviewStatus.Information => new CardPalette(
                    B(244, 248, 252), B(184, 211, 234), B(221, 235, 247), B(230, 240, 248), B(58, 110, 165)),
                DeletionReviewStatus.ReviewRecommended => new CardPalette(
                    B(255, 247, 246), B(232, 199, 195), B(247, 227, 224), B(245, 226, 223), B(166, 83, 75)),
                _ => new CardPalette(
                    B(255, 244, 242), B(228, 184, 178), B(247, 219, 215), B(243, 216, 212), B(153, 66, 58))
            };

            private static Brush B(byte red, byte green, byte blue)
            {
                var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                brush.Freeze();
                return brush;
            }
        }

        private sealed class ConfirmationEntry
        {
            public string ObjectType { get; init; }
            public string ObjectName { get; init; }
            public string PropertyLabel { get; init; }
            public string Property { get; init; }
            public string ImpactLabel { get; init; }
            public string Impact { get; init; }
            public string Status { get; init; }
            public PackIconMaterialKind Icon { get; init; }
            public Brush CardBackground { get; init; }
            public Brush CardBorder { get; init; }
            public Brush IconBackground { get; init; }
            public Brush StatusBackground { get; init; }
            public Brush Accent { get; init; }
        }

        private sealed record CardPalette(
            Brush CardBackground,
            Brush CardBorder,
            Brush IconBackground,
            Brush StatusBackground,
            Brush Accent);

        private static string L(string key) =>
            FlowBloxResourceUtil.GetLocalizedString(key, typeof(FlowBloxTexts));
    }
}
