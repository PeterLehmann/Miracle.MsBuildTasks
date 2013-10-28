/*
 * this code is from GitFlowVersoinTask https://github.com/Particular/GitFlowVersion to work for our needs
 * */
namespace Miracle.MsBuildTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using GitFlowVersion;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class ChangeVersionFromGitBranch : Task
    {
        [Required]
        public string ProjectFolder { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        [Output]
        public string Version { get; set; }
        [Output]
        public string FileVersion { get; set; }
        [Output]
        public string InfoVersion { get; set; }

        public override bool Execute()
        {
            var gitFolder = GitDirFinder.TreeWalkForGitDir(ProjectFolder);
            if (string.IsNullOrEmpty(gitFolder))
            {
                if (TeamCity.IsRunningInBuildAgent()) //fail the build if we're on a TC build agent
                {
                    this.LogError("Failed to find .git directory on agent. Please make sure agent checkout mode is enabled for you VCS roots - http://confluence.jetbrains.com/display/TCD8/VCS+Checkout+Mode");
                    return false;
                }

                var message = string.Format("No .git directory found in solution path '{0}'. This means the assembly may not be versioned correctly. To fix this warning either clone the repository using git or remove the `GitFlowVersion.Fody` nuget package. To temporarily work around this issue add a AssemblyInfo.cs with an appropriate `AssemblyVersionAttribute`.", ProjectFolder);
                this.LogWarning(message);

                return true;
            }

            var versionAndBranch = VersionCache.GetVersion(gitFolder);
            
            WriteTeamCityParameters(versionAndBranch);
            var semanticVersion = versionAndBranch.Version;
            Version = string.Format("{0}.{1}.{2}", semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch);
            FileVersion = string.Format("{0}.{1}.{2}", semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch);
            InfoVersion = versionAndBranch.ToLongString();
            this.LogInfo(String.Format("Version number is {0} and InfoVersion is {1}", Version, InfoVersion));
            var task = new UpdateAssemblyInfo(Files, Version, FileVersion);
            task.Execute();
            return true;
        }

        void WriteTeamCityParameters(VersionAndBranch versionAndBranch)
        {
            foreach (var buildParameters in TeamCity.GenerateBuildLogOutput(versionAndBranch))
            {
                this.LogWarning(buildParameters);
            }
        }
    }
}