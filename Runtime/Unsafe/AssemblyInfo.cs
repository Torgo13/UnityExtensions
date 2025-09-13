#if UNITY_EDITOR
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PKGE.Editor.Unsafe")]
[assembly: InternalsVisibleTo("PKGE.Editor.Unsafe.Tests")]
// Shared test assembly used as part of Unity testing conventions.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor-testable")]
[assembly: InternalsVisibleTo("Assembly-CSharp-testable")]
#endif
