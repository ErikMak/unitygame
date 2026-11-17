using UnityEngine;

[CreateAssetMenu(fileName = "CityDefinition", menuName = "Hanseatic/Cities/City Definition")]
public sealed class CityDefinition : ScriptableObject
{
    [SerializeField] private LocationId location;
    [SerializeField] private string displayName;
    [SerializeField] private EventDefinition firstVisitEvent;
    [SerializeField] private EventDefinition returnVisitEvent;
    [SerializeField] private EventDefinition churchEvent;

    [SerializeField] private GameObject worldModel;

    public GameObject WorldModel => worldModel;
    public LocationId Location => location;
    public string DisplayName => displayName;
    public EventDefinition FirstVisitEvent => firstVisitEvent;
    public EventDefinition ReturnVisitEvent => returnVisitEvent;
    public EventDefinition ChurchEvent => churchEvent;
}
