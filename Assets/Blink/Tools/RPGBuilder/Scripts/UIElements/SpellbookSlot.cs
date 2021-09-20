using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellbookSlot : MonoBehaviour
{
    public Image icon;
    public RPGSpellbook thisSpellbook;
    
    public void ClickSpellbookSlot()
    {
        SpellbookDisplayManager.Instance.SelectSpellbook(thisSpellbook);
    }
}
