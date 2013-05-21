using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Miracle.MsBuildTasks
{
    public class ChangeVersion : Task
    {
        private readonly Regex _version = new Regex(@"^\[assembly: (?<Type>(AssemblyVersion|AssemblyFileVersion))\(""[\.0-9]+""\)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public string VersionNumber { get; set; }

        public override bool Execute()
        {
            foreach (var taskItem in Files)
            {
                var fileName = taskItem.ItemSpec;
                var data = File.ReadAllText(fileName);
                var newFileData = _version.Replace(data, m =>
                    {
                        if (!m.Success)
                        {
                            return m.Value;
                        }
                        var type = m.Groups["Type"];
                        if (type == null || !type.Success)
                        {
                            return m.Value;
                        }
                        return string.Format("[assembly: {0}(\"{1}\")]", type.Value, VersionNumber);
                    });
                Log.LogMessage(MessageImportance.High, "Creating new file {0}", fileName);
                File.Delete(fileName);
                
                File.WriteAllText(fileName, newFileData, Encoding.UTF8);
            }
            return true;
        }
    }
}