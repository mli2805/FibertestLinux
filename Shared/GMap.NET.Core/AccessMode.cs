namespace GMap.NET
{
    /// <summary>
    ///     tile access mode
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        ///     access only server
        /// </summary>
        ServerOnly = 0,

        /// <summary>
        ///     access first server and caches locally
        /// </summary>
        ServerAndCache = 1,

        /// <summary>
        ///     access only cache
        /// </summary>
        CacheOnly = 2,
    }
}
