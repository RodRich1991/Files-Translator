using Newtonsoft.Json.Linq;

namespace Files_Translator
{
    public class CofigurationEngine
    {
        public static FilesPath config = ReadConfig();
        public List<string[]> sources;
        public List<string[]> translations;
        public List<string> pathsToTranslate;

        public CofigurationEngine()
        {
            sources = ReadSources();
            translations = ReadTranslations();
            pathsToTranslate = GetFilesList();
        }

        private static FilesPath ReadConfig()
        {
            return JObject.Parse(File.ReadAllText(@"../../../files-path.json")).ToObject<FilesPath>();
        }


        private string GetPath(string[] subpath)
        {
            return config.ProjectPath
                + "\\"
                + subpath.Aggregate((acc, curr) => acc + "\\" + curr);
        }

        private List<string[]> ReadSources()
        {
            return config.Sources
                .Select(source => GetPath(new string[] { config.TranslationFolder, source }))
                .Select(path => FileUtils.ReadFileAsText(path))
                .ToList();
        }

        private List<string[]> ReadTranslations()
        {
            return config.Translations
                .Select(translation => GetPath(new string[] { config.TranslationFolder, translation }))
                .Select(path => FileUtils.ReadFileAsText(path))
                .ToList();
        }

        private List<string> GetFilesList()
        {
            string translationPath = GetPath(new string[] { config.TranslationFolder });
            return Directory.GetFiles(config.ProjectPath, "*", SearchOption.AllDirectories)
                .Where(path => !path.StartsWith(translationPath))
                .ToList();
        }
    }
}
