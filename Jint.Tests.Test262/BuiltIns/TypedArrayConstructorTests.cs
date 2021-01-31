using Xunit;

namespace Jint.Tests.Test262.BuiltIns
{
    public class TypedArrayConstructorTests : Test262Test
    {
        [Theory(DisplayName = "built-ins\\TypedArrayConstructors")]
        [MemberData(nameof(SourceFiles), "built-ins\\TypedArrayConstructors", false)]
        [MemberData(nameof(SourceFiles), "built-ins\\TypedArrayConstructors", true, Skip = "Skipped")]
        protected void RunTest(SourceFile sourceFile)
        {
            RunTestInternal(sourceFile);
        }
    }
}