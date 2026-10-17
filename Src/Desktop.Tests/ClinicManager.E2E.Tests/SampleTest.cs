namespace ClinicManager.E2E.Tests
{
    
    public class EmptyTests : FlaUITestBase
    {
    
        [Fact]
        public void TestMethodName()
        {
            var shellWindow = GetShellWindow();
            //shellWindow.SendButton.Click();
           // Thread.Sleep(TimeSpan.FromSeconds(20));
            var path = TakeScreenShot("hello1");
            TestContext.Current.AddAttachment(
                            "hello1.png",
                            File.ReadAllBytes(path),
                            "image/png");
            throw new NotImplementedException();
        }

        
        
    }
}
