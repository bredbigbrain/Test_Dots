using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotsController : MonoBehaviour
{
    [System.Serializable]
    public class Data
    {
        public int defaultTurns = 10;
        protected int score, turns, bestScore;

        public int Score
        {
            get => score;
            set { UIController.Instance.SetScore(value); score = value; }
        }

        public int Turns
        {
            get => turns;
            set { UIController.Instance.SetTurns(value); turns = value; }
        }

        public int BestScore
        {
            get => bestScore;
            set { UIController.Instance.SetBestScore(value); bestScore = value; }
        }
    }

    public Dot dotPrefab;

    [SerializeField]
    protected Data gameData;
    [SerializeField]
    protected UnityEngine.Color[] colors;
    [SerializeField]
    protected Grid grid;
    [SerializeField]
    protected DotsConnector connector;

    protected Dot[,] dots;

    public enum GameState { Uninitialized, Initialization, MainLoop, EndGame }

    public GameState State { get; protected set; } = GameState.Uninitialized;

    protected void Awake()
    {
        grid.Init(transform.position);
        connector.Init();
        dots = new Dot[grid.Size, grid.Size];
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        SetState(GameState.Initialization);
    }

    public bool SetState(GameState state)
    {
        StopAllCoroutines();
        switch(state)
        {
            case GameState.Initialization:
                {
                    State = state;
                    StartCoroutine(Init());
                    UIController.Instance.ShowEndGame(false);
                    break;
                }
            case GameState.MainLoop:
                {
                    if (State == GameState.Uninitialized)
                        return false;
                    State = state;
                    StartCoroutine(ProcessPlayerInput());
                    break;
                }
            case GameState.EndGame:
                {
                    State = state;
                    if (gameData.BestScore < gameData.Score)
                    {
                        PlayerPrefs.SetInt("bestScore", gameData.Score);
                        gameData.BestScore = gameData.Score;
                    }
                    UIController.Instance.ShowEndGame(true);
                    ClearLastSessionData();
                    break;
                }
            default: return false;
        }

        return true;
    }

    protected IEnumerator Init()
    {
        Func<int, int, Color> GetColor;
        if (PlayerPrefs.HasKey("score") && PlayerPrefs.HasKey("turns"))
        {
            gameData.Score = PlayerPrefs.GetInt("score");
            gameData.Turns = PlayerPrefs.GetInt("turns");
            
            GetColor = (int x, int y) =>
            {
                if (PlayerPrefs.HasKey($"{x}:{y} R") && PlayerPrefs.HasKey($"{x}:{y} G") && PlayerPrefs.HasKey($"{x}:{y} G"))
                    return new Color(PlayerPrefs.GetFloat($"{x}:{y} R"), PlayerPrefs.GetFloat($"{x}:{y} G"), PlayerPrefs.GetFloat($"{x}:{y} B"), 1);
                return colors[UnityEngine.Random.Range(0, colors.Length)]; ;
            };
        }
        else
        {
            gameData.Score = 0;
            GetColor = (int x, int y) => { return colors[UnityEngine.Random.Range(0, colors.Length)]; };
        }
        gameData.BestScore = PlayerPrefs.GetInt("bestScore", 0);
        if (gameData.Turns <= 0)
            gameData.Turns = gameData.defaultTurns;

        grid.Positions.Foreach(PlaceDot);
        void PlaceDot(int x, int y, Vector3 position)
        {
            if (dots[x, y] == null)
                dots[x, y] = Instantiate<Dot>(dotPrefab, grid.Positions[x, y], Quaternion.identity, transform);
            
            var dot = dots[x, y];
            dot.gameObject.name = $"Dot {(x + 1) * (y + 1)}";
            dot.spriteRenderer.color = GetColor(x, y);
            dot.PlaySpawnAnimation();
            dot.x = x;
            dot.y = y;
            dots[x, y] = dot;
        }

        yield return StartCoroutine(WaitForDotsAnimation());
        SetState(GameState.MainLoop);
    }

    IEnumerator WaitForDotsAnimation(List<Dot> additionalDots = null)
    {
        yield return null;
        while (true)
        {
            bool NotReady(Dot d) { return d != null && !d.IsReady; }
            if (dots.Find(NotReady) != null || additionalDots?.Find(NotReady) != null)
                yield return null;
            else
                break;
        }
    }

    IEnumerator ProcessPlayerInput()
    {
        while (true)
        {
            if (IsPointerDown())
            {
                var hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(GetPointerPosition()));
                if (hit.collider != null)
                {
                    var hitDot = hit.collider.GetComponent<Dot>();
                    connector.AddDot(hitDot);

                    hit.collider.enabled = false;

                    dots.Foreach(DisableInvalidDots);
                    void DisableInvalidDots(int x, int y, Dot dot)
                    {
                        int dx = Mathf.Abs(dot.x - hitDot.x);
                        int dy = Mathf.Abs(dot.y - hitDot.y);
                        bool enabled = dx < 2 && dy < 2 && dx != dy                                                         //in range
                            && hitDot.spriteRenderer.color == dot.spriteRenderer.color                                      //same color
                            && connector.connectedDots.Find((Dot d) => { return d.x == dot.x && d.y == dot.y; }) == null;   //not connected
                        dot.GetComponent<CircleCollider2D>().enabled = enabled;
                    }
                }
                connector.UpdateLine(Camera.main.ScreenToWorldPoint(GetPointerPosition()));
            }
            else if(IsPointerUp())
            {
                if(connector.connectedDots.Count > 1)
                {
                    gameData.Score += connector.connectedDots.Count - 1;
                    --gameData.Turns;
                    yield return StartCoroutine(CollapseDots());
                }

                connector.Clear();
                dots.Foreach((int x, int y, Dot dot) => { dot.GetComponent<CircleCollider2D>().enabled = true; });
            }

            if (gameData.Turns <= 0)
            {
                SetState(GameState.EndGame);
                break;
            }

            yield return null;
        }

        bool IsPointerUp()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonUp(0);
#else
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#endif
        }

        bool IsPointerDown()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0) || Input.GetMouseButton(0);
#else
            return Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved);
#endif
        }

        Vector3 GetPointerPosition()
        {
#if UNITY_EDITOR
            return Input.mousePosition;
#else
            Vector3 position = Vector3.zero;
            if (Input.touchCount > 0)
                position = Input.GetTouch(0).position;
            return position;
#endif
        }
    }

    IEnumerator CollapseDots()
    {
        var dotsToCollapse = new List<Dot>(connector.connectedDots);
        connector.Clear();

        //Play collapse animation
        dotsToCollapse.ForEach((Dot dot) =>
        { 
            dot.PlayCollapseAnimation();
            dots[dot.x, dot.y] = null;
        });
        yield return StartCoroutine(WaitForDotsAnimation(dotsToCollapse));

        //Move dots down
        for(int x = 0; x < grid.Size; ++x)
        {
            int shift = 0;
            for(int y = 0; y < grid.Size; ++y)
            {
                if (dots[x, y] == null)
                    ++shift;
                else if(shift > 0)
                {
                    var dot = dots[x, y];
                    dots[x, y] = null;
                    dots[x, y - shift] = dot;
                    dot.MoveTowards(grid.Positions[x, y - shift]);
                    dot.y = y - shift;
                }
            }
        }
        yield return StartCoroutine(WaitForDotsAnimation(dotsToCollapse));

        //Fill empy positions
        dots.Foreach((int x, int y, Dot dot) =>
        {
            if (dot != null)
                return;
            dot = dotsToCollapse[dotsToCollapse.Count - 1];
            dot.spriteRenderer.color = colors[UnityEngine.Random.Range(0, colors.Length)];
            dot.transform.position = grid.Positions[x, y];
            dot.x = x;
            dot.y = y;
            dots[x, y] = dot;
            dot.PlaySpawnAnimation();
            dotsToCollapse.RemoveAt(dotsToCollapse.Count - 1);
        });
        yield return StartCoroutine(WaitForDotsAnimation());
    }

    protected void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveSessionData();
    }

    protected void OnApplicationQuit()
    {
        SaveSessionData();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            SaveSessionData();
    }

    void SaveSessionData()
    {
        PlayerPrefs.SetInt("score", gameData.Score);
        PlayerPrefs.SetInt("turns", gameData.Turns);

        dots.Foreach(SaveColor);
        void SaveColor(int x, int y, Dot dot)
        {
            if (dot == null)
                return;
            var color = dot.spriteRenderer.color;
            PlayerPrefs.SetFloat($"{x}:{y} R", color.r);
            PlayerPrefs.SetFloat($"{x}:{y} G", color.g);
            PlayerPrefs.SetFloat($"{x}:{y} B", color.b);
        }
    }

    void ClearLastSessionData()
    {
        gameData.BestScore = PlayerPrefs.GetInt("bestScore", 0);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("bestScore", gameData.BestScore);
    }

    private void OnDrawGizmos()
    {
        grid.Init(transform.position);
        grid.Positions.Foreach((int x, int y, Vector3 pos) => { Gizmos.DrawWireSphere(pos, 0.1f); });
    }
}
