namespace OpenCover.Framework
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using OpenCover.Framework.Strategy;

    public class IntermediateLanguageGeneratedBranch : ISkippedBranch
    {
        private readonly IList<MethodExclusion> ExcludedIntermediateLanguageConditionBranch;

        public IntermediateLanguageGeneratedBranch(IList<MethodExclusion> excludedIntermediateLanguageConditionBranch)
        {
            ExcludedIntermediateLanguageConditionBranch = excludedIntermediateLanguageConditionBranch;
        }

        public bool IsSkipped(InstructionData instructionData)
        {
            if (instructionData == null || instructionData.Instruction == null 
                || instructionData.Instruction.Previous == null 
                || !(instructionData.Instruction.Previous.Operand is MethodReference))
            {
                return false;
            }
            var instruction = instructionData.Instruction;
            var previous = instruction.Previous;
            var operand = previous.Operand as MethodReference;
            return
                ExcludedIntermediateLanguageConditionBranch.Any(
                    exclusion =>
                    operand.DeclaringType.FullName.StartsWith(exclusion.MethodClass) && operand.Name == exclusion.MethodName);
        }
    }
}