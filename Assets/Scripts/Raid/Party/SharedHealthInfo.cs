using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class SharedHealthInfo : MonoBehaviour
{
    public Animator healthAnimator;
    public HealthBar healthBar;

    public bool IsActive { get; private set; }
    public string SharedId { get; private set; }
    public PairedAttribute Health { get; private set; }
    public List<FormationUnit> SharedUnits { get; private set; }

    void Awake()
    {
        Health = new PairedAttribute(AttributeCategory.CombatStat);
        SharedUnits = new List<FormationUnit>();
    }

    public void Show()
    {
        if (isActiveAndEnabled)
            healthAnimator.SetBool("Hidden", false);
    }
    public void Hide()
    {
        if (isActiveAndEnabled)
            healthAnimator.SetBool("Hidden", true);
    }
    public void SetSelected()
    {
        healthAnimator.SetBool("Selected", true);
    }
    public void SetDeselected()
    {
        healthAnimator.SetBool("Selected", false);
    }

    public void UpdateOverlay()
    {
        healthBar.UpdateHealth(Health);
    }

    public void Initialize(List<FormationUnit> sharedUnits, SharedHealth healthComponent)
    {
        IsActive = true;
        SharedId = healthComponent.SharedId;
        Health.RawValue = sharedUnits.Sum(unit => unit.Character.Health.ModifiedValue);
        Health.ValueRatio = 1;
        SharedUnits.AddRange(sharedUnits);
        for (int i = 0; i < SharedUnits.Count; i++)
            SharedUnits[i].Character[AttributeType.HitPoints, true] = Health;
        Show();
    }
    public void Reset()
    {
        IsActive = false;
        SharedId = null;
        SharedUnits.Clear();
        Hide();
    }
}