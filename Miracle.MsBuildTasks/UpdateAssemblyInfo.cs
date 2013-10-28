namespace Miracle.MsBuildTasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;

    public class UpdateAssemblyInfo
    {
        private readonly Regex _version = new Regex(@"^\[assembly: (?<Type>(AssemblyVersion|AssemblyFileVersion))\(""[\.0-9]+""\)\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        private readonly IEnumerable<ITaskItem> files;
        private readonly string version;
        private readonly string fileVersion;

        public UpdateAssemblyInfo(IEnumerable<ITaskItem> files, string version, string fileVersion)
        {
            if (files == null) throw new ArgumentNullException("files");
            this.files = files;
            this.version = version;
            this.fileVersion = fileVersion;
        }

        public void Execute()
        {
            foreach (var taskItem in files)
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
                    return string.Format("[assembly: {0}(\"{1}\")]", type.Value, type.Value.Equals("AssemblyVersion", StringComparison.InvariantCultureIgnoreCase) ? version : fileVersion );
                });
                
                File.Delete(fileName);

                File.WriteAllText(fileName, newFileData, Encoding.UTF8);
            }
        }
    }
}