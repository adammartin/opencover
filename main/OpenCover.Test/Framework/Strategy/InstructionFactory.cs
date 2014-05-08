namespace OpenCover.Test.Framework.Strategy
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    class InstructionFactory
    {
        public static MethodDefinition CreateMethodDefinition(string nameSpace, string className, string methodName, string implementedInterface)
        {
            var typeReference = CreateTypeReference(nameSpace, className);
            var methodDefinition = new MethodDefinition(methodName, MethodAttributes.Final, typeReference);
            methodDefinition.DeclaringType = new TypeDefinition(
                typeReference.Namespace,
                typeReference.Name,
                TypeAttributes.Abstract);
            methodDefinition.DeclaringType.Interfaces.Add(CreateTypeReference(nameSpace, implementedInterface));
            return methodDefinition;
        }

        public static Instruction CreateInstruction(string NameSpace, string ClassName, string MethodName)
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
