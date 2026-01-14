using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public CharacterHealth target; 
    public Slider slider;

    private void Start()
    {
        if (target != null && slider != null)
        {
            slider.maxValue = target.maxHealth;
            slider.value = target.currentHealth;
        }
    }

    private void Update()
    {
        if (target != null && slider != null)
        {
            slider.value = target.currentHealth;
        }
    }
}
