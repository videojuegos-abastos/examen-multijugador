using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent), typeof(MeshRenderer))]
class Player : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI hitsTMP;
	[SerializeField] Material inmortalMaterial;
	[SerializeField] GameObject shield;
    NavMeshAgent agent;
	int lives = 3;
	bool immortal = false;
	bool immortalityUsed = false;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		UpdateLivesUI();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !immortalityUsed && lives > 0)
		{
			Inmortal();
		}

		if (Input.GetKeyDown(KeyCode.Return) && lives > 0)
		{
			CreateShield();
		}
	}

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

	void UpdateLivesUI()
	{
		if (lives >= 0)
		{
			hitsTMP.text = lives.ToString();
		}

		if (lives <= 0)
		{
			hitsTMP.color = Color.red;
		}
	}

	void Inmortal()
	{
		immortalityUsed = true;
		StartCoroutine(nameof(ImmortalCoroutine));
	}

	IEnumerator ImmortalCoroutine()
	{
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		Material previousMaterial = meshRenderer.material;
		meshRenderer.material = inmortalMaterial;

		immortal = true;

		const int INMORTALITY_SECONDS = 5;
		yield return new WaitForSeconds(INMORTALITY_SECONDS);

		meshRenderer.material = previousMaterial;
	
		immortal = false;
	}

	void CreateShield()
	{
		GameObject instance = Instantiate(shield, transform.position, Quaternion.identity);

		lives -= 1;
		UpdateLivesUI();
	}

	void OnCollisionEnter(Collision collision)
    {
		const string METEOR_TAG = "Meteor";
        if (collision.gameObject.tag == METEOR_TAG)
		{
			Destroy(collision.gameObject);

			if (immortal)
			{
				lives += 1;
			}
			else
			{
				lives -= 1;
			}

			UpdateLivesUI();
		}
    }
}