using System;
using System.IO;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.Capturing;
using FlaUI.Core.Logging;
using FlaUI.Core.Tools;
using Xunit;

namespace ClinicManager.E2E.Tests.Core
{
    /// <summary>
    /// Base class for ui tests with some helper methods (xUnit v3).
    /// This class allows recording videos, taking screen shots on failed tests and
    /// starts and stops the application under test for each test.
    /// xUnit creates a new instance of the test class per test, so all state here is per test.
    /// </summary>
    public abstract class FlaUITestBase : IAsyncLifetime
    {
        /// <summary>
        /// Member which holds the current video recorder.
        /// </summary>
        private VideoRecorder _recorder;

        /// <summary>
        /// Test info captured at start. The recorder runs on its own thread,
        /// so we can't rely on TestContext.Current there.
        /// </summary>
        private string _testMethodName;
        private string _testClassName;
        private string _testDisplayName;

        /// <summary>
        /// static member which holds the current execution date and time
        /// </summary>
        private static readonly string _testDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

        /// <summary>
        /// Instance of the current used automation object.
        /// </summary>
        protected AutomationBase Automation { get; private set; }

        /// <summary>
        /// Instance of the current running application.
        /// </summary>
        protected Application Application { get; set; }

        /// <summary>
        /// Flag to indicate if videos should be kept even if the test did not fail.
        /// Defaults to false.
        /// </summary>
        protected virtual bool KeepVideoForSuccessfulTests => false;

        /// <summary>
        /// Flag to indicate if screenshots should be taken in failing tests.
        /// Defaults to true.
        /// </summary>
        protected virtual bool TakeScreenshots => true;

        /// <summary>
        /// Flag to indicate if a video should be recorded for each test.
        /// Defaults to true.
        /// </summary>
        protected virtual bool RecordVideo => true;

        /// <summary>
        /// Path of the directory for the screenshots and videos for the tests.
        /// Defaults to c:\temp\testsmedia.
        /// </summary>
        protected virtual string TestsMediaPath =>
            Path.Combine(@"c:\temp\testsmedia", SanitizeFileName(_testMethodName ?? "unknown"), _testDateTime);

        protected  AutomationBase GetAutomation()
        {
            return new UIA3Automation();
        }

        public static string ApplicationPath = BuildInfo.ApplicationPath;
    
        protected  FlaUI.Core.Application StartApplication()
        { 
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ApplicationPath,
                WorkingDirectory = System.IO.Path.GetDirectoryName(ApplicationPath),
                UseShellExecute = false
            };

            return FlaUI.Core.Application.Launch(startInfo);
        }
        /// <summary>
        /// Setup for each test (replaces NUnit's [OneTimeSetUp] + [SetUp] in per-test mode).
        /// </summary>
        public virtual async ValueTask InitializeAsync()
        {
            var ctx = TestContext.Current;
            _testMethodName = ctx.TestMethod?.MethodName;
            _testClassName = ctx.TestClass?.TestClassName;
            _testDisplayName = ctx.Test?.TestDisplayName ?? _testMethodName ?? "unknown";

            Logger.Default = new XunitProgressLogger();
            Automation = GetAutomation();

            if (RecordVideo)
            {
                await StartVideoRecorder(SanitizeFileName(_testDisplayName));
            }

            Application = StartApplication();
        }

        /// <summary>
        /// Teardown for each test (replaces NUnit's [TearDown] + [OneTimeTearDown] in per-test mode).
        /// </summary>
        public virtual ValueTask DisposeAsync()
        {
            var failed = TestContext.Current.TestState?.Result == TestResult.Failed;

            if (failed && TakeScreenshots)
            {
                var path = TakeScreenShot(_testDisplayName);
                if (path != null)
                {
                    try
                    {
                        TestContext.Current.AddAttachment(
                            _testDisplayName,
                            File.ReadAllBytes(path),
                            ".png");
                    }
                    catch (Exception ex)
                    {
                        Logger.Default.Warn("Failed to attach screen shot {0}: {1}", path, ex);
                    }
                }
            }

            CloseApplication();

            if (RecordVideo)
            {
                StopVideoRecorder(failed);
            }

            if (Automation != null)
            {
                Automation.Dispose();
                Automation = null;
            }

            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Closes and starts the application.
        /// </summary>
        protected void RestartApplication()
        {
            CloseApplication();
            Application = StartApplication();
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        private void CloseApplication()
        {
            if (Application != null)
            {
                Application.Close();
                Retry.WhileFalse(() => Application.HasExited, TimeSpan.FromSeconds(2), ignoreException: true);
                Application.Dispose();
                Application = null;
            }
        }

        /// <summary>
        /// Method which captures the image for the video and screen shots.
        /// By default captures the main screen.
        /// </summary>
        protected virtual CaptureImage CaptureImage()
        {
            return Capture.MainScreen();
        }

        /// <summary>
        /// Method which allows customizing the settings for the video recorder.
        /// By default downloads ffmpeg and sets the path to ffmpeg.
        /// </summary>
        protected virtual async Task AdjustRecorderSettings(VideoRecorderSettings videoRecorderSettings)
        {
            // Download FFMpeg
            var ffmpegPath = await VideoRecorder.DownloadFFMpeg(@"C:\temp");
            videoRecorderSettings.ffmpegPath = ffmpegPath;
        }

        /// <summary>
        /// Starts the video recorder.
        /// </summary>
        /// <param name="videoName">The unique name of the video file.</param>
        private async Task StartVideoRecorder(string videoName)
        {
            // Refresh all the system information
            SystemInfo.RefreshAll();

            // Start the recorder
            var videoRecorderSettings = new VideoRecorderSettings
            {
                VideoFormat = VideoFormat.xvid,
                VideoQuality = 6,
                TargetVideoPath = Path.Combine(TestsMediaPath, $"{SanitizeFileName(videoName)}.avi")
            };
            await AdjustRecorderSettings(videoRecorderSettings);

            // Capture into locals: the recorder callback runs on its own thread
            var testName = _testClassName + "." + (_testMethodName ?? "[SetUp]");
            _recorder = new VideoRecorder(videoRecorderSettings, r =>
            {
                var img = CaptureImage();
                img.ApplyOverlays(new InfoOverlay(img)
                {
                    RecordTimeSpan = r.RecordTimeSpan,
                    OverlayStringFormat = @"{rt:hh\:mm\:ss\.fff} / {name} / CPU: {cpu} / RAM: {mem.p.used}/{mem.p.tot} ({mem.p.used.perc}) / " + testName
                }, new MouseOverlay(img));
                return img;
            });
            await Task.Delay(500, TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// Stops the video recorder.
        /// </summary>
        private void StopVideoRecorder(bool testFailed)
        {
            if (_recorder != null)
            {
                _recorder.Stop();
                if (!KeepVideoForSuccessfulTests && !testFailed)
                {
                    File.Delete(_recorder.TargetVideoPath);
                }
                _recorder.Dispose();
                if(File.Exists(_recorder.TargetVideoPath))
                {
                    var ctx = TestContext.Current;
                    ctx.AddAttachment(Path.GetFileName(_recorder.TargetVideoPath), File.ReadAllBytes(_recorder.TargetVideoPath), "video/x-msvideo");
                }
                _recorder = null;
            }
        }

        /// <summary>
        /// Takes a screen shot. Returns the file path, or null if saving failed.
        /// </summary>
        protected internal string TakeScreenShot(string testName)
        {
            var imagePath = CreateScreenShotPath(testName);
            try
            {
                Directory.CreateDirectory(TestsMediaPath);
                CaptureImage().ToFile(imagePath);
                return imagePath;
            }
            catch (Exception ex)
            {
                Logger.Default.Warn("Failed to save screen shot to directory: {0}, filename: {1}, Ex: {2}", TestsMediaPath, imagePath, ex);
                return null;
            }
        }

        /// <summary>
        /// Replaces all invalid characters with underlines.
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Generates full path for screenshot.
        /// </summary>
        private string CreateScreenShotPath(string testName)
        {
            var imageName = SanitizeFileName(testName) + ".png";
            imageName = imageName.Replace("\"", string.Empty);
            return Path.Combine(TestsMediaPath, imageName);
        }

        public TWindow GetWindow<TWindow>(string automationId) where TWindow : FlaUI.Core.AutomationElements.Window
        {
            var window = WindowFinder.FindWindowById(
                                        Automation,
                                        automationId,
                                        timeout: TimeSpan.FromSeconds(15),
                                        pollInterval: TimeSpan.FromMilliseconds(300));     
            return window.As<TWindow>();
        }

        public LoginWindow GetLoginWindow()
                   => GetWindow<LoginWindow>("LoginWindow"); 

        public ShellWindow GetShellWindow()
                   => GetWindow<ShellWindow>("ShellWindow"); 

   
        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetProcessDPIAware();
        }
    }

        /// <summary>
    /// FlaUI logger which writes to the current xUnit test's output
    /// (falls back to a runner diagnostic message when no test is active).
    /// </summary>
    public class XunitProgressLogger : LoggerBase
    {
        /// <inheritdoc />
        protected override void GatedTrace(string message) => Write($"Trace: {message}");

        /// <inheritdoc />
        protected override void GatedDebug(string message) => Write($"Debug: {message}");

        /// <inheritdoc />
        protected override void GatedInfo(string message) => Write($"Info: {message}");

        /// <inheritdoc />
        protected override void GatedWarn(string message) => Write($"Warn: {message}");

        /// <inheritdoc />
        protected override void GatedError(string message) => Write($"Error: {message}");

        /// <inheritdoc />
        protected override void GatedFatal(string message) => Write($"Fatal: {message}");

        private static void Write(string line)
        {
            var ctx = TestContext.Current;
            var output = ctx.TestOutputHelper;
            if (output != null)
            {
                output.WriteLine(line);
            }
            else
            {
                ctx.SendDiagnosticMessage(line);
            }
        }
    }
}
