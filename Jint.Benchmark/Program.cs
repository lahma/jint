using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using Jint.Benchmark;

//var benchmark = new JsonBenchmark();
//await benchmark.GlobalSetup();
//benchmark.Parse();
//benchmark.Stringify();
////
////
//DoParse(benchmark);
//DoStringify(benchmark);
////
//[MethodImpl(MethodImplOptions.NoInlining)]
//static void DoParse(JsonBenchmark jsonBenchmark)
//{
//    for (var i = 0; i < 10; ++i)
//    {
//        jsonBenchmark.Parse();
//    }
//}
////
//[MethodImpl(MethodImplOptions.NoInlining)]
//static void DoStringify(JsonBenchmark jsonBenchmark)
//{
//    for (var i = 0; i < 10; ++i)
//    {
//        jsonBenchmark.Stringify();
//    }
//}
////
//return;

BenchmarkSwitcher
    .FromAssembly(typeof(ArrayBenchmark).GetTypeInfo().Assembly)
    .Run(args);
