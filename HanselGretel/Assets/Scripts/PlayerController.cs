using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    [Header("Player Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float interactDistance = 2f;
    
    [SerializeField] private GameObject model;

    private Animator anim;
    
    private Vector3 checkPosition;
    
    private Vector2 moveInput;
    private Rigidbody rb;
    
    public Holdable heldObject;
    public bool isHolding => heldObject != null;
    public Transform holdPosition;
    
    public Material witchMaterial;
    
    public bool death = false;

    private NavMeshAgent agent;
    
    private Coroutine endCoroutine;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        
        GetComponent<NavMeshObstacle>().enabled = true;
        agent.enabled = false;
        
        GameManager.Instance.player = gameObject;
        InputManager.Instance.interact.performed += Interact;
        
        anim = model.GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            CheckDeath();
            return;
        }
        
        moveInput = InputManager.Instance.movement.ReadValue<Vector2>();
        
        print (moveInput);
        Move();
        Rotate();
        ApplyAnimation();
    }
    
    private void Move()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        rb.velocity = moveDirection * speed;
    }
    
    private void Rotate()
    {
        if (moveInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
    
    private void ApplyAnimation()
    {
        if (rb.velocity.magnitude > 0)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }
    
    private void Interact(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isGameOver)
        {
            return;
        }
        
        checkPosition = transform.position + transform.forward * interactDistance;
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.5f);
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject.TryGetComponent(out Interactable interactable))
            {
                interactable.Interact(this);
                break;
            }
        }
        
    }


    public void AddWitchMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            
            for (int i = 0; i < materials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            
            newMaterials[materials.Length] = witchMaterial;
            
            renderer.materials = newMaterials;
        }
    }

    public void RemoveWitchMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length - 1];
            
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            
            renderer.materials = newMaterials;
        }
    }
    
    private void CheckDeath()
    {
        anim.SetBool("isWalking", false);
        
        if (!rb.isKinematic) rb.velocity = Vector3.zero;
        
        if (heldObject != null)
        {
            Destroy(heldObject);
            GameManager.Instance.PlayParticle(holdPosition.position, 0);
        }
        
        if (death)
        {
            //GetComponent<NavMeshObstacle>().enabled = false;
            //agent.enabled = true;
            
            //agent.SetDestination(SpawnManager.Instance.playerDeathPoint.position);
            
            GetComponent<CapsuleCollider>().enabled = false;
            rb.isKinematic = true;
            
            transform.position = Vector3.Lerp(transform.position, GetAveragePosition(SpawnManager.Instance.enemies), Time.deltaTime * 5f);
            
            if (Vector3.Distance(transform.position, SpawnManager.Instance.playerDeathPoint.position) < 1f)
            {
                endCoroutine ??= StartCoroutine(End());
            }
        }
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(2);
        GameManager.Instance.EndGame();
    }

    public Vector3 GetAveragePosition(GameObject[] objs)
    {
        Vector3 average = Vector3.zero;
        int count = 0;
        
        foreach (var obj in objs)
        {
            if (obj == null) continue;
            average += obj.transform.position;
            count++;
        }
        
        return average / count;
    }
}
