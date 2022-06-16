using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
public class Sudoku : MonoBehaviour {
	
	[SerializeField] Cell _prefabCell;
	[SerializeField]Canvas _canvas;
	[SerializeField] Text _feedback;
	[SerializeField] float _stepDuration = 0.01f;
	[SerializeField,Range(1, 82)]public int _difficulty = 40;
	[SerializeField,Range(3, 10)]public int _numberOfSections = 3;	//nuevo numero de secciones
	
	Matrix<Cell> _board;
	Matrix<int> _createdMatrix;
   // List<int> _posibles = new List<int>();
   int watchdog = 0;
	int _totalSide;
    string _memory = "";
    string _canSolve = "";
    bool _canPlayMusic = false;
   // List<int> _nums = new List<int>();

    float _r = 1.0594f;
    float _frequency = 440;
    float _gain = 0.5f;
    float _increment;
    float _phase;
    float _samplingF = 48000;
    


    void Start()
    {
        long mem = System.GC.GetTotalMemory(true);
        _feedback.text = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        _memory = _feedback.text;
        _totalSide = _numberOfSections * _numberOfSections;
        _frequency = _frequency * Mathf.Pow(_r, 2);
        CreateEmptyBoard();
        ClearBoard();
        CreateNew();
    }
    
    void Update () {
	    if(Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
		    SolvedSudoku();
	    else if (Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0))
	    {
		    Debug.Log("create");
		     CreateSudoku();	
	    }
		    
    }
    
    void ClearBoard() {
		_createdMatrix = new Matrix<int>(_totalSide, _totalSide);
		foreach(var cell in _board) {
			cell.number = 0;
			cell.locked = cell.invalid = false;
		}
	}

	void CreateEmptyBoard()
	{
		float spacing = 68f;
		float startX = -spacing * 4f;
		float startY = spacing * 4f;

		_board = new Matrix<Cell>(_totalSide, _totalSide);
		for(int x = 0; x<_board.Width; x++) {
			for(int y = 0; y<_board.Height; y++) {
                var cell = _board[x, y] = Instantiate(_prefabCell);
				cell.transform.SetParent(_canvas.transform, false);
				cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
			}
		}
	}
	


	//IMPLEMENTAR
	
	bool RecuSolve(Matrix<int> matrixParent, int x, int y, int protectMaxDepth, List<Matrix<int>> solution)
    {
	    if (y >= matrixParent.Height) return true; //termina matrix

	    protectMaxDepth--;  //resto para revisar limite y que no haga stack overflow
		
	    if (protectMaxDepth <= 0)  return false;
	    

	    if (_board[x, y].locked) //reviso si el valor esta bloqueado - backtracking
	    {
		    Matrix<int> nextMatrix =  matrixParent.Clone();
			
		    int newX = x;
		    int newY = y;
				
		    if (x == matrixParent.Width - 1)  //reviso estar en la ultima columna, de estarlo reinicio x pero sumo en y para cambiar de renglon
		    {
			    newX = 0;
			    newY++;
		    }
		    else  //quedan columnas en el renglon current 
		    {
			    newX++; 
		    }

		    return (RecuSolve(nextMatrix, newX, newY, protectMaxDepth, solution)); //si los valores coinciden con la solucion, devuelvo verdadero

	    }
		
	   // int maxValue = numberOfSections * numberOfSections;	//para determinar en base a la cantidad de secciones configurable
	    for (int i = 1; i <= _totalSide; i++)
	    {
		    if (CanPlaceValue(matrixParent, i, x, y))
		    {
			    matrixParent[x, y] = i;
			    Matrix<int> nextMatrix =  matrixParent.Clone();
			    solution.Add(nextMatrix);  //agrego la newmatrix para mostrar el paso a paso

			    int newX = x;
			    int newY = y;
				
			    if (x == matrixParent.Width - 1)
			    {
				    newX = 0;
				    newY++;
			    }
			    else
			    {
				    newX++;
			    }

			    if (RecuSolve(nextMatrix, newX, newY, protectMaxDepth, solution))
				    return true;
		    }
	    }

	    matrixParent[x, y] = 0;  // reinicio el valor para comenzar desde el principio
	    return false; 
    }


    void OnAudioFilterRead(float[] array, int channels)
    {
        if(_canPlayMusic)
        {
            _increment = _frequency * Mathf.PI / _samplingF;
            for (int i = 0; i < array.Length; i++)
            {
                _phase = _phase + _increment;
                array[i] = (float)(_gain * Mathf.Sin((float)_phase));
            }
        }
        
    }
    void changeFreq(int num)
    {
        _frequency = 440 + num * 80;
    }

	//IMPLEMENTAR - punto 3
	IEnumerator ShowSequence(List<Matrix<int>> seq)
    {
	    for (int i = 0; i < seq.Count; i++)
	    {
		    TranslateAllValues(seq[i]);
		    _feedback.text = "Steps: " + (i + 1) + "/" + seq.Count + " - " + _memory + " - " + _canSolve;
		    yield return new WaitForSeconds(_stepDuration);
	    }
    }

	

	//modificar lo necesario para que funcione.
    void SolvedSudoku()
    {
        StopAllCoroutines();
        
        //_nums = new List<int>();
        
        var solution = new List<Matrix<int>>();
        watchdog = 100000;
        var result =RecuSolve(_createdMatrix, 0, 0, watchdog, solution);
        StartCoroutine(ShowSequence(solution));
        long mem = System.GC.GetTotalMemory(true);
        _memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        _canSolve = result ? " VALID" : " INVALID";
        
    }

    void CreateSudoku()
    {
	    //if (numberOfSections >= 4) return;
	    
	    StopAllCoroutines();
	   // _nums = new List<int>();
	    _canPlayMusic = false;
	    
        ClearBoard();

        List<Matrix<int>> l = new List<Matrix<int>>();
        watchdog = 100000;

        GenerateValidLine(_createdMatrix, 0, 0);
        
        var result =RecuSolve(_createdMatrix, 0, 1, watchdog, l);
        
        _createdMatrix= l.Last().Clone();
        
        LockRandomCells(); 
        ClearUnlocked(_createdMatrix);
        TranslateAllValues(_createdMatrix);
        
        long mem = System.GC.GetTotalMemory(true);
        _memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        _canSolve = result ? " VALID" : " INVALID";
        _feedback.text = "Steps: " + l.Count + "/" + l.Count + " - " + _memory + " - " + _canSolve;
        
    }
	void GenerateValidLine(Matrix<int> mtx, int x, int y)
	{
		int[]aux = new int[_totalSide];
		for (int i = 0; i < _totalSide; i++)
		{
			aux [i] = i + 1;
		}
		int numAux = 0;
		for (int j = 0; j < aux.Length; j++) 
		{
			int r = 1 + Random.Range(j,aux.Length);
			numAux = aux [r-1];
			aux [r-1] = aux [j];
			aux [j] = numAux;
		}
		for (int k = 0; k < aux.Length; k++) 
		{
			mtx [k, 0] = aux [k];
		}
	}


	void ClearUnlocked(Matrix<int> mtx)
	{
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					mtx[j,i] = Cell.EMPTY;
			}
		}
	}

	void LockRandomCells()
	{
		List<Vector2> posibles = new List<Vector2> ();
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					posibles.Add (new Vector2(j,i));
			}
		}
		for (int k = 0; k < 82-_difficulty; k++) {
			int r = Random.Range (0, posibles.Count);
			_board [(int)posibles [r].x, (int)posibles [r].y].locked = true;
			posibles.RemoveAt (r);
		}
	}

    void TranslateAllValues(Matrix<int> matrix)
    {
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
            }
        }
    }

    void TranslateSpecific(int value, int x, int y)
    {
        _board[x, y].number = value;
    }

    void TranslateRange(int x0, int y0, int xf, int yf)
    {
        for (int x = x0; x < xf; x++)
        {
            for (int y = y0; y < yf; y++)
            {
                _board[x, y].number = _createdMatrix[x, y];
            }
        }
    }
    void CreateNew()
    {
	    if (_numberOfSections >= 4) return;
        _createdMatrix = new Matrix<int>(Tests.validBoards.Last());
        LockRandomCells();
	    ClearUnlocked(_createdMatrix);
        TranslateAllValues(_createdMatrix);
    }

    bool CanPlaceValue(Matrix<int> mtx, int value, int x, int y)
    {
        List<int> fila = new List<int>();
        List<int> columna = new List<int>();
        List<int> area = new List<int>();
        List<int> total = new List<int>();

        Vector2 cuadrante = Vector2.zero;

        for (int i = 0; i < mtx.Height; i++)
        {
            for (int j = 0; j < mtx.Width; j++)
            {
                if (i != y && j == x) columna.Add(mtx[j, i]);
                else if(i == y && j != x) fila.Add(mtx[j,i]);
            }
        }

        cuadrante.x = (int)(x / _numberOfSections);

        if (x < _numberOfSections)
            cuadrante.x = 0;     
        else if (x < _numberOfSections * 2)
            cuadrante.x = _numberOfSections;
        else
            cuadrante.x = _numberOfSections * 2;

        if (y < _numberOfSections)
            cuadrante.y = 0;
        else if (y < _numberOfSections * 2)
            cuadrante.y = _numberOfSections;
        else
            cuadrante.y = _numberOfSections * 2;
         
        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + _numberOfSections, (int)cuadrante.y + _numberOfSections);
        total.AddRange(fila);
        total.AddRange(columna);
        total.AddRange(area);
        total = FilterZeros(total);

        if (total.Contains(value))
            return false;
        else
            return true;
    }


    List<int> FilterZeros(List<int> list)
    {
        List<int> aux = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != 0) aux.Add(list[i]);
        }
        return aux;
    }
}
