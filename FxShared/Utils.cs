using DevDaddyJacob.FxShared.Logger;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DevDaddyJacob.FxShared
{
    internal static class Utils
    {
        internal const string _CONSOLE_COLOUR_PATERN = @"(?:\u001B|)\[[;\d]*m";
        internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        internal static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        internal static string StripColour(string input)
        {
            return Regex.Replace(input, _CONSOLE_COLOUR_PATERN, string.Empty);
        }

        internal static string FormatBytes(float bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

        internal static bool ValidatePath(string path, bool isDirectory, string? fileExtension = null, bool preventAppLogging = false)
        {
            if (!preventAppLogging)
            {
                //Program.Logger.Debug($"Validating path \"{path}\" with params: isDirectory = {isDirectory}, fileExtension = {fileExtension ?? "null"}");
            }

            // Make sure the path exists
            if (!Path.Exists(path))
            {
                if (!preventAppLogging)
                {
                    //Program.Logger.Debug($"Validating path \"{path}\": Path doesn't exist");
                }
                return false;
            }
            if (!preventAppLogging)
            {
                //Program.Logger.Debug($"Validating path \"{path}\": Path does exist");
            }

            // Get the extension of the path
            ReadOnlySpan<char> extension = Path.GetExtension(path);
            if (!preventAppLogging)
            {
                //Program.Logger.Debug($"Validating path \"{path}\": Has extension \"{extension}\"");
            }

            // Validating a directory, ensure there is no extension
            if (isDirectory)
            {
                if (!preventAppLogging)
                {
                    //Program.Logger.Debug($"Validating path \"{path}\": Directory {(extension.IsEmpty ? "has extension" : "does not have extension")}");
                }
                return extension.IsEmpty;
            }

            // Validating a file with a specified extension, check it
            if (fileExtension != null)
            {
                if (!preventAppLogging)
                {
                    //Program.Logger.Debug($"Validating path \"{path}\": Checking if file extension of \"{extension.ToString()}\" matches \"{fileExtension}\" ({extension.ToString() == fileExtension})");
                }
                return extension.ToString() == fileExtension;
            }

            // Validating a file with any extension, ensure we have an extension
            if (!preventAppLogging)
            {
                //Program.Logger.Debug($"Validating path \"{path}\": File {(extension.IsEmpty ? "has extension" : "does not have extension")}");
            }
            return !extension.IsEmpty;
        }
    }
}