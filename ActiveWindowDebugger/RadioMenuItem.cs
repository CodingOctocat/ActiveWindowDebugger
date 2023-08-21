using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ActiveWindowDebugger;

public class RadioMenuItem : MenuItem
{
    private static readonly Geometry _radioDot = Geometry.Parse("M9,5.5L8.7,7.1 7.8,8.3 6.6,9.2L5,9.5L3.4,9.2 2.2,8.3 1.3,7.1L1,5.5L1.3,3.9 2.2,2.7 3.4,1.8L5,1.5L6.6,1.8 7.8,2.7 8.7,3.9L9,5.5z");

    public string? GroupName { get; set; }

#pragma warning disable IDE0051 // 删除未使用的私有成员

    private new bool IsCheckable { get; } = false;

#pragma warning restore IDE0051 // 删除未使用的私有成员

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("Glyph") is not Path p)
        {
            return;
        }

        p.Data = _radioDot;
    }

    protected override void OnClick()
    {
        if (Parent is ItemsControl c)
        {
            var rmi = c.Items.OfType<RadioMenuItem>()
                             .FirstOrDefault(i => i.GroupName == GroupName && i.IsChecked);

            if (rmi == this)
            {
                base.OnClick();

                return;
            }

            if (rmi is not null)
            {
                rmi.IsChecked = false;
            }

            IsChecked = true;
        }

        base.OnClick();
    }
}
