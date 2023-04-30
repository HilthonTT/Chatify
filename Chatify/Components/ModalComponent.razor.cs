using Microsoft.AspNetCore.Components;

namespace Chatify.Components;

public partial class ModalComponent
{
    [Parameter]
    [EditorRequired]
    public string Id { get; set; }

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public string FooterClass { get; set; }

    [Parameter]
    public RenderFragment ButtonContent { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }
}