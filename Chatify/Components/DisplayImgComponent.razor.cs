using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Chatify.Components;

public partial class DisplayImgComponent
{
    [Parameter]
    [EditorRequired]
    public string ImageSource { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment SettingsContent { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsClickable { get; set; }

    [Parameter]
    public Action OnClickEvent { get; set; }

    [Parameter]
    public Action OpenDetails { get; set; }

    [Parameter]
    public string ParentClass { get; set; }

    [Parameter]
    public string BodyClass { get; set; }

    private string CreateWebPath()
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), ImageSource);
    }

    private string ClickableClass()
    {
        if (IsClickable)
        {
            return "clickable";
        }

        return "unclickable";
    }

    private string ImageClass()
    {
        if (OpenDetails is not null)
        {
            return "clickable";
        }

        return "unclickable";
    }
}