namespace OpenCover.Test.Framework.Strategy
{
    using System.Collections.Generic;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using NUnit.Framework;

    using OpenCover.Framework;

    [TestFixture]
    class IntermediateLanguageGeneratedBranchTest
    {

        [Test]
        public void IsSkippedWhenMatchesAnExclusion()
        {
            var instruction = CreateInstruction("System", "Object", "Blah");
            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.True(skippedBranch.IsSkipped(instruction));
        }

        [Test]
        public void IsSkippedWhenMatchesAFilterWhenManyFiltersExist()
        {
            var instruction = CreateInstruction("System", "Object", "Blah");
            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah"), new MethodExclusion("Flinstone", "Smash") });

            Assert.True(skippedBranch.IsSkipped(instruction));
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
            var instruction = CreateInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = null;

            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.False(skippedBranch.IsSkipped(instruction));
        }

        [Test]
        public void IsNotSkippedWhenOperandOfInstructionIsNotAMethodType()
        {
            var instruction = CreateInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = new object();

            var skippedBranch = new IntermediateLanguageGeneratedBranch(new List<MethodExclusion> { new MethodExclusion("System.Object", "Blah") });

            Assert.False(skippedBranch.IsSkipped(instruction));
        }

        private static Instruction CreateInstruction(string NameSpace, string ClassName, string MethodName)
        {
            var previous = Instruction.Create(OpCodes.Call, CreateMethodReference(NameSpace, ClassName, MethodName));
            var next = Instruction.Create(OpCodes.Ret);
            var instruction = Instruction.Create(OpCodes.Brtrue, next);
            instruction.Previous = previous;
            return instruction;
        }

        private static MethodReference CreateMethodReference(string nameSpace, string className, string methodName)
        {
            return new MethodReference(methodName, CreateTypeReference(nameSpace, className)) { DeclaringType = CreateTypeReference(nameSpace, className) };
        }

        private static TypeReference CreateTypeReference(string nameSpace, string className)
        {
            return new TypeReference(nameSpace, className, ModuleDefinition.CreateModule("Blarg", ModuleKind.Console), null);
        }
    }
}
