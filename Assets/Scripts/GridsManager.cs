using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridsManager : MonoBehaviour
{
    

    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private Transform entitiesTran;

    
    public Grid<GridInfo> grids { get; private set; }
    private int cellSize = 200;

    public void Prepare(int amount)
    {
        //清除已有子物体
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < entitiesTran.childCount; i++)
        {
            Destroy(entitiesTran.GetChild(i).gameObject);
        }
        
        var rect = this.GetComponent<RectTransform>().rect;
        cellSize = (int)(rect.width / amount);
        var gridGroup = this.GetComponent<GridLayoutGroup>();
        gridGroup.cellSize = new Vector2(cellSize, cellSize);
        grids = new Grid<GridInfo>(amount,amount);
        for (int i = 0; i < grids.count; i++)
        {
            var go = Instantiate(gridPrefab, this.transform);
            var grid = go.AddComponent<GridInfo>();
            grids.SetItem(i,grid);
            var index2 = grids.GetIndex2(i);
            go.name = $"{index2.x}_{index2.y}";
        }

        GameManager.CurState.Value = GameState.Prepared;
    }
    public void GeneralAt(int i, int num)
    {
        var grid = grids.GetItem(i);
        GeneralAt(grid,num);
    }
    public void GeneralAt(GridInfo grid, int num)
    {
        if (!grid.isEmpty) return;
        var go = Instantiate(entityPrefab, entitiesTran);
        go.transform.position = grid.transform.position;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta=new Vector2(cellSize,cellSize);
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        var ctrl = go.GetComponent<EntityCtrl>();
        ctrl.SetEntity(num);
        grid.entity = ctrl;
    }
    public string GridsToHistory()
    {
        return grids.ToString();
    }
    public bool HistoryToGrids(string history)
    {
        var g = history.Split(',');
        if (g.Length != grids.count) return false;
        for (int i = 0; i < g.Length; i++)
        {
            var gi = Convert.ToInt32(g[i]);
            grids.GetItem(i).ClearGrid();
            if(gi >= 0) GeneralAt(i,gi);
        }

        return true;
    }

    public void RandomGenerate(int maxNum = 0)
    {
        var selable = grids.items.Where(g => g.isEmpty).ToArray();
        var i = Random.Range(0, selable.Length);
        var j = Random.Range(0, maxNum+1);
        GeneralAt(selable[i],j);
    }
    public IEnumerator Move(MoveDir dir)
    {
        bool hasChange = false;
        //向左聚拢并合并升级
        GridInfo[][] lor;
        switch (dir)
        {
            case MoveDir.Left:
                lor = grids.GetAllLines();
                break;
            case MoveDir.Right:
                lor = grids.GetAllLines(false);
                break;
            case MoveDir.Up:
                lor = grids.GetAllRows();
                break;
            case MoveDir.Down:
                lor = grids.GetAllRows(false);
                break;
            default: 
                lor = grids.GetAllLines();
                break;
        }
        
        for (int i = 0; i < lor.Length; i++)
        {
            hasChange = MoveLineOrRow(lor[i]) || hasChange;
        }
        
        yield return new WaitForSeconds(0.2f);
        //刷新entity
        var entities = entitiesTran.GetComponentsInChildren<EntityCtrl>();
        foreach (var e in entities)
        {
            e.Refresh();
        }

        var entityGrids = grids.items.Where(g => !g.isEmpty);
        foreach (var g in entityGrids)
        {
            g.entity.transform.position = new Vector3(g.transform.position.x, g.transform.position.y,g.entity.transform.position.z);
        }
        GameManager.CurState.Value = hasChange?GameState.CheckOver:GameState.WaitInput;
    }

    bool MoveLineOrRow(GridInfo[] lor)
    {
        bool hasChange = false;
        for (int i = 1; i < lor.Length; i++)
        {
            //空格
            if(lor[i].isEmpty) continue;
            
            //没有空格、最近的空格再后面时
            int aimIndex = i;
            for (var j = aimIndex-1; j >= 0; j--)
            {
                if (lor[j].isEmpty) aimIndex = j;
                else if (lor[i].num == lor[j].num && !lor[j].entity.hasUp&& !lor[j].entity.needDestroy)
                {
                    lor[j].entity.needDestroy = true;
                    lor[i].entity.num += 1;
                    lor[i].entity.hasUp = true;
                    aimIndex = j;
                    break;
                }
                else break;
            }
            //没有移动
            if(i == aimIndex) continue;
            //有移动
            var e = lor[i].entity;
            lor[i].ClearGrid();
            lor[aimIndex].entity = e;
            e.transform.DOMoveX(lor[aimIndex].transform.position.x, 0.1f);
            e.transform.DOMoveY(lor[aimIndex].transform.position.y, 0.1f);
            hasChange = true;
        }

        return hasChange;
    }
 

    public int points => entitiesTran.GetComponentsInChildren<EntityCtrl>().Sum(e => e.point);
    public bool IsGameOver()
    {
        if (grids.items.Where(g => g.isEmpty).Count() == 0)
        {
            for (var i = 0;i<grids.count;i++)
            {
                if (grids.HasLeft(i) && grids.GetLeft(i).num == grids.GetItem(i).num) return false;
                if (grids.HasRight(i) && grids.GetRight(i).num == grids.GetItem(i).num) return false;
                if (grids.HasUp(i) && grids.GetUp(i).num == grids.GetItem(i).num) return false;
                if (grids.HasDown(i) && grids.GetDown(i).num == grids.GetItem(i).num) return false;
            }
    
            return true;
        }
        return false;
    }

}
 public class GridInfo : MonoBehaviour
    {
        public EntityCtrl entity;  
        public int num => isEmpty ? -1 : entity.num;
        public bool isEmpty => entity == null;

        public void ClearGrid(bool destroy=false)
        {
            if (isEmpty) return;
            var e = entity;
            entity = null;
            if(destroy)e.Destroy();
        }

        public override string ToString()
        {
            return num.ToString();
        }
    }
    public enum MoveDir{Left,Right,Up,Down}