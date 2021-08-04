
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteraX : MonoBehaviour
{
    static private AsteraX _S;

    static List<Asteroid>           ASTEROIDS;
    static List<Bullet>             BULLETS;
    static private eGameState       _GAME_STATE = eGameState.mainMenu;
    
	static UnityEngine.UI.Text  	SCORE_GT;
    public static int           	SCORE { get; private set; }
    
    const float MIN_ASTEROID_DIST_FROM_PLAYER_SHIP = 5;
    const float DELAY_BEFORE_RELOADING_SCENE = 4;

	public delegate void CallbackDelegate();
    static public CallbackDelegate GAME_STATE_CHANGE_DELEGATE;
    
	public delegate void CallbackDelegateV3(Vector3 v); 
    
    [System.Flags]
    public enum eGameState
    {
          
        none = 0,       
        mainMenu = 1,   
        preLevel = 2,   
        level = 4,      
        postLevel = 8,  
        gameOver = 16,  
        all = 0xFFFFFFF 
    }

    [Header("Set in Inspector")]
    [Tooltip("This sets the AsteroidsScriptableObject to be used throughout the game.")]
    public AsteroidsScriptableObject asteroidsSO;

    [Header("These reflect static fields and are otherwise unused")]
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
        + "GAME_STATE_CHANGE_DELEGATE whenever GAME_STATE changes.")]
    protected eGameState  _gameState;

    private void Awake()
    {
        S = this;
        
		GAME_STATE_CHANGE_DELEGATE += delegate ()
        { 
            this._gameState = AsteraX.GAME_STATE;
            S._gameState = AsteraX.GAME_STATE;
        };
        
        _gameState = eGameState.mainMenu;
        GAME_STATE = _gameState;
    }

    private void OnDestroy()
    {
        AsteraX.GAME_STATE = AsteraX.eGameState.none;
    }

    void Start()
    {
        ASTEROIDS = new List<Asteroid>();
		AddScore(0);
        
        // Spawn the parent Asteroids, child Asteroids are taken care of by them
        for (int i = 0; i < 3; i++)
        {
            SpawnParentAsteroid(i);
        }
        GAME_STATE = eGameState.level;
    }


    void SpawnParentAsteroid(int i)
    {
        Asteroid ast = Asteroid.SpawnAsteroid();
        ast.gameObject.name = "Asteroid_" + i.ToString("00");
        // Find a good location for the Asteroid to spawn
        Vector3 pos;
        do
        {
            pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
        } while ((pos - PlayerShip.POSITION).magnitude < MIN_ASTEROID_DIST_FROM_PLAYER_SHIP);

        ast.transform.position = pos;
        ast.size = asteroidsSO.initialSize;
    }


    
	public void EndGame()
    {
        GAME_STATE = eGameState.gameOver;
        Invoke("ReloadScene", DELAY_BEFORE_RELOADING_SCENE);
    }

    void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    
    static private AsteraX S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AsteraX:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AsteraX:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public AsteroidsScriptableObject AsteroidsSO
    {
        get
        {
            if (S != null)
            {
                return S.asteroidsSO;
            }
            return null;
        }
    }


    static public eGameState GAME_STATE
    {
        get
        {
            return _GAME_STATE;
        }
        set
        {
            if (value != _GAME_STATE)
            {
                _GAME_STATE = value;
                
                if (GAME_STATE_CHANGE_DELEGATE != null)
                {
                    GAME_STATE_CHANGE_DELEGATE();
                }
            }
        }
    }

    
	static public void AddAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) == -1)
        {
            ASTEROIDS.Add(asteroid);
        }
    }
    static public void RemoveAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) != -1)
        {
            ASTEROIDS.Remove(asteroid);
        }
    }

    
    static public void GameOver()
    {
        _S.EndGame();
    }
    
    
	static public void AddScore(int num)
    {
        if (SCORE_GT == null)
        {
            GameObject go = GameObject.Find("ScoreGT");
            if (go != null)
            {
                SCORE_GT = go.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                Debug.LogError("AsteraX:AddScore() - Could not find a GameObject named ScoreGT.");
                return;
            }
            SCORE = 0;
        }
        
        SCORE += num;

        SCORE_GT.text = SCORE.ToString("N0");
    }


    const int RESPAWN_DIVISIONS = 8;
    const int RESPAWN_AVOID_EDGES = 2; 
    static Vector3[,] RESPAWN_POINTS;
    
    static public IEnumerator FindRespawnPointCoroutine(Vector3 prevPos, CallbackDelegateV3 callback)
    {
        if (RESPAWN_POINTS == null)
        {
            RESPAWN_POINTS = new Vector3[RESPAWN_DIVISIONS + 1, RESPAWN_DIVISIONS + 1];
            Bounds playAreaBounds = ScreenBounds.BOUNDS;
            float dX = playAreaBounds.size.x / RESPAWN_DIVISIONS;
            float dY = playAreaBounds.size.y / RESPAWN_DIVISIONS;
            for (int i = 0; i <= RESPAWN_DIVISIONS; i++)
            {
                for (int j = 0; j <= RESPAWN_DIVISIONS; j++)
                {
                    RESPAWN_POINTS[i, j] = new Vector3(
                        playAreaBounds.min.x + i * dX,
                        playAreaBounds.min.y + j * dY,
                        0);
                }
            }
        }

        // Wait a few seconds before choosing the nextPos
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.8f);

        float distSqr, closestDistSqr = float.MaxValue;
        int prevI = 0, prevJ = 0;

        // Check points against prevPos (avoiding edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                distSqr = (RESPAWN_POINTS[i, j] - prevPos).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    prevI = i;
                    prevJ = j;
                }
            }
        }

        float furthestDistSqr = 0;
        Vector3 nextPos = prevPos;
        
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                if (i == prevI && j == prevJ)
                {
                    continue;
                }
                closestDistSqr = float.MaxValue;
                // Find distance to the closest Asteroid
                for (int k = 0; k < ASTEROIDS.Count; k++)
                {
                    distSqr = (ASTEROIDS[k].transform.position - RESPAWN_POINTS[i, j]).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                    }
                }
                
                if (closestDistSqr > furthestDistSqr)
                {
                    furthestDistSqr = closestDistSqr;
                    nextPos = RESPAWN_POINTS[i, j];
                }
            }
        }
        
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.2f);
        
        callback(nextPos);
    }

}
