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
            sources = ReadSources(config);
            translations = ReadTranslations(config);
            pathsToTranslate = GetFilesList(config);
        }

        private static FilesPath ReadConfig()
        {
            return JObject.Parse(File.ReadAllText(@"../../../files-path.json")).ToObject<FilesPath>();
        }


        private string GetPath(FilesPath config, string[] subpath)
        {
            return config.ProjectPath
                + "\\"
                + subpath.Aggregate((acc, curr) => acc + "\\" + curr);
        }

        private List<string[]> ReadSources(FilesPath config)
        {
            return config.Sources
                .Select(source => GetPath(config, new string[] { config.TranslationFolder, source }))
                .Select(path => FileUtils.ReadFileAsText(path))
                .ToList();
        }

        private List<string[]> ReadTranslations(FilesPath config)
        {
            return config.Translations
                .Select(translation => GetPath(config, new string[] { config.TranslationFolder, translation }))
                .Select(path => FileUtils.ReadFileAsText(path))
                .ToList();
        }

        private List<string> GetFilesList(FilesPath config)
        {
            string translationPath = GetPath(config, new string[] { config.TranslationFolder });
            return Directory.GetFileSystemEntries(config.ProjectPath, "*", SearchOption.AllDirectories)
                .Where(path => !path.StartsWith(translationPath))
                .ToList();
        }
    }
}
