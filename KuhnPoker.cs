namespace bluffstopCFR {

    // Kuhn Poker with Game class
    public class KuhnPoker : Game {
        // game parent class
        public List<int> cards;
        public List<int> playerHands;
        public string actionHistory;
        public double[] utility = { 0, 0 };
        // init
        public KuhnPoker() {
            cards = new List<int>();
            playerHands = new List<int>();
            actionHistory = "";
            reset();
        }
        // setup
        public void gameSetup() {
            if (cards.Count == 0) {
                cards.Add(0);
                cards.Add(1);
                cards.Add(2);
            }
            // shuffle the cards
            Random rnd = new Random();
            for (int i = 0; i < cards.Count; i++) {
                int j = rnd.Next(i, cards.Count);
                int temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
            // deal the cards
            if (playerHands.Count != 0) {
                playerHands.Clear();
            }
            playerHands.Add(cards[0]);
            playerHands.Add(cards[1]);
        }
        public override int getUtility(int player = -1){
            // Return the utility for the given player
            // If player is -1, return the utility for the current player
            if (player == -1) {
                player = currentPlayer;
            }
            return (int)utility[player];
        }
        public override bool isTerminal()
        {
            // Return true if the game is over
            // BB / PP / BP terminal
            if (actionHistory.Length < 2) {
                return false;
            }
            string last_two_actions = actionHistory.Substring(actionHistory.Length - 2);
            if (last_two_actions == "BB" || last_two_actions == "PP" || last_two_actions == "BP") {
                // if PP +1 to player with higher card
                // if BP +1 to player who bet
                // if BB +2 to player with higher card
                if (last_two_actions == "PP"){
                    if (playerHands[0] > playerHands[1]) {
                        utility = new double[] { 1, -1 };
                    } else {
                        utility = new double[] { -1, 1 };
                    }
                } else if (last_two_actions == "BP") {
                    utility = new double[] { 1, -1 };
                } else if (last_two_actions == "BB") {
                    if (playerHands[0] > playerHands[1]) {
                        utility = new double[] { 2, -2 };
                    } else {
                        utility = new double[] { -2, 2 };
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }
        public override string getInfoState(int player = -1)
        {
            // Return the information state for the given player
            // If player is -1, return the information state for the current player
            // player hand + action history
            if (player == -1) {
                player = currentPlayer;
            }
            return "P" + player.ToString() + playerHands[player].ToString() + actionHistory;
        }
        public override List<int> legalActions(int player = -1)
        {
            // Return the valid moves for the given player
            // If player is -1, return the valid moves for the current player
            // 0: B bet | 1: P pass
            if (player == -1) {
                player = currentPlayer;
            }
            return new List<int> { 0, 1 };
        }
        public override void applyAction(int action, int player = -1){
            // Apply the given action for the given player
            // If player is -1, apply the action for the current player
            if (player == -1) {
                player = currentPlayer;
            }
            if (action == 0) {
                actionHistory += "B";
            }
            else if (action == 1) {
                actionHistory += "P";
            }
            else {
                throw new Exception("Invalid action");
            }
            currentPlayer = 1 - currentPlayer;
        }
        // reset
        public void reset()
        {
            // Reset the game
            currentPlayer = 0;
            actionHistory = "";
            gameSetup();
        }
    }

}