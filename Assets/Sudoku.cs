using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
public class Sudoku : MonoBehaviour {
	
	public Cell prefabCell;
	public Canvas canvas;
	public Text feedback;
	public float stepDuration = 0.05f;
	[Range(1, 82)]public int difficulty = 40;
	[Range(3, 10)]public int numberOfSections = 3;	//nuevo numero de secciones

	Matrix<Cell> _board;
	Matrix<int> _createdMatrix;
    List<int> posibles = new List<int>();
	int _smallSide;
	int _bigSide;
    string memory = "";
    string canSolve = "";
    bool canPlayMusic = false;
    List<int> nums = new List<int>();

    float r = 1.0594f;
    float frequency = 440;
    float gain = 0.5f;
    float increment;
    float phase;
    float samplingF = 48000;


    void Start()
    {
        long mem = System.GC.GetTotalMemory(true);
        feedback.text = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        memory = feedback.text;
        _smallSide = numberOfSections;
        _bigSide = _smallSide * numberOfSections;
        frequency = frequency * Mathf.Pow(r, 2);
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
		_createdMatrix = new Matrix<int>(_bigSide, _bigSide);
		foreach(var cell in _board) {
			cell.number = 0;
			cell.locked = cell.invalid = false;
		}
	}

	void CreateEmptyBoard() {
		float spacing = 68f;
		float startX = -spacing * 4f;
		float startY = spacing * 4f;

		_board = new Matrix<Cell>(_bigSide, _bigSide);
		for(int x = 0; x<_board.Width; x++) {
			for(int y = 0; y<_board.Height; y++) {
                var cell = _board[x, y] = Instantiate(prefabCell);
				cell.transform.SetParent(canvas.transform, false);
				cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
			}
		}
	}
	


	//IMPLEMENTAR
	int watchdog = 0;
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
		
	    int maxValue = numberOfSections * numberOfSections;	//para determinar en base a la cantidad de secciones configurable
	    for (int i = 1; i <= maxValue; i++)
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
        if(canPlayMusic)
        {
            increment = frequency * Mathf.PI / samplingF;
            for (int i = 0; i < array.Length; i++)
            {
                phase = phase + increment;
                array[i] = (float)(gain * Mathf.Sin((float)phase));
            }
        }
        
    }
    void changeFreq(int num)
    {
        frequency = 440 + num * 80;
    }

	//IMPLEMENTAR - punto 3
	IEnumerator ShowSequence(List<Matrix<int>> seq)
    {
	    for (int i = 0; i < seq.Count; i++)
	    {
		    TranslateAllValues(seq[i]);
		    feedback.text = "Steps: " + (i + 1) + "/" + seq.Count + " - " + memory + " - " + canSolve;
		    yield return new WaitForSeconds(stepDuration);
	    }
    }

	

	//modificar lo necesario para que funcione.
    void SolvedSudoku()
    {
        StopAllCoroutines();
        nums = new List<int>();
        var solution = new List<Matrix<int>>();
        watchdog = 100000;
        var result =RecuSolve(_createdMatrix, 0, 0, watchdog, solution);
        StartCoroutine(ShowSequence(solution));
        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";
        
    }

    void CreateSudoku()
    {
	    if (numberOfSections >= 4) return;
	    
	    StopAllCoroutines();
	    nums = new List<int>();
	    canPlayMusic = false;
        ClearBoard();
        List<Matrix<int>> l = new List<Matrix<int>>();
        watchdog = 100000;
        GenerateValidLine(_createdMatrix, 0, 0);
        var result =RecuSolve(_createdMatrix, 0, 0, watchdog, l);

      
        for (int y = 0; y < _createdMatrix.Height; y++)
        {
	        for (int x = 0; x < _createdMatrix.Width; x++)
	        {
		        _createdMatrix[x, y] = l.Last()[x, y];
		        Debug.Log(_createdMatrix[x, y]);
	        }
        }
        
        LockRandomCells(); 
        ClearUnlocked(_createdMatrix);
        
        TranslateAllValues(_createdMatrix);
        
    
        //TranslateAllValues(_createdMatrix);
        
        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";
        feedback.text = "Steps: " + l.Count + "/" + l.Count + " - " + memory + " - " + canSolve;
        
    }
	void GenerateValidLine(Matrix<int> mtx, int x, int y)
	{
		int[]aux = new int[9];
		for (int i = 0; i < 9; i++) 
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
		for (int k = 0; k < 82-difficulty; k++) {
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
	    if (numberOfSections >= 4) return;
	  // int numRangeBoards = Random.Range(0, Tests.validBoards.Length);
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

        cuadrante.x = (int)(x / numberOfSections);

        if (x < numberOfSections)
            cuadrante.x = 0;     
        else if (x < numberOfSections * 2)
            cuadrante.x = numberOfSections;
        else
            cuadrante.x = numberOfSections * 2;

        if (y < numberOfSections)
            cuadrante.y = 0;
        else if (y < numberOfSections * 2)
            cuadrante.y = numberOfSections;
        else
            cuadrante.y = numberOfSections * 2;
         
        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + numberOfSections, (int)cuadrante.y + numberOfSections);
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
