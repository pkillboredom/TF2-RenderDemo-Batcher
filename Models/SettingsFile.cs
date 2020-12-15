using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2_RenderDemo_Batcher.Models
{
    public class SettingsFile
    {
        /// <summary>
        /// Path to TF2's hl2.exe.
        /// </summary>
        public string hl2Path { get; set; }
        /// <summary>
        /// Path to Demo directory, either relative to 'tf' directory or full path, without file extension.
        /// </summary>
        public string demoDirectory { get; set; }
        /// <summary>
        /// The path prefix to export the video file to, either relative to 'tf' directory or full path.
        /// </summary>
        public string exportPrefix { get; set; }
        /// <summary>
        /// The recording profile to use for export. video|audio|both
        /// </summary>
        public string profile { get; set; } = "both";
        /// <summary>
        /// Should the ouput overwrite existing files? yes|no|ask
        /// </summary>
        public string overwrite { get; set; } = "yes";
        /// <summary>
        /// TF2 Launch Options.
        /// </summary>
        public string launchOptions { get; set; } = "";
        /// <summary>
        /// TF2 Window State hidden|broken|fixed
        /// </summary>
        public string windowState { get; set; } = "broken";
        /// <summary>
        /// A command to run before the demo is recorded.
        /// </summary>
        public string preRecordCommand { get; set; } = "";
    }
}
