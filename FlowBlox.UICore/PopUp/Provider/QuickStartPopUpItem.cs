using System.Windows.Media;

namespace FlowBlox.UICore.PopUp.Provider
{
    public class QuickStartPopUpItem
    {
        public QuickStartPopUpItem(string headline, string description, ImageSource image)
        {
            Headline = headline;
            Description = description;
            Image = image;
        }

        public string Headline { get; }

        public string Description { get; }

        public ImageSource Image { get; }
    }
}
