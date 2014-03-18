namespace OpenCover.Framework
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using OpenCover.Framework.Strategy;

    public class MethodExclusion
    {
        public string MethodName { get; private set; }
        public string MethodClass { get; private set; }

        public MethodExclusion(string methodClass, string methodName)
        {
            MethodName = methodName;
            MethodClass = methodClass;
        }
    }

    public class IntermediateLanguageGeneratedBranch : ISkippedBranch
    {
        private readonly IList<MethodExclusion> ExcludedIntermediateLanguageConditionBranch;

        public IntermediateLanguageGeneratedBranch(IList<MethodExclusion> excludedIntermediateLanguageConditionBranch)
        {
            ExcludedIntermediateLanguageConditionBranch = excludedIntermediateLanguageConditionBranch;
        }

        public bool IsSkipped(Instruction instruction)
        {
            if (instruction == null || instruction.Previous == null || !(instruction.Previous.Operand is MethodReference))
            {
                return false;
            }
            var previous = instruction.Previous;
            var operand = previous.Operand as MethodReference;
            return
                ExcludedIntermediateLanguageConditionBranch.Any(
                    exclusion =>
                    operand.DeclaringType.FullName.StartsWith(exclusion.MethodClass) && operand.Name == exclusion.MethodName);
        }
    }
}