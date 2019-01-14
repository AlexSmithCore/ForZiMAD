using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject {
	
	[Header("Item Settings")]
	new public string name = "New Item";

	public enum Type{
		ore = 0,
		instrument = 1,
	}

	public Type type;
	public Sprite icon = null; 
	public Color itemColor;
	public GameObject prefab;

	public int cost;
}
