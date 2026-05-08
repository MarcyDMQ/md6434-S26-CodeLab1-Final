using UnityEngine;
using UnityEngine.AI;
public class followBall : MonoBehaviour
{
    public Transform hero;
    private NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //create navmesh agent component and assign it to the variable agent
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(hero.position);
    }
}
