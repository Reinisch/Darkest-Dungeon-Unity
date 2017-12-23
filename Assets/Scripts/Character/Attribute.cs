using UnityEngine;

public class SingleAttribute : BaseAttribute
{
    public float ModifiedValue
    {
        get
        {
            if (IsModificationCurrent)
                return ModifiedBaseValue;
            else
            {
                ModifiedBaseValue = (RawValue + FlatAddition) * Multiplier;
                IsModificationCurrent = true;
                return ModifiedBaseValue;
            }
        }
    }

    public SingleAttribute() : base(0)
    {
    }

    public SingleAttribute(float initialValue) : base(initialValue)
    {
    }
}

public class PairedAttribute : BaseAttribute
{
    public float CurrentValue
    {
        get
        {
            if (IsModificationCurrent)
                return currentValue;
            else
            {
                UpdateValue();
                return currentValue;
            }
        }
        set
        {
            if (IsModificationCurrent)
            {
                currentValue = Mathf.Clamp(value, 0, ModifiedBaseValue);
            }
            else
            {
                UpdateValue();
                currentValue = Mathf.Clamp(value, 0, ModifiedBaseValue);
            }
        }
    }

    public float ModifiedValue
    {
        get
        {
            if (IsModificationCurrent)
                return ModifiedBaseValue;
            else
            {
                UpdateValue();
                return ModifiedBaseValue;
            }
        }
    }

    public float ValueRatio
    {
        get
        {
            if (!IsModificationCurrent)
                UpdateValue();

            if (ModifiedBaseValue == 0)
                return 0;

            return currentValue / ModifiedBaseValue;
        }
        set
        {
            if (!IsModificationCurrent)
                UpdateValue();

            value = Mathf.Clamp(value, 0, 1);
            currentValue = value * ModifiedBaseValue;
        }
    }

    private bool PreservePercentage { get; set; }

    private float currentValue;

    public PairedAttribute() : base(0)
    {
        PreservePercentage = true;
    }

    public PairedAttribute(float initialValue,float initialMaxValue, bool preservePercentage) : base(initialMaxValue)
    {
        PreservePercentage = preservePercentage;

        currentValue = initialValue > initialMaxValue ? initialMaxValue : initialValue;
    }

    public void IncreaseValue(float amount)
    {
        if (!IsModificationCurrent)
            UpdateValue();

        currentValue = Mathf.Clamp(currentValue + amount, 0, ModifiedBaseValue);
    }

    public void DecreaseValue(float amount)
    {
        if (!IsModificationCurrent)
            UpdateValue();

        currentValue = Mathf.Clamp(currentValue - amount, 0, ModifiedBaseValue);
    }

    private void UpdateValue()
    {
        float newModifiedValue = (RawValue + FlatAddition) * Multiplier;
        if (ModifiedBaseValue == newModifiedValue)
            return;
        else if (newModifiedValue > ModifiedBaseValue)
        {
            if (PreservePercentage)
            {
                float ratio;
                if (ModifiedBaseValue == 0)
                    ratio = 1;
                else
                    ratio = currentValue / ModifiedBaseValue;
                ModifiedBaseValue = newModifiedValue;
                currentValue = ModifiedBaseValue * ratio;
            }
            else
                ModifiedBaseValue = newModifiedValue;
        }
        else
        {
            if (PreservePercentage)
            {
                float ratio = currentValue / ModifiedBaseValue;
                ModifiedBaseValue = newModifiedValue;
                currentValue = ModifiedBaseValue * ratio;
            }
            else
            {
                ModifiedBaseValue = newModifiedValue;
                if (currentValue > ModifiedBaseValue)
                    currentValue = ModifiedBaseValue;
            }
        }
        IsModificationCurrent = true;
    }
}

public abstract class BaseAttribute
{
    protected float ModifiedBaseValue;

    private float rawValue;
    private float flatAddition;
    private float multiplier;

    public float RawValue
    {
        get { return rawValue; }
        set
        {
            rawValue = value;
            IsModificationCurrent = false;
        }
    }

    public float FlatAddition
    {
        get { return flatAddition; }
        set
        {
            flatAddition = value;
            IsModificationCurrent = false;
        }
    }

    public float Multiplier
    {
        get { return multiplier; }
        set
        {
            multiplier = value;
            IsModificationCurrent = false;
        }
    }

    protected bool IsModificationCurrent;

    protected BaseAttribute(float initialValue)
    {
        rawValue = initialValue;
        flatAddition = 0;
        multiplier = 1;
        ModifiedBaseValue = rawValue;

        IsModificationCurrent = true;
    }
}