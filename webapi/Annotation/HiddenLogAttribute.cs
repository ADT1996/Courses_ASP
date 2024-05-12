namespace webapi;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class HiddenLogAttribute: Attribute {}