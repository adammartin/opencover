using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Strategy
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class InstructionData
    {
        public Instruction Instruction { get; private set; }
        public MethodDefinition EnclosingMethod { get; private set; }

        public InstructionData(Instruction instruction, MethodDefinition enclosingMethod)
        {
            Instruction = instruction;
            EnclosingMethod = enclosingMethod;
        }
    }
}
