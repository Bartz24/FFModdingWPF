using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class RandoUI
{
    private static Action<string, int, int> SetUIProgress { get; set; }
    private static Action IncrementTotalProgress { get; set; }
    private static Action<int> SwitchTab { get; set; }
    public static void Init(Action<string, int, int> setProgress, Action incrementTotal, Action<int> switchTab)
    {
        SetUIProgress = setProgress;
        IncrementTotalProgress = incrementTotal;
        SwitchTab = switchTab;
    }

    public static void ShowTempUIMessage(string text)
    {
        SetUIProgress?.Invoke(text, 0, 0);
    }

    public static void SetUIProgressIndeterminate(string text)
    {
        SetUIProgress?.Invoke(text, -1, -1);
    }

    public static void SetUIProgressDeterminate(string text, int value, int max)
    {
        if (value < 0 || max < 0)
        {
            throw new ArgumentException("Value and max must be non-negative");
        }

        SetUIProgress?.Invoke(text, value, max);
    }

    public static void IncrementTotalProgressUI()
    {
        IncrementTotalProgress?.Invoke();
    }

    public static void SwitchUITab(int tab)
    {
        SwitchTab?.Invoke(tab);
    }
}
