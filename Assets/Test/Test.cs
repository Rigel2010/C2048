using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Test : MonoBehaviour
{
    public string[] testString;
    public int h, w;
    public Grid<string> grid;

    [Button]
    void MakeGridAndShow()
    {
        grid = new Grid<string>(h, w);
        for (int i = 0; i < grid.count; i++)
        {
            if (i < testString.Length)
                grid.SetItem(i, testString[i]);
            else grid.SetItem(i, "");
        }

        Debug.Log(grid.ToString());
    }

    [Button]
    void ShowAllLines()
    {
        var allLines = grid.GetAllLines(false);
        for (int i = 0; i < allLines.Length; i++)
        {
            var r = $"line{i}:";
            for (int j = 0; j < allLines[i].Length; j++)
            {
                r += allLines[i][j];
            }
            Debug.Log(r);
        }
    }
    [Button]
    void ShowAllRows()
    {
        var allRows = grid.GetAllRows();
        for (int i = 0; i < allRows.Length; i++)
        {
            var r = $"row{i}:";
            for (int j = 0; j < allRows[i].Length; j++)
            {
                r += allRows[i][j];
            }
            Debug.Log(r);
        }
    }

    public int index;
    public int dir;

    [Button]
    void CheckLRUD()
    {
        switch (dir)
        {
          case 0:
              if (grid.HasLeft(index))
                  Debug.Log(grid.GetLeft(index));
              else Debug.Log("no left");
              break;
          case 1:
              if(grid.HasRight(index))
              Debug.Log(grid.GetRight(index));
              else Debug.Log("no right");
              break;
          case 2:
              if(grid.HasUp(index))
              Debug.Log(grid.GetUp(index));
              else Debug.Log("no up");
              break;
          case 3:
              if(grid.HasDown(index))
              Debug.Log(grid.GetDown(index));
              else Debug.Log("no down");
              break;
          default: break;
        }
    }
}
