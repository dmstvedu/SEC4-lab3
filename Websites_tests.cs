using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace TestProject3
{
    public class Websites_tests
    {
        ChromeDriver driver;

        private const string AddRemoveElementsUrl = "https://the-internet.herokuapp.com/add_remove_elements/";
        private const string CheckboxesUrl = "https://the-internet.herokuapp.com/checkboxes";
        private const string StatusCodesUrl = "https://the-internet.herokuapp.com/status_codes";
        private const string InputsUrl = "https://the-internet.herokuapp.com/inputs";
        private const string DropdownUrl = "https://the-internet.herokuapp.com/dropdown";
        private const string ShiftingContentUrl = "https://the-internet.herokuapp.com/shifting_content";
        private const string DragAndDropUrl = "https://the-internet.herokuapp.com/drag_and_drop";
        private const string GeolocationUrl = "https://the-internet.herokuapp.com/geolocation";
        private const string JavaScriptAlertsUrl = "https://the-internet.herokuapp.com/javascript_alerts";
        private const string ExitIntentUrl = "https://the-internet.herokuapp.com/exit_intent";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);

            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                File.Exists("/.dockerenv"))
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--window-size=1920,1080");

                if (File.Exists("/usr/bin/chromium"))
                {
                    options.BinaryLocation = "/usr/bin/chromium";
                }

                if (File.Exists("/usr/bin/chromedriver"))
                {
                    driver = new ChromeDriver("/usr/bin", options);
                    return;
                }
            }

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        private IAlert WaitForAlert(int timeoutSeconds = 5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d =>
            {
                try
                {
                    return d.SwitchTo().Alert();
                }
                catch (NoAlertPresentException)
                {
                    return null;
                }
            });
        }

        [Test]
        public void AddRemoveElements()
        {
            driver.Navigate().GoToUrl(AddRemoveElementsUrl);

            var addButton = driver.FindElement(By.XPath("//button[text()='Add Element']"));
            addButton.Click();

            var deleteButton = driver.FindElement(By.ClassName("added-manually"));
            Assert.That(deleteButton.Displayed, Is.True);
            Assert.That(deleteButton.Text, Is.EqualTo("Delete"));

            deleteButton.Click();

            var remainingButtons = driver.FindElements(By.ClassName("added-manually"));
            Assert.That(remainingButtons, Is.Empty);
        }

        [Test]
        public void Checkboxes()
        {
            driver.Navigate().GoToUrl(CheckboxesUrl);

            var checkboxes = driver.FindElements(By.CssSelector("#checkboxes input[type='checkbox']"));
            Assert.That(checkboxes.Count, Is.EqualTo(2));

            var firstCheckbox = checkboxes[0];
            var secondCheckbox = checkboxes[1];

            Assert.That(firstCheckbox.Selected, Is.False);
            Assert.That(secondCheckbox.Selected, Is.True);

            if (!firstCheckbox.Selected)
            {
                firstCheckbox.Click();
            }
            Assert.That(firstCheckbox.Selected, Is.True);

            if (secondCheckbox.Selected)
            {
                secondCheckbox.Click();
            }
            Assert.That(secondCheckbox.Selected, Is.False);
        }

        [Test]
        public void StatusCodes()
        {
            driver.Navigate().GoToUrl(StatusCodesUrl);

            var status200Link = driver.FindElement(By.LinkText("200"));
            status200Link.Click();

            Assert.That(driver.Url, Is.EqualTo("https://the-internet.herokuapp.com/status_codes/200"));

            var messageElement = driver.FindElement(By.CssSelector(".example p"));
            Assert.That(messageElement.Text, Does.Contain("This page returned a 200 status code."));
        }

        [Test]
        public void Inputs()
        {
            driver.Navigate().GoToUrl(InputsUrl);

            var numberInput = driver.FindElement(By.CssSelector("input[type='number']"));
            numberInput.Clear();
            numberInput.SendKeys("12345");
            Assert.That(numberInput.GetAttribute("value"), Is.EqualTo("12345"));

            numberInput.SendKeys(Keys.ArrowUp);
            Assert.That(numberInput.GetAttribute("value"), Is.EqualTo("12346"));
        }

        [Test]
        public void Dropdown()
        {
            driver.Navigate().GoToUrl(DropdownUrl);

            var dropdownElement = driver.FindElement(By.Id("dropdown"));
            var select = new SelectElement(dropdownElement);

            select.SelectByText("Option 1");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 1"));

            select.SelectByValue("2");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 2"));
        }

        [Test]
        public void ShiftingContent()
        {
            driver.Navigate().GoToUrl(ShiftingContentUrl);

            driver.FindElement(By.PartialLinkText("Example 1: Menu Element")).Click();

            var menuItems = driver.FindElements(By.CssSelector("#content ul li a"));
            Assert.That(menuItems.Count, Is.EqualTo(5));
            Assert.That(menuItems.Last().Text, Is.EqualTo("Gallery"));

            var shiftLink = driver.FindElement(By.CssSelector(".example p a[href*='mode=random']"));
            shiftLink.Click();

            var galleryItemAfterShift = driver.FindElement(By.LinkText("Gallery"));
            Assert.That(galleryItemAfterShift.Displayed, Is.True);
        }

        [Test]
        public void DragAndDrop()
        {
            driver.Navigate().GoToUrl(DragAndDropUrl);

            var columnA = driver.FindElement(By.Id("column-a"));
            var columnB = driver.FindElement(By.Id("column-b"));

            Assert.That(columnA.FindElement(By.TagName("header")).Text, Is.EqualTo("A"));
            Assert.That(columnB.FindElement(By.TagName("header")).Text, Is.EqualTo("B"));

            var actions = new Actions(driver);
            actions.DragAndDrop(columnA, columnB).Perform();

            if (columnA.FindElement(By.TagName("header")).Text == "A")
            {
                string jsDragAndDrop = @"
                    var src = arguments[0], tgt = arguments[1];
                    var dataTransfer = new DataTransfer();
                    src.dispatchEvent(new DragEvent('dragstart', { dataTransfer: dataTransfer }));
                    tgt.dispatchEvent(new DragEvent('drop', { dataTransfer: dataTransfer }));
                    src.dispatchEvent(new DragEvent('dragend', { dataTransfer: dataTransfer }));
                ";
                ((IJavaScriptExecutor)driver).ExecuteScript(jsDragAndDrop, columnA, columnB);
            }

            Assert.That(columnA.FindElement(By.TagName("header")).Text, Is.EqualTo("B"));
            Assert.That(columnB.FindElement(By.TagName("header")).Text, Is.EqualTo("A"));
        }

        [Test]
        public void Geolocation()
        {
            driver.Navigate().GoToUrl(GeolocationUrl);

            var coordinates = new Dictionary<string, object>
            {
                { "latitude", 50.4332288 },
                { "longitude", 30.3890432 },
                { "accuracy", 1 }
            };
            driver.ExecuteCdpCommand("Emulation.setGeolocationOverride", coordinates);

            var whereAmIButton = driver.FindElement(By.XPath("//button[text()='Where am I?']"));
            whereAmIButton.Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var latElement = wait.Until(d => d.FindElement(By.Id("lat-value")));
            var longElement = driver.FindElement(By.Id("long-value"));

            Assert.That(latElement.Text, Does.Contain("50.4332288"));
            Assert.That(longElement.Text, Does.Contain("30.3890432"));
        }

        [Test]
        public void JavaScriptAlerts()
        {
            driver.Navigate().GoToUrl(JavaScriptAlertsUrl);

            driver.FindElement(By.XPath("//button[text()='Click for JS Alert']")).Click();
            IAlert alert = WaitForAlert();
            Assert.That(alert.Text, Is.EqualTo("I am a JS Alert"));
            alert.Accept();

            var resultText = driver.FindElement(By.Id("result"));
            Assert.That(resultText.Text, Is.EqualTo("You successfully clicked an alert"));

            driver.FindElement(By.XPath("//button[text()='Click for JS Confirm']")).Click();
            IAlert confirmAlert = WaitForAlert();
            Assert.That(confirmAlert.Text, Is.EqualTo("I am a JS Confirm"));
            confirmAlert.Dismiss();
            Assert.That(resultText.Text, Is.EqualTo("You clicked: Cancel"));

            driver.FindElement(By.XPath("//button[text()='Click for JS Prompt']")).Click();
            IAlert promptAlert = WaitForAlert();
            promptAlert.SendKeys("Selenium Test");
            promptAlert.Accept();
            Assert.That(resultText.Text, Is.EqualTo("You entered: Selenium Test"));
        }

        [Test]
        public void ExitIntent()
        {
            driver.Navigate().GoToUrl(ExitIntentUrl);

            var modal = driver.FindElement(By.Id("ouibounce-modal"));
            Assert.That(modal.Displayed, Is.False);

            string triggerExitIntentJs = @"
                var event = new MouseEvent('mouseleave', {
                    view: window,
                    bubbles: true,
                    cancelable: true,
                    clientX: 100,
                    clientY: -10
                });
                document.documentElement.dispatchEvent(event);
            ";
            ((IJavaScriptExecutor)driver).ExecuteScript(triggerExitIntentJs);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(d => d.FindElement(By.Id("ouibounce-modal")).Displayed);

            var modalTitle = wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("#ouibounce-modal .modal-title h3"));
                return !string.IsNullOrWhiteSpace(el.Text) ? el : null;
            });
            Assert.That(modalTitle.Text.ToUpper(), Is.EqualTo("THIS IS A MODAL WINDOW"));

            var closeButton = driver.FindElement(By.CssSelector("#ouibounce-modal .modal-footer p"));
            closeButton.Click();

            wait.Until(d => !d.FindElement(By.Id("ouibounce-modal")).Displayed);
            Assert.That(modal.Displayed, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
