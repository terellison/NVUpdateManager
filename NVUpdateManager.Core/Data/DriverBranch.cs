namespace NVUpdateManager.Core.Data
{
    /// <summary>
    /// The driver line to track. NVIDIA ships two release branches for the same GPUs.
    /// </summary>
    public enum DriverBranch
    {
        /// <summary>
        /// Game Ready Driver: released alongside major game launches.
        /// </summary>
        GameReady,

        /// <summary>
        /// NVIDIA Studio Driver: released on a slower cadence and validated for creative applications.
        /// </summary>
        Studio
    }
}
