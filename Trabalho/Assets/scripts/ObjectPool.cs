using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject prefab;

    private Queue<GameObject> queue;
    private int poolSize;
    private int powerUp =1 ;
    public ObjectPool(GameObject prefab, int poolSize)
    {
        this.prefab = prefab;
        this.poolSize = poolSize;;
        queue = new Queue<GameObject>();

        for (int i = 0; i < this.poolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }

    public GameObject GetFromPool()
    {
        GameObject obj = queue.Peek();
        if (obj.activeSelf)
            return null;

        queue.Dequeue();
        queue.Enqueue(obj);
        BombController bomb = obj.GetComponent<BombController>();
        if (bomb != null)
        {
            bomb.SetPowerUp(powerUp);
            obj = bomb.gameObject;
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
    public void SetPowerUp(int powerUp)
    {
        this.powerUp = powerUp;
    }
}