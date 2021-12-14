using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Nuke.Common;
<!--#if (!NoPipeline)
using Nuke.Common.Git;
#endif-->
using Serilog;

partial class Build
{
    readonly Regex StreamRegex = new(" (.+?) ", RegexOptions.Compiled);

    Target CreateInstaller => _ => _
        .TriggeredBy(Compile)
        .Produces(ArtifactsDirectory / "*.msi")
<!--#if (!NoPipeline)
        .OnlyWhenStatic(() => IsLocalBuild || GitRepository.IsOnMainOrMasterBranch())
#endif-->
        .Executes(() =>
        {
            var installerProject = BuilderExtensions.GetProject(Solution, InstallerProject);
            var buildDirectories = GetBuildDirectories();
            var configurations = GetConfigurations(InstallerConfiguration);

            foreach (var directoryGroup in buildDirectories)
            {
                var directories = directoryGroup.ToList();
                var exeArguments = BuildExeArguments(directories.Select(info => info.FullName).ToList());
                var exeFile = installerProject.GetExecutableFile(configurations, directories);
                if (string.IsNullOrEmpty(exeFile))
                {
                    Log.Warning("No installer executable was found for these packages:\n {Directories}", string.Join("\n", directories));
                    continue;
                }

                var proc = new Process();
                proc.StartInfo.FileName = exeFile;
                proc.StartInfo.Arguments = exeArguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                while (!proc.StandardOutput.EndOfStream) ParseProcessOutput(proc.StandardOutput.ReadLine());
                proc.WaitForExit();
                if (proc.ExitCode != 0) throw new Exception("The installer creation failed.");
            }
        });

    void ParseProcessOutput([CanBeNull] string value)
    {
        if (value is null) return;
        var match = StreamRegex.Match(value);
        if (match.Success)
        {
            var parameter = match.Value.Substring(1, match.Value.Length - 2);
            var line = StreamRegex.Replace(value, "{Parameter}");
            Log.Information(line, parameter);
        }
        else
        {
            Log.Debug(value);
        }
    }

    static string BuildExeArguments(IReadOnlyList<string> args)
    {
        var argumentBuilder = new StringBuilder();
        for (var i = 0; i < args.Count; i++)
        {
            if (i > 0) argumentBuilder.Append(' ');
            var value = args[i];
            if (value.Contains(' ')) value = $"\"{value}\"";
            argumentBuilder.Append(value);
        }

        return argumentBuilder.ToString();
    }
}