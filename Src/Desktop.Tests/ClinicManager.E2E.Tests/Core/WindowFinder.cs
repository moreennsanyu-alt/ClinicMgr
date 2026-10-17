using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Exceptions;
using FlaUI.Core.Tools;

namespace ClinicManager.E2E.Tests.Core;

public static class WindowFinder
{
    /// <summary>
    /// Finds a window by its AutomationId, retrying until found or timeout.
    /// </summary>
    /// <param name="automation">The FlaUI AutomationBase (UIA2 or UIA3)</param>
    /// <param name="windowId">AutomationId of the window to find</param>
    /// <param name="timeout">Max time to wait (default 30s)</param>
    /// <param name="pollInterval">Time between retries (default 500ms)</param>
    public static Window FindWindowById(
        AutomationBase automation,
        string windowId,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        pollInterval ??= TimeSpan.FromMilliseconds(500);

        Window foundWindow = null;

        bool success = Retry.WhileFalse(() =>
        {
            try
            {
                var desktop = automation.GetDesktop();
                var condition = automation.ConditionFactory.ByAutomationId(windowId);
                var element = desktop.FindFirstChild(condition);

                if (element != null)
                {
                    foundWindow = element.AsWindow();
                    return foundWindow != null;
                }
                return false;
            }
            catch (Exception)
            {
                // swallow transient errors (e.g. element not ready) and retry
                return false;
            }
        },
        timeout: timeout.Value,
        interval: pollInterval.Value,
        throwOnTimeout: false).Success;

        if (!success || foundWindow == null)
        {
            throw new Exception(
                $"Window with AutomationId '{windowId}' was not found within {timeout.Value.TotalSeconds} seconds.");
        }

        return foundWindow;
    }
}
