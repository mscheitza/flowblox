using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Authentication
{
    public class FlowBloxAccountManager
    {
        private static readonly Lazy<FlowBloxAccountManager> _instance = new Lazy<FlowBloxAccountManager>(() => new FlowBloxAccountManager());

        private FlowBloxAccountManager()
        {
        }

        public static FlowBloxAccountManager Instance => _instance.Value;

        public bool IsLoggedIn
        {
            get
            {
                return ActiveUser != null;
            }
        }

        public FbUserData ActiveUser { get; set; }

        public string UserToken { get; set; }
    }
}
