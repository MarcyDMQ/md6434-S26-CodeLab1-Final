using UnityEngine;

public class coolnewscript : MonoBehaviour
{   

    int point = 0;
    public int increment = 1;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Debug.Log(point);
        // point = point + 33;
        // Debug.Log(point);
    }

    // Update is called once per frame
    void Update()
    {
        point = point + increment;
        Debug.Log(point);
        if (point == 1000) {
            //create a new cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0, 0, 0);
            cube.transform.localScale = new Vector3(1, 1, 1);
            Debug.Log("I'm a thousand!");
        }
        //Debug.Log("Hello World!");
    }
}
