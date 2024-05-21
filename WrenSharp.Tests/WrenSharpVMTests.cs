using System.Text;
using Xunit.Abstractions;

namespace WrenSharp.Tests
{
    public class WrenSharpVMTests : IDisposable
    {
        public class WrenModuleLoader : IWrenModuleProvider, IWrenModuleResolver
        {
            private ITestOutputHelper m_Output;

            public WrenModuleLoader(ITestOutputHelper output)
            {
                m_Output = output;
            }

            public IWrenSource GetModuleSource(WrenVM vm, string moduleName)
            {
                m_Output.WriteLine($"WrenModuleLoader -> Loading: {moduleName}");

                if (moduleName == "testModule")
                {
                    return new WrenStringSource(@"
                        class A {
                            static greet() {
                                System.print(""hello from testModule!"")
                            }
                        }
                    ");
                }

                return null!;
            }

            public void OnModuleLoadComplete(WrenVM vm, string moduleName, IWrenSource source)
            {
                m_Output.WriteLine($"WrenModuleLoader -> Loaded: {moduleName}");
                source.Dispose();
            }

            public string ResolveModule(WrenVM vm, string importer, string moduleName)
            {
                m_Output.WriteLine($"WrenModuleLoader -> Resolving: importer={importer}, moduleName={moduleName}");
                return moduleName;
            }
        }

        private readonly ITestOutputHelper m_Output;
        private readonly StringBuilder m_WriteBuffer;
        private readonly WrenSharpVM m_VM;

        public WrenSharpVMTests(ITestOutputHelper output)
        {
            m_WriteBuffer = new StringBuilder();
            m_Output = output;

            var moduleLoader = new WrenModuleLoader(m_Output);

            m_VM = new WrenSharpVM(new WrenVMConfiguration()
            {
                LogErrors = true,
                ModuleProvider = moduleLoader,
                ModuleResolver = moduleLoader,
                WriteOutput = (WrenDelegateWriteOutput)((vm, text) => m_WriteBuffer.Append(text)),
                ErrorOutput = (WrenDelegateErrorOutput)((vm, errorType, moduleName, lineNumber, message) =>
                {
                    switch (errorType)
                    {
                        case WrenErrorType.Compile:
                            m_Output.WriteLine($"[{moduleName}: ln {lineNumber}] [Error] {message}");
                            break;

                        case WrenErrorType.StackTrace:
                            m_Output.WriteLine($"[{moduleName}: ln {lineNumber}] in {message}");
                            break;

                        case WrenErrorType.Runtime:
                            m_Output.WriteLine($"[Error] {message}");
                            break;
                    }
                }),
            });
        }

        public void Dispose()
        {
            FlushWriteBuffer();
            m_VM?.Dispose();
        }

        private void FlushWriteBuffer()
        {
            if (m_WriteBuffer.Length <= 0)
                return;

            // Remove last newline, since output will add one
            if (m_WriteBuffer[m_WriteBuffer.Length - 1] == '\n')
            {
                m_WriteBuffer.Length--;
            }

            m_Output.WriteLine(m_WriteBuffer.ToString());
            m_WriteBuffer.Clear();
        }

        [Fact]
        public void Interpet_String_Pass()
        {
            m_VM.Interpret("main", "System.print(\"Hello world\")", throwOnFailure: true);
        }

        [Fact]
        public void Interpet_String_Fail()
        {
            Assert.Throws<WrenInterpretException>(() =>
            {
                m_VM.Interpret("main", "_System.print($\"Invalid code\")", throwOnFailure: true);
            });
        }

        [Fact]
        public void Interpet_CharSpan_Pass()
        {
            string source = "prefixed junk\nSystem.print(\"Hello world\")\n some extra junk";
            m_VM.Interpret("main", source.AsSpan(14, 27), throwOnFailure: true);
        }

        [Fact]
        public void Interpet_StringBuilder_Pass()
        {
            var sb = new StringBuilder();
            sb.Append("System.print(\"Hello world\")\n");
            sb.Append("System.print(\"Foo Bar!\")\n");
            sb.Append("System.print(\"Lorem ipsum dolor sit amet\")\n");
            m_VM.Interpret("main", sb, throwOnFailure: true);
        }

        [Fact]
        public void LoadModule_Pass()
        {
            m_VM.Interpret("main", @"
                import ""testModule"" for A
                System.print(A)
            ",
            throwOnFailure: true);
        }

        [Fact]
        public void LoadModule_Fail()
        {
            Assert.Throws<WrenInterpretException>(() =>
            {
                m_VM.Interpret("main", @"
                    import ""nonExistentModule"" for A
                    System.print(A)
                ",
                throwOnFailure: true);
            });
        }

        [Fact]
        public void ForiegnClass_Pass()
        {
            const int DataValue = 1234;

            bool allocatorCalled = false;
            bool finalizerCalled = false;
            bool methodCalled = false;

            m_VM
            .Foreign("main", "TestForeign")
            .Allocate((WrenCallContext call, ref int value) =>
            {
                value = (int)call.GetArgDouble(0);
                allocatorCalled = true;
                m_Output.WriteLine($"allocate called, data: {value}");
            })
            .Finalize((ref int data) =>
            {
                finalizerCalled = true;
                m_Output.WriteLine($"finalize called, data: {data}");
            })
            .Instance(signature: "testMethod()", m =>
            {
                methodCalled = true;

                int receiverData = m.GetReceiverForeign<int>();
                m_Output.WriteLine($"testMethod() called, data: {receiverData}");
                Assert.Equal(DataValue, receiverData);
            });

            m_VM.Interpret("main", @"
                foreign class TestForeign {
                    construct new(value) {}

                    foreign testMethod()
                }

                // Instance is created inside a function so it can be garbage collected
                // and we can ensure the finalizer runs.
                Fn.new { 
                    var instance = TestForeign.new(1234)
                    instance.testMethod()
                }.call()

                // Instances's finalizer should run when it is cleaned up
                System.gc()
            ",
            throwOnFailure: true);

            Assert.True(allocatorCalled);
            Assert.True(methodCalled);
            Assert.True(finalizerCalled);
        }

        [Fact]
        public void CreateFunction_Pass()
        {
            var fnHandle = m_VM.CreateFunction("main", "arg1, arg2", @"
                System.print(""arg1=%(arg1)"")
                System.print(""arg2=%(arg2)"")
                return arg2 * 2
            ",
            throwOnFailure: true);

            var call = m_VM.CreateFunctionCall(fnHandle, 2);
            call.SetArg(0, true);
            call.SetArg(1, 1234);
            call.Call(throwOnFailure: true);

            var result = call.GetReturnDouble();

            FlushWriteBuffer();
            m_Output.WriteLine($"result={result}");

            Assert.Equal(1234 * 2, result);
        }
    }
}
