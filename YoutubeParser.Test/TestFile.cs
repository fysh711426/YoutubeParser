using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeParser.Test
{
    public static class TestFile
    {
        private static string ExecutingPath =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
        private static string ProjectPath =>
            Regex.Match(ExecutingPath, @"(.*?)\\bin").Groups[1].Value;
        public static string Path =>
            System.IO.Path.Combine(ProjectPath, "TestFile");
    }
}
