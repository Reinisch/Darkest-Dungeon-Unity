using UnityEngine;
using System.Collections.Generic;

public class SkillArtInfo
{
    public string SkillId { get; private set; }
    public string AnimationId { get; private set; }
    public string ArtId { get; private set; }

    public string IconId { get; private set; }
    public bool? CanDisplaySelection { get; private set; }

    public string TargetFx { get; private set; }
    public string TargetChestFx { get; private set; }
    public string TargetHeadFx { get; private set; }

    public Vector2 AreaOffset { get; private set; }
    public Vector2 TargetAreaOffset { get; private set; }

    public SkillArtInfo(List<string> data, bool isMonster)
    {
        LoadData(data, isMonster);
    }

    private void LoadData(List<string> data, bool isMonster)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    SkillId = data[++i];
                    break;
                case ".anim":
                    AnimationId = data[++i];
                    break;
                case ".fx":
                    ArtId = data[++i];
                    break;
                case ".targfx":
                    TargetFx = data[++i];
                    break;
                case ".targchestfx":
                    TargetChestFx = data[++i];
                    break;
                case ".targheadfx":
                    TargetHeadFx = data[++i];
                    break;
                case ".area_pos_offset":
                    AreaOffset = isMonster ?
                        new Vector2(-int.Parse(data[++i]), int.Parse(data[++i])) :
                        new Vector2(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case ".target_area_pos_offset":
                    TargetAreaOffset = new Vector2(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case ".misstargfx":
                case ".reset_source_stance":
                case ".reset_target_stance":
                    ++i;
                    break;
                case ".can_display_selection":
                    CanDisplaySelection = bool.Parse(data[++i]);
                    break;
                case ".icon":
                    IconId = data[++i];
                    break;
                default:
                    Debug.LogError(string.Format("Skill Art unknown token: " + data[i] + " in " + SkillId));
                    break;
            }
        }
    }
}