using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        PLAY,
        WIN,
        LOSE
    }

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (!instance) instance = GameObject.FindObjectOfType<GameManager>();
            return instance;
        }
    }

    //eww gross
    public static int SetMaxX = 4;
    public static int SetMaxY = 4;
    public static int StartingNodeCount = 2;


    public GameObject GridSpotRef;
    public GridLayoutGroup LayoutGroup;
    public RectTransform NodeContainer;

    public Text ScoreText;
    public Text WinText;

    public Node NodeRef;

    public Dictionary<Vector2,GameObject> GridPositions = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, Node> ActiveNodes = new Dictionary<Vector2, Node>();

    public int MaxX;
    public int MaxY;

    public Color[] ColorList = new Color[11];

    private int Score;
    private GameState State;

    IEnumerator Start()
    {
        if(SetMaxX != 0)
        {
            MaxX = SetMaxX;
            MaxY = SetMaxY;
        }

        LayoutGroup.constraintCount = MaxX;

        for (int y = 0; y < MaxY; y++)
        {
            for (int x = 0; x < MaxX; x++)
            {
                AddGridSpot(x, y);
            }
        }

        GridSpotRef.SetActive(false); //should be cleaner but time constraints ya know?

        LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutGroup.GetComponent<RectTransform>());

        for (int i = 0; i < StartingNodeCount; i++)
        {
            yield return new WaitForSeconds(0.05f);
            AddNode();
        }

        AddScore(0);

        yield return null;
    }

    void AddGridSpot(int X, int Y)
    {  
        GameObject GridObject = Instantiate(GridSpotRef, GridSpotRef.transform.parent);
        GridPositions.Add(new Vector2(X, Y), GridObject);
    }

    public void AddNode()
    {
        if (ActiveNodes.Count == GridPositions.Count) return; // no space to add a node

        List<Vector2> AvailablePositions = GridPositions.Keys.ToList();

        for (int i = 0; i < ActiveNodes.Count; i++)
        {
            Vector2 KEY = ActiveNodes.Keys.ToArray()[i];
            if (AvailablePositions.Contains(KEY)) AvailablePositions.Remove(KEY);
        }

        InstantiateNode(AvailablePositions[Random.Range(0, AvailablePositions.Count)]);
    }

    public void InstantiateNode(Vector2 position)
    {
        Node NewNode = (Node)Instantiate(NodeRef, NodeContainer);

        NewNode.GetComponent<RectTransform>().localPosition = GridPositions[position].GetComponent<RectTransform>().localPosition;
        NewNode.NodePosition = position;

        ActiveNodes.Add(position, NewNode);
    }

    //MOVEMENT -----------------------------------------------------------------------------------------------------
    void MoveNodesRight()
    {
        for (int y = 0; y < MaxY; y++)
        {
            int BestMove = MaxX - 1;
            for (int x = BestMove; x >= 0; x--)
            {
                Vector2 KEY = new Vector2(x, y);
                if (ActiveNodes.ContainsKey(KEY))
                {
                    UpdateKey(ref BestMove, KEY, false, true);
                }

            }
        }
    }

    void MoveNodesLeft()
    {
        for (int y = 0; y < MaxY; y++)
        {
            int BestMove = 0;
            for (int x = 0; x < MaxX; x++)
            {
                Vector2 KEY = new Vector2(x, y);
                if (ActiveNodes.ContainsKey(KEY))
                {
                    UpdateKey(ref BestMove, KEY, false, false);
                }
            }
        }
    }

    void MoveNodesDown()
    {
        for (int x = 0; x < MaxX; x++)
        {
            int BestMove = MaxY - 1;
            for (int y = BestMove; y >= 0; y--)
            {
                Vector2 KEY = new Vector2(x, y);
                if (ActiveNodes.ContainsKey(KEY))
                {
                    UpdateKey(ref BestMove, KEY, true, true);
                }

            }
        }
    }
    void MoveNodesUp()
    {
        for (int x = 0; x < MaxX; x++)
        {
            int BestMove = 0;
            for (int y = 0; y < MaxY; y++)
            {
                Vector2 KEY = new Vector2(x, y);


                if (ActiveNodes.ContainsKey(KEY))
                {
                    UpdateKey(ref BestMove, KEY, true, false);
                }
            }
        }
    }
    // -----------------------------------------------------------------------------------------------------------------------

    void UpdateKey(ref int BestMove, Vector2 KEY, bool Vertical, bool Negative)
    {    
        Node CheckingNode = ActiveNodes[KEY];

        float CheckMove = (Vertical) ? CheckingNode.NodePosition.y : CheckingNode.NodePosition.x;
        bool IgnoreMoveBest = false;

        //if (CheckMove != BestMove)
        {
            Vector2 NewKey = (Vertical) ? new Vector2(KEY.x, BestMove) : new Vector2(BestMove, KEY.y);
            Vector2 AdjecentNodePos = Vector2.zero;

            if (!Vertical)
            {
                if(Negative) AdjecentNodePos = new Vector2(NewKey.x + 1, KEY.y);
                else AdjecentNodePos = new Vector2(NewKey.x - 1 , KEY.y);
            }
            else
            {
                if (Negative) AdjecentNodePos = new Vector2 (KEY.x ,NewKey.y + 1);
                else AdjecentNodePos = new Vector2(KEY.x, NewKey.y - 1);
            }

            IgnoreMoveBest = CheckForScore(KEY, NewKey, ref CheckingNode, AdjecentNodePos);
        }

        if (!IgnoreMoveBest) BestMove += (Negative) ? -1 : 1;
    }

    bool CheckForScore(Vector2 KEY, Vector2 NewKEY, ref Node CheckingNode, Vector2 AdjecentNodePos)
    {
        bool NodeKilled = false;

        if (ActiveNodes.ContainsKey(AdjecentNodePos))
        {
            Node AdjecentNode = ActiveNodes[AdjecentNodePos];

            if (CheckingNode.score == AdjecentNode.score)
            {
                AdjecentNode.SetNodeScore();
                CheckingNode.SetNodePhysicalPosition(GridPositions[AdjecentNodePos].GetComponent<RectTransform>().localPosition);
                CheckingNode.Kill = true;
                ActiveNodes.Remove(KEY);

                NodeKilled = true;
            }
        }

        if (!NodeKilled)
        {
            CheckingNode.SetNodePhysicalPosition(GridPositions[NewKEY].GetComponent<RectTransform>().localPosition);
            ActiveNodes.Remove(KEY);
            CheckingNode.NodePosition = NewKEY;
            ActiveNodes.Add(NewKEY, CheckingNode);
        }

        return NodeKilled;
    }


    void CheckForMoves()
    {
        if (ActiveNodes.Count < GridPositions.Count) return; // still space

        for (int i = 0; i < ActiveNodes.Count; i++)
        {
            Node CheckNode = ActiveNodes[ActiveNodes.Keys.ToArray()[i]];
            if(CheckNode.NodePosition.x + 1 < MaxX)
            {
                Vector2 newPos = new Vector2(CheckNode.NodePosition.x + 1, CheckNode.NodePosition.y);
                Node nextNode = ActiveNodes[newPos];
                if (nextNode.score == CheckNode.score) return; // thers an x move to make
            }

            if (CheckNode.NodePosition.y + 1 < MaxY)
            {
                Vector2 newPos = new Vector2(CheckNode.NodePosition.x, CheckNode.NodePosition.y + 1);
                Node nextNode = ActiveNodes[newPos];
                if (nextNode.score == CheckNode.score) return; // thers an x move to make
            }
        }

        State = GameState.LOSE;
        WinText.text = "LOSE";
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (State != GameState.PLAY) return;

        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveNodesRight();
            AddNode();
            CheckForMoves();
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveNodesLeft();
            AddNode();
            CheckForMoves();
        }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveNodesUp();
            AddNode();
            CheckForMoves();
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveNodesDown();
            AddNode();
            CheckForMoves();
        }
    }

    public void Win()
    {
        State = GameState.WIN;
        WinText.text = "WIN!";
    }

    public void AddScore(int score)
    {
        Score += score;

        ScoreText.text = "SCORE:\n" + Score;
    }

    public void RebuildBoard()
    {
        SceneManager.LoadScene(0); // Beauty!
    }
}
