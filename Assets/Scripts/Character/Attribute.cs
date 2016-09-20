using UnityEngine;
using System.Collections;

public class SingleAttribute : BaseAttribute
{
    public SingleAttribute(AttributeCategory attributeCategory)
        : base(0, attributeCategory)
    {

    }
    public SingleAttribute(float initialValue, AttributeCategory attributeCategory)
        : base(initialValue, attributeCategory)
    {

    }

    public float ModifiedValue
    {
        get
        {
            if (isModificationCurrent)
                return modifiedValue;
            else
            {
                modifiedValue = (RawValue + FlatAddition) * Multiplier + FinalFlatAddition;
                isModificationCurrent = true;
                return modifiedValue;
            }
        }        
    }
}
public class PairedAttribute : BaseAttribute
{
    float currentValue;

    public bool PreservePercentage { get; set; }

    public PairedAttribute(AttributeCategory attributeCategory)
        :base(0, attributeCategory)
    {
        PreservePercentage = true;
    }

    public PairedAttribute(float initialValue,float initialMaxValue, bool preservePercentage, AttributeCategory attributeCategory)
        :base(initialMaxValue, attributeCategory)
    {
        PreservePercentage = preservePercentage;

        if (initialValue > initialMaxValue)
            currentValue = initialMaxValue;
        else
            currentValue = initialValue;
    }
    
    void UpdateValue()
    {
        float newModifiedValue = (RawValue + FlatAddition) * Multiplier + FinalFlatAddition;
        if (modifiedValue == newModifiedValue)
            return;
        else if (newModifiedValue > modifiedValue)
        {
            if (PreservePercentage)
            {
                float ratio;
                if (modifiedValue == 0)
                    ratio = 1;
                else
                    ratio = currentValue / modifiedValue;
                modifiedValue = newModifiedValue;
                currentValue = modifiedValue * ratio;
            }
            else
                modifiedValue = newModifiedValue;
        }
        else
        {
            if (PreservePercentage)
            {
                float ratio = currentValue / modifiedValue;
                modifiedValue = newModifiedValue;
                currentValue = modifiedValue * ratio;
            }
            else
            {
                modifiedValue = newModifiedValue;
                if (currentValue > modifiedValue)
                    currentValue = modifiedValue;
            }
        }
        isModificationCurrent = true;
    }

    public void IncreaseValue(float amount)
    {
        if (!isModificationCurrent)
            UpdateValue();

        currentValue = Mathf.Clamp(currentValue + amount, 0, modifiedValue);
    }
    public void DecreaseValue(float amount)
    {
        if (!isModificationCurrent)
            UpdateValue();

        currentValue = Mathf.Clamp(currentValue - amount, 0, modifiedValue);
    }

    public float CurrentValue
    {
        get
        {
            if(isModificationCurrent)
                return currentValue;
            else
            {
                UpdateValue();
                return currentValue;
            }
        }
        set
        {
            if (isModificationCurrent)
            {
                currentValue = Mathf.Clamp(value, 0, modifiedValue);
            }
            else
            {
                UpdateValue();
                currentValue = Mathf.Clamp(value, 0, modifiedValue);
            }
        }
    }
    public float ModifiedValue
    {
        get
        {
            if (isModificationCurrent)
                return modifiedValue;
            else
            {
                UpdateValue();
                return modifiedValue;
            }
        }        
    }
    public float ValueRatio
    {
        get
        {
            if (!isModificationCurrent)
                UpdateValue();

            if (modifiedValue == 0)
                return 0;

            return currentValue / modifiedValue;
        }
        set
        {
            if (!isModificationCurrent)
                UpdateValue();

            value = Mathf.Clamp(value, 0, 1);
            currentValue = value * modifiedValue;
        }
    }
}

public abstract class BaseAttribute
{
    public AttributeCategory AttributeCategory { get; set; }

    protected float rawValue;
    protected float flatAddition;
    protected float multiplier;
    protected float finalFlatAddition;
    protected float modifiedValue;

    public float RawValue
    { 
        get { return rawValue; }
        set { rawValue = value; isModificationCurrent = false; }
    }
    public float FlatAddition
    {
        get { return flatAddition; }
        set { flatAddition = value; isModificationCurrent = false; }
    }
    public float Multiplier
    {
        get { return multiplier; }
        set { multiplier = value; isModificationCurrent = false; }
    }
    public float FinalFlatAddition
    {
        get { return finalFlatAddition; }
        set { finalFlatAddition = value; isModificationCurrent = false; }
    }

    protected bool isModificationCurrent;

    public BaseAttribute(float initialValue, AttributeCategory attributeCategory)
    {
        AttributeCategory = attributeCategory;
        rawValue = initialValue;
        flatAddition = 0;
        multiplier = 1;
        finalFlatAddition = 0;
        modifiedValue = rawValue;

        isModificationCurrent = true;
    }

}
