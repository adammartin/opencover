namespace OpenCover.Test.Framework.Strategy
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using OpenCover.Framework;
    using OpenCover.Framework.Strategy;

    [TestFixture]
    class IntermediateLanguageGeneratedBranchTest
    {

        [Test]
        public void IsSkippedWhenMatchesAnExclusion()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "Blah");
            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, null)));
        }

        [Test]
        public void IsSkippedWhenMatchesAFilterWhenManyFiltersExist()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "Blah");
            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah"), new MethodExclusion("Flinstone", "Smash") });

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, null)));
        }

        [Test]
        public void IsNotSkippedWhenInstructionIsNull()
        {
            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.False(skippedBranch.IsSkipped(null));
        }

        [Test]
        public void IsNotSkippedWhenOperandOfInstructionIsNull()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = null;

            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.False(skippedBranch.IsSkipped(new InstructionData(instruction, null)));
        }

        [Test]
        public void IsNotSkippedWhenOperandOfInstructionIsNotAMethodType()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = new object();

            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.False(skippedBranch.IsSkipped(new InstructionData(instruction, null)));
        }
     }
}
