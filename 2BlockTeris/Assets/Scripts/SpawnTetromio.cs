using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnTetromio : Singleton<SpawnTetromio>
{
    public CameraShake camShake;
    public float falltime = 0.8f;
    float nextFall = 0.8f;
    float fastCoolDown = 1f;
    float previousTime;
    Transform spawnPoint1;
    Transform spawnPoint2;
    bool falled1 = false;
    bool falled2 = false;
    TerisBlock teris1;
    TerisBlock teris2;

    bool waitingForNextRound = false;

    public static int height = 20;
    public static int width = 10;

    static Transform[,] grid = new Transform[width, height];

    List<TerisItem> items = new List<TerisItem>();
    float[] itemChances;

    TerisItem next1;
    TerisItem next2;

    public Transform alltiles;
    // Start is called before the first frame update
    void Start()
    {
        InitTeris();
        spawnPoint1 = transform.Find("SpawnPoint1");
        spawnPoint2 = transform.Find("SpawnPoint2");

        Sound.Instance.PlayBg("背景音乐1");
        Game.Instance.gameOver = false;
        RandomNextTeris();
        Invoke("NewTetromino", 2f);// NewTetromino();
        //GameEvents.Instance.onTetrominosFalled += CheckTwoTerisFalled;
    }

    void InitTeris()
    {
        TerisItem I = new TerisItem("Images/I", "Prefabs/I",1);
        TerisItem T = new TerisItem("Images/T", "Prefabs/T",1);
        TerisItem J = new TerisItem("Images/J", "Prefabs/J",1);
        TerisItem L = new TerisItem("Images/L", "Prefabs/L",1);
        TerisItem O = new TerisItem("Images/O", "Prefabs/O",1);
        TerisItem S = new TerisItem("Images/S", "Prefabs/S",1);
        TerisItem Z = new TerisItem("Images/Z", "Prefabs/Z",1);
        TerisItem Bomb = new TerisItem("Images/BOMB", "Prefabs/BOMB",1);

        TerisItem C = new TerisItem("Images/C", "Prefabs/C",3);
        TerisItem Y = new TerisItem("Images/Y", "Prefabs/Y",3);

        items.Add(I);
        items.Add(T);
        items.Add(J);
        items.Add(L);
        items.Add(O);
        items.Add(S);
        items.Add(Z);
        items.Add(Bomb);
        items.Add(C);
        items.Add(Y);
        itemChances = new float[items.Count - 1];
        for(int i = 0; i < items.Count-1; i++)
        {
            itemChances[i] = items[i].Chance;
        }

    }
    public int RandomNumber(float[] pros)
    {
        float total = 0f;
        foreach (float elem in pros)
        {
            total += elem;
        }
        float randomPoint = Random.value * total;
        for (int i = 0; i < pros.Length; i++)
        {
            if (randomPoint < pros[i])
            {
                return i;
            }
            else
            {
                randomPoint -= pros[i];
            }
        }
        return pros.Length - 1;

    }

    void SetGridStatic()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                grid[i, j] = null;
            }
        }

        Transform[] allStatics = alltiles.GetComponentsInChildren<Transform>();
        foreach (Transform child in allStatics)
        {
            if (child.CompareTag("Block"))
            {
                int roundedX = Mathf.RoundToInt(child.position.x);
                int roundedY = Mathf.RoundToInt(child.position.y);
                grid[roundedX, roundedY] = child;
            }


        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                if (grid[i, j] != null)
                {
                    Gizmos.DrawCube(grid[i, j].position, new Vector3(0.2f, 0.2f, 0.2f));
                }
            }
        }
    }

 

    void RandomNextTeris()
    {
        next1 = items[RandomNumber(itemChances)];
        next2 = items[RandomNumber(itemChances)];
        //next1 = items[Random.Range(0, items.Count)];
        //next2 = items[Random.Range(0, items.Count)];
        UImanager.Instance.ShowNextTeris(next1, next2);
    }

    public void NewTetromino()
    {
        if (Game.Instance.gameOver)
            return;

        SetGridStatic();
        falled1 = false;
        falled2 = false;

        GameObject go1 = Instantiate(Resources.Load<GameObject>(next1.prefabPath), spawnPoint1.position, Quaternion.identity);
        teris1 = go1.GetComponent<TerisBlock>();
        teris1.ID = 1;
        GameObject go2 = Instantiate(Resources.Load<GameObject>(next2.prefabPath), spawnPoint2.position, Quaternion.identity);
        teris2 = go2.GetComponent<TerisBlock>();
        teris2.ID = 2;
        waitingForNextRound = false;

        RandomNextTeris();
    }

    void CheckTwoTerisFalled()
    {
        if (falled1 && falled2)
        {

            //waitingForNextRound = false;
            //NewTetromino();
            StartCoroutine(StartNewRound());
        }

    }

    IEnumerator StartNewRound()
    {
        waitingForNextRound = true;
        yield return new WaitForSeconds(0.01f);
        teris1 = null;
        teris2 = null;
        NewTetromino();
    }
    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.gameOver)
            return;
        
        if (!waitingForNextRound)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                teris1.Move(new Vector3(-1, 0, 0));
                teris2.Move(new Vector3(-1, 0, 0));
                int valid = ValidMove();
                if (valid != 1)
                {
                    Sound.Instance.PlayEffect("移动");
                }
                MoveBack(valid, false);

            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {

                teris1.Move(new Vector3(1, 0, 0));
                teris2.Move(new Vector3(1, 0, 0));
                int valid = ValidMove();
                if (valid != 1)
                {
                    Sound.Instance.PlayEffect("移动");
                }
                MoveBack(valid, false);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                teris1.Rot();
                teris2.Rot();
                int valid = ValidMove();
                bool rotable = teris1.CheckRotClash(teris2, false);
                if (valid != 1 && rotable)
                {
                    Sound.Instance.PlayEffect("旋转");
                }
                RotBack(ValidMove());

            }

            
            if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
            {
                nextFall = 0.08f;

            }
            else
            {
                nextFall = falltime;
            }

            if (Time.time - previousTime > nextFall)
            {

                teris1.Move(new Vector3(0, -1, 0));

                teris2.Move(new Vector3(0, -1, 0));
                int valid = ValidMove();

                MoveBack(valid, true);

                previousTime = Time.time;
            }
        }
    }



    void RotBack(int situation)
    {
        switch (situation)
        {
            case 0:
                break;
            case 1:
                teris1.RotBack();
                teris2.RotBack();

                break;
            case 2:
                teris2.RotBack();
                teris2.CheckRotClash(teris1, true);

                break;
            case 3:
                teris1.RotBack();
                teris1.CheckRotClash(teris2, true);
                break;
        }
    }


    void MoveBack(int situation, bool isDown)
    {

        switch (situation)
        {
            case 0:
                break;
            case 1:
                teris1.MoveBack();
                teris2.MoveBack();
                if (isDown)
                {
                    AddToGrid(teris1, teris2);
                }
                break;
            case 2:
                teris2.MoveBack();
                teris2.CheckClash(teris1);
                if (isDown)
                    AddToGrid(teris2);
                break;
            case 3:
                teris1.MoveBack();
                teris1.CheckClash(teris2);
                if (isDown)
                    AddToGrid(teris1);
                break;
        }
    }
    int ValidMove()
    {

        bool t1CanMove = true;
        bool t2CanMove = true;
        foreach (Transform children in teris1.transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height || grid[roundedX, roundedY] != null)
            {
                t1CanMove = false;
                break;
            }

        }
        foreach (Transform children in teris2.transform)
        {
            int roundedX2 = Mathf.RoundToInt(children.transform.position.x);
            int roundedY2 = Mathf.RoundToInt(children.transform.position.y);
            if (roundedX2 < 0 || roundedX2 >= width || roundedY2 < 0 || roundedY2 >= height || grid[roundedX2, roundedY2] != null)
            {
                t2CanMove = false;
                break;
            }

        }
        if (t1CanMove && t2CanMove)
            return 0;
        else if (!t1CanMove && !t2CanMove)
            return 1;
        else if (t1CanMove && !t2CanMove)
            return 2;
        else if (!t1CanMove && t2CanMove)
            return 3;
        else
            return 0;

    }

    void BombEffect(TerisBlock teris)
    {
        teris.isBomb = false;
        teris.enabled = false;
        GameObject effect = Instantiate(Resources.Load<GameObject>("Effect/Bomb_Anim_Effect"), teris.transform.position, Quaternion.identity);
        Sound.Instance.PlayEffect("Explosion");
        StartCoroutine(camShake.Shake(.15f, .25f));


        Transform children = teris.transform.GetChild(0);
        int roundedX = Mathf.RoundToInt(children.transform.position.x);
        int roundedY = Mathf.RoundToInt(children.transform.position.y);

        for (int x = roundedX - 2; x < roundedX + 3; x++)
        {
            for (int y = roundedY - 2; y < roundedY + 3; y++)
            {
                int newx = Mathf.Clamp(x, 0, 9);
                int newy = Mathf.Clamp(y, 0, 19);
                if (grid[newx, newy] != null)
                {
                    Destroy(grid[newx, newy].gameObject);
                    grid[newx, newy] = null;
                }
            }


        }

        switch (teris.ID)
        {
            case 1:
                falled1 = true;
                break;
            case 2:
                falled2 = true;
                break;
        }

        Destroy(effect, 0.55f);
        teris.gameObject.SetActive(false);
    }

    void AddToGrid(TerisBlock teris1, TerisBlock teris2)
    {
        teris1.transform.SetParent(alltiles);
        teris2.transform.SetParent(alltiles);
        if (teris1.isBomb)
        {
            BombEffect(teris1);
        }
        else
        {
            
            foreach (Transform children in teris1.transform)
            {
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);
                grid[roundedX, roundedY] = children;
                if (CheckForGameover(roundedY))
                {
                    break;
                }
            }
            teris1.enabled = false;
        }
        if (teris2.isBomb)
        {
            BombEffect(teris2);
        }
        else
        {
            foreach (Transform children in teris2.transform)
            {
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);
                grid[roundedX, roundedY] = children;
                if (CheckForGameover(roundedY))
                {
                    break;
                }
            }
            teris2.enabled = false;

        }

        falled1 = true;
        falled2 = true;

        CheckForLines();
        CheckTwoTerisFalled();
    }

    void AddToGrid(TerisBlock teris)
    {
        teris.transform.SetParent(alltiles);
        if (teris.isBomb)
        {
            BombEffect(teris);
        }
        else
        {
            foreach (Transform children in teris.transform)
            {
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);
                grid[roundedX, roundedY] = children;
                if (CheckForGameover(roundedY))
                {
                    break;
                }
            }
            switch (teris.ID)
            {
                case 1:
                    falled1 = true;
                    break;
                case 2:
                    falled2 = true;
                    break;
            }
            teris.enabled = false;
        }

        CheckForLines();
        CheckTwoTerisFalled();
    }



    bool CheckForGameover(int hei)
    {
        if (hei >= height - 1.5f)
        {
            UImanager.Instance.Gameover();
            return true;
        }
        return false;
    }



    void CheckForLines()
    {
        int perfect = 0;
        for (int i = height - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                perfect++;
                DeleteLine(i);
                StartCoroutine(RowDownCor(i));
            }
        }
        int addscore = 100 * perfect + 20 * perfect * perfect;
        UImanager.Instance.AddScore(addscore);
    }

    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            if (grid[j, i] == null)
                return false;
        }
        return true;
    }

    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            grid[j, i].GetComponent<Animator>().SetTrigger("Dispear");
            Destroy(grid[j, i].gameObject, 0.5f);
            grid[j, i] = null;
        }
    }


    IEnumerator RowDownCor(int i)
    {
        yield return new WaitForSeconds(0.5f);
        RowDown(i);
    }

    void RowDown(int i)
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }
}
