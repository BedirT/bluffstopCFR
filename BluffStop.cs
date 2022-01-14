using System;
using System.Collections.Generic;


namespace bluffstopCFR {
    public class Card {
        
        public int rank;
        public int suit;
        public Card (string suit, int rank) {
            this.suit = suit;
            this.rank = rank;
        }
        public override string ToString () {
            return suit + rank;
        }
    }
    public class BluffStop{
    /*
        Bluff Stoppers is an imperfect information two-player zero-sum card game.
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
        
        public List<Dictionary<string, List<int>>> playerBluffableCards = new  List<Dictionary<string, List<int>>>();
        public int currentPlayer; // The current player

        public BluffStop() {
            this.currentPlayer = 0;

            // Initialize the game
            setupDeck(); 
            shuffleCards();
            dealCards();

            // Put the first card on the table
            placeCardOnTable();
        }

        // Copy constructor
        public BluffStop(BluffStop bluffStop) {
            this.currentPlayer = bluffStop.currentPlayer;
            this.initialDeckSize = bluffStop.initialDeckSize;
            this.refreeActionHistory = new List<string>(bluffStop.refreeActionHistory);
            this.playerActionHistory = new List<List<string>>(bluffStop.playerActionHistory);
            this.playerHands = new List<List<string>>(bluffStop.playerHands);
            this.cardHistory = new List<string>(bluffStop.cardHistory);
            this.deck = new List<string>(bluffStop.deck);
            this.bluffCards = new List<List<string>>(bluffStop.bluffCards);
        }

        public void setupDeck(){
            // Initialize the deck
            string[] suits = {"c", "d", "h", "s"};
            string[] numbers = {"2", "3", "4", "5", "6", "7", "8", "9", "t", "j", "q", "k", "a"};
            foreach(string suit in suits){
                foreach(string number in numbers){
                    deck.Add(suit + number);
                    // update bluff cards
                    bluffCards[0].Add(suit + number);
                    bluffCards[1].Add(suit + number);
                }
            }
        }

        public void shuffleCards(){
            // shuffle cards
            Random rnd = new Random();
            int deckSize = deck.Count;
            for (int i = 0; i < deckSize; i++){
                int j = rnd.Next(i, deckSize);
                string temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        public void dealCards(){
            // Deal cards to players
            for (int i = 0; i < 7; i++){
                playerHands[0].Add(deck[i]);
                playerHands[1].Add(deck[i + 7]);
                // Remove the dealt cards from the deck
                deck.RemoveAt(i);
                deck.RemoveAt(i + 7);
            }
        }

        public void placeCardOnTable(){
            revealCard(deck[0]);
            // Remove the card from the deck
            deck.RemoveAt(0);
        }

        public void drawCards(int player, int numCards){
            // Draw cards from the deck
            for (int i = 0; i < numCards; i++){
                playerHands[player].Add(deck[i]);
                // Remove the dealt cards from the deck
                deck.RemoveAt(i);
            }
        }

        public void revealCard(string card){
            // Reveal the card on the table
            cardHistory.Add(card);
            // Update the bluff cards
            for (int i = 0; i < 2; i++){
                for (int j = 0; j < bluffCards[i].Count; j++){
                    if (bluffCards[i][j] == card){
                        bluffCards[i].RemoveAt(j);
                        break; // ? Is it one loop or both (should be one)
                    }
                }
            }
        }

        public void updatePlayer(){
            // Switch to the next player
            currentPlayer = 1 - currentPlayer;
        }

        public bool isTerminal(){
            // return true if game is over
            // The game is over if the deck is empty or if one player has no cards
            return (deck.Count == 0 || playerHands[0].Count == 0 || playerHands[1].Count == 0);
        }

        public string getInfoState(int player = -1){
            // return info state of player
            // Information State includes (in order):
            // 1. The cards on the player's hand
            // 2. The cards on the table
            // (The cards player can bluff are already deducable from 1, 2)
            // 3. Action history for the player
            if (player == -1){
                player = currentPlayer;
            }
            string infoState = "";
            infoState += String.Join("", playerHands[player]);
            infoState += String.Join("", cardHistory);
            infoState += String.Join("", playerActionHistory[player]);
            return infoState;
        }

        public int getUtility(int player = -1) {
            // return utility of player
            if (player == -1){
                player = currentPlayer;
            }
            // If the player has no cards, they win
            if (playerHands[player].Count == 0){
                return 1;
            } else if (playerHands[1 - player].Count == 0){
                return -1;
            } else {
                return 0;
            }
        }

        public int getNumActions(string oppClaim, int player = -1){
            // return number of actions
            if (player == -1){
                player = currentPlayer;
            } 
            int num_cards = playerHands[player].Count;
            // how many cards are not seen yet and greater than the current card (same suit)
            string oppSuit = oppClaim[0];
            int opponentValue = int.Parse(oppClaim[1]);
            int num_actions = 0;
            for (int i = 0; i < num_cards; i++){
                string card = playerHands[player][i];
                if (card[0] == oppSuit && int.Parse(card[1]) > opponentValue){
                    num_actions++;
                }
            }
            int bluffActionSize = initialDeckSize - cardHistory.Count - num_cards;
            return bluffActionSize * num_cards + num_cards + 2;
        }

        public void applyAction(int action, int player = -1){
            // apply action to current player
            // honest actions; bluff actions; fold/pass; call bluff
            // always in order
            // check if valid action
            int num_acts = getNumActions(player);
            if (action < 0 || action >= num_acts){
                throw new System.Exception("Invalid action");
            }
            if (player == -1){
                player = currentPlayer;
            }
            // Action encoding
            string enc = "";
            if (action < playerHands[player].Count){
                // honest action
                enc = "H" + playerHands[player][action];
            } else if (action == num_acts - 1){
                // call bluff
                enc = "C";
            } else if (action == num_acts - 2){
                // pass
                enc = "P";
            } else {
                // bluff action
                int bluff_action = action - playerHands[player].Count;
                string bluffCard = getBluffCard(player, bluff_action);
                string realCard = getRealCard(player, bluff_action);
                enc = "B" + bluffCard + "R" + realCard;
            }

            // update history
            refreeActionHistory.Add(enc);
            playerActionHistory[player].Add(enc);

            // update cards
            if (enc[0] == 'H'){
                // honest action
                string card = enc.Substring(1);
                playerHands[player].Remove(card);
            } else if (enc[0] == 'P'){
                // pass
                // do nothing
            } else if (enc[0] == 'C'){
                // call bluff
                // do nothing
            } else if (enc[0] == 'B'){
                // bluff action
                string realCard = enc.Substring(4, 2);
                playerHands[player].Remove(realCard);
            } else {
                throw new System.Exception("Invalid action encoding");
            }
 
            // iterate the game
            if (enc[0] == 'P'){
                // place a card on the table
                placeCardOnTable();
            } else if (enc[0] == 'C'){
                // check if the last card is a bluff
                // if so, opponent draws 2 cards
                // if not, player draws 3 cards
                // the last card is revealed
                // turn continues from the next player
                int lastCard = cardHistory.Count - 1;
                string lastCardEnc;
                if (refreeActionHistory[lastCard][0] == 'B'){
                    // opponent draws 2 cards
                    drawCards(1 - player, 2);
                    // reveal the last card
                    lastCardEnc = refreeActionHistory[lastCard].Substring(4, 2);
                } else {
                    // player draws 3 cards
                    drawCards(player, 3);
                    // reveal the last card
                    lastCardEnc = refreeActionHistory[lastCard].Substring(1, 2);
                }
                revealCard(lastCardEnc);
            } 
            updatePlayer(); // ? Is it always the next player?
        }

        public string getBluffCard(int player, int bluff_action){
            // return the bluff card
            // bluff cards are encoded as:
            // B<bluff card>R<real card>
            // This gives us number of bluff cards times the number of real cards
            // many action possibilities.
            return bluffCards[player][bluff_action % playerHands[player].Count];
        }

        public string getRealCard(int player, int bluff_action){
            // return the real card
            int action_index = bluff_action / playerHands[player].Count;
            return playerHands[player][action_index];
        }

        public BluffStop clone(){
            // clone the current game
            BluffStop newGame = new BluffStop(this);
            return newGame;
        }
    }

    public class GameTest{
        public static void test(){
            // test the game
            BluffStop game = new BluffStop(true);
            while (!game.isTerminal()){
                int action = game.getNumActions() - 1;
                game.applyAction(action);
            }
            Console.WriteLine("Game over");
        }
    }

}