using UnityEngine;
using UnityEngine.Events;

internal enum Choice
{
    OneTime, AutoReverse, Loop
}

[System.Serializable]
public class OnValueChangedEvent : UnityEvent<float> { }

public class AutomaticSlider : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 1;

    [SerializeField] private bool smoothStep = false;

    [SerializeField] private Choice type = Choice.OneTime;
    
    [SerializeField] private OnValueChangedEvent onValueChanged;

    public bool AutoReverse
    {
        get => type == Choice.AutoReverse;
        set => type = Choice.AutoReverse;
    }
    
    public bool Reversed { get; set; }
    
    private float value;
    
    private float SmoothedValue => 3f * value * value - 2f * value * value * value;

    private void FixedUpdate()
    {
        ComputeValue();
    }

    private void ComputeValue()
    {
        float delta = Time.deltaTime / duration;

        if (Reversed)
        {
            value -= delta;

            HandleUnderflow();
        }
        else
        {
            value += delta;

            HandleOverflow();
        }

        onValueChanged.Invoke(smoothStep ? SmoothedValue : value);
    }

    private void HandleOverflow()
    {
        if (!(value >= 1f)) return;

        switch (type)
        {
            case Choice.AutoReverse:
                value = Mathf.Max(0f, 2f - value);
                Reversed = true;
                break;
            case Choice.Loop:
                value = 0f;
                break;
            default:
                value = 1f;
                enabled = false;
                break;
        }
    }

    private void HandleUnderflow()
    {
        if (!(value <= 0f)) return;
        
        
        if (type == Choice.AutoReverse)
        {
            value = Mathf.Min(1f, -value);
            Reversed = false;
        }
        else
        {
            value = 0f;
            enabled = false;
        }
    }
}
