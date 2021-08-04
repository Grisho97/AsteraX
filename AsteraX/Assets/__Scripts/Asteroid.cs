// These were used to test a case where some Asteroids were getting lost off screen.
//#define DEBUG_Asteroid_TestOOBVel 
//#define DEBUG_Asteroid_ShotOffscreenDebugLines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_Asteroid_TestOOBVel
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Asteroid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int          size = 3;

    Rigidbody           rigid; 
    OffScreenWrapper    offScreenWrapper;
    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        offScreenWrapper = GetComponent<OffScreenWrapper>();
    }
    
    void Start()
    {
        AsteraX.AddAsteroid(this);

        transform.localScale = Vector3.one * size * AsteraX.AsteroidsSO.asteroidScale;
        if (parentIsAsteroid)
        {
            InitAsteroidChild();
        }
        else
        {
            InitAsteroidParent();
        }

        // Spawn child Asteroids
        if (size > 1)
        {
            Asteroid ast;
            for (int i = 0; i < AsteraX.AsteroidsSO.numSmallerAsteroidsToSpawn; i++)
            {
                ast = SpawnAsteroid();
                ast.size = size - 1;
                ast.transform.SetParent(transform);
                Vector3 relPos = Random.onUnitSphere / 2;
                ast.transform.rotation = Random.rotation;
                ast.transform.localPosition = relPos;

                ast.gameObject.name = gameObject.name + "_" + i.ToString("00");
            }
        }
    }

    private void OnDestroy()
    {
        AsteraX.RemoveAsteroid(this);
    }

    public void InitAsteroidParent()
    {
        offScreenWrapper.enabled = true;
        rigid.isKinematic = false;
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        InitVelocity();
    }

    public void InitAsteroidChild()
    {
        offScreenWrapper.enabled = false;
        rigid.isKinematic = true;
        transform.localScale = transform.localScale.ComponentDivide(transform.parent.lossyScale);
    }

    public void InitVelocity()
    {
        Vector3 vel;
        
        if (ScreenBounds.OOB(transform.position))
        {
            vel = ((Vector3)Random.insideUnitCircle * 4) - transform.position;
            vel.Normalize();
        }
        else
        { 
            do
            {
                vel = Random.insideUnitCircle;
                vel.Normalize();
            } while (Mathf.Approximately(vel.magnitude, 0f));
        }

        vel = vel * Random.Range(AsteraX.AsteroidsSO.minVel, AsteraX.AsteroidsSO.maxVel) / (float)size;
        rigid.velocity = vel;

        rigid.angularVelocity = Random.insideUnitSphere * AsteraX.AsteroidsSO.maxAngularVel;
    }
    
    bool parentIsAsteroid
    {
        get
        {
            return (parentAsteroid != null);
        }
    }

    Asteroid parentAsteroid
    {
        get
        {
            if (transform.parent != null)
            {
                Asteroid parentAsteroid = transform.parent.GetComponent<Asteroid>();
                if (parentAsteroid != null)
                {
                    return parentAsteroid;
                }
            }
            return null;
        }
    }

    public void OnCollisionEnter(Collision coll)
    {
        // If this is the child of another Asteroid, pass this collision up the chain
        if (parentIsAsteroid)
        {
            parentAsteroid.OnCollisionEnter(coll);
            return;
        }

        GameObject otherGO = coll.gameObject;

        if (otherGO.tag == "Bullet" || otherGO.transform.root.gameObject.tag == "Player")
        {
            if (otherGO.tag == "Bullet")
            {
                Destroy(otherGO);
                AsteraX.AddScore( AsteraX.AsteroidsSO.pointsForAsteroidSize[size] );
            }

            if (size > 1)
            {
                // Detach the children Asteroids
                Asteroid[] children = GetComponentsInChildren<Asteroid>();
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] == this || children[i].transform.parent != transform)
                    {
                        continue;
                    }
                    children[i].transform.SetParent(null, true);
                    children[i].InitAsteroidParent();
                }
            }

            Destroy(gameObject);
        }
    }

    static public Asteroid SpawnAsteroid()
    {
        GameObject aGO = Instantiate<GameObject>(AsteraX.AsteroidsSO.GetAsteroidPrefab());
        Asteroid ast = aGO.GetComponent<Asteroid>();
        return ast;
    }
}
