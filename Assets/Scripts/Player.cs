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
	[SerializeField] Material otherMaterial;
	[SerializeField] GameObject shield;
    NavMeshAgent agent;
	NetworkVariable<int> lives = new NetworkVariable<int>(value: 3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	bool immortal = false;
	bool immortalityUsed = false;

	public override void OnNetworkSpawn()
	{
		agent = GetComponent<NavMeshAgent>();
		UpdateLivesUI(0, lives.Value);

		lives.OnValueChanged += UpdateLivesUI;

		if (IsOwner)
		{
			GetComponent<PointerManager>().onPositionSelected.AddListener( MoveTo );
		}
		else
		{
			GetComponent<MeshRenderer>().material = otherMaterial;
			
			const int OTHER_PLAYER_LAYER = 7;
			gameObject.layer = OTHER_PLAYER_LAYER;
		}
	}

	void Update()
	{

		if (IsOwner)
		{
			if (Input.GetKeyDown(KeyCode.Space) && !immortalityUsed && lives.Value > 0)
			{
				immortalityUsed = true;
				Immortal_ServerRpc();
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

	[ServerRpc]
	void Immortal_ServerRpc()
	{
		Immortal_ClientRpc();
	}

	[ClientRpc]
	void Immortal_ClientRpc()
	{
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

	[ServerRpc]
	void DestroyMeteor_ServerRpc(NetworkObjectReference networkReference, ServerRpcParams parameters = default)
	{
		if (networkReference.TryGet(out NetworkObject networkObject))
		{
			networkObject.Despawn();
		}
	}

	void OnCollisionEnter(Collision collision)
    {
		if (IsOwner)
		{
			const string METEOR_TAG = "Meteor";
			if (collision.gameObject.tag == METEOR_TAG)
			{
				var referenece = new NetworkObjectReference(collision.gameObject.GetComponent<NetworkObject>());
				DestroyMeteor_ServerRpc(referenece);

				collision.gameObject.SetActive(false);

				if (immortal)
				{
					lives.Value += 1;
				}
				else
				{
					lives.Value -= 1;
				}
			}
		
			if (collision.gameObject.TryGetComponent<Shield>(out Shield shield))
			{
				agent.SetDestination(transform.position);
			}
		}	
    }
}