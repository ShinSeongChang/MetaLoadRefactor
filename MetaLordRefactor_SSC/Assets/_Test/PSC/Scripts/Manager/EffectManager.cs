using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField, Range(5, 100)]
    int poolSize=5;

    public static EffectManager instance;
    public EffectObject[] effects;

    private Dictionary<EffectList, Stack<EffectObject>> effectPool;
    private Stack<EffectObject> muzzleEffectPool;
    private Stack<EffectObject> explosionEffectPool;
    private Stack<EffectObject> smallCoinEffectPool;
    private Stack<EffectObject> bigCoinEffectPool;
    private Stack<EffectObject> recordEffectPool;

    private GameObject usedObjects;
    private GameObject unusedObjects;

    private void Awake()
    {
        instance = this;

        muzzleEffectPool = new Stack<EffectObject>();
        explosionEffectPool = new Stack<EffectObject>();
        smallCoinEffectPool = new Stack<EffectObject>();
        bigCoinEffectPool = new Stack<EffectObject>();
        recordEffectPool = new Stack<EffectObject>();
        effectPool = new Dictionary<EffectList, Stack<EffectObject>>
        {
            { EffectList.GunMuzzle, muzzleEffectPool },
            { EffectList.GunExplosion, explosionEffectPool },
            { EffectList.SmallStarGet, smallCoinEffectPool },
            { EffectList.BigStarGet, bigCoinEffectPool },
            { EffectList.ItemGet, recordEffectPool }
        };

        usedObjects = new GameObject("used");
        unusedObjects = new GameObject("unused");

        InitEffectPool(EffectList.GunMuzzle);
        InitEffectPool(EffectList.GunExplosion);
        InitEffectPool(EffectList.SmallStarGet);
        InitEffectPool(EffectList.BigStarGet);
        InitEffectPool(EffectList.ItemGet);
    }

    private void InitEffectPool(EffectList effect)
    {
        for (int i = 0; i < poolSize; i++)
        {
            EffectObject effectObj = Instantiate(effects[(int)effect]);
            effectObj.gameObject.SetActive(false);
            effectObj.transform.SetParent(usedObjects.transform);
            effectPool[effect].Push(effectObj);
        }
    }


    public EffectObject GetEffect(EffectList effect)
    {
        if (effectPool[effect].Count == 0) 
        { 
            InitEffectPool(effect);
        }
        EffectObject effectObj = effectPool[effect].Pop();
        effectObj.transform.SetParent(null);
        effectObj.gameObject.SetActive(true);
        return effectObj;

    }
    public void PlayEffect(EffectList effect, Vector3 position , Quaternion rotate , Transform parent = null)
    {
        if (effectPool[effect].Count == 0)
        {
            InitEffectPool(effect);
        }
        EffectObject effectObj = effectPool[effect].Pop();
        effectObj.transform.SetParent(null);
        effectObj.gameObject.SetActive(true);
        effectObj.transform.position = position;
        effectObj.transform.rotation = rotate;
        effectObj.transform.parent = parent;
        effectObj.Play();

    }
    public void PlayEffect(EffectList effect, Vector3 position, Vector3 forward, Transform parent = null)
    {
        if (effectPool[effect].Count == 0)
        {
            InitEffectPool(effect);
        }
        EffectObject effectObj = effectPool[effect].Pop();
        effectObj.transform.SetParent(null);
        effectObj.gameObject.SetActive(true);
        effectObj.transform.position = position;
        effectObj.transform.forward = forward;
        effectObj.transform.parent = parent;
        effectObj.Play();

    }

    public void ReturnEffectPool(EffectList effect, EffectObject effectObj)
    {
        effectObj.gameObject.SetActive(false);
        effectObj.transform.SetParent(usedObjects.transform);
        effectPool[effect].Push(effectObj);
    }
}

public enum EffectList
{
    SmallStarGet=0,
    BigStarGet,
    ItemGet,
    GunMuzzle,
    GunExplosion
}
