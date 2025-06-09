using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GoodsContentController : MonoBehaviour
{
    public Image texture;
    //<name> | <current_total>/存货<total_storage>\n单价<price>金 | 合计<total_price>金币
    public string item_name;
    public TextMeshProUGUI description;
    public Collider2D collider_upper;
    public Collider2D collider_lower;
    int current_total = 0;
    int total_storage = 0;
    int price = 0;
    int total_price = 0;

    public void Init(ItemManager.Item item, int price, int storage)
    {
        this.texture.sprite = item.texture;
        this.price = price;
        this.item_name = item.name;
        this.total_storage = storage;

        UpdateText();
    }

    private void UpdateText()
    {
        total_price = current_total * price;
        description.text = item_name + " | " + current_total.ToString() + "/存货" + total_storage + 
                                "\n单价" + price.ToString() + "金 | " + total_price.ToString() + "金币";  
    }

    public void Add()
    {
        if (current_total == total_storage)
            return;
        if (current_total > total_storage){
            current_total = total_storage - 1;
        }

        current_total += 1;
        UpdateText();
    }
    public void Minus()
    {
        current_total -= 1;
        //NumberClamp from StorageManager
        UpdateText();
    }
}
