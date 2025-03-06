using Cosmic;
using Cosmic.Element;
using Cosmic.Tree;
using static Cosmic.Cosmic;

unsafe
{
    Initialize(MemoryUsage.High);

    BeginLayout();

    var stack = Stack()
        .Add(Rectangle())
        .Add(Stack().Orientation(LayoutDirection.TopToBottom).Add(Rectangle()));


    TreeWalker.Init(ref stack);
    TreeWalker.Dfs();

    EndLayout();
}