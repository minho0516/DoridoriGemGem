using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject bgTilePrefab;
    public Gem[] gemPrefabs;
    public Gem[,] allGems;
    public MatchFinder matchFinder;

    public enum BoardState { wait, move }
    public BoardState currentState = BoardState.move;

    public Gem bomb;
    public float bombChance = 2f;

    public RoundManager roundManager;

    private void Awake()
    {
        matchFinder = FindObjectOfType<MatchFinder>();
        roundManager = FindObjectOfType<RoundManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        allGems = new Gem[width, height];
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        //matchFinder.FindAllMatches();

        if (Input.GetKeyDown(KeyCode.S))
        {
            SuffleBoard();
        }
    }

    private void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject bgTile = Instantiate(bgTilePrefab, pos, Quaternion.identity);
                bgTile.transform.parent = transform;
                bgTile.name = "BG Tile" + x + "," + y;

                int gemToUse = Random.Range(0, gemPrefabs.Length);
                while (MatcheAt(new Vector2Int(x, y), gemPrefabs[gemToUse]))
                {
                    gemToUse = Random.Range(0, gemPrefabs.Length);
                }

                SpawnGem(new Vector2Int(x, y), gemPrefabs[gemToUse]);
            }
        }
    }

    private void SpawnGem(Vector2Int pos, Gem gemToSpawn)
    {
        if (Random.Range(0f, 100f) < bombChance)
        {
            gemToSpawn = bomb;
        }
        Gem gem = Instantiate(gemToSpawn, new Vector3(pos.x, pos.y + height, 0f), Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = "Gem" + pos.x + "," + pos.y;
        allGems[pos.x, pos.y] = gem;
        gem.SetupGem(pos, this);
    }

    //초기에 스폰할 때 매칭여부 확인
    private bool MatcheAt(Vector2Int posToCheck, Gem gemToCheck)
    {
        if (posToCheck.x > 1)
        {
            if (allGems[posToCheck.x - 1, posToCheck.y].type == gemToCheck.type &&
             allGems[posToCheck.x - 2, posToCheck.y].type == gemToCheck.type)
            {
                return true;
            }
        }
        if (posToCheck.y > 1)
        {
            if (allGems[posToCheck.x, posToCheck.y - 1].type == gemToCheck.type &&
             allGems[posToCheck.x, posToCheck.y - 2].type == gemToCheck.type)
            {
                return true;
            }
        }
        return false;
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < matchFinder.currentMatches.Count; i++)
        {
            if (matchFinder.currentMatches[i] != null)
            {
                ScoreCheck(matchFinder.currentMatches[i]);
                DestroyMatchedGemAt(matchFinder.currentMatches[i].posIndex);
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private void DestroyMatchedGemAt(Vector2Int posIndex)
    {
        if (allGems[posIndex.x, posIndex.y] != null)
        {
            if (allGems[posIndex.x, posIndex.y].isMatched)
            {
                Instantiate(allGems[posIndex.x, posIndex.y].destroyEffect, new Vector2(posIndex.x, posIndex.y), Quaternion.identity);
                Destroy(allGems[posIndex.x, posIndex.y].gameObject);
                allGems[posIndex.x, posIndex.y] = null;
            }
        }
    }

    //매칭이 된 빈 공간에 보석을 내리는 함수
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(0.2f);

        int nullCounter = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                    nullCounter++;
                else if (nullCounter > 0)
                {
                    allGems[x, y].posIndex.y -= nullCounter;
                    allGems[x, y - nullCounter] = allGems[x, y];
                    allGems[x, y] = null;
                }
            }
            nullCounter = 0;
        }
        StartCoroutine(FillBoardCo());
    }

    //빈공간에 새로운 Gem을 채우는 함수
    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    int gemToUse = Random.Range(0, gemPrefabs.Length);

                    SpawnGem(new Vector2Int(x, y), gemPrefabs[gemToUse]);
                }
            }
        }
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(.5f);
        RefillBoard();

        yield return new WaitForSeconds(.5f);
        matchFinder.FindAllMatches();
        if (matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(.5f);
            currentState = BoardState.move;
        }
    }

    public void SuffleBoard()
    {
        if (currentState != BoardState.wait)
        {
            currentState = BoardState.wait;
            List<Gem> gemFromBoard = new List<Gem>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gemFromBoard.Add(allGems[x, y]);
                    allGems[x, y] = null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int gemToUse = Random.Range(0, gemFromBoard.Count);
                    while (MatcheAt(new Vector2Int(x, y), gemFromBoard[gemToUse]) && gemFromBoard.Count > 1)
                    {
                        gemToUse = Random.Range(0, gemFromBoard.Count);
                    }

                    gemFromBoard[gemToUse].SetupGem(new Vector2Int(x, y), this);
                    allGems[x, y] = gemFromBoard[gemToUse];
                    gemFromBoard.RemoveAt(gemToUse);
                }
            }

            StartCoroutine(FillBoardCo());
        }
    }

    public void ScoreCheck(Gem gemToCheck)
    {
        roundManager.currentScore += gemToCheck.scoreValue;
    }
}