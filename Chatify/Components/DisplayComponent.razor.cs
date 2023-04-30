using Microsoft.AspNetCore.Components;

namespace Chatify.Components;

public partial class DisplayComponent
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment SettingsContent { get; set; }

    [Parameter]
    public Action OpenDetails { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsClickable { get; set; }

    [Parameter]
    public string BodyClass { get; set; }

    private string ClickableClass()
    {
        if (IsClickable)
        {
            return "clickable";
        }

        return "unclickable";
    }
}