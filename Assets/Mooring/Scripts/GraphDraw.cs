using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphDraw : MonoBehaviour
{
    // отладочные графики
    public List<GraphData> Graphics;

    // размер координатной сетки
    public Vector2 MinValue;
    public Vector2 MaxValue;

    // размер текстуры
    public int TexWidth = 512;
    public int TexHeight = 512;

    private Texture2D _tex;
    private Renderer _rend;

    // для  линейных размеров предметной области на текстуру
    private float _xScale;
    private float _yScale;

    private void Start()
    {
        Graphics = new List<GraphData>();
    }

    public void AddData( List<Vector2> gr, Color col )
    {
        if(Graphics.Count == 0)
        { 
            MinValue.x = MaxValue.x = gr[0].x;
            MinValue.y = MaxValue.y = gr[0].y;
        }
        // для последующего рисования графиков определяем границы значений
        for (int i=0; i<gr.Count; i++)
        {
            if (gr[i].x < MinValue.x) MinValue.x = gr[i].x;
            if (gr[i].y < MinValue.y) MinValue.y = gr[i].y;
            if (gr[i].x > MaxValue.x) MaxValue.x = gr[i].x;
            if (gr[i].y > MaxValue.y) MaxValue.y = gr[i].y;
            //print(gr[i]);
        }
        print("MinValue = " + MinValue);
        print("MaxValue = " + MaxValue);

        GraphData grData = new GraphData();
        grData.color = col;
        grData.points = gr;
        Graphics.Add(grData);
    }

    public void DrawAll()
    {
        _rend = GetComponent<Renderer>();
        _tex = new Texture2D(TexWidth, TexHeight);
        _rend.material.mainTexture = _tex;

        _xScale = (float)TexWidth / (MaxValue.x - MinValue.x);
        _yScale = (float)TexHeight / (MaxValue.y - MinValue.y);

        if (Graphics != null)
        {
            for (int i = 0; i < Graphics.Count; i++)
            {
                DrawGraphic(Graphics[i]);
            }
        }

        _tex.Apply();
    }

    public void DrawGraphic(GraphData gd)
    {
        //print(gd.points.Count);
        for (int i = 1; i < gd.points.Count; i++)
        {
            DrawLine(gd.points[i - 1], gd.points[i], gd.color);
        }
    }

    public void DrawLine(Vector2 start, Vector2 end, Color col)
    {
        // алгоритм Брезенхама
        int x0 = (int)((start.x - MinValue.x) * _xScale);
        int x1 = (int)((end.x - MinValue.x) * _xScale);
        int y0 = (int)((start.y - MinValue.y) * _yScale);
        int y1 = (int)((end.y - MinValue.y) * _yScale);

        if (x0 == x1 && y0 == y1)
        {
            setPoint(x0, y0,  col);
            return;
        }
            

        int A, B, sign;
        A = y1 - y0;
        B = x0 - x1;
        if (Math.Abs(A) > Math.Abs(B)) sign = 1;
        else sign = -1;

        int signa, signb;
        if (A < 0) signa = -1;
        else signa = 1;
        if (B < 0) signb = -1;
        else signb = 1;
        int f = 0;
        setPoint(x0, y0, col);
        int x = x0, y = y0;
        if (sign == -1)
        {
            do
            {
                f += A * signa;
                if (f > 0)
                {
                    f -= B * signb;
                    y += signa;
                }
                x -= signb;
                setPoint(x, y, col);
            } while (x != x1 || y != y1);
        }
        else
        {
            do
            {
                f += B * signb;
                if (f > 0)
                {
                    f -= A * signa;
                    x -= signb;
                }
                y += signa;
                setPoint(x, y, col);
            } while (x != x1 || y != y1);
        }

    }

    public void onePoint(float x, float y, Color col)
    {
        setPoint((int)((x - MinValue.x) * _xScale), (int)((y - MinValue.y) * _yScale), col);
    }

    // Отображает точку, (x,y) - в координатах текстуры, но с нижнего левого угла
    private void setPoint(int x, int y, Color col)
    {
        _tex.SetPixel(TexWidth - x, TexHeight - y, col);    // видимо, текстура отсчитывается от правого верхнего
    }


}

[Serializable]
public class GraphData
{
    public List<Vector2> points;
    public Color color;

    public GraphData()
    {
        points = new List<Vector2>();
        color = new Color(1, 1, 1);
    }
}
