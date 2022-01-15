using System;
using System.Collections.Generic;


namespace bluffstopCFR
{
    public class Card
    {

        public int rank;
        public string suit; // spades, hearts, diamonds, clubs and wild
        public Card(string suit, string rank)
        {
            this.suit = suit;
            if (rank == "a")
            {
                this.rank = 14;
            }
            else if (rank == "k")
            {
                this.rank = 13;
            }
            else if (rank == "q")
            {
                this.rank = 12;
            }
            else if (rank == "j")
            {
                this.rank = 11;
            }
            else if (rank == "t")
            {
                this.rank = 10;
            }
            else
            {
                this.rank = int.Parse(rank);
            }
        }

        public Card(char suit, char rank)
        {
            this.suit = suit.ToString();
            if (rank == 'a')
            {
                this.rank = 14;
            }
            else if (rank == 'k')
            {
                this.rank = 13;
            }
            else if (rank == 'q')
            {
                this.rank = 12;
            }
            else if (rank == 'j')
            {
                this.rank = 11;
            }
            else if (rank == 't')
            {
                this.rank = 10;
            }
            else
            {
                this.rank = int.Parse(rank.ToString());
            }
        }

        public override string ToString()
        {
            string r = "";
            if (rank == 14)
            {
                r = "a";
            }
            else if (rank == 13)
            {
                r = "k";
            }
            else if (rank == 12)
            {
                r = "q";
            }
            else if (rank == 11)
            {
                r = "j";
            }
            else if (rank == 10)
            {
                r = "t";
            }
            else
            {
                r = rank.ToString();
            }
            return suit + r;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Card card = obj as Card;
            if (card == null)
            {
                return false;
            }
            return this.rank == card.rank && (this.suit == card.suit || this.suit == "w");
        }

        public override int GetHashCode()
        {
            // ? Check if ever called
            return rank * 131 + suit.GetHashCode();
        }

        public bool greaterThan(Card card)
        {
            return this.rank > card.rank && (this.suit == card.suit || card.suit == "w");
        }
    }
    public class BluffStop
    {
        /*
            Bluff Stopp is an imperfect information two-player zero-sum card game.
            The game is played with a standard 52-card deck. The players are each dealt
            7 cards at the start of the game. The goal is to finish your cards before
            your opponent does. For the beginning a card from the deck is placed face up
            on the table. The first player then chooses a card from his hand that is
            higher than the card on the table and from the same suit. The card is then
            placed on the table and the next player continues. The players can bluff,
            meaning that they can choose a card from their hand that is different from
            what they have stated. The other player can call bluff if they believe that
            the card they have chosen is not the one on the table. If a bluff was called
            and it was indeed a bluff, the player who bluffed draw 2 cards from the deck.
            If it was not a bluff, the player who called bluff draws 3 cards from the deck.
            If bluff was called or if someone passed, the suit and the number of the card
            in the middle becomes a wild card (any suit and any number are valid plays).
            Cards are encoded as follows:
            c: clubs - d: diamonds - h: hearts - s: spades
            2: 2 - 3: 3 - 4: 4 - 5: 5 - 6: 6 - 7: 7 - 8: 8 - 9: 9 - t: 10 - j: jack - q: queen - k: king - a: ace
            i.e. c2 is the 2 of clubs, d3 is the 3 of diamonds, h4 is the 4 of hearts, etc.
        */
        public List<string> refreeActionHistory = new List<string>(); // the full history of the game 
        public List<List<string>> playerActionHistory = new List<List<string>>(); // The actions players have taken
        public List<List<Card>> playerHands = new List<List<Card>>(); // The cards in each player's hand
        public List<Card> cardHistory = new List<Card>(); // The cards only both of the players see on the table
        public int initialDeckSize; // The initial size of the deck
        public List<Card> deck = new List<Card>(); // The deck of cards
        public List<Dictionary<string, List<Card>>> playerBluffableCards = new List<Dictionary<string, List<Card>>>();
        public int currentPlayer; // The current player
        public int numValidActions; // The number of valid actions
        public List<int> moveOrder = new List<int>(); // The order in which the players took their actions

        public BluffStop()
        {
            this.currentPlayer = 0;

            // Initialize the game
            setupDeck();
            shuffleCards();
            dealCards();

            // Put the first card on the table
            placeCardOnTable();
        }

        // Copy constructor
        public BluffStop(BluffStop bluffStop)
        {
            this.currentPlayer = bluffStop.currentPlayer;
            this.initialDeckSize = bluffStop.initialDeckSize;
            this.refreeActionHistory = new List<string>(bluffStop.refreeActionHistory);
            this.playerActionHistory = new List<List<string>>(bluffStop.playerActionHistory);
            this.playerHands = new List<List<Card>>(bluffStop.playerHands);
            this.cardHistory = new List<Card>(bluffStop.cardHistory);
            this.deck = new List<Card>(bluffStop.deck);
            this.playerBluffableCards = new List<Dictionary<string, List<Card>>>(bluffStop.playerBluffableCards);
            this.numValidActions = bluffStop.numValidActions;
        }

        public void setupDeck()
        {
            // Initialize the deck
            string[] suits = { "c", "d", "h", "s" };
            string[] numbers = { "2", "3", "4", "5", "6", "7", "8", "9", "t", "j", "q", "k", "a" };

            playerBluffableCards.Add(new Dictionary<string, List<Card>>());
            playerBluffableCards.Add(new Dictionary<string, List<Card>>());

            playerActionHistory.Add(new List<string>());
            playerActionHistory.Add(new List<string>());

            foreach (string suit in suits)
            {
                foreach (string number in numbers)
                {
                    this.deck.Add(new Card(suit, number));
                    for (int i = 0; i < 2; i++)
                    {
                        if (!playerBluffableCards[i].ContainsKey(suit))
                        {
                            playerBluffableCards[i][suit] = new List<Card>();
                        }
                        playerBluffableCards[i][suit].Add(new Card(suit, number));
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                playerBluffableCards[i]["w"] = new List<Card>();
                playerBluffableCards[i]["w"].Add(new Card("w", "0"));
            }
        }

        public void shuffleCards()
        {
            // shuffle cards
            Random rnd = new Random();
            int deckSize = deck.Count;
            for (int i = 0; i < deckSize; i++)
            {
                int j = rnd.Next(i, deckSize);
                Card temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        public void dealCards()
        {
            playerHands.Add(new List<Card>());  // Player 0
            playerHands.Add(new List<Card>());  // Player 1
            // Deal cards to players
            for (int i = 0; i < 7; i++)
            {
                playerHands[0].Add(deck[i]);
                playerHands[1].Add(deck[i + 7]);
                // Remove the dealt cards from the deck
                deck.RemoveAt(i);
                deck.RemoveAt(i + 7);
            }
        }

        public void placeCardOnTable()
        {
            revealCard(deck[0]);
            // Remove the card from the deck
            refreeActionHistory.Add("X" + deck[0].ToString());
            deck.RemoveAt(0);
        }

        public void drawCards(int player, int numCards)
        {
            for (int i = 0; i < numCards; i++)
            {
                if (deck.Count > 0)
                {
                    playerHands[player].Add(deck[0]);
                    // Remove the dealt cards from the deck
                    deck.RemoveAt(0);
                }
            }
        }

        public void revealCard(Card card)
        {
            // Reveal the card on the table
            cardHistory.Add(card);
            // Update the bluff cards
            for (int i = 0; i < 2; i++)
            {
                playerBluffableCards[i][card.suit].Remove(card); // ? Test if works properly
            }
        }

        public void updatePlayer()
        {
            // Switch to the next player
            currentPlayer = 1 - currentPlayer;
        }

        public bool isTerminal()
        {
            // return true if game is over
            // The game is over if the deck is empty or if one player has no cards
            return (deck.Count == 0 || playerHands[0].Count == 0 || playerHands[1].Count == 0);
        }

        public string getInfoState(int player = -1)
        {
            // return info state of player
            // Information State includes (in order):
            // 0. Player
            // 1. The cards on the player's hand
            // 2. The cards on the table
            // (The cards player can bluff are already deducable from 1, 2)
            // 3. Action history for the player
            if (player == -1)
            {
                player = currentPlayer;
            }
            string infoState = "";
            // Always keep the card ordered
            infoState += player.ToString() + "\t";
            infoState += String.Join("", playerHands[player].Select(x => x.ToString()).ToArray()) + "\t";
            infoState += String.Join("", cardHistory.Select(x => x.ToString()).ToArray()) + "\t";
            infoState += String.Join("", playerActionHistory[player].Select(x => x.ToString()).ToArray());
            return infoState;
        }

        public int getUtility(int player = -1)
        {
            // return utility of player
            if (player == -1)
            {
                player = currentPlayer;
            }
            // If the player has no cards, they win
            if (playerHands[player].Count == 0)
            {
                return 1;
            }
            else if (playerHands[1 - player].Count == 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public Card getLastClaimedCard()
        {
            // return the last move claimed by the previous player
            // use refreeActionHistory
            if (refreeActionHistory.Count == 0) {
                // this should never happen
                throw new Exception("No previous move");
            }
            // check history until found a card
            int i = refreeActionHistory.Count - 1;
            while (i >= 0 && refreeActionHistory[i].Length < 3)
            {
                i--;
            }
            if (i < 0)
            {
                // this should never happen
                throw new Exception("No previous move");
            } 
            string lastCard = refreeActionHistory[i];
            return new Card(lastCard.Substring(1, 1), lastCard.Substring(2, 1));
        }

        public List<string> validMoves(Card oppClaimedCard, int player = -1)
        {
            // return the valid moves for player
            if (player == -1)
            {
                player = currentPlayer;
            }
            List<string> validMoves = new List<string>();
            // Honest moves
            for (int i = 0; i < playerHands[player].Count; i++)
            {
                if (playerHands[player][i].greaterThan(oppClaimedCard))
                {
                    validMoves.Add("H" + playerHands[player][i].ToString());
                }
            }
            // Bluff moves
            for (int i = 0; i < playerBluffableCards[player][oppClaimedCard.suit].Count; i++)
            {
                if (playerBluffableCards[player][oppClaimedCard.suit][i].greaterThan(oppClaimedCard) &&
                    playerBluffableCards[player][oppClaimedCard.suit][i] != oppClaimedCard)
                {
                    for (int j = 0; j < playerHands[player].Count; j++)
                    {
                        validMoves.Add("B" + playerBluffableCards[player][oppClaimedCard.suit][i].ToString() + playerHands[player][j].ToString());
                    }
                }
            }
            validMoves.Add("P");
            // If first move, or you were the last player you can't call bluff
            if (moveOrder.Count < 1 || moveOrder[moveOrder.Count - 1] == player)
            {}
            // Only if the last move was a bluff or honest move
            else if (refreeActionHistory[refreeActionHistory.Count - 1][0] == 'B' || refreeActionHistory[refreeActionHistory.Count - 1][0] == 'H')
            {
                validMoves.Add("C");
            }
            numValidActions = validMoves.Count;
            return validMoves;
        }

        public void applyAction(string action, int player = -1)
        {
            // Todo: Take into considiration; the start of the game, the end of the game
            // apply action to current player
            // honest actions; bluff actions; pass; call bluff
            // always in order
            // Todo: check if valid action
            if (player == -1)
            {
                player = currentPlayer;
            }
            string move = action;
            moveOrder.Add(player);

            // update history
            refreeActionHistory.Add(move);
            playerActionHistory[player].Add(move);

            // update cards
            if (move[0] == 'H')
            {
                // honest action
                Card card = new Card(move[1], move[2]);
                playerHands[player].Remove(card);
            }
            else if (move[0] == 'P')
            {
                // pass
                // do nothing
            }
            else if (move[0] == 'C')
            {
                // call bluff
                // do nothing
            }
            else if (move[0] == 'B')
            {
                // bluff action
                // Card bluffCard = new Card(move[1], move[2]); 
                Card realCard = new Card(move[3], move[4]);
                playerHands[player].Remove(realCard);
            }
            else
            {
                throw new System.Exception("Invalid action encoding");
            }
            
            Card claimed = getLastClaimedCard();
            playerActionHistory[1 - player].Add("o" + claimed.ToString());

            // iterate the game
            if (move[0] == 'P')
            {
                // place a card on the table
                placeWildCardOnTable();
            }
            else if (move[0] == 'C')
            {
                // check if the last card is a bluff
                // if so, opponent draws 2 cards
                // if not, player draws 3 cards
                // the last card is revealed
                // turn continues from the current player
                int lastCard = 0;
                while (lastCard < refreeActionHistory.Count && refreeActionHistory[lastCard].Length < 3)
                {
                    lastCard++;
                }
                string lastCardEnc;
                if (refreeActionHistory[lastCard][0] == 'B')
                {
                    // opponent draws 2 cards
                    drawCards(1 - player, 2);
                    // reveal the last card
                    lastCardEnc = refreeActionHistory[lastCard].Substring(4, 2);
                    currentPlayer = 1 - currentPlayer;
                }
                else
                {
                    // player draws 3 cards
                    drawCards(player, 3);
                    // reveal the last card
                    lastCardEnc = refreeActionHistory[lastCard].Substring(1, 2);
                }
                Card lastCardRevealed = new Card(lastCardEnc[0], lastCardEnc[1]);
                revealCard(lastCardRevealed);
            }
            updatePlayer(); // ? Is it always the next player? no
        }

        public void placeWildCardOnTable(){
            // place a wild card on the table
            // wild card value is 0
            // suit is 'w'
            Card wildCard = new Card('w', '0');
            cardHistory.Add(wildCard);
            refreeActionHistory.Add("Xw0");
        }
        
        public BluffStop clone()
        {
            // clone the current game
            BluffStop newGame = new BluffStop(this);
            return newGame;
        }
    }
}