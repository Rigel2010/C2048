
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid<T>
{
    public T[] items { get; private set; }
    public int sizeH { get; private set; }
    public int sizeW { get; private set; }
    public int count => sizeH * sizeW;

    public Grid(int sizeH,int sizeW)
    {
        this.sizeH = sizeH;
        this.sizeW = sizeW;
        items = new T[count];
    }

    public T GetItem(int index)
    {
        return items[index];
    }

    public T GetItem(int h, int w)
    {
        if (h < 0 || h >= sizeH || w < 0 || w >= sizeW) 
            throw new IndexOutOfRangeException($"grid size is ({sizeH}，{sizeW}),require item({h},{w}) is out of range");
        
        return items[h * sizeW + w];
    }

    public int GetIndex(T item)
    {
        return Array.IndexOf(items, item);
    }

    public Vector2Int GetIndex2(int index)
    {
        var h = index / sizeW;
        var w = index % sizeW;
        return new Vector2Int(h, w);
    }

    public Vector2Int GetIndex2(T item)
    {
        var index = GetIndex(item);
        return GetIndex2(index);
    }

    public T[] GetLine(int h, bool l2r = true)
    {
        var startIndex = h * sizeW;
        var r = items.Skip(startIndex).Take(sizeW);
        return l2r ? r.ToArray() : r.Reverse().ToArray();
    }
    public T[] GetRow(int w, bool t2b = true)
    {
        T[] r = new T[sizeH];
        for (int i = 0; i < sizeH; i++)
        {
            r[i] = items[i * sizeW + w];
        }

        return t2b ? r : r.Reverse().ToArray();
    }

    public T[][] GetAllLines(bool l2r = true)
    {
        var r = new T[sizeH][];
        for (int i = 0; i < sizeH; i++)
        {
            r[i] = GetLine(i,l2r);
        }

        return r;
    }
    public T[][] GetAllRows(bool t2b = true)
    {
        var r = new T[sizeW][];
        for (int i = 0; i < sizeW; i++)
        {
            r[i] = GetRow(i,t2b);
        }

        return r;
    }

    public bool HasLeft(int index) => index % sizeW > 0;
    public bool HasRight(int index) => index % sizeW < (sizeW-1);
    public bool HasUp(int index) => index / sizeW > 0;
    public bool HasDown(int index) => index/sizeW < (sizeH-1);
    public T GetLeft(int index) => items[index - 1];
    public T GetRight(int index) => items[index + 1];
    public T GetUp(int index) => items[index - sizeW];
    public T GetDown(int index) => items[index + sizeW];
    

    public void SetItem(int index, T value)
    {
        items[index] = value;
    }

    public void SetItem(int h, int w, T value)
    {
        var index = h * sizeW + w;
        SetItem(index,value);
    }

    public override string ToString()
    {
        string[] r = new string[count] ;
        for (int i = 0; i < count; i++)
        {
            r[i] = items[i].ToString();
        }

        return string.Join(",",r);
    }
    public string ToDebugString()
    {
        var r = "\n";
        for (int i = 0; i < count; i++)
        {
            r += items[i].ToString();
            if (i % sizeW == sizeW - 1) r += "\n";
        }

        return r;
    }
}
