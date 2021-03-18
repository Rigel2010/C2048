using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region UI

    [SerializeField] private GridsManager _gridsManager;
    [SerializeField] private TMP_Text pointText;
    [SerializeField] private GameObject popWnd;
    [SerializeField] private GameObject overPopPrefab;
    #endregion

    public int amount;

    private ReactiveProperty<int> point = new ReactiveProperty<int>(0);

    private const int maxHistoryLength = 10;
    private List<string> gridsHistories = new List<string>();
    public static ReactiveProperty<GameState> CurState = new ReactiveProperty<GameState>(GameState.None);
    
    void Start()
    {
        //UI
        point.Subscribe(_ =>
        {
            pointText.text = point.Value.ToString();
        }).AddTo(this);
        popWnd.SetActive(false);
        //Main
        CurState.Where(s => s == GameState.None)
            .Subscribe(_ =>
            {
                _gridsManager.Prepare(amount);
            }).AddTo(this);
        CurState.Where(s => s == GameState.Prepared)
            .DelayFrame(1)
            .Subscribe(_ =>
            {
                CurState.Value = GameState.FirstEntities;
                gridsHistories = UserData.data.Load<List<string>>("history", new List<string>());
                if (gridsHistories.Count > 0) _gridsManager.HistoryToGrids(gridsHistories[gridsHistories.Count - 1]);
                else
                {
                     _gridsManager.RandomGenerate();
                     _gridsManager.RandomGenerate(1);
                     //_gridsManager.GeneralAt(0,0);
                     //_gridsManager.GeneralAt(3,0);
                    AddNowIntoHistory(_gridsManager.GridsToHistory());
                }

                point.Value = _gridsManager.points;
                CurState.Value = GameState.WaitInput;
            }).AddTo(this);

        #region  move
        Observable.EveryUpdate()
                    .Where(_ => CurState.Value == GameState.WaitInput && GetInput())
                    .Subscribe(_ =>
                    {
                        if (_gridsManager.IsGameOver()) CurState.Value = GameState.Over;
                        else
                        {
                            CurState.Value = GameState.Move;
                            if (Input.GetKeyDown(KeyCode.A))
                                StartCoroutine(_gridsManager.Move(MoveDir.Left));
                            if (Input.GetKeyDown(KeyCode.D))
                                StartCoroutine(_gridsManager.Move(MoveDir.Right));
                            if (Input.GetKeyDown(KeyCode.W))
                                StartCoroutine(_gridsManager.Move(MoveDir.Up));
                            if (Input.GetKeyDown(KeyCode.S))
                                StartCoroutine(_gridsManager.Move(MoveDir.Down));
                        }
                        
                    }).AddTo(this);

        #endregion
        
        CurState.Where(s => s == GameState.CheckOver)
            .Subscribe(_ =>
            {
                point.Value = _gridsManager.points;
                if (_gridsManager.IsGameOver()) CurState.Value = GameState.Over;
                else
                {
                    CurState.Value = GameState.Generate;
                }
            }).AddTo(this);
        CurState.Where(s => s == GameState.Generate)
            .Subscribe(_ =>
            {
                _gridsManager.RandomGenerate(1);
                AddNowIntoHistory(_gridsManager.GridsToHistory());
                CurState.Value = GameState.WaitInput;
            }).AddTo(this);
        CurState.Where(s => s == GameState.Over)
            .Subscribe(_ =>
            {
                var bestScore = UserData.data.Load<int>("best", 0);
                bestScore = Mathf.Max(bestScore, point.Value);
                UserData.data.Save<int>("best", bestScore);
                UserData.Save("one game over");
                var overpop = PopUp(overPopPrefab).GetComponent<OverPopView>();
                overpop.SetText(point.Value,bestScore);
                overpop.btn.onClick.AddListener(() =>
                {
                    UserData.data.DeleteKey("history");
                    CurState.Value = GameState.None;
                    popWnd.SetActive(false);
                });
            }).AddTo(this);
    }
    #region  move
    public void MoveLeft()
    {
        if (CurState.Value != GameState.WaitInput) return;
        CurState.Value = GameState.Move;
        StartCoroutine(_gridsManager.Move(MoveDir.Left));
    }
    public void MoveRight()
    {
        if (CurState.Value != GameState.WaitInput) return;
        CurState.Value = GameState.Move;
        StartCoroutine(_gridsManager.Move(MoveDir.Right));
    }
    public void MoveUp()
    {
        if (CurState.Value != GameState.WaitInput) return;
        CurState.Value = GameState.Move;
        StartCoroutine(_gridsManager.Move(MoveDir.Up));
    }
    public void MoveDown()
    {
        if (CurState.Value != GameState.WaitInput) return;
        CurState.Value = GameState.Move;
        StartCoroutine(_gridsManager.Move(MoveDir.Down));
    }
    #endregion
    void AddNowIntoHistory(string history)
    {
        if(gridsHistories.Count>=maxHistoryLength) gridsHistories.RemoveAt(0);
        gridsHistories.Add(history);
        UserData.data.Save<List<string>>("history", gridsHistories);
    }

    bool GetInput()
    {
        return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W);
    }

    private void OnApplicationQuit()
    {
        UserData.Save("quit");
    }

    GameObject PopUp(GameObject popPrefab)
    {
        popWnd.gameObject.SetActive(true);
        return Instantiate(popPrefab, popWnd.transform);
    }
}
public enum GameState{None,Prepared,FirstEntities,WaitInput,Move,CheckOver,Generate,Over}

