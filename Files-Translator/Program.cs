
using System.Text.RegularExpressions;

namespace Files_Translator
{
    public class Program
    {

        static void Main(string[] args)
        {
            try
            {
                CofigurationEngine cofigurationEngine = new CofigurationEngine();
                cofigurationEngine
                     .pathsToTranslate
                     .ForEach(path => TranslateFile(cofigurationEngine, path));
                FileUtils.Log(CofigurationEngine.config.ProjectPath, $"Successfully finished translations.");
            } catch (Exception ex)
            {
                FileUtils.Log(CofigurationEngine.config.ProjectPath, $"Error while processing files.\nDetails: {ex.StackTrace}");
            }
        }

        public static void TranslateFile(CofigurationEngine cofigurationEngine, string path)
        {
            try
            {
                var currentFile = FileUtils.ReadFileAsText(path);
                var translatedLines = currentFile
                    .Select((line, idx) => TranslateLine(cofigurationEngine, line, path, idx + 1))
                    .ToArray();
                FileUtils.WriteIntoOutputCorrespondentPath(path, translatedLines, true);
                FileUtils.Log(path, $"Successfully processed {path}");
            }
            catch (Exception ex)
            {
                FileUtils.Log(path, $"Error while processing file {path}\nDetails: {ex.StackTrace}");
            }
        }

        private static string TranslateLine(CofigurationEngine cofigurationEngine, string line, string currentPath, int linePos)
        {
            var matches = FileUtils.BETWEEN_SINGLE_QUOTES.Matches(line);

            if (matches.Count <= 0)
                return line;

            var translations = matches
                .Select(match => TranslateText(cofigurationEngine, match.Groups[FileUtils.SINGLE_QUOTES_GROUP_NAME].Value, currentPath, linePos))
                .ToArray();

            return FileUtils.ReplaceAccumulating(line, matches, translations);
        }

        private static string TranslateText(CofigurationEngine cofigurationEngine, string text, string currentPath, int linePos)
        {

            if (FileUtils.HEX_COLOR.IsMatch(text)) // If is a hexadecimal color skipping searching on files
            {
                Console.WriteLine($"Skipping color: {text}, on file: {currentPath}:{linePos}");
                return text;
            }

            if (FileUtils.ONLY_NUMBERS.IsMatch(text)) // If is only a number skipping searching on files
            {
                Console.WriteLine($"Skipping number: {text}, on file: {currentPath}:{linePos}");
                return text;
            }

            var translationId = GetTranslationId(cofigurationEngine, text, currentPath, linePos);
            var translated =  GetTranslatedTextById(cofigurationEngine, translationId, currentPath, linePos);
            return String.IsNullOrWhiteSpace(translated)
                ? text
                : translated;
        }

        private static string GetTranslationId(CofigurationEngine cofigurationEngine, string input, string currentPath, int linePos)
        {
            if (String.IsNullOrWhiteSpace(input))
                return input;

            Console.WriteLine($"Searching Id of text: \"{input}\" (Path: {currentPath}:{linePos})");
            var escapeInput = Regex.Replace(Regex.Escape(input), "’|'|`", "('|’|`)"); // Normalize str single quotes to consider (’ ' `) as same shit, and escape special characters
            Regex targetRx = new Regex("\"(?<id>.*)\":\\s\"" + escapeInput + '"'); // Regex to find ID of correspondent line into search files
            try
            {
                return cofigurationEngine.sources
                   .SelectMany(i => i)
                   .Where(source => targetRx.IsMatch(source))
                   .Select(source => targetRx.Match(source).Groups["id"].Value)
                   .First();
            }
            catch (InvalidOperationException ex)
            {
                var text = $"Id not found for text \"{input}\" (Path: {currentPath}:{linePos})";
                Console.WriteLine(text);
                FileUtils.Log(currentPath, text);
                return "";
            }
        }

        private static string GetTranslatedTextById(CofigurationEngine cofigurationEngine, string labelId, string currentPath, int linePos)
        {
            if (String.IsNullOrWhiteSpace(labelId))
                return labelId;

            Console.WriteLine($"Searching Translation for Id: {labelId} (Path: {currentPath}:{linePos})");
            Regex translatedRx = new Regex('"' + Regex.Escape(labelId) + "\":\\s\"(?<translation>.*)\""); // Regex to find correspondent translated text by id
            try
            {
                return cofigurationEngine.translations
                    .SelectMany(i => i)
                    .Where(translated => translatedRx.IsMatch(translated))
                    .Select(translated => translatedRx.Match(translated).Groups["translation"].Value)
                    .First();
            }
            catch (InvalidOperationException ex)
            {
                var text = $"Translation not found with Id \"{labelId}\" (Path: {currentPath}:{linePos})";
                Console.WriteLine(text);
                FileUtils.Log(currentPath, text);
                return "";
            }
        }
    }
}