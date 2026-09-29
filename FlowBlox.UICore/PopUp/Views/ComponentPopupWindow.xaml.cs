using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Resources;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FlowBlox.UICore.PopUp.Views
{
    public partial class ComponentPopupWindow : MetroWindow
    {
        private static readonly Regex UrlRegex = new(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly SolidColorBrush ActiveStepBrush = new(Color.FromRgb(30, 126, 223));
        private static readonly SolidColorBrush InactiveStepBrush = new(Color.FromRgb(207, 216, 226));
        private readonly IReadOnlyList<ComponentPopupItem> _items;
        private int _currentIndex;

        public ComponentPopupWindow(string title, IReadOnlyList<ComponentPopupItem> items)
        {
            InitializeComponent();

            Title = title;
            _items = items ?? throw new ArgumentNullException(nameof(items));
            ShowAgainCheckBox.IsChecked = false;
            ShowCurrentItem();
        }

        public bool ShowAgain => ShowAgainCheckBox.IsChecked == true;

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex >= _items.Count - 1)
            {
                DialogResult = true;
                Close();
                return;
            }

            _currentIndex++;
            ShowCurrentItem();
        }

        private void ShowCurrentItem()
        {
            var item = _items[_currentIndex];
            var isLastItem = _currentIndex >= _items.Count - 1;

            HeadlineTextBlock.Text = item.Headline;
            SetDescription(item.Description);
            StepImage.Source = item.Image;
            StepImage.Visibility = item.Image != null ? Visibility.Visible : Visibility.Collapsed;
            ImagePlaceholderTextBlock.Visibility = item.Image == null ? Visibility.Visible : Visibility.Collapsed;

            NextButtonTextBlock.Text = isLastItem
                ? ComponentPopupWindowResources.Button_Close
                : ComponentPopupWindowResources.Button_Next;
            NextButtonIcon.Kind = isLastItem
                ? PackIconMaterialKind.Check
                : PackIconMaterialKind.ArrowRight;

            StepIndicator.ItemsSource = Enumerable
                .Range(0, _items.Count)
                .Select(i => i == _currentIndex ? ActiveStepBrush : InactiveStepBrush)
                .ToList();
        }

        private void SetDescription(string description)
        {
            DescriptionTextBlock.Inlines.Clear();

            var currentIndex = 0;
            foreach (Match match in UrlRegex.Matches(description ?? string.Empty))
            {
                if (match.Index > currentIndex)
                    DescriptionTextBlock.Inlines.Add(new Run(description[currentIndex..match.Index]));

                var url = match.Value.TrimEnd('.', ',', ';', ':', ')');
                var linkButton = new Button
                {
                    Content = url,
                    Style = (Style)FindResource("DescriptionLinkStyle"),
                    ToolTip = url
                };
                linkButton.Click += (_, _) => OpenLink(url);
                DescriptionTextBlock.Inlines.Add(new InlineUIContainer(linkButton)
                {
                    BaselineAlignment = BaselineAlignment.Center
                });

                if (url.Length < match.Length)
                    DescriptionTextBlock.Inlines.Add(new Run(match.Value[url.Length..]));

                currentIndex = match.Index + match.Length;
            }

            if (currentIndex < (description?.Length ?? 0))
                DescriptionTextBlock.Inlines.Add(new Run(description[currentIndex..]));
        }

        private static void OpenLink(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch
            {
                // Keep the popup usable if no browser can handle the link.
            }
        }
    }
}
