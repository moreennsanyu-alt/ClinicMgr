using System;
using System.Linq;
using Fallout.Solutions;
using Fallout.Common;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Execution;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.GitVersion;
using Fallout.Common.Tools.ReportGenerator;
using Fallout.Common.Tools.Xunit;
using Fallout.Common.Utilities;
using Fallout.Common.Tools.Git;
using Fallout.Common.Tools.GitHub;
using Fallout.Common.Utilities.Collections;
using Fallout.Components;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Fallout.Common.Tools.Xunit.XunitTasks;
using Fallout.Common.Tools.NerdbankGitVersioning;
using Octokit;
using static Fallout.Common.IO.PathConstruction;

using static Serilog.Log;

[UnsetVisualStudioEnvironmentVariables]
[DotNetVerbosityMapping]
class Build : FalloutBuild
{
    /* Support plugins are available for:
       - JetBrains ReSharper        https://fallout.build/resharper
       - JetBrains Rider            https://fallout.build/rider
       - Microsoft VisualStudio     https://fallout.build/visualstudio
       - Microsoft VSCode           https://fallout.build/vscode
    */

    public static int Main() => Execute<Build>( x => x.E2ETests);

    [Parameter("The solution configuration to build. Default is 'Debug' (local) or 'CI' (server).")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.CI;

    [Parameter("Use this parameter if you encounter build problems in any way, " +
        "to generate a .binlog file which holds some useful information.")]
    readonly bool? GenerateBinLog;

    [Parameter("GitHub token used to create the release. Falls back to GITHUB_TOKEN env var or the GitHubActions context.")]
    [Secret]
    readonly string GitHubToken;

	[Secret]
    readonly string AppKey;
    
    
    [GitRepository] 
    readonly GitRepository GitRepository;


    [Solution(GenerateProjects = true)]
    readonly Fallout.Common.ProjectModel.Solution Solution;

    [Required]
    [NerdbankGitVersioning()]
    readonly NerdbankGitVersioning NerdbankVersioning;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath InstallersDirectory => ArtifactsDirectory / "installers";
	
    AbsolutePath AttachmentsDirectory => ArtifactsDirectory / "Attachments";

    AbsolutePath BuildLogsDirectory => AttachmentsDirectory / "build_logs";

    AbsolutePath CoverageDirectory => AttachmentsDirectory / "Coverage";

    AbsolutePath TestResultsDirectory => AttachmentsDirectory / "TestResults";

  
    string SemVer;
    
    AbsolutePath ChangesFile => InstallersDirectory / "changes.txt";
    

    Target Clean => _ => _
        .Executes(() =>
        {
		    
            ArtifactsDirectory.CreateOrCleanDirectory();
            TestResultsDirectory.CreateOrCleanDirectory();
            InstallersDirectory.CreateOrCleanDirectory();
            BuildLogsDirectory.CreateOrCleanDirectory();
       });
        
    Target RestoreTools => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNet("tool restore");
            
            DotNet("wix extension add -g WixToolset.UI.wixext/6.0.2");
            
        });
        
    Target Restore => _ => _
        .DependsOn(Clean)
        .DependsOn(RestoreTools)
        .Executes(() =>
        {
			
            DotNetRestore(s => s
                    .SetProjectFile(Solution)
                    .SetConfigFile(RootDirectory / "nuget.config")
                    .EnableNoCache());
	
        });
    Target InjectLicenseKey => _ => _
    .Executes(() =>
    {
        var appSetupFile = RootDirectory / "src" / "desktop" / "ClinicManager.Win" / "App.Setup.cs";
        
        var content = File.ReadAllText(appSetupFile);
        content = content.Replace("YOUR_LICENSE_KEY_HERE", EnvironmentInfo.GetVariable("AppKey"));
        
        File.WriteAllText(appSetupFile, content);
        
    });
	
    Target Compile => _ => _
        .DependsOn(Restore)
		//.DependsOn(InjectLicenseKey)
        .Executes(() =>
        {
            var configurations = new List<string>();

            if (ExecutionPlan.Any(x => x.Name == nameof(E2ETests)))
                configurations.Add("Debug");

			if (ExecutionPlan.Any(x => x.Name == nameof(UnitTests)))
                configurations.Add("Debug");

            if (ExecutionPlan.Any(x => x.Name == nameof(Release)))
                configurations.Add("CI");

            if (!string.IsNullOrWhiteSpace(Configuration))
                configurations.Add(Configuration);

            configurations = configurations.Distinct().ToList();

            if (!configurations.Any())
                configurations.Add("Release");

            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .EnableNoRestore()
                .EnableNoCache()
                .CombineWith(configurations, (settings, config) => settings
                    .SetConfiguration(config)),
            degreeOfParallelism: 1,//configurations.Length,
            completeOnFailure: true);
        });
        
    Target E2ETests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjectNames = new[] { "ClinicManager.E2E.Tests" };

            var unitTestProjects = testProjectNames.Select(x => Solution.GetAllProjects(x).First());
         
            var testCombinations =
                from project in unitTestProjects
                let frameworks = project.GetTargetFrameworks()
                from framework in frameworks
                select new { project, framework };

               // E2ETestProjects.ForEach(x=>Information(x.Name)
              DotNetRun(s => s
                .SetConfiguration(Configuration.Debug)
                .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
                .SetProcessEnvironmentVariable("NUNIT_RESULTS_DIRECTORY", TestResultsDirectory)
                .EnableNoBuild()
                .CombineWith(
                    testCombinations,
                    (settings, v) => settings
                        .SetProjectFile(v.project)
                        .SetFramework(v.framework)
                        .SetProperty("RunWorkingDirectory", ArtifactsDirectory / "bin" / "ClinicManager.Win" / "debug" )
						.SetProcessAdditionalArguments(
                            "--",
							"--coverage",
							"--coverage-output-format cobertura",
							$"--coverage-output {CoverageDirectory / $"{v.project.Name}_{v.framework}.cobertura.xml"}",
                            $"--results-directory {TestResultsDirectory}"
                         )
                    )
                );
				
		});

		
    Target UnitTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjectNames = new[] { "ClinicManager.Win.Tests",
                                           //"Application.FunctionalTests",
                                           //"Domain.UnitTests",
                                           //"Application.UnitTests",
                                           //"Infrastructure.IntergrationTests",        
            };

            var unitTestProjects = testProjectNames.Select(x => Solution.GetAllProjects(x).First());
         
            var testCombinations =
                from project in unitTestProjects
                let frameworks = project.GetTargetFrameworks()
                from framework in frameworks
                select new { project, framework };

               // E2ETestProjects.ForEach(x=>Information(x.Name));

            DotNetRun(s => s
                .SetConfiguration(Configuration.Debug)
                .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
                .EnableNoBuild()
                .CombineWith(
                    testCombinations,
                    (settings, v) => settings
                        .SetProjectFile(v.project)
                        .SetFramework(v.framework)
                        //.SetProperty("RunWorkingDirectory", ArtifactsDirectory / "bin" / "ClinicManager.Win" / "debug_win-x64" )
						.SetProcessAdditionalArguments(
                            "--",
							"--coverage",
							"--coverage-output-format cobertura",
							$"--coverage-output {CoverageDirectory / $"{v.project.Name}_{v.framework}.cobertura.xml"}",
                            $"--results-directory {TestResultsDirectory}"
                         )
                    )
                );
        });
		
    Target Tests => _ => _
        .DependsOn(E2ETests)
        .DependsOn(UnitTests)
        .Executes(() =>
        {
            
        });
        
    
    Target Installers => _ => _
        .DependsOn(Restore)
        .DependsOn(Compile)
        .Executes(() =>
        {
            var setupProjectName = "ClinicManager.Setup";
            
            var setupProject = Solution.GetAllProjects(setupProjectName).First();
         
            DotNetBuild(s => s
                .SetProjectFile(setupProject)
                .SetConfiguration(Configuration)
                .When(_ => GenerateBinLog == true, c => c
                    .SetBinaryLog(BuildLogsDirectory / $"ClinicManagerSetup.build.binlog")
                )
                .EnableNoLogo());
        });

        Target Release => _ => _
        .DependsOn(Installers)
        .DependsOn(Tests)
        .Executes(async () =>
        {
            var token = ResolveGitHubToken();
            var (owner, repo) = ResolveRepository();

            var msiFile = InstallersDirectory
                .GlobFiles("*.msi")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .FirstOrDefault();

            Assert.NotNull(msiFile, $"No .msi file found in {InstallersDirectory}");

            var version = ResolveVersion();
            var tagName = $"v{version}";

            var github = new GitHubClient(new ProductHeaderValue("nuke-build"))
            {
                Credentials = new Credentials(token)
            };

            var lastReleaseTag = await GetLastReleaseTagAsync(github, owner, repo);
            var changesText = BuildChangeLog(lastReleaseTag);

           // EnsureExistingDirectory(InstallersDirectory);
            File.WriteAllText(ChangesFile, changesText);
        
            CreateAndPushTag(tagName); 
            Information("Creating GitHub release {Tag} for {Owner}/{Repo}", tagName, owner, repo);

            var newRelease = new NewRelease(tagName)
            {
                Name = tagName,
                Body = changesText,
                Draft = false,
                Prerelease = version.Contains('-'),
                TargetCommitish = GitRepository?.Commit ?? "main"
            };

            var release = await github.Repository.Release.Create(owner, repo, newRelease);

            await UploadAssetAsync(github, release, msiFile, "application/octet-stream");
            await UploadAssetAsync(github, release, ChangesFile, "text/plain");

            Information("Release published: {Url}", release.HtmlUrl);
        });

        void CreateAndPushTag(string tagName)
        {
            GitTasks.Git("config user.name github-actions[bot]");
            GitTasks.Git("config user.email github-actions[bot]@users.noreply.github.com");

    var existingTags = GitTasks.Git("tag --list", logOutput: false)
        .Select(o => o.Text)
        .ToList();

    if (existingTags.Contains(tagName))
    {
        Warning("Tag {Tag} already exists locally, skipping tag creation", tagName);
        return;
    }

    Information("Creating tag {Tag}", tagName);
    GitTasks.Git($"tag -a {tagName} -m \"Release {tagName}\"");
    GitTasks.Git($"push origin {tagName}");
}
    
    // ---------------------------------------------------------------------
    // Token resolution: explicit parameter -> GITHUB_TOKEN env var -> GitHubActions context
    // ---------------------------------------------------------------------
    string ResolveGitHubToken()
    {
        if (!string.IsNullOrWhiteSpace(GitHubToken))
            return GitHubToken;

        var envToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(envToken))
            return envToken;

        var ctxToken = GitHubActions.Instance?.Token;
        if (!string.IsNullOrWhiteSpace(ctxToken))
            return ctxToken;

        return null;
    }

    // ---------------------------------------------------------------------
    // Repository resolution: GitHubActions context -> local git remote
    // ---------------------------------------------------------------------
    (string Owner, string Repo) ResolveRepository()
    {
        var gha = GitHubActions.Instance;
        if (gha != null && !string.IsNullOrWhiteSpace(gha.Repository))
        {
            // gha.Repository is formatted as "owner/repo"
            var parts = gha.Repository.Split('/');
            if (parts.Length == 2)
                return (parts[0], parts[1]);
        }

        Assert.NotNull(GitRepository, "Could not resolve repository: no GitHubActions context and no git remote found.");
        return (GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName());
    }

    // ---------------------------------------------------------------------
    // Version resolution via Nerdbank.GitVersioning, using only the first
    // three components (major.minor.patch) of the computed version, e.g.
    // "1.4.2+abc1234" or "1.4.2.15" -> "1.4.2"
    // ---------------------------------------------------------------------
    string ResolveVersion()
    {
        Assert.NotNull(NerdbankVersioning, "NerdbankGitVersioning could not be resolved.");

        // Strip any build-metadata suffix (e.g. "+commitHash") first
        var version = NerdbankVersioning.Version.Split('+')[0];

        var components = version.Split('.');
        Assert.True(components.Length >= 3, $"Unexpected version format from Nerdbank.GitVersioning: {NerdbankVersioning.Version}");

        return string.Join(".", components.Take(3));
    }

    async Task<string> GetLastReleaseTagAsync(GitHubClient github, string owner, string repo)
    {
        try
        {
            var latest = await github.Repository.Release.GetLatest(owner, repo);
            return latest.TagName;
        }
        catch (NotFoundException)
        {
            // No prior release exists yet
            return null;
        }
    }

    // ---------------------------------------------------------------------
    // Builds changes.txt from `git log` between the last release tag and HEAD.
    // If there is no prior tag, the full history is used.
    // ---------------------------------------------------------------------
    string BuildChangeLog(string sinceTag)
    {
        var range = string.IsNullOrWhiteSpace(sinceTag) ? "" : $"{sinceTag}..HEAD";
        var format = "--pretty=format:- %s (%h)";

        IReadOnlyCollection<Output> output = string.IsNullOrWhiteSpace(range)
            ? GitTasks.Git($"log {format}")
            : GitTasks.Git($"log {range} {format}");

        var lines = output
            .Select(o => o.Text)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var header = string.IsNullOrWhiteSpace(sinceTag)
            ? $"Changes (initial release) — {DateTime.UtcNow:yyyy-MM-dd}"
            : $"Changes since {sinceTag} — {DateTime.UtcNow:yyyy-MM-dd}";

        var body = lines.Count > 0
            ? string.Join(Environment.NewLine, lines)
            : "No changes recorded.";

        return $"{header}{Environment.NewLine}{Environment.NewLine}{body}{Environment.NewLine}";
    }

    static async Task UploadAssetAsync(GitHubClient github, Release release, AbsolutePath file, string contentType)
    {
        await using var stream = File.OpenRead(file);
        var assetUpload = new ReleaseAssetUpload
        {
            FileName = Path.GetFileName(file),
            ContentType = contentType,
            RawData = stream
        };

        await github.Repository.Release.UploadAsset(release, assetUpload);
    }
    }




        
