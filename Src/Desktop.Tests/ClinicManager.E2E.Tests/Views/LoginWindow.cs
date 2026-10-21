namespace ClinicManager.E2E.Tests.Views;

public class LoginWindow(FrameworkAutomationElementBase element) : Window(element)
{
  public Button LoginButton => this.Find("LoginButton").AsButton();

  public Button CancelButton => this.Find("CancelButton").AsButton();

  public TextBox UsernameTextBox => this.Find("UsernameTextBox").AsTextBox();

  public TextBox PasswordTextBox => this.Find("PasswordTextBox").AsTextBox();

  public Label LoginStatusLabel => this.Find("LoginStatusLabel").AsLabel();
  
}
