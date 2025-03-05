namespace Cosmic.Element;

public unsafe ref struct ChildInfo
{
    public ChildInfo* Next {get; set;}
    public ElementKind Kind  {get; set;}
    public void* Address  {get; set;}
}