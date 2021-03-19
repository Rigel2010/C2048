using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

public class Game2048Manager : MonoBehaviour
{
    #region UI

    [SerializeField] private GridsManager gridsManager;
    [SerializeField] private TMP_Text pointText;
    #endregion

    //可在游戏中设定、MasterData管理
    private const int MAXHistoryLength = 10;
    public int amount;

    private ReactiveProperty<int> _point = new ReactiveProperty<int>(0);
    private List<string> _gridsHistories = new List<string>();
    public static ReactiveProperty<GameState> CurState = new ReactiveProperty<GameState>(GameState.None);
    void Start()
    {
        #region UI

        _point.Subscribe(_ =>
        {
            pointText.text = _point.Value.ToString();
        }).AddTo(this);

        #endregion

        #region main
        CurState.Where(s => s == GameState.None)
            .Subscribe(_ =>
            {
                gridsManager.Prepare(amount);
            }).AddTo(this);
        CurState.Where(s => s == GameState.Prepared)
            .DelayFrame(1)
            .Subscribe(_ =>
            {
                CurState.Value = GameState.FirstEntities;
                _gridsHistories = UserData.data.Load<List<string>>("history", new List<string>());
                if (_gridsHistories.Count > 0) gridsManager.HistoryToGrids(_gridsHistories[_gridsHistories.Count - 1]);
                else
                {
                     gridsManager.RandomGenerate();
                     gridsManager.RandomGenerate(1);
                     //_gridsManager.GeneralAt(0,0);
                     //_gridsManager.GeneralAt(3,0);
                    AddNowIntoHistory(gridsManager.GridsToHistory());
                }

                _point.Value = gridsManager.points;
                CurState.Value = GameState.WaitInput;
            }).AddTo(this);

        
        CurState.Where(s => s == GameState.CheckOver)
            .Subscribe(_ =>
            {
                _point.Value = gridsManager.points;
                if (gridsManager.IsGameOver()) CurState.Value = GameState.Over;
                else
                {
                    CurState.Value = GameState.Generate;
                }
            }).AddTo(this);
        CurState.Where(s => s == GameState.Generate)
            .Subscribe(_ =>
            {
                gridsManager.RandomGenerate(1);
                AddNowIntoHistory(gridsManager.GridsToHistory());
                CurState.Value = GameState.WaitInput;
            }).AddTo(this);
        CurState.Where(s => s == GameState.Over)
            .Subscribe(_ =>
            {
                var bestScore = UserData.data.Load<int>("best", 0);
                bestScore = Mathf.Max(bestScore, _point.Value);
                UserData.data.Save<int>("best", bestScore);
                UserData.Save("one game over");
                var popup = UIPopup.GetPopup("OverPop");
                if (popup == null) return;
                popup.Data.SetLabelsTexts("Game Over",
                    $"Score:<indent=50%>{_point}</indent>",
                    $"Best Score:<indent=50%>{bestScore}</indent>");
                popup.Data.SetButtonsCallbacks(() =>
                {
                    UserData.data.DeleteKey("history");
                    CurState.Value = GameState.None;
                    if(popup != null) popup.Hide();
                });
                popup.Show();
            }).AddTo(this);
      
        #endregion
        
        #region debug

        #if UNITY_EDITOR
                Observable.EveryUpdate()
                    .Where(_ => CurState.Value == GameState.WaitInput && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W)))
                    .Subscribe(_ =>
                    {
                        if (gridsManager.IsGameOver()) CurState.Value = GameState.Over;
                        else
                        {
                            CurState.Value = GameState.Move;
                            if (Input.GetKeyDown(KeyCode.A))
                                StartCoroutine(gridsManager.Move(MoveDir.Left));
                            if (Input.GetKeyDown(KeyCode.D))
                                StartCoroutine(gridsManager.Move(MoveDir.Right));
                            if (Input.GetKeyDown(KeyCode.W))
                                StartCoroutine(gridsManager.Move(MoveDir.Up));
                            if (Input.GetKeyDown(KeyCode.S))
                                StartCoroutine(gridsManager.Move(MoveDir.Down));
                        }
                                
                    }).AddTo(this);
        
        #endif

        #endregion

    }
    
    #region  swipMove
    public void MoveLeft()
    {
        if (CurState.Value != GameState.WaitInput) return;
        if (gridsManager.IsGameOver())
        {
            CurState.Value = GameState.Over;
            return;
        }
        CurState.Value = GameState.Move;
        StartCoroutine(gridsManager.Move(MoveDir.Left));
    }
    public void MoveRight()
    {
        if (CurState.Value != GameState.WaitInput) return;
        if (gridsManager.IsGameOver())
        {
            CurState.Value = GameState.Over;
            return;
        }
        CurState.Value = GameState.Move;
        StartCoroutine(gridsManager.Move(MoveDir.Right));
    }
    public void MoveUp()
    {
        if (CurState.Value != GameState.WaitInput) return;
        if (gridsManager.IsGameOver())
        {
            CurState.Value = GameState.Over;
            return;
        }
        CurState.Value = GameState.Move;
        StartCoroutine(gridsManager.Move(MoveDir.Up));
    }
    public void MoveDown()
    {
        if (CurState.Value != GameState.WaitInput) return;
        if (gridsManager.IsGameOver())
        {
            CurState.Value = GameState.Over;
            return;
        }
        CurState.Value = GameState.Move;
        StartCoroutine(gridsManager.Move(MoveDir.Down));
    }
    #endregion

    #region sub_main
    void AddNowIntoHistory(string history)
    {
        if(_gridsHistories.Count>=MAXHistoryLength) _gridsHistories.RemoveAt(0);
        _gridsHistories.Add(history);
        UserData.data.Save<List<string>>("history", _gridsHistories);
    }

    #endregion
    
    

    //TODO Move to GameManager
    private void OnApplicationQuit()
    {
        UserData.Save("quit");
    }

    
}
public enum GameState{None,Prepared,FirstEntities,WaitInput,Move,CheckOver,Generate,Over,UseItem,SelGrid,ItemInflu}

