using System;
using Jint.Native.Array;
using Xunit;

namespace Jint.Tests.Runtime
{
    public class ArrayTests
    {
        private readonly Engine _engine;

        public ArrayTests()
        {
            _engine = new Engine()
                    .SetValue("log", new Action<object>(Console.WriteLine))
                    .SetValue("assert", new Action<bool>(Assert.True))
                    .SetValue("equal", new Action<object, object>(Assert.Equal));
        }

        private void RunTest(string source)
        {
            _engine.Execute(source);
        }


        [Fact]
        public void ArrayPrototypeToStringWithArray()
        {
            var result = _engine.Execute("Array.prototype.toString.call([1,2,3]);").GetCompletionValue().AsString();

            Assert.Equal("1,2,3", result);
        }

        [Fact]
        public void ArrayPrototypeToStringWithNumber()
        {
            var result = _engine.Execute("Array.prototype.toString.call(1);").GetCompletionValue().AsString();

            Assert.Equal("[object Number]", result);
        }

        [Fact]
        public void ArrayPrototypeToStringWithObject()
        {
            var result = _engine.Execute("Array.prototype.toString.call({});").GetCompletionValue().AsString();

            Assert.Equal("[object Object]", result);
        }

        [Fact]
        public void EmptyStringKey()
        {
            var result = _engine.Execute("var x=[];x[\"\"]=8;x[\"\"];").GetCompletionValue().AsNumber();

            Assert.Equal(8, result);
        }

        [Fact]
        public void LargeArraySize()
        {
            const string code = @"
            let arr = [];
            for (let i = 0; i < 10000; i++) arr.push(i);
            for (let i=0;i<10000;i++) arr.splice(0, 1);
            ";
            var engine = new Engine();
            engine.Execute(code);
        }

        [Fact]
        public void ArrayLengthFromInitialState()
        {
            var engine = new Engine();
            var array = new ArrayInstance(engine, 0);
            var length = (int) array.Length;
            Assert.Equal(0, length);
        }

        [Fact]
        public void ArraySortIsStable()
        {
            const string code = @"
                var items = [
                    { name: 'Edward', value: 0 },
                    { name: 'Sharpe', value: 0 },
                    { name: 'And', value: 0 },
                    { name: 'The', value: 1 },
                    { name: 'Magnetic', value: 0 },
                    { name: 'Zeros', value: 0 }
                ];

                // sort by value
                function compare(a, b) {
                    return a.value - b.value;
                }

                var a = items.sort();

                assert(a[0].name == 'Edward');
                assert(a[1].name == 'Sharpe');
                assert(a[2].name == 'And');
                assert(a[3].name == 'The');
                assert(a[4].name == 'Magnetic');
                assert(a[5].name == 'Zeros');

                var a = items.sort(compare);

                assert(a[0].name == 'Edward');
                assert(a[1].name == 'Sharpe');
                assert(a[2].name == 'And');
                assert(a[3].name == 'Magnetic');
                assert(a[4].name == 'Zeros');
                assert(a[5].name == 'The');
            ";

            _engine.Execute(code);
        }

        [Fact]
        public void ExtendingArrayAndInstanceOf()
        {
            const string script = @"
                class MyArr extends Array {
                    constructor(...args) {
                        super(...args);
                    } 
                }";

            _engine.Execute(script);
            _engine.Execute("const a = new MyArr(1,2);");
            Assert.True(_engine.Execute("a instanceof MyArr").GetCompletionValue().AsBoolean());
        }
    }
}