namespace ClinicManager.E2E.Tests.Views;

public class ShellWindow(FrameworkAutomationElementBase element) : Window(element)
{
  public Button SendButton => this.Find("SendButton").AsButton();

    
}
