using System.Diagnostics;
using System.Globalization;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Translate with format arguments (issue #24).
    /// </summary>
    public sealed class FormatArgumentsTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        private readonly CultureInfo previousCulture = CultureInfo.CurrentCulture;

        public FormatArgumentsTests()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = previousCulture;
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Fact]
        public void TranslateFormat_FormatsTheValue()
        {
            Assert.Equal("Valore 42", LocalizationManager.TranslateFormat("Format", 42));
        }

        [Fact]
        public void TranslateFormat_UsesTheCultureOfTheVocabulary_NotTheThreadCulture()
        {
            CultureInfo.CurrentCulture = English;

            Assert.Equal("Valore 1,5", LocalizationManager.TranslateFormat("Format", 1.5m));

            LocalizationManager.Instance.SetCulture(English);
            CultureInfo.CurrentCulture = Italian;

            Assert.Equal("Value 1.5", LocalizationManager.TranslateFormat("Format", 1.5m));
        }

        [Fact]
        public void TranslateFormat_WithManyArguments()
        {
            Assert.Equal("Hello Mario, you have 3 messages",
                LocalizationManager.TranslateFormat("Missing", "Hello {0}, you have {1} messages", new object[] { "Mario", 3 }));
        }

        [Fact]
        public void TranslateFormat_StringArguments_AreNotTakenAsTheDefaultValue()
        {
            Assert.Equal("Valore Mario", LocalizationManager.TranslateFormat("Format", "Mario"));
            Assert.Equal("Valore Mario", LocalizationManager.TranslateFormat("Format", "Mario", "Rossi"));
        }

        [Fact]
        public void TranslateFormat_MissingKey_FormatsTheDefaultValueOrTheKey()
        {
            Assert.Equal("Ciao Mario", LocalizationManager.TranslateFormat("Missing", defaultValue: "Ciao {0}", args: new object[] { "Mario" }));
            Assert.Equal("Missing", LocalizationManager.TranslateFormat("Missing", "Mario"));
        }

        [Fact]
        public void TranslateFormat_WithCulture_FormatsWithThatCulture()
        {
            CultureInfo.CurrentCulture = Italian;

            Assert.Equal("Value 1.5", LocalizationManager.TranslateFormat("Format", English, 1.5m));
            Assert.Equal("Total 1.5", LocalizationManager.TranslateFormat("Missing", English, "Total {0}", new object[] { 1.5m }));
        }

        [Fact]
        public void TranslateFormat_WithoutArguments_ReturnsTheText()
        {
            Assert.Equal("Valore {0}", LocalizationManager.TranslateFormat("Format"));
            Assert.Equal("Valore {0}", LocalizationManager.TranslateFormat("Format", (object[])null));
        }

        [Theory]
        [InlineData("Hello {0} {1}")]      // missing argument
        [InlineData("Hello {name}")]       // not a composite format
        [InlineData("Hello {0")]           // unbalanced brace
        public void TranslateFormat_WrongFormat_ReturnsTheUnformattedText_AndWritesAWarning(string format)
        {
            var listener = new RecordingTraceListener();
            Trace.Listeners.Add(listener);
            try
            {
                var value = LocalizationManager.TranslateFormat("Missing", format, new object[] { "Mario" });

                Assert.Equal(format, value);
                Assert.Contains(listener.Messages, m => m.Contains(format));
            }
            finally
            {
                Trace.Listeners.Remove(listener);
            }
        }

        private sealed class RecordingTraceListener : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string message) => Messages.Add(message);

            public override void WriteLine(string message) => Messages.Add(message);
        }
    }
}
