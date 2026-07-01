namespace sayclip
{
    internal class SayclipPluginContext : ISayclipPluginContext
    {
        public ISayclipAccessibility Accessibility { get; }
        public ISayclipLogger Logger { get; }

        public SayclipPluginContext()
        {
            Accessibility = new SayclipAccessibilityAdapter();
            Logger = new SayclipLoggerAdapter();
        }
    }
}
