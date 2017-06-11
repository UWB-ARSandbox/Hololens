using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTest : MonoBehaviour {

    GameObject target;
    GameObject Cube;
     GameObject ramp;
    GameObject cylinder;
    GameObject target2;
    GameObject arrow;
    GameObject Gear1;
    GameObject Gear2;
    GameObject Gear3;
    GameObject gearTarget;
    Color original;

    bool one; bool scene1;
    bool two ; bool scene2;
    bool three ; bool scene3;
    bool four; bool scene4;
    bool five; bool scene5;
    bool six; bool scene6;
    bool seven; bool scene7;

    void Start()
    {
        target = GameObject.Find("Target");original = new Color(1, 1, 1, 0);
        
        Cube = GameObject.Find("CubeTest");
        cylinder = GameObject.Find("Cylinder");
        ramp = GameObject.Find("Ramp");
        arrow= GameObject.Find("Arrow2");
        Gear1 = GameObject.Find("GearTurn");
        Gear2 = GameObject.Find("GearObject");
        Gear3 = GameObject.Find("GearFollow");
        gearTarget = GameObject.Find("GearTarget");

        cylinder.SetActive(false);
        ramp.SetActive(false);
        
        Gear1.SetActive(false);
        Gear2.SetActive(false);
        Gear3.SetActive(false);
        gearTarget.SetActive(false);

    }

   

    public void Scene1()
    {
        scene1 = true; scene2 = false; scene2 = false;
        Gear1.SetActive(false); Gear2.SetActive(false); Gear3.SetActive(false);
        gearTarget.SetActive(false); Cube.SetActive(true); target.SetActive(true);arrow.SetActive(true);
        target.transform.position = new Vector3(1.6f, 1f, -1.64f);
        Cube.transform.position = new Vector3(1.9f, 0.48f,-3);
        target.transform.rotation = new Quaternion(0, 0, 0, 0);
        target.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }
    public void Scene2()
    {
        scene2 = true; scene1 = false; scene3 = false;
        Cube.SetActive(true); target.SetActive(true);
        target.transform.position = new Vector3(2.016f, 0.337f, -1.191f);
        Cube.transform.position = new Vector3(1.016f, 0.337f, -3f);
        target.transform.Rotate(0,45,0);
        target.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }
    public void Scene3()
    {
        scene3 = true; scene1 = false; scene2 = false;
        Cube.SetActive(true); target.SetActive(true);
        target.transform.position = new Vector3(2.016f, 0.337f, -1.191f);
        Cube.transform.position = new Vector3(1.016f, 0.337f, -3f);
        target.transform.rotation = new Quaternion(0, 0, 0, 0);
        target.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
    }

    //put cube on top of table
    public void Scene4()
    {
        target.SetActive(false);arrow.SetActive(false);
        Cube.transform.position = new Vector3(1.67f,0.49f, -3.191f);
    }
    //put cube on top of table and ramp ontop of cube
    public void Scene5()
    {
        ramp.SetActive(true);
        Cube.transform.position = new Vector3(1.65f, 0.49f, -3.191f);
        ramp.transform.position = new Vector3(2.3f, 0.6f, -3.191f);
    }
    //Put cylinder on ramp to slide
    public void Scene6()
    {
        
        ramp.SetActive(true);
        cylinder.SetActive(true);
        cylinder.transform.position = new Vector3(1.7f, 0.49f, -2.191f);
        cylinder.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }
    //put gear on middle
    public void Scene7()
    {
        ramp.SetActive(false);
        Cube.SetActive(false);
        cylinder.SetActive(false);
        Gear1.SetActive(true); Gear2.SetActive(true); Gear3.SetActive(true);
        gearTarget.SetActive(true);
    }

    void Update()
    {
        arrow.transform.position = target.transform.position; arrow.transform.localScale = target.transform.localScale*2; arrow.transform.rotation = target.transform.rotation;
        one = Input.GetKeyDown(KeyCode.Alpha1);
        two = Input.GetKeyDown(KeyCode.Alpha2);
        three = Input.GetKeyDown(KeyCode.Alpha3);
        four = Input.GetKeyDown(KeyCode.Alpha4);
        five = Input.GetKeyDown(KeyCode.Alpha5);
        six = Input.GetKeyDown(KeyCode.Alpha6);
        seven = Input.GetKeyDown(KeyCode.Alpha7);

        if (one)
        {
            Scene1(); 
        }
        else if (two)
        {
            Scene2(); 
        }
        else if (three)
        {
            Scene3(); 
        }
        else if (four)
        {
            Scene4(); 
        }
        else if (five)
        {
            Scene5(); 
        }
        else if (six)
        {
            Scene6(); 
        }
        else if (seven)
        {
            Scene7(); 
        }

        if (scene1)
        {
            float Distance = Vector3.Distance(target.transform.position, Cube.transform.position);
            if (Distance < 0.1)
            {
                target.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
                target.GetComponent<Renderer>().material.color = original;
        }
        else  if (scene2)
        {
            float Distance = Vector3.Distance(target.transform.localEulerAngles, Cube.transform.localEulerAngles);
           
            if (Distance < 11 )
            {
                target.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                target.GetComponent<Renderer>().material.color = original;
            }
        }
        else if (scene3)
        {
            float Distance = Vector3.Distance(target.transform.localScale, Cube.transform.localScale);
           
            if (Distance < 0.1)
            {
                target.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
                target.GetComponent<Renderer>().material.color = original;
        }
        
            
        
    }
}
