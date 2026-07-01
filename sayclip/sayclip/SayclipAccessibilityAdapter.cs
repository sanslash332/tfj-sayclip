namespace sayclip
{
    internal class SayclipAccessibilityAdapter : ISayclipAccessibility
    {
        public void Speak(string message, bool interrupt = true)
        {
            ScreenReaderControl.speech(message, interrupt);
        }
    }
}
