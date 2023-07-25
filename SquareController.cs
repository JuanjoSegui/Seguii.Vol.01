using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



// Controla el comportamiento de una casilla del juego
namespace MyGame 

{
    public class SquareController : MonoBehaviour
    {
        private Vector3 screenPoint;
        private Vector3 offset;
        //private LogicaTablero boardManager;
        public LogicaTablero logicaTablero;
        private LogicaTablero board;

        // Obtiene la referencia al objeto LogicaTablero y establece la posición inicial de las casillas.
        void Start()
        {
            board = FindObjectOfType<LogicaTablero>();
            if (board == null)
            {
                Debug.LogError("LogicaTablero no encontrado en el objeto padre.");
            }

            //boardManager = FindObjectOfType<LogicaTablero>();
        }
        // Detecta el evento de click en la casilla y la mueve hacia abajo.
        void OnMouseDown()
        {
            Vector3 currentPosition = transform.position;
            Vector3 newPosition = currentPosition - new Vector3(0, 1, 0);
            Collider2D squareCollider = Physics2D.OverlapPoint(newPosition);

            if (squareCollider != null)
            {
                // Intercambia las casillas.
                squareCollider.transform.position = currentPosition;
                transform.position = newPosition;
            }

            board.OnSquareSwapped.Invoke();
            bool rowCompleted = board.CheckForRowCompletion();
            if (rowCompleted)
            {
                Debug.Log("Score: " + board.CalcularPuntuacion());
            }

        }
        // Intercambia la casilla con la de abajo.
        public void SwapSquareWithBelow()
        {
            Vector3 belowPosition = transform.position + new Vector3(0, -1, 0);
            Collider2D belowCollider = Physics2D.OverlapPoint(belowPosition);

            if (belowCollider != null)
            {
                GameObject belowSquare = belowCollider.gameObject;
                belowCollider.transform.position = transform.position;
                transform.position = belowPosition;
            }
        }


    }
       
}