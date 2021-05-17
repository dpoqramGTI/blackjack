using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;

    public int[] values = new int[52];
    int cardIndex = 0;    
       
    private void Awake()
    {    
        InitCardValues();        

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();        
    }

    private void InitCardValues()
    {
        int palosBarajaContador = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (i - palosBarajaContador < 10)
            {
                // Cartas del 1 al 10
                values[i] = (i - palosBarajaContador) + 1;
            }
            else if (i - palosBarajaContador < 13)
            {
                // Cartas J Q K
                values[i] = 10;
            }
            else if (i - palosBarajaContador == 13)
            {
                // Al terminar con un palo sumo las 13 cartas al contador
                palosBarajaContador += 13;
                // Comienzo la iteracion con el 1 para el siguiente palo
                values[i] = 1;
            }
        }
    }

    private List<int> getDeckArrayRandomIndexes()
    {
        int max = values.Length;
        int randomIndex;
        List<int> arrayIndexes = new List<int>();
        List<int> arrayIndexesShuffled = new List<int>();

        // Array con los indiceso ordenados del 0 al 51
        for (int e = 0; e < values.Length; e++)
        {
            arrayIndexes.Add(e);
        }

        /* 
         * Voy metiendo valores de arrayIndexes a arrayIndexesShuffled
         * Elijo un elemento aleatorio y hago un push
         * Borro el elemento del array inicial y disminuyo el rango de numeros aleatorios
         */
        for (int i = 0; i < values.Length; i++)
        {
            randomIndex = Random.Range(0, max);
            arrayIndexesShuffled.Add(arrayIndexes[randomIndex]);
            arrayIndexes.RemoveAt(randomIndex);
            max--;
        }

        return arrayIndexesShuffled;
    }

    private void ShuffleCards()
    {
        Sprite auxSprite;
        int auxInt;
        List<int> randomIndexesArray = getDeckArrayRandomIndexes();
        for (int e = 0; e < randomIndexesArray.Count; e++)
        {
            // Guardo el sprite que esta en la posicion aleatoria dada por el arrayRandom
            auxSprite = faces[randomIndexesArray[e]];
            // Asigno la nueva posicion del sprite dada por el array random
            faces[e] = faces[randomIndexesArray[e]];
            // El spriteAux que guarde antes lo pongo en la posicion que ha quedado libre al hacer el cambio
            faces[randomIndexesArray[e]] = auxSprite;

        }

        for (int i = 0; i < randomIndexesArray.Count; i++)
        {
            // Guardo el sprite que esta en la posicion aleatoria dada por el arrayRandom
            auxInt = values[randomIndexesArray[i]];
            // Asigno la nueva posicion del sprite dada por el array random
            values[i] = values[randomIndexesArray[i]];
            // El spriteAux que guarde antes lo pongo en la posicion que ha quedado libre al hacer el cambio
            values[randomIndexesArray[i]] = auxInt;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
            /*TODO:
             * Si alguno de los dos obtiene Blackjack, termina el juego y mostramos mensaje
             */
        }
    }

    private void CalculateProbabilities()
    {
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */
    }

    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }       

    public void Hit()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */
        
        //Repartimos carta al jugador
        PushPlayer();

        /*TODO:
         * Comprobamos si el jugador ya ha perdido y mostramos mensaje
         */      

    }

    public void Stand()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */

        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */                
         
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
    
}
