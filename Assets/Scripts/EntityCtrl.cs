using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class EntityCtrl : MonoBehaviour
{
    
    [Header("UI")]
    [SerializeField] private TMP_Text numText;

    [Header("setting")] [SerializeField] private List<Color> colors;
    public int num;
    public bool hasUp;
    public bool needDestroy;
    public int point => (int)Mathf.Pow(2, num + 1);
    public void SetEntity(int num)
    {
        this.num = num;
        numText.text = Mathf.Pow(2,num+1).ToString();
        var image = this.GetComponent<Image>();
        image.color = colors[num % colors.Count];
    }

    public void Refresh()
    {
        if (needDestroy)
        {
            Destroy();
            return;
        }
        if(hasUp)
        {
            SetEntity(num);
            hasUp = false;
        }
        
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    
    [Button]
    void Test(){SetEntity(num);}
#endif
    
    
    
}
