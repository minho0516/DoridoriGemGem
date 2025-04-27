using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public Vector2Int posIndex;
    private Vector2Int previousPos;
    public Board board;

    private Vector2 firstTouchPos;
    private Vector2 lastTouchPos;
    private bool mousePressed;
    private float swipeAngle = 0;
    private Gem otherGem;

    public enum GemType { blue, green, red, purple, yellow, bomb }
    public GemType type;
    //나를 기준으로 왼오위아 같은 타입이 3개 이상
    public bool isMatched;

    public GameObject destroyEffect;
    public int blastSize = 2;

    public int scoreValue = 10;

    // Update is called once per frame
    void Update()
    {
        //위치가 바뀐거
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, 7 * Time.deltaTime);
        }
        //위치가 안바뀐거
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            board.allGems[posIndex.x, posIndex.y] = this;
        }


        if (Input.GetMouseButtonUp(0) && mousePressed)
        {
            mousePressed = false;
            if (board.currentState == Board.BoardState.move)
            {
                lastTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CacluateAngle();
            }
        }
    }

    private void CacluateAngle()
    {
        swipeAngle = Mathf.Atan2(lastTouchPos.y - firstTouchPos.y, lastTouchPos.x - firstTouchPos.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        if (Vector3.Distance(firstTouchPos, lastTouchPos) > .5f)
        {
            MovePieces();
        }
    }

    public void SetupGem(Vector2Int pos, Board theboard)
    {
        posIndex = pos;
        board = theboard;
    }

    private void OnMouseDown()
    {
        if (board.currentState == Board.BoardState.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }

    //움직는 위치를 지정
    private void MovePieces()
    {
        previousPos = posIndex;
        //위 : 45 ~ 135
        if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < board.height - 1)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y + 1];
            otherGem.posIndex.y--;
            posIndex.y++;
        }
        //오 : -45 ~ 45
        else if (swipeAngle > -45 && swipeAngle <= 45 && posIndex.x < board.width - 1)
        {
            otherGem = board.allGems[posIndex.x + 1, posIndex.y];
            otherGem.posIndex.x--;
            posIndex.x++;
        }
        else if (swipeAngle > 135 || swipeAngle <= -135 && posIndex.x > 0)
        {
            otherGem = board.allGems[posIndex.x - 1, posIndex.y];
            otherGem.posIndex.x++;
            posIndex.x--;
        }
        else if (swipeAngle > -135 && swipeAngle <= -45 && posIndex.y > 0)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y - 1];
            otherGem.posIndex.y++;
            posIndex.y--;
        }


        if (otherGem != null)
        {
            board.allGems[posIndex.x, posIndex.y] = this;
            board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;
        }

        StartCoroutine(CheckMoveCo());
    }

    private IEnumerator CheckMoveCo()
    {
        board.currentState = Board.BoardState.wait;

        yield return new WaitForSeconds(0.5f);
        board.matchFinder.FindAllMatches();
        if (otherGem != null)
        {
            //매치가 안된 상황
            if (!isMatched && !otherGem.isMatched)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                board.allGems[posIndex.x, posIndex.y] = this;
                board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;

                yield return new WaitForSeconds(0.5f);
                board.currentState = Board.BoardState.move;
            }
            else
            {
                Debug.Log("매치하하하하");
                RoundManager.Instance.MatchAndAddTime();
                board.DestroyMatches();
            }
        }
    }
}