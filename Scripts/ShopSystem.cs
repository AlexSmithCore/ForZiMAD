using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Trade
{
    [System.Serializable]
    public class ItemTabs
    {
        public List<Item> items = new List<Item>();
    }

    [System.Serializable]
    public class AcquiredItems
    {
        public Item item;
        public int count;

        public AcquiredItems(Item newItem, int newCount)
        {
            item = newItem;
            count = newCount;
        }
    }

    public class ShopSystem : MonoBehaviour
    {
        public bool wait;

        [Space]
        [Header("Main")]
        Player.PlayerManager pm;
        [SerializeField]
        private string[] tabNames;

        private float timer;
        [SerializeField]
        private float timeToWait;
        [SerializeField]
        private CoalBagSystem cbs;

        private ContainerSystem[] cs;

        SmoothCameraMovement scm;

        [Header("UI Elements")]

        public Text playerGold;

        public GameObject shopMainPanel;

        public GameObject tabsList;

        public GameObject deliveryList;

        public Text listHeaderText;

        public GameObject acquiredItems;

        public Text oCost;

        public int orderCost;

        [Space]
        [Header("Item Lists")]
        
        public List<AcquiredItems> aItems = new List<AcquiredItems>();

        public ItemTabs[] itemTabs;

        void Awake()
        {
            pm = Player.PlayerManager.instance;
            UpdatePlayerGoldText();

            SwitchTab(0);

            scm = FindObjectOfType<SmoothCameraMovement>();
            cs = FindObjectsOfType<ContainerSystem>();
        }

        public void SwitchTab(int ID){
            DisableAllTabs();

            tabsList.GetComponent<ScrollRect>().content = tabsList.transform.GetChild(ID).GetComponent<RectTransform>();

            tabsList.transform.GetChild(ID).gameObject.SetActive(true);
            listHeaderText.text = tabNames[ID];

            UpdateTab(ID);
        }

        private void DisableAllTabs()
        {
            for (int i = 0; i < tabsList.transform.childCount; i++)
            {
                tabsList.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void UpdateTab(int tabID)
        {
            Transform tab = tabsList.transform.GetChild(tabID);

            for (int i = 0; i < tab.childCount; i++)
            {
                if (i >= itemTabs[tabID].items.Count)
                {
                    tab.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    tab.GetChild(i).gameObject.SetActive(true);
                    UpdateTabUI(tab,tabID,i);
                }
            }

            UpdateDelivery();
        }

        private void UpdateTabUI(Transform tab, int tabID, int i){
            Button button = tab.GetChild(i).GetComponent<Button>();

            button.onClick.RemoveAllListeners();
            Item testItem = itemTabs[tabID].items[i];
            button.onClick.AddListener(delegate { AddItem(testItem, button); });

            tab.GetChild(i).GetChild(0).GetComponent<Text>().text = itemTabs[tabID].items[i].name;
            tab.GetChild(i).GetChild(1).GetComponent<Image>().sprite = itemTabs[tabID].items[i].icon;
            tab.GetChild(i).GetChild(1).GetComponent<Image>().color = itemTabs[tabID].items[i].itemColor;
            tab.GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().text = itemTabs[tabID].items[i].cost + "G";
        }

        public void UpdateDelivery()
        {
            oCost.text = orderCost + "G";

            if (aItems.Count > 0)
            {
                for (int i = 0; i < deliveryList.transform.childCount; i++)
                {
                    if (i >= aItems.Count)
                    {
                        deliveryList.transform.GetChild(i).gameObject.SetActive(false);
                    }
                    else
                    {
                        deliveryList.transform.GetChild(i).gameObject.SetActive(true);

                        //acquiredItems.transform.GetChild(i).GetComponent<Image>().color = aItems[i].item.itemColor;

                        Button button = deliveryList.transform.GetChild(i).GetComponent<Button>();

                        button.onClick.RemoveAllListeners();
                        int id = i;
                        button.onClick.AddListener(delegate { DeleteItem(button, id); });

                        deliveryList.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = "x" + aItems[i].count;
                        deliveryList.transform.GetChild(i).GetChild(1).GetComponent<Image>().sprite = aItems[i].item.icon;
                    }
                }
            }
            else
            {
                DisableAllItems();
            }
        }

        private void DisableAllItems()
        {
            for (int i = 0; i < deliveryList.transform.childCount; i++)
            {
                deliveryList.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void AddItem(Item item, Button b)
        {
            if(pm.gold >= orderCost + item.cost){
                if (!CheckSameItem(item))
                {
                    aItems.Add(new AcquiredItems(item, 1));
                }

                orderCost += item.cost;
            }
            UpdateDelivery();
        }

        public void DeleteItem(Button b, int ID)
        {
            orderCost -= aItems[ID].item.cost;
            if (!CheckSameItemDel(ID))
            {
                aItems.RemoveAt(ID);
            }

            UpdateDelivery();
        }

        private bool CheckSameItem(Item item)
        {
            for (int c = 0; c < aItems.Count; c++)
            {
                if (aItems[c].item == item)
                {
                    aItems[c].count++;
                    return true;
                }
            }
            return false;
        }

        private bool CheckSameItemDel(int id)
        {
            if (aItems[id].count > 1)
            {
                aItems[id].count--;
                return true;
            }
            return false;
        }

        public void SendForMaterials()
        {
            wait = true;
            timer = timeToWait;
            pm.Payment(orderCost);
            UpdatePlayerGoldText();
            Exit();
        }

        private void UpdatePlayerGoldText(){
            playerGold.text = pm.gold + " G";
        }
        
        void LateUpdate()
        {
            if (wait)
            {
                Debug.Log(timer);
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    for (int i = 0; i < aItems.Count; i++)
                    {
                        CheckFreeSpace(i);
                    }
                    wait = false;
                    RemoveAllItems();
                    UpdateTab(0);
                }
            }
        }

        private void CheckFreeSpace(int i){
            for (int c = 0; c <= aItems[i].count; c++)
            {
                if (aItems[i].item.name == "Coal")
                {
                    cbs.coalCount++;
                    continue;
                }
                for (int f = 0; f < cs.Length; f++)
                {
                    if (!cs[f].SearchForFreePlace(aItems[i].item))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void RemoveAllItems()
        {
            for (int i = 0; i < aItems.Count; i++)
            {
                aItems.RemoveAt(i);
            }
            orderCost = 0;
        }

        public void Exit()
        {
            scm.freeze = false;
            shopMainPanel.SetActive(false);
        }

    }
}
