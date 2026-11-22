using System;
using TMPro;
using UnityEngine;

public class CombatLogCopyScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _combatLogText;
    
    public void CopyCombatLogToClipboard()
    {
        if (_combatLogText == null)
        {
            Debug.LogWarning("CombatLogCopyScript: _combatLogText is not assigned.");
            return;
        }
        string combatLog = _combatLogText.text;
        GUIUtility.systemCopyBuffer = combatLog;
        Debug.Log("Combat log copied to clipboard.");
    }

    public void CopyToClipboardUnTagged()
    {
        if (_combatLogText == null)
        {
            Debug.LogWarning("CombatLogCopyScript: _combatLogText is not assigned.");
            return;
        }
        string combatLog = _combatLogText.text;
        string untaggedLog = StripTags(combatLog);
        GUIUtility.systemCopyBuffer = untaggedLog;
        Debug.Log("Combat log copied to clipboard (untagged).");
    }

    private string StripTags(string combatLog)
    {
        combatLog = System.Text.RegularExpressions.Regex.Replace(combatLog, "<color=.*?>", "");
        combatLog = System.Text.RegularExpressions.Regex.Replace(combatLog, "</color>", "");
        return combatLog;
    }
}
