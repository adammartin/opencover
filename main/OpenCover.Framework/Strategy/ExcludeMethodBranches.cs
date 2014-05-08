namespace OpenCover.Framework.Strategy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Mono.Cecil.Cil;

    class ExcludeMethodBranches : ISkippedBranch
    {
        public ExcludeMethodBranches(IList<MethodExclusion> methodExclusions)
        {
            MethodExclusions = methodExclusions;
        }

        public IList<MethodExclusion> MethodExclusions { get; private set; }

        public bool IsSkipped(InstructionData instructionData)
        {
            return MethodExclusions.Any(x => Regex.IsMatch(instructionData.EnclosingMethod.Name, x.MethodName.ValidateAndEscape()));
        }
    }
}
