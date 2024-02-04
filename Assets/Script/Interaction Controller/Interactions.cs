using System.Collections;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Interactions : MonoBehaviour
{

    [SerializeField] private Transform _interactPoint;
    [SerializeField] private float _interactPointRadius;
    [SerializeField] private LayerMask _interactLayerMask;

    private readonly Collider[] _colliders = new Collider[1];
    [SerializeField] private int _numFound;
    [SerializeField] public GameObject buttonInteract;

    [Header("Get Component")]
    MainCharMovement mainChar;
    public Inventory inventory;

    [Header("Test")]
    public Sprite imgTest;

    // Start is called before the first frame update
    void Start()
    {
        mainChar = GetComponent<MainCharMovement>();
        buttonInteract.SetActive(false);

        inventory.ItemAdded += InventoryScript_ItemAdded;
        inventory.ItemRemoved += InventoryScript_ItemRemoved;
    }

    // Update is called once per frame
    void Update()
    {
        _numFound = Physics.OverlapSphereNonAlloc(_interactPoint.position, _interactPointRadius, _colliders, _interactLayerMask);
        Transform btnTransform = transform;
        TextMeshProUGUI btnConTxt = btnTransform.GetComponent<TextMeshProUGUI>();

        if (_numFound > 0)
        {
            buttonInteract.SetActive(true);
            btnConTxt.text = "E";
            
        } else
        {
            buttonInteract.SetActive(false);
            btnConTxt.text = "";
        }
    }

    public void buttonCondition()
    {
        Transform btnTransform = transform;
        TextMeshPro btnConTxt = btnTransform.GetComponent<TextMeshPro>();
        if (btnConTxt.text == "E")
        {
            removeItem();
        }
        else if (btnConTxt.text == "A")
        {

        }
    }


    public void removeItem()
    {
        if (_numFound == 1)
        {
            var interactableItem = _colliders[0].GetComponent<Interactable>();
            IInventoryItem item = _colliders[0].GetComponent<IInventoryItem>();

            if (interactableItem != null)
            {
                interactableItem.Interact(this);
            }

            if (item != null)
            {
                inventory.AddItem(item);
            }
        }
    }

    private void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)
    {
        GameObject otherGameObject = GameObject.Find("Screen");
        Transform inventoryPanel = otherGameObject.transform.Find("Inventory");

        foreach (Transform slot in inventoryPanel)
        {
            // Check if the slot has any children
            if (slot.childCount > 0)
            {
                Transform imageTransform = slot.GetChild(0).GetChild(0);
                Image image = imageTransform.GetComponent<Image>();
                ItemDragHandler itemDragHandler = imageTransform.GetComponent<ItemDragHandler>();
                InventoryVariable inventoryVariable = imageTransform.GetComponent<InventoryVariable>();

                if (!image.enabled)
                {
                    image.enabled = true;
                    image.sprite = e.Item.image;

                    inventoryVariable.jenisSampah = e.Item.jenisSampah;

                    itemDragHandler.Item = e.Item;

                    if (mainChar != null)
                    {
                        mainChar.cubeVal++;
                    }

                    break;
                }
            }
        }
    }

    private void InventoryScript_ItemRemoved(object sender, InventoryEventArgs e)
    {
        GameObject otherGameObject = GameObject.Find("Screen");
        Transform inventoryPanel = otherGameObject.transform.Find("Inventory");

        foreach (Transform slot in inventoryPanel)
        {
            Transform imageTransform = slot.GetChild(0).GetChild(0);
            Image image = imageTransform.GetComponent<Image>();
            ItemDragHandler itemDragHandler = imageTransform.GetComponent<ItemDragHandler>();

            if (itemDragHandler.Item.Equals(e.Item))
            {
                image.enabled = false;
                image.sprite = null;
                itemDragHandler.Item = null;
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactPoint.position, _interactPointRadius);
    }
}
