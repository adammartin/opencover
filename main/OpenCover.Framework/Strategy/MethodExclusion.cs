namespace OpenCover.Framework.Strategy
{
    public class MethodExclusion
    {
        public string MethodName { get; private set; }
        public string MethodClass { get; private set; }

        public MethodExclusion(string methodName)
        {
            MethodName = methodName;
        }

        public MethodExclusion(string methodClass, string methodName)
        {
            MethodName = methodName;
            MethodClass = methodClass;
        }
    }
}
