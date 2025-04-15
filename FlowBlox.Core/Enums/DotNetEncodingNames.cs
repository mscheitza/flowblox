using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum DotNetEncodingNames
    {
        [Display(Name = "Standard (UTF-8)")]
        Default,

        [Display(Name = "UTF-8")]
        UTF8,

        [Display(Name = "UTF-16 (Unicode)")]
        UTF16,

        [Display(Name = "UTF-16 LE")]
        UTF16_LE,

        [Display(Name = "UTF-16 BE")]
        UTF16_BE,

        [Display(Name = "UTF-32")]
        UTF32,

        [Display(Name = "ASCII")]
        ASCII,

        [Display(Name = "ISO-8859-1 (Latin-1)")]
        ISO_8859_1,

        [Display(Name = "Windows-1252")]
        WINDOWS_1252
    }
}