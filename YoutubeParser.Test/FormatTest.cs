﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace YoutubeParser.Test
{
    [TestClass]
    public class FormatTest
    {
        [TestMethod]
        public void Video_GetUploadDate()
        {
            var streaming = "Started streaming on Jan 14, 2022";
            var streamed = "Streamed live 21 hours ago";
            var scheduled = "Scheduled for Jun 12, 2023";
            var premiere = "Premiere in progress. Started 2 minutes ago";
            var premiered = "Premiered Jun 25, 2022";
            var premieres = "Premieres Jul 1, 2022";
            var video = "Jun 17, 2022";

            var regex = @"([A-Z][a-z]+\s[0-9]+,\s[0-9]+)";
            Assert.AreEqual(
                Regex.Match(streaming, regex).Groups[1].Value, "Jan 14, 2022");
            Assert.AreEqual(
                Regex.Match(streamed, regex).Groups[1].Value, "");
            Assert.AreEqual(
                Regex.Match(scheduled, regex).Groups[1].Value, "Jun 12, 2023");
            Assert.AreEqual(
                Regex.Match(premiere, regex).Groups[1].Value, "");
            Assert.AreEqual(
                Regex.Match(premiered, regex).Groups[1].Value, "Jun 25, 2022");
            Assert.AreEqual(
                Regex.Match(premieres, regex).Groups[1].Value, "Jul 1, 2022");
            Assert.AreEqual(
                Regex.Match(video, regex).Groups[1].Value, "Jun 17, 2022");
        }
    }
}
