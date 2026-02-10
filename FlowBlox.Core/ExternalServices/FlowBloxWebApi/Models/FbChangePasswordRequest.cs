namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
	public class FbChangePasswordRequest
	{
		public string EmailOrUsername { get; set; }

		public string ResetCode { get; set; }

		public string NewPassword { get; set; }
	}
}