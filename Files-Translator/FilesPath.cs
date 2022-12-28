namespace Files_Translator
{
    public class FilesPath
    {
        public string ProjectPath { get; set; }
        public string TranslationFolder { get; set; }
        public string[] Sources { get; set; }
        public string[] Translations { get; set; }
        public string OutputFolder { get; set; }
        public string LogFileName { get; set; }
    }
}
