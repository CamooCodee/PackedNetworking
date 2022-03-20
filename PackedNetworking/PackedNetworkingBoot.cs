namespace PackedNetworking.Util
{
    public static class PackedNetworkingBoot
    {
        private static BootProperty<bool> forceServerBuild;
        
        public static void SetForceServerBuild(bool newValue)
        {
            forceServerBuild.Value = newValue;
        }
        public static bool GetForceServerBuild(bool current)
        {
            return forceServerBuild.WasSet ? forceServerBuild.Value : current;
        }

        private struct BootProperty<T>
        {
            private T _value;
            public T Value
            {
                get => _value;
                set
                {
                    WasSet = true;
                    _value = value;
                }
            }

            public bool WasSet { get; private set; }

            public BootProperty(T value)
            {
                _value = value;
                WasSet = false;
            }
        }
    }
}