using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

public class MazeGeneration
{
    
    private Cell[,] maze;
    private int size;
    private Random rand = new Random();
    private List<Trap> traps = new List<Trap>(); 
    private (int x, int y) exit; 

    public MazeGeneration(int size)
    { 
        this.size = size;
        this.maze = new Cell[size,size];

        for(int i=0; i< size;i++)
        {
            for(int j=0; j<size;j++)
            {
                maze[i,j]=new Cell(false); // Todo paredes
            }
        }
        
        GenerateTheMaze(0, 0);
        SetExit();
        GenerateTraps();
        
    }
    public int Size => size;
    private void GenerateTheMaze(int x, int y) // recursive backtracking 
{
    var directions = new (int dx, int dy)[]
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    Shuffle(directions); //asegura aleatoriedad
    // Marcar la celda como pasillo
    maze[x, y].isOpen = true;

    foreach (var (dx, dy) in directions)
    {
        int nx = x + dx * 2; // Mover 2 celdas en esa direccion
        int ny = y + dy * 2; // saltar la pared

        if (nx >= 0 && nx < size && ny >= 0 && ny < size && !maze[nx, ny].isOpen)
        {
            //Quitar la pared entre las celdas 
            maze[x + dx, y + dy].isOpen = true; 
            GenerateTheMaze(nx, ny); // Recursivamente 
        }
    }

    //Asegurar por lo menos una celda abierta en la ultima fila
    if (!HasOpenCellInRow(size - 1))
    {
        int col = rand.Next(size);
        maze[size - 1, col].isOpen = true;
        Console.WriteLine($"Opened a random cell in the last row at column: {col}");
    }
}

private bool HasOpenCellInRow(int row)
{
    for (int col = 0; col < size; col++)
    {
        if (maze[row, col].isOpen)
            return true;
    }
    return false;
}

       
    private void Shuffle((int dx, int dy)[] directions) //Fisher-Yates shuffle,In-Place Shuffling, cada elemento es cambiado una vez
    {
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1); //cada elemento desde el inicio del array hasta i tiene la misma probabilidad de ser escogida
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }

    }
    public void PrintMaze()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (i == exit.x && j == exit.y)
                {
                    Console.Write("E");
                }
                else 
                {
                    Trap trap = IsTrapAtPosition(i, j)!;  
                    if (trap != null)
                    {
                    Console.Write(trap.Name);  
                    }
                    else
                    {
                    Console.Write(maze[i, j].isOpen ? "." : "#");  
                    }
                }
            }
            Console.WriteLine();
        }
    }

    public class Cell
    {
        public bool isOpen;
        public Cell(bool shouldBeOpen)
        {
            isOpen=shouldBeOpen;
        }
    }

private void GenerateTraps()
{
    int trapCount = 0; 
    int totalTraps = 3; 

    string[] trapEffects = {
        "Pierdes 1 turno",          // Effect for T1
        "Atrasas 2 casillas",    // Effect for T2
        "Reduce tu velocidad 3 turnos"        // Effect for T3
        //"Regresa a la casilla inicial"
        //para mapas mas grandes, repetir las mismas trampas
        //validate the minimum size of the maze
    };

    //Todas las posiciones validas para trampas
    List<(int x, int y)> validPositions = new List<(int x, int y)>();
    List<Trap> trapsList = new List<Trap>();

    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            //Una posicion valida es un pasillo
            if (maze[i, j].isOpen)
            {
                validPositions.Add((i, j));
            }
        }
    }

    // Convertir a un array para el shuffling
    var validPositionsArr = validPositions.ToArray();

    Shuffle(validPositionsArr);

    // Poner trampa en la primera posicion valida en totalTraps
    foreach (var (x, y) in validPositionsArr)
    {
        if (trapCount < totalTraps)
        {
            string trapName = "T" + (trapCount + 1); // Nombre "T1", "T2", "T3"
            string effect = trapEffects[trapCount]; // Tomar su efecto

            trapsList.Add(new Trap(x, y, trapName, effect));
            trapCount++;
        }

        if (trapCount == totalTraps)
            break;
    }
    traps = trapsList;  // Asignar la lista de trampas al objeto MazeGeneration 
    Console.WriteLine($"Placed {trapCount} traps in the maze.");
}

public Trap? IsTrapAtPosition(int i, int j)
{
    foreach (var trap in traps)
    {
        if (trap.X == i && trap.Y == j)
        {
            return trap; 
        }
    }
    return null; 
}

private void SetExit()
{
    int exitRow = size - 1;

    // Ver si hay una casilla abierta alcanzable en la ultima fila
    for (int col = 0; col < size; col++)
    {
        if (maze[exitRow, col].isOpen && IsExitReachable(exitRow, col))
        {
            exit = (exitRow, col);
            Console.WriteLine($"Exit position set at: ({exitRow}, {col})");
            return;
        }
    }

    // Fallback:abrir una casilla aleatoria en la ultima fila y asegurar que sea alcanzable
    for (int col = 0; col < size; col++)
    {
        if (maze[exitRow, col].isOpen || !maze[exitRow, col].isOpen)
        {
            maze[exitRow, col].isOpen = true; 
            if (IsExitReachable(exitRow, col))
            {
                exit = (exitRow, col);
                Console.WriteLine($"Fallback exit position set at: ({exitRow}, {col})");
                return;
            }

            //si no es alcanzable, cerrarla 
            maze[exitRow, col].isOpen = false;
        }
    }

    //como ultima instancia abrir una celda aleatoria como salida
    int randomCol = rand.Next(size);
    maze[exitRow, randomCol].isOpen = true;
    exit = (exitRow, randomCol);
    Console.WriteLine($"Random fallback exit set at: ({exitRow}, {randomCol})");
}

public bool IsExitReachable(int exitRow, int exitCol) //Algortimo de Lee
{
        bool[,] visited=new bool[size,size];

        //marcar la celda inicial como abierta
        visited[0,0]=true;
        int[] dr={-1,1,0,0,};
        int[] dc={0,0,1,-1};

        bool change;
        do{
            change=false;
            for(int r=0;r<size;r++){
                for(int c=0;c<size;c++){
                    //saltar las celdas sin marcar
                    if(!visited[r,c]) continue;
    
//chequear celdas vecinas a [r,c]
                    for(int d=0;d<dr.Length;d++){
                        int vr=r+dr[d];
                        int vc=c+dc[d];
//determinar si el vecino es valido y no ha sido chequeado
                        if(ValidPosition(size,vr,vc) && !visited[vr,vc] && maze[vr,vc].isOpen){
                            visited[vr,vc]=true;
                            change=true;
                            if(vr==exitRow&&vc==exitCol){
                                return true;
                            }
                        }
                    }
                }
            }
        }
        while(change);
        return false; //salida no es alcanzable
    }

    static bool ValidPosition(int size,int row,int col){
        return row >= 0 && row < size && col >= 0 && col < size;
    }
    
public bool IsWall(int x, int y)
{
    // Check if the coordinates are outside the maze bounds
    if (x < 0 || x >= size || y < 0 || y >= size)
    {
        Console.WriteLine($"Position ({x}, {y}) is outside the maze bounds.");
        return true; // Out-of-bounds positions are treated as walls
    }

    // Determine if the current cell is a wall based on the `isOpen` property
    bool isWall = !maze[y, x].isOpen;
    Console.WriteLine($"Position ({x}, {y}) is {(isWall ? "a wall" : "not a wall")}.");

    return isWall;
}
}




