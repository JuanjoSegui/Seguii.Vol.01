using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mono.Data.Sqlite;
using System.Data;


// Controla la lógica del juego, como la generación del tablero, el agregado de nuevas filas y la detección de juego terminado.
// También controla el aumento de nivel y la dificultad, y administra la inserción de datos en la base de datos.
namespace MyGame
{
public class LogicaTablero : MonoBehaviour
{

        public GameObject whiteSquarePrefab;
        public GameObject redSquarePrefab;
        public GameObject blueSquarePrefab;
        public GameObject greenSquarePrefab;
        public GameObject yellowSquarePrefab;


        public int rows = 10;
        public int columns = 6;
        public float tiempoCasilla = 5f;

        public event Action OnGameOver;
        private int score = 0;
        public UnityEvent OnSquareSwapped;

        public LayerMask squareLayer;
        public static int NivelDeDificultad = 0;
        private int lineasCompletadas = 0;
        private DatabaseManager databaseManager;


        // Genera el tablero inicial y comienza la corutina de agregar filas nuevas.
        void Start()
        {
            GenerateBoard();
            StartCoroutine(AddRowCoroutine());

            //databaseManager = new DatabaseManager();
            databaseManager = FindObjectOfType<DatabaseManager>();


        }

        // Detecta el evento de clic y mueve la casilla seleccionada hacia abajo.
        // También aumenta el nivel y la dificultad del juego, y verifica si se ha completado una fila.
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, Vector2.zero, 0f, squareLayer);

                if (hit.collider != null)
                {
                    SquareController squareController = hit.collider.GetComponent<SquareController>();

                    if (squareController != null)
                    {
                        squareController.SwapSquareWithBelow();
                        CheckForRowCompletion();

                        AumentarNivel();
                        AumentarDificultad();

                    }

                }
            }
        }

        public int CalcularPuntuacion()
        {
            return score;
        }

        public int CalcularLineas()
        {
            return lineasCompletadas;
        }

        //Se encarga de generar un tablero inicial de casillas blancas.
        //Utiliza dos bucles anidados para crear una cuadrícula de casillas y posicionarlas en el tablero.
        private void GenerateBoard()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                   Vector3 position = new Vector3(j, i, 0);
                   GameObject newSquare = Instantiate(whiteSquarePrefab, position, Quaternion.identity);
                   newSquare.transform.SetParent(transform);
                }
            }
        }
        //Devuelve una casilla de un color aleatorio.
        //Los porcentajes se suman por lo que modificando estas variables conseguimos mayor o menor dificultad
        private GameObject CreateRandomSquare()
        {
            float randomValue = UnityEngine.Random.value;

            if (randomValue < 0.6f)
            {
                return whiteSquarePrefab;
            }
            else if (randomValue < 0.9f)
            {
                return redSquarePrefab;
            }
            else if (randomValue < 0.94f)
            {
                return blueSquarePrefab;
            }
            else if (randomValue < 0.97f)
            {
                return greenSquarePrefab;
            }
            else
            {
                return yellowSquarePrefab;
            }
        }
        //Mueve las casillas existentes hacia arriba, luego crea nuevas casillas en la parte inferior de cada columna.
        public void AddNewRow()
        {
            for (int j = 0; j < columns; j++)
            {
                // Mueve las casillas existentes hacia arriba.
                for (int i = rows - 1; i >= 0; i--)
                {
                    Vector3 currentPosition = new Vector3(j, i, 0);
                    Collider2D squareCollider = Physics2D.OverlapPoint(currentPosition);
                    if (squareCollider != null)
                    {
                        Vector3 newPosition = currentPosition + new Vector3(0, 1, 0);
                        squareCollider.transform.position = newPosition;
                    }
                }

                // Crea una nueva casilla en la posición vacía de la columna.
                Vector3 newSquarePosition = new Vector3(j, 0, 0);
                GameObject randomSquarePrefab = CreateRandomSquare();
                GameObject newSquare = Instantiate(randomSquarePrefab, newSquarePosition, Quaternion.identity);
                newSquare.transform.SetParent(transform);
            }

            // Verifica si el juego ha terminado.
            CheckGameOver();
        }


        private IEnumerator AddRowCoroutine()
        {
            while (true)
            {
                AddNewRow();
                yield return new WaitForSeconds(tiempoCasilla); // Espera n segundos antes de agregar otra fila.
            }
        }

        private void CheckGameOver()
        {
            for (int j = 0; j < columns; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    Vector3 currentPosition = new Vector3(j, i, 0);
                    Collider2D squareCollider = Physics2D.OverlapPoint(currentPosition);

                    if (squareCollider != null && squareCollider.gameObject != whiteSquarePrefab)
                    {
            
                        if (squareCollider.transform.position.y >= rows)
                        {
                            if (OnGameOver != null)
                            {
                                OnGameOver.Invoke();
                            }

                            break;
                        }
                    }
                }
            }
        }
        //Verifica si se ha completado una fila y, si es así,
        //reemplaza las casillas de esa fila con casillas blancas y aumenta la puntuación.
        public bool CheckForRowCompletion()
        {
            bool anyRowCompleted = false;

            for (int i = 0; i < rows; i++)
            {
                if (IsRowComplete(i))
                {
                    ReplaceRowWithWhiteSquares(i);
                    
                    // Insertar datos en la base de datos
                    string fecha = DateTime.Now.ToString();
                    int puntos = CalcularPuntuacion();
                    int lineas = CalcularLineas();

                    databaseManager.InsertFecha(fecha);
                    databaseManager.InsertPuntos(puntos);
                    databaseManager.InsertLineas(lineas);
                }
            }
            return anyRowCompleted;

        }
        //Verifica si una fila con casillas del mismo color está completa.
        private bool IsRowComplete(int row)
        {
            Color firstColor = Color.clear;
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(j, row, 0);
                Collider2D squareCollider = Physics2D.OverlapPoint(position);

                if (squareCollider != null)
                {
                    Color currentColor = squareCollider.GetComponent<SpriteRenderer>().color;

                    if (currentColor == Color.white) // Ignora las casillas blancas
                        return false;

                    if (firstColor == Color.clear)
                    {
                        firstColor = currentColor;
                    }
                    else if (currentColor != firstColor)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        //Reemplaza las casillas de una fila completa con casillas blancas.
        private void ReplaceRowWithWhiteSquares(int row)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(j, row, 0);
                Collider2D squareCollider = Physics2D.OverlapPoint(position);

                if (squareCollider != null)
                {
                    Destroy(squareCollider.gameObject);
                    GameObject newSquare = Instantiate(whiteSquarePrefab, position, Quaternion.identity);
                    newSquare.transform.SetParent(transform);
                    score += 100;
                    Debug.Log("Score: " + score);
                    lineasCompletadas++;
                }
            }
        }

        //Ambas funciones deben actuar juntas, el tiempo entre filas se reduce a medida que aumenta el nivel de dificultad.
        void AumentarNivel()
        {
            switch (score)

            {
                case 2000:
                    NivelDeDificultad = 1; break;
                case 6000:
                    NivelDeDificultad = 2; break;
                case 20000:
                    NivelDeDificultad = 3; break;

            }
        }

        void AumentarDificultad()
        {
            switch (NivelDeDificultad)
            {
                case 1: tiempoCasilla = 4f; break;

                case 2: tiempoCasilla = 3f; break;

                case 3: tiempoCasilla = 2f; break;
            }
        }
    }
}
