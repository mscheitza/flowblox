using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.Models
{
    public class ConvertedCaptchaResponse
    {
        public string CaptchaId { get; set; }

        public BitmapImage CaptchaImage { get; set; }
    }
}
