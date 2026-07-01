namespace sayclip
{
    public interface ISayclipPluginContext
    {
        ISayclipAccessibility Accessibility { get; }
        ISayclipLogger Logger { get; }
    }
}
