using System.Collections.Generic;

namespace bluffstopCFR {
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
        public List<List<string>> playerHands = new List<List<string>>(); // The cards in each player's hand
        public List<string> cardHistory = new List<string>(); // The cards both players see on the table
        public int initialDeckSize = 52; // The initial size of the deck


        
        public BluffStoppers(){
            shuffleCards();
        }

        public void shuffleCards(){
            // shuffle cards

        }

        public int currentPlayer(){
            // return current player
            return 0;
        }

        public bool isTerminal(){
            // return true if game is over
            return false;
        }

        public string getInfoState(int player = -1){
            // return info state of player
            if (player == -1){
                player = currentPlayer();
            }
            return "";
            // ...
        }

        public int getUtility(int player = -1) {
            // return utility of player
            if (player == -1){
                player = currentPlayer();
            }
            // ...
            return 0;
        }

        public int getNumActions(int player = -1){
            // return number of actions
            if (player == -1){
                player = currentPlayer();
            } 
            int num_cards = playerHands[player].Count;
            // how many cards are not seen yet?
            int bluffActionSize = initialDeckSize - cardHistory.Count - num_cards;
            return bluffActionSize * num_cards + num_cards + 2;
        }

        public void applyAction(int action, int player = -1){
            // apply action to current player
            // honest actions; bluff actions; fold/pass; call bluff
            // always in order
            // check if valid action
            int num_acts = getNumActions();
            if (action < 0 || action >= num_acts){
                throw new System.Exception("Invalid action");
            }
            if (player == -1){
                player = currentPlayer();
            }
            // Action encoding
            string enc = "";
            if (action < playerHands[player].Count){
                // honest action
                enc = "H" + playerHands[player][action];
            } else if (action == num_acts - 2){
                // fold
                enc = "F";
            } else if (action == num_acts - 1){
                // pass
                enc = "P";
            } else {
                // bluff action
                int bluff_action = action - playerHands[player].Count;
                string bluffCard = getBluffCard(bluff_action);
                string realCard = getRealCard(bluff_action);
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
            } else if (enc[0] == 'F'){
                // fold
                // do nothing
            } else {
                // bluff action
                string bluffCard = enc.Substring(1, 2);
                string realCard = enc.Substring(4, 2);
                playerHands[player].Remove(bluffCard);
                if (bluffCard != realCard){
                    // bluff was called
                    playerHands[player].Add(realCard);
                }
            }
                

        }

        public BluffStoppersWrapper clone(){
            // clone the current game

        }
    }
}