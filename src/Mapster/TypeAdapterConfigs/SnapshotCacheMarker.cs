namespace Mapster.TypeAdapterConfigs
{
    /// <summary>
    /// Determines whether a snapshot is capable of creating or using a global static cache.
    /// </summary>
    public enum SnapshotCacheMarker
    {
        /// <summary>
        /// Creation/use of a global cache is permitted
        /// </summary>
        Attached = 0,

        /// <summary>
        /// Creating/using a global cache is prohibited.<br/>
        /// The settings in the snapshot do not match the global settings.
        /// </summary>
        Detached = 1
    }

    public static class SnapshotCacheMarkerHelpers
    {
        public static bool IsAttached (this SnapshotCacheMarker marker) => marker == SnapshotCacheMarker.Attached;
    }
}
