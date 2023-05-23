using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{

    [SerializeField] Vector3Int center = Vector3Int.zero;
    [SerializeField] Vector2Int size = Vector2Int.one;
    [SerializeField] float waitTime = 2f;
    [SerializeField] GameObject meteor;


	void Start()
	{
		StartCoroutine(nameof(SpawnMeteors));
	}

    IEnumerator SpawnMeteors()
    {
        while (isActiveAndEnabled)
        {
            Vector3 position = new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.y / 2, size.y / 2));
            GameObject instance = Instantiate(meteor, transform.position + center + position, Quaternion.identity, transform);

            yield return new WaitForSeconds(waitTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + center, new Vector3(size.x, 0, size.y));
    }

}