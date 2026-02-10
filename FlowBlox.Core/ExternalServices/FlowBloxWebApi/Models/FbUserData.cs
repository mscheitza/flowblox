namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbUserProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string OrderId { get; set; }
    }

    public class FbUserData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public List<FbUserProduct> Products { get; set; }
    }
}
