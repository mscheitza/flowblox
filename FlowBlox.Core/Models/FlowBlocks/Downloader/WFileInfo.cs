namespace FlowBlox.Core.Models.FlowBlocks.Downloader
{
    public class WFileInfo
    {
        public string FileName { get; set; }
        public string URLToFile { get; set; }
        public string MIMEType { get; set; }
        public long FileSize { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; }= string.Empty;
    } 	
}
