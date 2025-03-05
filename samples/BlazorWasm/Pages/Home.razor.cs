using Cosmic.Allocator;
using Cosmic.Element;
using static Cosmic.Cosmic;

namespace BlazorWasm.Pages;

public partial class Home
{
    protected override void OnInitialized()
    {
        unsafe
        {
            var arena = ArenaManager.Create(10 * 1024 * 1024); //10 MB
            Initialize(Cosmic.MemoryUsage.High);
            var root = Stack().Add(ref Rectangle());
            Dispose();
        }

    }
}