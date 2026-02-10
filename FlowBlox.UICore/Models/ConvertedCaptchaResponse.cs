using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.Models
{
    public class ConvertedCaptchaResponse
    {
        public string CaptchaId { get; set; }

        public BitmapImage CaptchaImage { get; set; }
    }
}
