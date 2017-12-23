using System.Collections.Generic;
using UnityEngine;

public class DisplayModifier
{
    public bool DisableHalos { get; private set; }
    public bool UseCentreSkillAnnouncment { get; private set; }

    public List<string> DisabledPopups { get; private set; }

    public DisplayModifier(List<string> data)
    {
        DisabledPopups = new List<string>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".disable_halos":
                    DisableHalos = bool.Parse(data[++i].ToLower());
                    break;
                case ".use_centre_skill_announcement":
                    UseCentreSkillAnnouncment = bool.Parse(data[++i].ToLower());
                    break;
                case ".disabled_popup_text_types":
                    while (++i < data.Count && data[i--][0] != '.')
                        DisabledPopups.Add(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in display modifier: " + data[i]);
                    break;
            }
        }
    }
}