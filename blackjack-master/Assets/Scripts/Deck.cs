using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public Sprite[] facesCopy;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text moneyText;
    private int money = 1000;
    private int onPlayBetAmmount = 0;

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
        moneyText.text = money.ToString();
    }

    private void InitCardValues()
    {
        int palosBarajaContador = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (i - palosBarajaContador == 0)
            {
                values[i] = 11;
            }
            else if (i - palosBarajaContador < 10)
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
                // Comienzo la iteracion con el 11 (1 o 10) para el siguiente palo
                values[i] = 11;
            }
        }
        // Copia de array sin referencia
        facesCopy.CopyTo(faces, 0);
    }

    private void ShuffleCards()
    {
        InitCardValues();

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

        Invoke("checkForInitialBlackjack", 1f);
    }
    private void checkForInitialBlackjack()
    {
        List<GameObject> dealerCards = dealer.GetComponent<CardHand>().cards;

        if (values[1] == 10 && values[3] == 11)
        {
            dealerCards[0].GetComponent<CardModel>().ToggleFace(true);
            Debug.LogError("BLACKJACK BANCA");
            handleGameFinish("BANCA");
        }
        else if (values[3] == 10 && values[1] == 11)
        {
            dealerCards[0].GetComponent<CardModel>().ToggleFace(true);
            Debug.LogError("BLACKJACK BANCA");
            handleGameFinish("BANCA");
        }
        else if (values[0] == 10 && values[2] == 11)
        {
            dealerCards[0].GetComponent<CardModel>().ToggleFace(true);
            Debug.LogError("BLACKJACK JUGADOR");
            handleGameFinish("JUGADOR");
        }
        else if (values[2] == 10 && values[0] == 11)
        {
            dealerCards[0].GetComponent<CardModel>().ToggleFace(true);
            Debug.LogError("BLACKJACK JUGADOR");
            handleGameFinish("JUGADOR");
        }
    }
    private void CalculateProbabilities()
    {
        //Debug.Log("probabilidad de sacar > 21 en la siguiente carta -> " + probOver21());
        probMessage.text = "+21    => " + probOver21().ToString() + "\n" +
            "17-21 => " + probBetween17and21().ToString() + "\n" +
            "dealer winning => " + probDealerWinningHand().ToString();
    }

    // Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
    private float probDealerWinningHand()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;
        int totalCardsLeft = 1;
        int winningHandCards = 0;

        // Tengo en consideración tambien la carta que está bocabajo
        if (values[1] + dealerPoints >= playerPoints)
        {
            winningHandCards++;
        }

        for (int i = cardIndex; i < values.Length; i++)
        {
            totalCardsLeft++;
            if (values[i] + values[3] > playerPoints)
            {
                winningHandCards++;
            }

        }
        float res = (float)decimal.Divide(winningHandCards, totalCardsLeft);
        return res;
    }

    // Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
    private float probBetween17and21()
    {
        int totalCardsLeft = 0;
        int cardCounterBetween17and21 = 0;
        int playerPoints = player.GetComponent<CardHand>().points;
        // A partir de 12 puntos el jugador puede pasarse de puntos
        if (playerPoints >= 6)
        {
            for (int i = cardIndex; i < values.Length; i++)
            {
                totalCardsLeft++;
                if (values[i] + playerPoints <= 21 && values[i] + playerPoints >= 17)
                {
                    cardCounterBetween17and21++;
                }
                // Si se pasa de 21 compruebo que no sea un AS, si lo es chequeo si con el valor de 1 se encuentra en el rango 17<x<21
                else if (values[i] + playerPoints > 21 && values[i] == 11)
                {
                    if (playerPoints + 1 <= 21 && +playerPoints + 1 >= 17)
                    {
                        cardCounterBetween17and21++;
                    }
                }
            }
            // probabilidad cardsover21 / total cards
            float res = (float)decimal.Divide(cardCounterBetween17and21, totalCardsLeft);
            return res;
        }
        else
        {
            // probabilidad del 0%
            return 0;
        }
    }

    // Probabilidad de que el jugador obtenga más de 21 si pide una carta
    private float probOver21()
    {
        int totalCardsLeft = 0;
        int cardCounterOver21 = 0;
        int playerPoints = player.GetComponent<CardHand>().points;
        // A partir de 12 puntos el jugador puede pasarse de puntos
        if (playerPoints >= 12)
        {
            for (int i = cardIndex; i < values.Length; i++)
            {
                totalCardsLeft++;
                if (values[i] + playerPoints > 21)
                {
                    // Si se pasa de 21 compruebo que no sea un AS ya que en principio vale 11 pero realmente es un 10 o 1
                    if (values[i] == 11)
                    {
                        if (1 + playerPoints > 21)
                        {
                            cardCounterOver21++;
                        }
                    }
                    else
                    {
                        cardCounterOver21++;
                    }
                }
            }
            // probabilidad cardsover21 / total cards
            float res = (float)decimal.Divide(cardCounterOver21, totalCardsLeft);
            return res;
        }
        else
        {
            // probabilidad del 0%
            return 0;
        }

    }
    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        //CalculateProbabilities();

        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        List<GameObject> playerCards = player.GetComponent<CardHand>().cards;
        int aceCounter = 0;

        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */

        //Repartimos carta al jugador
        PushPlayer();

        /*TODO:
         * Comprobamos si el jugador ya ha perdido y mostramos mensaje
         */
        for (int i = 0; i < playerCards.Count; i++)
        {
            if (playerCards[i].GetComponent<CardModel>().value == 11)
            {
                aceCounter++;
            }
        }

        if (playerPoints > 21)
        {
            if (aceCounter != 0)
            {
                if (playerPoints - (10 * aceCounter) > 21)
                {
                    handleGameFinish("BANCA");
                }
            }
            else { handleGameFinish("BANCA"); }
        }

    }

    public void bet10()
    {
        onPlayBetAmmount = 10;
    }
    public void bet100()
    {
        onPlayBetAmmount = 100;
    }
    public void bet1000()
    {
        onPlayBetAmmount = 1000;
    }
    private void finishBet(int lossOrGanancy)
    {
        // No puede jugar con dinero que no tienes
        if (money >= onPlayBetAmmount)
        {
            money += lossOrGanancy;
            moneyText.text = money.ToString();
        }
    }

    private string winCheck()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        if (playerPoints <= 21 && dealerPoints <= 21)
        {
            if (playerPoints > dealerPoints)
            {
                finishBet(onPlayBetAmmount);
                return "JUGADOR";
            }
            else if (dealerPoints >= playerPoints)
            {
                finishBet(-onPlayBetAmmount);
                return "BANCA";
            }
        }
        else
        {
            if (playerPoints > 21)
            {
                finishBet(-onPlayBetAmmount);
                return "BANCA";
            }
            else if (dealerPoints > 21)
            {
                finishBet(onPlayBetAmmount);
                return "JUGADOR";
            }
        }

        return "NONE";
    }
    public void Stand()
    {
        int dealerPoints;
        int aceCounter;
        List<GameObject> dealerCards = dealer.GetComponent<CardHand>().cards;
        /*TODO: 
            * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
            */
        dealerCards[0].GetComponent<CardModel>().ToggleFace(true);

        while (1 != 0)
        {
            dealerPoints = dealer.GetComponent<CardHand>().points;
            aceCounter = 0;

            for (int i = 0; i < dealerCards.Count; i++)
            {
                if (dealerCards[i].GetComponent<CardModel>().value == 11)
                {
                    aceCounter++;
                }
            }

            if (dealerPoints > 21)
            {
                if (aceCounter != 0)
                {
                    if (dealerPoints - (10 * aceCounter) < 22)
                    {
                        dealerPoints = dealerPoints - (10 * aceCounter);
                    }
                }
            }

            if (dealerPoints <= 16)
            {
                PushDealer();
            }
            else if (dealerPoints >= 17)
            {
                handleGameFinish(winCheck());
                break;
            }
        }
        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */

    }
    public void handleGameFinish(string winner)
    {
        finalMessage.text = "Partida finalizada, Ha ganado: " + winner;
        /*if (EditorUtility.DisplayDialog("Partida finalizada", "Ha ganado: " + winner + " \n ¿Quieres jugar de nuevo?", "Yes", "No"))
        {
            PlayAgain();
        }
        else
        {
            EditorApplication.isPlaying = false;
            // Solo funciona en el .exe
            // Application.Quit();
        }*/
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
