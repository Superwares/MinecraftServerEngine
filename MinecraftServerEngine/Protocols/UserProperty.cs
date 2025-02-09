
namespace MinecraftServerEngine.Protocols
{
    internal readonly struct UserProperty
    {
        internal readonly string Name;
        internal readonly string Value;
        internal readonly string Signature;

        internal UserProperty(string name, string value, string signature)
        {
            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false);
            System.Diagnostics.Debug.Assert(value != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(value) == false);

            Name = name;
            Value = value;
            Signature = signature;
        }
    }
}
