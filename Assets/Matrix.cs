using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Matrix<T> : IEnumerable<T>
{
    //IMPLEMENTAR: ESTRUCTURA INTERNA- DONDE GUARDO LOS DATOS?
    T[,] _matrix;
    private int _width;
    private int _height;
    private int _capacity;

    public Matrix(int width, int height)
    {
        //IMPLEMENTAR: constructor
        _width = width;
        _height = height;
        _capacity = width * height;
        _matrix = new T[width,height];
    }

	public Matrix(T[,] copyFrom)
    {
	    
        //IMPLEMENTAR: crea una version de Matrix a partir de una matriz básica de C#
        
        _matrix = copyFrom;
    }

	public Matrix<T> Clone() 
	{
        Matrix<T> aux = new Matrix<T>(Width, Height);

        for (int y = 0; y < _width; y++)
        {
	        for (int x = 0; x < _height; x++)
	        {
		        aux[x, y] = this[x, y];
	        }
        }
        //IMPLEMENTAR
        return aux;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item)
	{
        //IMPLEMENTAR: iguala todo el rango pasado por parámetro a item
        
        // primero tengo que en encontrar que valor de index y height pertenece a x0,y0 e x1,y1
        // luego cada uno de los valores dentro de ese rango con un for los recorro y cambio por item
        for (int x = x0; x < x1; x++)
        {
	        for (int y = y0; y < y1; y++)
	        {
		        _matrix[x, y] = item;
	        }
        }
    }

    //Todos los parametros son INCLUYENTES
    public List<T> GetRange(int x0, int y0, int x1, int y1) 
    {
	    List<T> l = new List<T>();
	    
        //IMPLEMENTAR
        //mismo razonamiento que Set Range
        
        for (int x = x0; x < x1; x++)
        {
	        for (int y = y0; y < y1; y++)
	        {
		        l.Add(this[x, y]);
	        }
        }
        return l;
	}

    //Para poder igualar valores en la matrix a algo
    public T this[int x, int y] 
    {
		get { return _matrix[x,y]; }
		set { _matrix[x, y] = value; }
	}

    public int Width
    {
	    get { return _width; }
	    set { _width = value;}
    }

    public int Height { 
	    get {return _height; }
	    set { _height = value; }
    }

    public int Capacity
    {
	    get { return _capacity; }
	    set { _capacity = value; }
    }

    public IEnumerator<T> GetEnumerator()
    {
        //IMPLEMENTAR
        
        foreach (var spot in _matrix)
        {
	        yield return spot;
        }
    }

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
