using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace FlowBlox.Core.Util.Fonts
{
    public static class FontInstaller
    {
        [DllImport("gdi32.dll")]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private const string FontsRegistryPath = @"Software\Microsoft\Windows NT\CurrentVersion\Fonts";

        private static bool IsFontInstalled(string fontName)
        {
            using (var font = new Font(fontName, 8))
            {
                return 0 == string.Compare(
                  fontName,
                  font.Name,
                  StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static bool InstallFontForCurrentUser(string fontPath)
        { 
            string fontFileName = Path.GetFileName(fontPath);
            string userFontDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "Microsoft", "Windows", "Fonts");

            Directory.CreateDirectory(userFontDir);
            string targetPath = Path.Combine(userFontDir, fontFileName);

            string fontName = GetFontName(fontPath);

            if (IsFontInstalled(fontName))
                return false;

            File.Copy(fontPath, targetPath, overwrite: true);

            AddFontResourceEx(targetPath, 0, IntPtr.Zero);

            using (var regKey = Registry.CurrentUser.CreateSubKey(FontsRegistryPath))
            {
                regKey?.SetValue(fontName, fontFileName, RegistryValueKind.String);
            }

            return true;
        }

        private static string GetFontName(string fontPath)
        {
            using var collection = new System.Drawing.Text.PrivateFontCollection();
            collection.AddFontFile(fontPath);
            if (collection.Families.Length > 0)
                return collection.Families[0].Name;

            throw new InvalidOperationException($"Unable to retrieve display name from font file: '{fontPath}'. The file may be corrupted or not a valid font.");
        }
    }
}