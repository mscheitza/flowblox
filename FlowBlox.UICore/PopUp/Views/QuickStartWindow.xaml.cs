using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Resources;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Media;

namespace FlowBlox.UICore.PopUp.Views
{
    public partial class QuickStartWindow : MetroWindow
    {
        private static readonly SolidColorBrush ActiveStepBrush = new(Color.FromRgb(30, 126, 223));
        private static readonly SolidColorBrush InactiveStepBrush = new(Color.FromRgb(207, 216, 226));
        private readonly IReadOnlyList<QuickStartPopUpItem> _items;
        private int _currentIndex;

        public QuickStartWindow(string title, IReadOnlyList<QuickStartPopUpItem> items)
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
            DescriptionTextBlock.Text = item.Description;
            StepImage.Source = item.Image;
            StepImage.Visibility = item.Image != null ? Visibility.Visible : Visibility.Collapsed;
            ImagePlaceholderTextBlock.Visibility = item.Image == null ? Visibility.Visible : Visibility.Collapsed;

            NextButtonTextBlock.Text = isLastItem
                ? SequenceDetectionPopUpTexts.Button_Close
                : SequenceDetectionPopUpTexts.Button_Next;
            NextButtonIcon.Kind = isLastItem
                ? PackIconMaterialKind.Check
                : PackIconMaterialKind.ArrowRight;

            StepIndicator.ItemsSource = Enumerable
                .Range(0, _items.Count)
                .Select(i => i == _currentIndex ? ActiveStepBrush : InactiveStepBrush)
                .ToList();
        }
    }
}
