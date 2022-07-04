using System.Reflection;
using System.Text.RegularExpressions;

namespace YoutubeParser.Test.Utils
{
    public static class TestFile
    {
        private static string ExecutingPath =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        private static string TestProjectPath =>
            Regex.Match(ExecutingPath, @"(.*?)\\bin").Groups[1].Value;
        public static string DirPath =>
            Path.Combine(TestProjectPath, "TestFile");
    }
}
