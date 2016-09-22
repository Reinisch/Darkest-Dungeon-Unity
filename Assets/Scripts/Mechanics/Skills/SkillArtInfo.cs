using UnityEngine;
using System.Collections.Generic;

public class SkillArtInfo
{
    public string SkillId { get; set; }
    public string AnimationId { get; set; }
    public string ArtId { get; set; }

    public string IconId { get; set; }

    public string TargetFX { get; set; }
    public string TargetChestFX { get; set; }
    public string TargetHeadFX { get; set; }
    public string TargetMissFX { get; set; }

    public bool? ResetSourceStance { get; set; }
    public bool? ResetTargetStance { get; set; }
    public bool? CanDisplaySelection { get; set; }

    public Vector2 AreaOffset { get; set; }
    public Vector2 TargetAreaOffset { get; set; }

    public SkillArtInfo(List<string> data, bool isMonster)
    {
        LoadData(data, isMonster);
    }

    public void LoadData(List<string> data, bool isMonster)
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
                    TargetFX = data[++i];
                    break;
                case ".targchestfx":
                    TargetChestFX = data[++i];
                    break;
                case ".targheadfx":
                    TargetHeadFX = data[++i];
                    break;
                case ".misstargfx":
                    TargetMissFX = data[++i];
                    break;
                case ".area_pos_offset":
                    if (isMonster)
                        AreaOffset = new Vector2(-int.Parse(data[++i]), int.Parse(data[++i]));
                    else
                        AreaOffset = new Vector2(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case ".target_area_pos_offset":
                    TargetAreaOffset = new Vector2(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case ".reset_source_stance":
                    ResetSourceStance = bool.Parse(data[++i]);
                    break;
                case ".reset_target_stance":
                    ResetTargetStance = bool.Parse(data[++i]);
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