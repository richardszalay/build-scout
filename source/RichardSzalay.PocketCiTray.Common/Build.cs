using System;

namespace RichardSzalay.PocketCiTray
{
    public class Build
    {
        private BuildResult buildResult;

        public BuildResultChange Change { get; set; }
        public BuildResult Result { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Label { get; set; }

        public static BuildResultChange GetBuildResultChange(BuildResult oldValue, BuildResult newValue)
        {
            if (oldValue == newValue)
            {
                return BuildResultChange.None;
            }

            switch (newValue)
            {
                case BuildResult.Success: return BuildResultChange.Fixed;
                case BuildResult.Failed: return BuildResultChange.Failed;
                default: return BuildResultChange.Unavailable;
            }
        }
    }
}
