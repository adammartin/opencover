using System;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;

namespace OpenCover.Test.Framework
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Mono.Cecil.Cil;

    using OpenCover.Framework.Strategy;
    using OpenCover.Test.Framework.Strategy;

    [TestFixture]
    public class FilterTests
    {
        #region TestData for AddFilter tests

        public class AssemblyClassData
        {
            public string AssemblyClass { get; set; }
            public string AssemblyResult { get; set; }
            public string ClassResult { get; set; }
            public FilterType FilterTypeResult { get; set; }
        }

#pragma warning disable 169

        private string[] _invalidAssemblyClassPairs = new[] { "Garbage", "+[]", "-[ ]", "[ ", " ]", "+[]]", "-[][", @"-[\]", @"+[X]\", "-[X]]", "+[X][" };

        private AssemblyClassData[] _assemblyClassPairs = new[]
                                                              {
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]Console",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[My App]Namespace",
                                                                          AssemblyResult = "My App",
                                                                          ClassResult = "Namespace",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = "",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "-[System.*]Console",
                                                                          AssemblyResult = @"System\..*",
                                                                          ClassResult = "Console",
                                                                          FilterTypeResult = FilterType.Exclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "+[System]Console.*",
                                                                          AssemblyResult = "System",
                                                                          ClassResult = @"Console\..*",
                                                                          FilterTypeResult = FilterType.Inclusion, 
                                                                      },
                                                                  new AssemblyClassData()
                                                                      {
                                                                          AssemblyClass = "-[System.*]Console.*",
                                                                          AssemblyResult = @"System\..*",
                                                                          ClassResult = @"Console\..*",
                                                                          FilterTypeResult = FilterType.Exclusion, 
                                                                      }
                                                              };
#pragma warning restore 169   
        #endregion
        private MethodDefinition _methodDefinition = InstructionFactory.CreateMethodDefinition("System", "Object", "Blarg", null);


        [Test]
        public void AddFilter_ThrowsException_WhenInvalid_AssemblyClassPair(
            [ValueSource("_invalidAssemblyClassPairs")]string assemblyClassPair)
        {
            // arrange
            var filter = new Filter();

            // act/assert
            Assert.Catch<InvalidOperationException>(() => filter.AddFilter(assemblyClassPair), 
                "'{0}' should be invalid", assemblyClassPair);     
        }

        [Test]
        public void AddFilter_Adds_ValidAssemblyClassPair(
            [ValueSource("_assemblyClassPairs")]AssemblyClassData assemblyClassPair)
        {
            // arrange
            var filter = new Filter();

            // act
            filter.AddFilter(assemblyClassPair.AssemblyClass);

            // assert
            Assert.AreEqual(1, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ? 
                filter.InclusionFilter.Count : filter.ExclusionFilter.Count);

            Assert.AreEqual(assemblyClassPair.AssemblyResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilter[0].Key : filter.ExclusionFilter[0].Key);

            Assert.AreEqual(assemblyClassPair.ClassResult, assemblyClassPair.FilterTypeResult == FilterType.Inclusion ?
                filter.InclusionFilter[0].Value : filter.ExclusionFilter[0].Value);
        }

        #region Test Data or UseAssembly tests

        public class UseAssemblyData
        {
            public string[] Filters { get; set; }
            public string Assembly { get; set; }
            public bool ExpectedResult { get; set; }
        }

        private UseAssemblyData[] _useAssemblyData = new[]
                                                         {
                                                             new UseAssemblyData()
                                                                 {
                                                                     Filters = new string[0],
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"-[System.*]R*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"-[System.*]*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"+[System.*]*"},
                                                                     Assembly = "System.Debug",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"-[mscorlib]*", "-[System.*]*", "+[*]*"},
                                                                     Assembly = "mscorlib",
                                                                     ExpectedResult = false
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"+[XYZ]*"},
                                                                     Assembly = "XYZ",
                                                                     ExpectedResult = true
                                                                 },
                                                                 new UseAssemblyData()
                                                                 {
                                                                     Filters = new [] {"+[XYZ]*"},
                                                                     Assembly = "XYZA",
                                                                     ExpectedResult = false
                                                                 }
                                                         };
        #endregion

        [Test]
        public void UseAssembly_Tests(
            [ValueSource("_useAssemblyData")]UseAssemblyData data)
        {
            // arrange
            var filter = new Filter();
            data.Filters.ToList().ForEach(filter.AddFilter);

            // act
            var result = filter.UseAssembly(data.Assembly);

            // result
            Assert.AreEqual(data.ExpectedResult, result, 
                "Filter: '{0}' Assembly: {1} => Expected: {2}", 
                string.Join(",", data.Filters), data.Assembly, data.ExpectedResult);
        }

        #region Test Data for InstrumentClass tests

        public class InstrumentClassData
        {
            public string[] Filters { get; set; }
            public string Assembly { get; set; }
            public string Class { get; set; }
            public bool ExpectedResult { get; set; }
        }

        private InstrumentClassData[] _instrumentClassData = new[]
                                                                 {
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]A*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ*]A*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]A*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]*Class"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]*Name"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[XYZ]*space.C*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = true
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[XYZ*]*"},
                                                                             Assembly = "XYZA",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[XYZ]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[*]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[X*Z]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[XYZ]*Class"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"-[XYZ]*Unknown"},
                                                                             Assembly = "XYZ",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[*]*"},
                                                                             Assembly = "",
                                                                             Class = "Namespace.Class",
                                                                             ExpectedResult = false
                                                                         },
                                                                     new InstrumentClassData()
                                                                         {
                                                                             Filters = new[] {"+[*]*"},
                                                                             Assembly = "XYZ",
                                                                             Class = "",
                                                                             ExpectedResult = false
                                                                         }

                                                                 };
        #endregion

        [Test]
        public void InstrumentClass_Tests(
            [ValueSource("_instrumentClassData")]InstrumentClassData data)
        {
            //// arrange
            var filter = new Filter();
            data.Filters.ToList().ForEach(filter.AddFilter);

            // act
            var result = filter.InstrumentClass(data.Assembly, data.Class);

            // result
            Assert.AreEqual(data.ExpectedResult, result,
               "Filter: '{0}' Assembly: {1} Class: {2} => Expected: {3}", 
               string.Join(",", data.Filters), data.Assembly, data.Class, data.ExpectedResult);
        }

        [Test]
        public void AddAttributeExclusionFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(new []{ null, "" });

            Assert.AreEqual(1, filter.ExcludedAttributes.Count);
        }

        [Test]
        public void AddAttributeExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddAttributeExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedAttributes[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void Entity_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter();
            var entity = new Mock<IMemberDefinition>();

            Assert.IsFalse(filter.ExcludeByAttribute(entity.Object));
        }

        [Test]
        public void AddFileExclusionFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(null);

            Assert.AreEqual(0, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.ExcludedFiles.Count);
        }

        [Test]
        public void AddFileExclusionFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddFileExclusionFilters(new[] { ".*" });

            Assert.IsTrue(filter.ExcludedFiles[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void AddIntermediateLanguageConditionExclusion_HandlesNullByAddingNothing()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");

            filter.AddIntermediateLanguageConditionExclusion(null);

            Assert.False(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void AddIntermediateLanguageConditionExclusion_HandlesInvalidByAddingNothing()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");

            filter.AddIntermediateLanguageConditionExclusion("System.Object");

            Assert.False(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));           
        }

        [Test]
        public void AddIntermediateLanguageConditionExclusion_HandlesEmptyMethod()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");

            filter.AddIntermediateLanguageConditionExclusion("System.Object::");

            Assert.False(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranchReturnsTrueWhenMatchesAFilter()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");

            filter.AddIntermediateLanguageConditionExclusion("System.Object::Blah");

            Assert.True(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranchReturnsTrueWhenMatchesAFilterWhenManyFilters()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");

            filter.AddIntermediateLanguageConditionExclusion("Flinstone::Smash");
            filter.AddIntermediateLanguageConditionExclusion("System.Object::Blah");

            Assert.True(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranchReturnsFalseWhenInstructionIsNull()
        {
            var filter = new Filter();
            
            filter.AddIntermediateLanguageConditionExclusion("System.Object::Blah");

            Assert.False(filter.IsExcludedConditionBranch(null));            
        }

        [Test]
        public void IsExcludedConditionBranchReturnsFalseWhenOperandOfInstructionIsNull()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = null;

            filter.AddIntermediateLanguageConditionExclusion("System.Object::Blah");

            Assert.False(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranchReturnsFalseWhenOperandOfInstructionIsNotAMethodType()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");
            instruction.Previous.Operand = new object();

            filter.AddIntermediateLanguageConditionExclusion("System.Object::Blah");

            Assert.False(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void ExcludedBranchesInMethodsReturnsMultipleBranchIgnores()
        {
            var filter = new Filter();
            var instruction = CreatePreviousCallInstruction("System", "Object", "Blah");
            filter.AddMethodExclusion("Blarg");

            Assert.True(filter.IsExcludedConditionBranch(new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void AddIntermediateLanguageBranchExclusion_HandlesNull()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion(null);

            Assert.IsEmpty(filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface")));
        }

        [Test]
        public void AddIntermediateLanguageBranchExclusion_HandlesInvalidClassOrMethod()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System::0");

            Assert.IsEmpty(filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface")));
        }

        [Test]
        public void AddIntermediateLanguageBranchExclusion_HandlesInvalidBranches()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System::Object");

            Assert.IsEmpty(filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface")));
        }

        [Test]
        public void AddIntermediateLanguageBranchExclusion_HandlesEmptyBranches()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System::Object::");

            Assert.IsEmpty(filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface")));
        }

        [Test]
        public void AddIntermediateLanguageBranchExclusion_HandlesInvalidCharacters()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System::Object::b,c");

            Assert.IsEmpty(filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface")));            
        }

        [Test]
        public void ExcludedIntermediateLanguageBranchesReturnsSingleBranchIgnores()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System.Interface::ToString::0");

            var result = filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface"));

            Assert.AreEqual(new List<int>() { 0 }, result);
        }

        [Test]
        public void ExcludedIntermediateLanguageBranchesReturnsMultipleBranchIgnores()
        {
            var filter = new Filter();

            filter.AddIntermediateLanguageBranchExclusion("System.Interface::ToString::2,4");

            var result = filter.ExcludedIntermediateLanguageBranches(InstructionFactory.CreateMethodDefinition("System", "Object", "ToString", "Interface"));

            Assert.AreEqual(new List<int>() { 2, 4 }, result);
        }

        [Test]
        public void AddTestFileFilters_HandlesNull()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(null);

            Assert.AreEqual(0, filter.TestFiles.Count);
        }

        [Test]
        public void AssemblyIsIncludedForTestMethodGatheringWhenFilterMatches()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new []{"A*"});

            Assert.IsTrue(filter.UseTestAssembly("ABC.dll"));
            Assert.IsFalse(filter.UseTestAssembly("XYZ.dll"));
            Assert.IsFalse(filter.UseTestAssembly(""));
        }

        [Test]
        public void AddTestFileFilters_Handles_Null_Elements()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new[] { null, "" });

            Assert.AreEqual(1, filter.TestFiles.Count);
        }

        [Test]
        public void AddTestFileFilters_Escapes_Elements_And_Matches()
        {
            var filter = new Filter();

            filter.AddTestFileFilters(new[] { ".*" });

            Assert.IsTrue(filter.TestFiles[0].Value.Match(".ABC").Success);
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_Filters_Set()
        {
            var filter = new Filter();

            Assert.IsFalse(filter.ExcludeByFile("xyz.cs"));
        }

        [Test]
        public void File_Is_Not_Excluded_If_No_File_Not_Supplied()
        {
            var filter = new Filter();

            Assert.IsFalse(filter.ExcludeByFile(""));
        }

        [Test]
        public void File_Is_Not_Excluded_If_Does_Not_Match_Filter()
        {
            var filter = new Filter();
            filter.AddFileExclusionFilters(new[]{"XXX.*"});

            Assert.IsFalse(filter.ExcludeByFile("YYY.cs"));
        }

        [Test]
        public void File_Is_Excluded_If_Matches_Filter()
        {
            var filter = new Filter();
            filter.AddFileExclusionFilters(new[] { "XXX.*" });

            Assert.IsTrue(filter.ExcludeByFile("XXX.cs"));
        }

        [Test]
        public void Can_Identify_Excluded_Methods()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof (Samples.Concrete).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods)
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter) continue;
                Assert.True(filter.ExcludeByAttribute(methodDefinition));                
            }

        }

        [Test]
        public void Can_Identify_Excluded_Properties()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Concrete).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var propertyDefinition in type.Properties)
            {
                Assert.True(filter.ExcludeByAttribute(propertyDefinition));
            }
        }

        [Test]
        public void Can_Identify_Excluded_Anonymous_Issue99()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Anonymous).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Anonymous).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods.Where(x=>x.Name.Contains("EXCLUDE")))
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter || methodDefinition.IsConstructor) continue;
                Assert.True(filter.ExcludeByAttribute(methodDefinition), "failed to execlude {0}", methodDefinition.Name);
            }
        }

        [Test]
        public void Can_Identify_Included_Anonymous_Issue99()
        {
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Anonymous).Assembly.Location);

            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.Anonymous).FullName);

            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            foreach (var methodDefinition in type.Methods.Where(x => x.Name.Contains("INCLUDE")))
            {
                if (methodDefinition.IsSetter || methodDefinition.IsGetter || methodDefinition.IsConstructor) continue;
                Assert.False(filter.ExcludeByAttribute(methodDefinition), "failed to include {0}", methodDefinition.Name);
            }
        }

        [Test]
        public void Handles_Issue117()
        {
            var filter = new Filter();
            filter.AddAttributeExclusionFilters(new[] { "*ExcludeMethodAttribute" });

            var mockDefinition = new Mock<IMemberDefinition>();

            mockDefinition.SetupGet(x => x.HasCustomAttributes).Returns(true);
            mockDefinition.SetupGet(x => x.CustomAttributes).Returns(new Collection<CustomAttribute>());
            mockDefinition.SetupGet(x => x.Name).Returns("<>f_ddd");
            mockDefinition.SetupGet(x => x.DeclaringType).Returns(new TypeDefinition("","f_ddd", TypeAttributes.Public));

            Assert.DoesNotThrow(() => filter.ExcludeByAttribute(mockDefinition.Object));
        }

        [Test]
        public void CanIdentify_AutoImplementedProperties()
        {
            // arrange
            var sourceAssembly = AssemblyDefinition.ReadAssembly(typeof(Samples.Concrete).Assembly.Location);
            var type = sourceAssembly.MainModule.Types.First(x => x.FullName == typeof(Samples.DeclaredMethodClass).FullName);

            // act/assert
            var filter = new Filter();
            var wasTested = false;
            foreach (var methodDefinition in type.Methods
                .Where(x => x.IsGetter || x.IsSetter).Where(x => x.Name.EndsWith("AutoProperty")))
            {
                wasTested = true;
                Assert.IsTrue(filter.IsAutoImplementedProperty(methodDefinition));
            }
            Assert.IsTrue(wasTested);

            wasTested = false;
            foreach (var methodDefinition in type.Methods
                .Where(x => x.IsGetter || x.IsSetter).Where(x => x.Name.EndsWith("PropertyWithBackingField")))
            {
                wasTested = true;
                Assert.IsFalse(filter.IsAutoImplementedProperty(methodDefinition));
            }
            Assert.IsTrue(wasTested);
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnFalseWhenSuppliedNull()
        {
            var filter = new Filter();
            filter.ExcludeLambdaAutoGeneratedBranches();
            Assert.False(filter.IsExcludedConditionBranch(null));
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnTrueWhenSuppliedACompilerGeneratedAttribute()
        {
            var filter = new Filter();
            filter.ExcludeLambdaAutoGeneratedBranches();
            var instruction = CreatePreviousLoadFieldInstruction("wat", typeof(CompilerGeneratedAttribute));

            Assert.True(filter.IsExcludedConditionBranch(﻿new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnFalseWhenExcludeLambdaAutoGeneratedNotEnabled()
        {
            var filter = new Filter();
            var instruction = CreatePreviousLoadFieldInstruction("wat", typeof(CompilerGeneratedAttribute));

            Assert.False(filter.IsExcludedConditionBranch(﻿new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnFalseWhenSuppliedAnInstructionWithNoAttributes()
        {
            var filter = new Filter();
            filter.ExcludeLambdaAutoGeneratedBranches();
            var instruction = CreatePreviousLoadFieldInstruction("wat", null);

            Assert.False(filter.IsExcludedConditionBranch(﻿new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnFalseWhenSuppliedAnInstructionWithNoPrevious()
        {
            var filter = new Filter();
            filter.ExcludeLambdaAutoGeneratedBranches();
            var instruction = CreatePreviousLoadFieldInstruction("wat", typeof(CompilerGeneratedAttribute));
            instruction.Previous = null;
            Assert.False(filter.IsExcludedConditionBranch(﻿new InstructionData(instruction, _methodDefinition)));
        }

        [Test]
        public void IsExcludedConditionBranch_WillReturnFalseWhenSuppliedAPreviousThatIsNotAFieldDefinition()
        {
            var filter = new Filter();
            filter.ExcludeLambdaAutoGeneratedBranches();
            var instruction = CreatePreviousLoadFieldInstruction("wat", typeof(CompilerGeneratedAttribute));
            instruction.Previous.OpCode = OpCodes.Call;
            instruction.Previous.Operand = new MethodDefinition("blah", MethodAttributes.Final, CreateTypeReference("some", "foo"));

            Assert.False(filter.IsExcludedConditionBranch(﻿new InstructionData(instruction, _methodDefinition)));
        }

        private static Instruction CreatePreviousCallInstruction(string NameSpace, string ClassName, string MethodName)
        {
            var previous = Instruction.Create(OpCodes.Call, CreateMethodReference(NameSpace, ClassName, MethodName));
            var next = Instruction.Create(OpCodes.Ret);
            var instruction = Instruction.Create(OpCodes.Brtrue, next);
            instruction.Previous = previous;
            return instruction;
        }

        private static MethodReference CreateMethodReference(string nameSpace, string className, string methodName)
        {
            var typeReference = CreateTypeReference(nameSpace, className);
            return new MethodReference(methodName, typeReference) { DeclaringType = typeReference };
        }

        private static Instruction CreatePreviousLoadFieldInstruction(string name, Type attributeType)
        {
            var previous = Instruction.Create(OpCodes.Ldsfld, CreateFieldDefinition(name, attributeType));
            var next = Instruction.Create(OpCodes.Ret);
            var instruction = Instruction.Create(OpCodes.Brtrue, next);
            instruction.Previous = previous;
            return instruction;
        }

        private static FieldDefinition CreateFieldDefinition(string name, Type attributeType)
        {
            var fieldDefinition = new FieldDefinition(name, FieldAttributes.CompilerControlled, CreateTypeReference("some", "foo"));
            if (attributeType != null)
            {
                fieldDefinition.CustomAttributes.Add(new CustomAttribute(CreateMethodReference(attributeType)));
            }
            return fieldDefinition;
        }

        private static MethodReference CreateMethodReference(Type attributeType)
        {
            return ModuleDefinition.CreateModule("Blarg", ModuleKind.Console).Import(attributeType.GetConstructor(Type.EmptyTypes));
        }

        private static TypeReference CreateTypeReference(string nameSpace, string className)
        {
            return new TypeReference(nameSpace, className, ModuleDefinition.CreateModule("Blarg", ModuleKind.Console), null);
        }
    }
}
