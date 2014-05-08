namespace OpenCover.Test.Framework.Strategy
{
    using System.Collections.Generic;
    using System.Dynamic;

    using NUnit.Framework;

    using OpenCover.Framework;
    using OpenCover.Framework.Strategy;

    [TestFixture]
    class ExcludeMethodBranchesTest
    {
        [Test]
        public void WillExcludeBranchPointForExactMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("EnclosingMethod") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));            
        }

        [Test]
        public void WillNotExcludeBranchPointForNonMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("Opine") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.False(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }

        [Test]
        public void WillExcludeBranchPointForTrailingWildcardMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("EnclosingMeth*") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }

        [Test]
        public void WillExcludeBranchPointForLeadingWildcardMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("*EnclosingMethod") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }

        [Test]
        public void WillExcludeBranchPointForLeadingAndTrailingWildcardMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("*closingMeth*") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }

        [Test]
        public void WillExcludeBranchPointForMiddleWildcardMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("Enclosin*Method") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }

        [Test]
        public void WillExcludeBranchPointForLeadingTrailingAndMiddleWildcardMatch()
        {
            var instruction = InstructionFactory.CreateInstruction("System", "Object", "MethodToCall");
            var skippedBranch = new ExcludeMethodBranches(new List<MethodExclusion> { new MethodExclusion("*closin*Meth*") });
            var methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "EnclosingMethod", null);

            Assert.True(skippedBranch.IsSkipped(new InstructionData(instruction, methodDefinition)));
        }
    }
}
