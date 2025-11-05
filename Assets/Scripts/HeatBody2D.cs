using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class HeatBody2D : MonoBehaviour
{
    [Header("Heat Settings")]
    public float temperature = 20f;           // Temperatura actual
    public float thermalConductivity = 0.5f;  // Qué tan rápido transfiere calor
    public float heatCapacity = 1f;           
    public float contactArea = 1f;            
    public float conductionRange = 1.0f;      

    [Header("Color Visual")]
    public Gradient temperatureColor;         // Gradiente para color según temperatura
    public float minTemperature = 0f;         
    public float maxTemperature = 100f;

    [Header("Events")]
    public float meltTemperature = 80f;  
     [Space(10)]     
    public UnityEvent onMelt;
    public float freezeTemperature = 0f;
     [Space(10)]      
    public UnityEvent onFreeze;               


    private SpriteRenderer sprite;
    private bool hasMelted;
    private bool hasFrozen;

    void OnEnable()
    {
        sprite = GetComponent<SpriteRenderer>();
        CustomCollisionManager.RegisterHeatBody(this);
    }

    void OnDisable()
    {
        CustomCollisionManager.UnregisterHeatBody(this);
    }

    void Update()
    {
        UpdateColor();

        // Detectar límites térmicos y activar eventos solo una vez
        if (!hasMelted && temperature >= meltTemperature)
        {
            hasMelted = true;
            onMelt.Invoke();
        }

        if (!hasFrozen && temperature <= freezeTemperature)
        {
            hasFrozen = true;
            onFreeze.Invoke();
        }
    }

    void UpdateColor()
    {
        if (!sprite || temperatureColor == null) return;

        float t = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);
        sprite.color = temperatureColor.Evaluate(t);
    }

    public void TransferHeat(HeatBody2D other, float deltaTime)
    {
        float tempDiff = other.temperature - temperature;
        float heatFlow = thermalConductivity * contactArea * tempDiff * deltaTime;

        temperature += heatFlow / heatCapacity;
        other.temperature -= heatFlow / other.heatCapacity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, conductionRange);
    }
}