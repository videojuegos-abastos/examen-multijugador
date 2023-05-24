using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent), typeof(MeshRenderer))]
class Player : NetworkBehaviour
{
	[SerializeField] TextMeshProUGUI hitsTMP;
	[SerializeField] Material inmortalMaterial;
	[SerializeField] GameObject shield;
    NavMeshAgent agent;
	NetworkVariable<int> lives = new NetworkVariable<int>(value: 3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	bool immortal = false;
	bool immortalityUsed = false;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		UpdateLivesUI(0, lives.Value);

		lives.OnValueChanged += UpdateLivesUI;

		if (IsOwner)
		{
			GetComponent<PointerManager>().onPositionSelected.AddListener( MoveTo );
		}
	}

	void Update()
	{

		if (IsOwner)
		{
			if (Input.GetKeyDown(KeyCode.Space) && !immortalityUsed && lives.Value > 0)
			{
				Inmortal();
			}

			if (Input.GetKeyDown(KeyCode.Return) && lives.Value > 0)
			{
				lives.Value -= 1;
				CreateShield_ServerRpc();
			}
		}
	}

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

	void UpdateLivesUI(int oldValue, int newValue)
	{
		if (newValue >= 0)
		{
			hitsTMP.text = lives.Value.ToString();
		}

		if (newValue <= 0)
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

	[ServerRpc]
	void CreateShield_ServerRpc(ServerRpcParams parameters = default)
	{
		GameObject instance = Instantiate(shield, transform.position, Quaternion.identity);
		instance.GetComponent<NetworkObject>().SpawnWithOwnership(parameters.Receive.SenderClientId);
	}

	void OnCollisionEnter(Collision collision)
    {
		const string METEOR_TAG = "Meteor";
        if (collision.gameObject.tag == METEOR_TAG)
		{
			Destroy(collision.gameObject);

			if (immortal)
			{
				lives.Value += 1;
			}
			else
			{
				lives.Value -= 1;
			}
		}
    }
}