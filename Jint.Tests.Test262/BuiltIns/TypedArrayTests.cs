using Xunit;

namespace Jint.Tests.Test262.BuiltIns
{
    public class TypedArrayTests : Test262Test
    {
        [Theory(DisplayName = "built-ins\\TypedArray")]
        [MemberData(nameof(SourceFiles), "built-ins\\TypedArray", false)]
        [MemberData(nameof(SourceFiles), "built-ins\\TypedArray", true, Skip = "Skipped")]
        protected void RunTest(SourceFile sourceFile)
        {
            RunTestInternal(sourceFile);
        }
    }
}