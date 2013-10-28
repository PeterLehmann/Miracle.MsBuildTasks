using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Miracle.MsBuildTasks
{
    public class ChangeVersion : Task
    {
        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public string VersionNumber { get; set; }

        public override bool Execute()
        {
            var task = new UpdateAssemblyInfo(Files, VersionNumber, VersionNumber);
            task.Execute();
            
            return true;
        }
    }
}