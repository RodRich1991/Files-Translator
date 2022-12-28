
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Files_Translator
{
    public static class FileUtils
    {

        public static string SINGLE_QUOTES_GROUP_NAME = "str";
        private static string PROJECT_ROOT_FOLDER = Path.GetDirectoryName(CofigurationEngine.config.ProjectPath);
        public static Regex BETWEEN_SINGLE_QUOTES = new Regex("'(?<str>.*?)'"); // Regex to find strings inside single quotes
        public static Regex HEX_COLOR = new Regex("^(?:[0-9a-fA-F]{3,4}){1,2}$"); // Regex to find hexadecimal color
        public static Regex ONLY_NUMBERS = new Regex("^\\d+$"); // Regex to check if has only numbers

        public static string[] ReadFileAsText(string CaminhoArquivo)
        {
            return File.ReadAllLines(CaminhoArquivo);
        }

        public static string ReplaceAccumulating(string input, MatchCollection collection, string[] replacements)
        {
            string translated = input;
            for (int i = 0; i < collection.Count; i++)
            {
                var strings = BETWEEN_SINGLE_QUOTES.Matches(translated);
                translated = ReplaceMatch(translated, strings[i], SINGLE_QUOTES_GROUP_NAME, replacements[i]);
            }
            return translated;
        }

        public static string ReplaceMatch(string input, Match matched, string groupName, string replacement)
        {
            var sb = new StringBuilder();
            sb.Append(input.Substring(0, matched.Groups[groupName].Index));
            sb.Append(replacement);
            sb.Append(input.Substring(matched.Groups[groupName].Index + matched.Groups[groupName].Length));
            return sb.ToString();
        }

        public static string RelativePath(string path)
        {
            var onlyPath = Path.GetDirectoryName(path);
            return onlyPath.Split(PROJECT_ROOT_FOLDER)[1];
        }

        public static string GetOutputPath(string originalPath)
        {
            var relativePath = RelativePath(originalPath);
            var destinationPath = $"{CofigurationEngine.config.OutputFolder}{relativePath}";

            if (!new DirectoryInfo(destinationPath).Exists)
                Directory.CreateDirectory(destinationPath);

            return $"{destinationPath}\\{Path.GetFileName(originalPath)}";
        }

        public static void WriteIntoOutputCorrespondentPath(string originalPath, string text, bool eraseFile = false)
        {
            var destinationFile = GetOutputPath(originalPath);
            Console.WriteLine($"Writting into file: {destinationFile}");

            if (eraseFile)
                File.WriteAllText(destinationFile, text);
            else
                File.AppendAllText(destinationFile, text);
        }


        public static void WriteIntoOutputCorrespondentPath(string originalPath, string[] lines, bool eraseFile = false)
        {
            var destinationFile = GetOutputPath(originalPath);
            Console.WriteLine($"Writting into file: {destinationFile}");

            if (eraseFile)
                File.WriteAllLines(destinationFile, lines);
            else
                File.AppendAllLines(destinationFile, lines);
        }


        public static void Log(string path, string text)
        {
            var log = $"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}]: {text}\n";
            Console.WriteLine(log);
            WriteIntoOutputCorrespondentPath($"{Path.GetDirectoryName(path)}\\{CofigurationEngine.config.LogFileName}", log);
        }
    }
}
