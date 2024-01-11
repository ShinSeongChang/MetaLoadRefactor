
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DevelopeSlider : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    Controller_Physics player;

    [SerializeField]
    Slider slider;
    [SerializeField]
    TMP_InputField valueText;

    [SerializeField]
    SliderType type;

    private void Awake()
    {
        if(slider==null)
            slider = GetComponent<Slider>();
        if (valueText == null)
            valueText = GetComponentInChildren<TMP_InputField>();
    }

    private void OnEnable()
    {
        InitValue();
    }

    public void SlideValue()
    {
        if (previousValue != slider.value)
        {
            previousValue = slider.value;
            valueText.text = slider.value.ToString();
            SetValue();
        }
    }
    public void SetValue()
    {
        float.TryParse(valueText.text, out float result);
        slider.value = result;
        player.SetValue(type, result);
    }

    float previousValue = 0;
    public void OnPointerUp(PointerEventData eventData)
    {
        if(previousValue != slider.value) 
        {
            previousValue = slider.value;
            valueText.text = slider.value.ToString();
            SetValue();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
    void InitValue()
    {
        if (player == null)
            player = FindObjectOfType<Controller_Physics>();
        slider.value = player.GetValue(type);
        valueText.text = player.GetValue(type).ToString();
        previousValue = slider.value;
    }

}

public enum SliderType
{
    Move,
    Jump,
    Gravity,
    OneShot,
    repeatShot,
    Grab,
    Range,
    Capacity,
    ObjGravity,
    Speed
}