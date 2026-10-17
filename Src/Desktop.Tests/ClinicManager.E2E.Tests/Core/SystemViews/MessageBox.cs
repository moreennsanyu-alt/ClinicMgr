using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace ClinicManager.E2E.Tests.Core.SystemViews;

public class MessageBox(FrameworkAutomationElementBase element) : Window(element)
{
    public string Message => this.Find(x => x.ByControlType(ControlType.Text)).Name;

    public Button[] Buttons => this.FindAll(x => x.ByControlType(ControlType.Button)).Select(x => x.AsButton()).ToArray();
}
