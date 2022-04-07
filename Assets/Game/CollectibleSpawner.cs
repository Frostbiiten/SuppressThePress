using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolReturner : MonoBehaviour
{
    public IObjectPool<GameObject> pool;

    void OnDisable()
    {
        pool.Release(gameObject);
    }
}

public class CollectibleSpawner : MonoBehaviour
{
    public GameObject collectiblePrefab;
    public PlayerCore player;

    [Header("Pooling")]
    public int poolDefaultSize = 50;
    public int poolMaxSize = 50;
    
    //Pooling
    public IObjectPool<GameObject> p { get; private set; }
    public IObjectPool<GameObject> CollectiblePool
    {
        get
        {
            return p ??= new ObjectPool<GameObject>(CreateCollectible, CollectibleSpawn, CollectibleDespawn,
                CollectibleDestroy, false, poolDefaultSize, poolMaxSize);
        }
    }
    GameObject CreateCollectible()
    {
        GameObject g = Instantiate(collectiblePrefab);
        PoolReturner r = g.AddComponent<PoolReturner>();
        r.pool = CollectiblePool;
        return g;
    }
    void CollectibleDestroy(GameObject go) { Destroy(go); }
    void CollectibleDespawn(GameObject go)
    {
        if (collectParticles)
        {
            collectParticles.transform.position = go.transform.position;
            collectParticles.Play();
        }
    }
    void CollectibleSpawn(GameObject go) { go.gameObject.SetActive(true); }
    
    // Spawning
    [Header("RNG")]
    public float minDepth;
    public float maxDepth;
    public float floorWidth;
    public float spawnFrequency;
    public float distributionDegree;
    public int startSpawnNumber;

    public Vector2 randomSizeRange;
    public Transform collectiblesParent;
    public ParticleSystem collectParticles;
    
    // VFX
    public List<Transform> spawningCollectibles = new List<Transform>();

    bool prespawned = false;
    public void Start()
    {
        for (int i = 0; i < startSpawnNumber; ++i) SpawnCollectible();
        prespawned = true;
        InvokeRepeating(nameof(SpawnCollectible), 0f, spawnFrequency);
    }
    
    public void SpawnCollectible()
    {
        if (prespawned && player.startTime < 0f) return;
        
        float Distributed01 = 1f - Mathf.Pow(Random.Range(0f, 1f), distributionDegree);
        Vector2 position = new Vector2
        (
            Random.Range(-floorWidth, floorWidth), 
            Distributed01 * (maxDepth - minDepth) + minDepth
        );

        GameObject spawned = CollectiblePool.Get();
        spawned.transform.position = position;
        spawned.transform.localScale = Vector3.one * Random.Range(randomSizeRange.x, randomSizeRange.y);
        spawned.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        spawned.transform.parent = collectiblesParent;
        var child = spawned.transform.GetChild(0);
        child.localScale = Vector3.zero;
        spawningCollectibles.Add(child);
    }

    void Update()
    {
        for (int i = 0; i < spawningCollectibles.Count; ++i)
        {
            spawningCollectibles[i].localScale = Vector3.one * Mathf.Lerp(spawningCollectibles[i].localScale.x, 1f, Time.deltaTime * 10f);
            if (spawningCollectibles[i].localScale.x >= 0.95f)
            {
                spawningCollectibles.RemoveAt(i);
                --i;
            }
        }
    }
}
