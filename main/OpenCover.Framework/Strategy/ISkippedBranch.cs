namespace OpenCover.Framework.Strategy
{
    /// <summary>
    /// Interface to determine if an instrumented branch is skipped 
    /// </summary>
    interface ISkippedBranch
    {
        /// <summary>
        /// Return true of branch associated to instruction will be skipped.
        /// </summary>
        /// <param name="instruction">Instruction at branch point (uses Mono.Cecil)</param>
        /// <returns>bool</returns>
        bool IsSkipped(InstructionData instructionData);
    }
}
