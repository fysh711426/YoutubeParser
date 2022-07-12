namespace YoutubeParser.Shares
{
    public enum VideoStatus
    {
        /// <summary>
        /// Streamed or premiered or upload video.
        /// </summary>
        Default,

        /// <summary>
        /// Streaming or premiering.
        /// </summary>
        Live,

        /// <summary>
        /// Scheduled or premieres.
        /// </summary>
        Upcoming
    }
}
