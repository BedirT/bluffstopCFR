using System;
using System.Collections.Generic;
using System.Linq;

namespace VanillaCFR
{
    public enum Suit { Hjärter, Ruter, Spader, Klöver, Wild };

    class Card
    {
        public int Value { get; private set; } //Kortets värde enligt reglerna i Bluffstopp, t.ex. dam = 12
        public Suit Suit { get; private set; }

        //public int Id { get; private set; } //Typ av kort, t.ex dam = 12

        public Card(int value, Suit suit)
        {
            Suit = suit;
            Value = value;

        }

        public void PrintCard()
        {
            string cardname = "";
            if (Value == 14)
            {
                cardname = "ess    ";
            }
            else if (Value == 11)
            {
                cardname = "knekt  ";
            }
            else if (Value == 12)
            {
                cardname = "dam     ";
            }
            else if (Value == 13)
            {
                cardname = "kung   ";
            }
            else
            {
                cardname = Value + "      ";
            }
            if (Suit == Suit.Hjärter)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (Suit == Suit.Ruter)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (Suit == Suit.Spader)
                Console.ForegroundColor = ConsoleColor.Gray;
            else if (Suit == Suit.Klöver)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(" " + Suit + " " + cardname);
            if (cardname == "")
            {
                Console.Write(Value + " ");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Card card = (Card)obj;
            if (card.Value == Value && card.Suit == Suit)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    class Game
    {
        private List<Card> CardDeck = new List<Card>();
        List<Card> DiscardPile = new List<Card>();
        public bool FirstRound = true;
        public int CurrentValue;
        public Suit CurrentSuit;
        public int LastCurrentValue;
        private Card RealCard;
        int Cardnumber;
        public int Printlevel;
        public int Discardnumber;
        public Player Player1;
        public Player Player2;
        Random RNG = new Random();
        public int NbrOfRounds;
        private string Reason;

        public Game()
        {

        }
       
        public bool initialize(bool firstGame)
        {
            Cardnumber = -1;
            Discardnumber = 52;
            CardDeck = new List<Card>();
            DiscardPile = new List<Card>();
            Player1.Hand = new List<Card>();
            Player2.Hand = new List<Card>();
            Player1.OpponentLatestCard = null;
            Player2.OpponentLatestCard = null;
            int value;
            int suit;
            for (int i = 0; i < 52; i++)
            {
                value = i % 13 + 2;
                suit = i % 4;
                CardDeck.Add(new Card(value, (Suit)suit));
            }
            Shuffle();
            int p1Sum = 0;
            int p2Sum = 0;
            for (int i = 0; i < 7; i++)
            {
                Player1.Hand.Add(DrawCard());
                Player2.Hand.Add(DrawCard());
                p1Sum += Player1.Hand[i].Value;
                p2Sum += Player2.Hand[i].Value;
            }
            Card first = DrawCard();
            Discard(first);
            Player1.OpponentLatestCard = first;
            Player2.OpponentLatestCard = first;
            if (Math.Abs(p1Sum - p2Sum) <= 5)
                return true;
            else
                return false;
        }

        public int PlayAGame(bool player1starts)
        {
            NbrOfRounds = 0;
            FirstRound = true;
            Player playerInTurn, playerNotInTurn, temp;
            if (player1starts)
            {
                playerInTurn = Player1;
                playerNotInTurn = Player2;
            }
            else
            {
                playerInTurn = Player2;
                playerNotInTurn = Player1;
            }
            Player1.OpponentLatestCard = null;
            Player2.OpponentLatestCard = null;
            while (Cardnumber < 48 && NbrOfRounds < 100)
            {
                NbrOfRounds++;
                bool result = PlayARound(playerInTurn, playerNotInTurn);
                FirstRound = false;
                if (result)
                {
                    if (Printlevel > 1)
                        printHand(playerNotInTurn);
                    playerNotInTurn.SpelSlut(playerNotInTurn.Hand.Count, playerInTurn.Hand.Count);
                    playerInTurn.SpelSlut(playerInTurn.Hand.Count, playerNotInTurn.Hand.Count);
                    if (Printlevel > 0)
                    {
                        Console.SetCursorPosition(15, playerNotInTurn.PrintPosition + 7);
                        Console.Write(playerNotInTurn.Name + " fick slut på kort och vann spelet!");
                        Console.ReadLine();
                    }
                    if (Player1 == playerNotInTurn)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    temp = playerNotInTurn;
                    playerNotInTurn = playerInTurn;
                    playerInTurn = temp;
                }
            }

            if (Printlevel > 0)
            {
                Console.SetCursorPosition(15, 20);
                Console.WriteLine("Korten tog slut utan att någon spelare vann.");
                Console.ReadLine();
            }
            playerInTurn.SpelSlut(playerInTurn.Hand.Count, playerNotInTurn.Hand.Count);
            playerNotInTurn.SpelSlut(playerNotInTurn.Hand.Count, playerInTurn.Hand.Count);

            return 0;
        }

        public void StateReason(string reason)
        {
            if (Reason.Length + reason.Length < 100)
            {
                Reason += reason + ". ";
            }
            else
            {
                Reason += " Långt!";
            }

        }

        private void ShowReason(int x, int y)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.SetCursorPosition(x, y);
            Console.Write(Reason);
            for (int i = Reason.Length; i < 100; i++)
            {
                Console.Write(" ");
            }
            Reason = "";
            Console.ResetColor();
        }
 
        private void printHand(Player player)
        {
            Console.SetCursorPosition(0, player.PrintPosition);
            Console.WriteLine(player.Name + " har ");
            for (int i = 0; i < player.Hand.Count; i++)
            {
                player.Hand[i].PrintCard();
                Console.WriteLine();
            }
            Console.Write("              ");
        }

        private bool PlayARound(Player player, Player otherPlayer)
        {
            Reason = "";
            if (FirstRound)
            {
                CurrentSuit = DiscardPile.Last().Suit;
                CurrentValue = DiscardPile.Last().Value;
            }
            if (Printlevel > 1)
            {
                printHand(player);
                Console.SetCursorPosition(7, 13);
                if (FirstRound)
                {
                    Console.Write("Första rundan. På skräphögen ligger ");
                    DiscardPile.Last().PrintCard();
                }
                else
                {
                    Console.Write("                                                          ");
                }
            }
            if (!FirstRound && CurrentValue != 0)
            {
                player.totalNumberOfCallChances++;
            }
            otherPlayer.WasLastCardCalled = false;
            if (!FirstRound && CurrentValue != 0 && player.BluffStopp(CurrentValue, CurrentSuit, LastCurrentValue))
            {
                otherPlayer.WasLastCardCalled = true;
                otherPlayer.DidOpponentPass = false;
                player.totalNumberOfCalls++;
                player.OpponentLatestCard = new Card(CurrentValue, CurrentSuit);
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 4);
                    Console.Write(player.Name + " säger Bluffstopp!                                      ");
                    ShowReason(20, player.PrintPosition + 5);

                }
                if (TrueCard())
                {
                    player.Hand.Add(DrawCard());
                    player.Hand.Add(DrawCard());
                    CurrentValue = 0;
                    LastCurrentValue = 0;
                    CurrentSuit = Suit.Wild;
                    player.WasLastCallCorrect = false;
                    if (Printlevel > 1)
                    {
                        Console.SetCursorPosition(20, player.PrintPosition + 6);
                        Console.Write(player.Name + " hade fel och får ta upp två straffkort.                  ");
                        Console.SetCursorPosition(20, player.PrintPosition + 7);
                        Console.Write(player.Name + " tog ");
                        player.Hand[player.Hand.Count - 1].PrintCard();
                        player.Hand[player.Hand.Count - 2].PrintCard();
                        printHand(player);
                        Console.ReadKey();
                        Console.SetCursorPosition(20, player.PrintPosition + 7);
                        Console.Write("                                                                          ");

                    }
                    if (otherPlayer.Hand.Count() == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    otherPlayer.Hand.Add(DrawCard());
                    otherPlayer.Hand.Add(DrawCard());
                    otherPlayer.Hand.Add(DrawCard());
                    player.WasLastCallCorrect = true;
                    player.goodCalls++;
                    otherPlayer.badBluffs++;

                    CurrentValue = 0;
                    CurrentSuit = Suit.Wild;
                    LastCurrentValue = 0;
                    if (Printlevel > 1)
                    {
                        Console.SetCursorPosition(20, player.PrintPosition + 6);
                        Console.Write(player.Name + " hade rätt! " + otherPlayer.Name + " får ta upp tre straffkort.                 ");
                        Console.SetCursorPosition(20, player.PrintPosition + 7);
                        Console.Write(otherPlayer.Name + " tog ");
                        otherPlayer.Hand[otherPlayer.Hand.Count - 1].PrintCard();
                        otherPlayer.Hand[otherPlayer.Hand.Count - 2].PrintCard();
                        otherPlayer.Hand[otherPlayer.Hand.Count - 3].PrintCard();
                        printHand(otherPlayer);
                        Console.ReadKey();
                        Console.SetCursorPosition(20, player.PrintPosition + 7);
                        Console.Write("                                                                          ");
                    }
                }
            }
            if (otherPlayer.Hand.Count() == 0)
            {
                return true;
            }
            RealCard = null;
            if (CurrentValue < 14)
            {
                RealCard = player.LäggEttKort(CurrentValue, CurrentSuit);
                player.totalNumberOfBluffChances++;
                if (RealCard != null && !player.Hand.Contains(RealCard))
                {
                    Console.SetCursorPosition(60, 0);
                    Console.WriteLine("Fusk! " + player.Name + " försöker spela ett felaktigt kort");
                    Console.ReadKey();
                    return true;
                }
                player.Hand.Remove(RealCard);
            }
            if (RealCard != null)
            {
                Card fakeCard = player.SägEttKort(CurrentValue, CurrentSuit);
                fakeCard = FixFakeValue(fakeCard);
                LastCurrentValue = CurrentValue;
                otherPlayer.DidOpponentPass = false;
                CurrentValue = fakeCard.Value;
                CurrentSuit = fakeCard.Suit;
                if (!TrueCard())
                {
                    player.totalNumberOfBluffs++;
                }
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 4);
                    Console.Write(player.Name + " spelar ");
                    RealCard.PrintCard();
                    Console.Write("                        ");
                    Console.SetCursorPosition(20, player.PrintPosition + 5);
                    Console.Write(player.Name + " säger att kortet är ");
                    fakeCard.PrintCard();
                    Console.Write("                        ");
                    ShowReason(20, player.PrintPosition + 6);
                    printHand(player);

                    ClearMessage(otherPlayer.PrintPosition + 4);

                    if (LastCurrentValue != 0 && !FirstRound)
                    {
                        Console.SetCursorPosition(20, 13);
                        Console.Write("På skräphögen låg:                                        ");
                        Console.SetCursorPosition(38, 13);
                        new Card(LastCurrentValue, CurrentSuit).PrintCard();
                    }

                    Console.ReadKey();
                }
            }
            else
            {
                //player.Hand.Add(DrawCard());
                otherPlayer.DidOpponentPass = true;
                CurrentValue = 0;
                CurrentSuit = Suit.Wild;
                LastCurrentValue = 0;
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 4);
                    //Console.Write(player.Name + " säger pass och får ta ett straffkort");
                    Console.Write(player.Name + " säger pass");
                    ShowReason(20, player.PrintPosition + 5);
                    //Console.SetCursorPosition(20, player.PrintPosition + 6);
                    //Console.Write(player.Name + " tog ");
                    //player.Hand.Last().PrintCard();
                    Console.Write("                           ");
                    printHand(player);
                    Console.ReadKey();
                }
            }
            return false;
        }

        private void ClearMessage(int yOffset)
        {
            for (int y = 0; y < 6; y++)
            {
                Console.SetCursorPosition(20, yOffset + y);
                for (int x = 0; x < 100; x++)
                {
                    Console.Write(" ");
                }
            }

        }

        private Card FixFakeValue(Card fakeCard)
        {
            if (CurrentValue == 0)
            {
                if (fakeCard.Value > 14 || fakeCard.Value < 2 || (int)fakeCard.Suit > 3)
                {
                    return new Card(2, Suit.Hjärter);
                }
            }
            else
            {
                if (fakeCard.Suit != CurrentSuit || fakeCard.Value <= CurrentValue || fakeCard.Value > 14)
                {
                    return new Card(CurrentValue + 1, CurrentSuit);
                }
            }
            return fakeCard;
        }

        private bool TrueCard()
        {
            if (RealCard.Value == CurrentValue && RealCard.Suit == CurrentSuit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Discard(Card card)
        {
            Discardnumber--;
            DiscardPile.Add(card);
        }

        private Card DrawCard()
        {
            Cardnumber++;
            Card card = CardDeck.First();
            CardDeck.RemoveAt(0);
            return card;
        }

        private Card PickDiscarded()
        {
            Card card = DiscardPile.Last();
            Discardnumber++;
            return card;
        }

        private void Shuffle()
        {
            for (int i = 0; i < 200; i++)
            {
                switchCards();
            }
        }

        private void switchCards()
        {
            int card1 = RNG.Next(CardDeck.Count);
            int card2 = RNG.Next(CardDeck.Count);
            Card temp = CardDeck[card1];
            CardDeck[card1] = CardDeck[card2];
            CardDeck[card2] = temp;
        }

        public int opponentHandSize(Player askingPlayer) //Returnerar antalet kort motståndaren har. Anropa med (this) som parameter.
        {
            if (askingPlayer == Player1)
            {
                return Player2.Hand.Count;
            }
            else
            {
                return Player1.Hand.Count;
            }
        }
        
        public List<Card> SortHandBySuit(List<Card> hand) //Sorterar handen efter suit. Hjärter->ruter->spader->klöver.
        {
            return hand.OrderBy(o => o.Suit).ToList();
        }
        
        public List<Card> SortHandByValue(List<Card> hand) //Sorterar handen efter värde, lägst först.
        {
            return hand.OrderBy(o => o.Value).ToList();
        }

        public double ProbabilityOfCardBetterThan(int value, int handSize) //Returnerar sannolikheten för att en spelare med handSize antal kort på handen har ett kort i rätt suit med värde över value
        {
            double prob = (double)(14 - value) / 52;
            prob = 1.0 - Math.Pow(1.0 - prob, handSize);
            return prob;
        }

        public List<Card> MostCommonSuitCards(List<Card> hand) //Returnerar en lista med korten i den vanligaste suiten på handen
        {
            List<Card>[] SuitLists = new List<Card>[4];
            for (int i = 0; i < 4; i++)
            {
                SuitLists[i] = new List<Card>();
            }
            for (int i = 0; i < hand.Count; i++)
            {
                SuitLists[(int)hand[i].Suit].Add(hand[i]);
            }
            int bestNumberOfCards = 0;
            int bestSuit = 0;
            for (int i = 0; i < 4; i++)
            {
                if (SuitLists[i].Count > bestNumberOfCards)
                {
                    bestNumberOfCards = SuitLists[i].Count;
                    bestSuit = i;
                }
            }
            return SuitLists[bestSuit];
        }

        public int DeckSize()
        {
            return CardDeck.Count();
        }
    }

    abstract class Player
    {
        public string Name;

        // Dessa variabler får ej ändras
        public List<Card> Hand = new List<Card>();  // Lista med alla kort i handen. 
        public int PrintPosition;
        public Game Game;
        public bool DidOpponentPass; //True om motståndaren sa "pass" på ditt förra kort
        public bool WasLastCardCalled; //True om motstånadren sa "bluffstopp" på förra kortet
        public bool WasLastCallCorrect; //True om spelarens senaste "bluffstopp" var korrekt
        public Card OpponentLatestCard; //Det senaste av motståndarens kort som spelaren fick se. Null om spelaren inte fått se något kort.
        public string OpponentName;  //Motståndarens namn

        // Dessa variabler används för att samla statistik. Får ej ändras, men får användas.
        public int totalNumberOfBluffChances; //Antalet chanser spelaren haft att bluffa
        public int totalNumberOfBluffs;  //Antalet gånger spelaren bluffat
        public int totalNumberOfCallChances;  //Antalet chanser spelaren haft att säga bluffstopp
        public int totalNumberOfCalls;   //Antalet gånger spelaren sagt bluffstopp
        public int badBluffs;  //Antalet misslyckade bluffar spelaren gjort
        public int goodCalls;  //Antalet gånger som spelaren sagt bluffstopp och haft rätt


        public abstract bool BluffStopp(int cardValue, Suit cardSuit, int cardValueToBeat);
        //Skall returnera true om spelaren vill säga bluffstopp. 
        //cardValue och cardSuit är värden på kortet motståndaren sa. cardValueToBeat är värdet på förra kortet.
        public abstract Card LäggEttKort(int cardValue, Suit cardSuit);
        //Skall returnera det kort spelaren vill lägga. Kortet måste finnas på spelarens hand.
        //Om spelaren väljer att säga pass, returnera null.
        //cardValue och cardSuit är värden på kortet motståndaren sa. cardSuit = Suit.Wild om valfritt kort kan läggas.
        public abstract Card SägEttKort(int cardValue, Suit cardSuit);
        //Skall returnera det kort spelaren säger att den lägger. Kortet måste vara högre och i samma Suit som det som ligger.
        //cardValue och cardSuit är värden på kortet motståndaren sa. cardSuit = Suit.Wild om valfritt kort kan läggas.

        public abstract void SpelSlut(int cardsLeft, int opponentCardsLeft);
        //Anropas varje gång ett spel tar slut. Kan vara bra att använda för att nollställa listor etc.
    }
