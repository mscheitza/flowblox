using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbPasswordResetRequest
    {
        public string EmailOrUsername { get; set; }

        public string CaptchaId { get; set; }

        public string CaptchaCode { get; set; }
    }
}
