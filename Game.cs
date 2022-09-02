namespace bluffstopCFR {

    // abstract class for games
    public abstract class Game {
        // game parent class
        public int currentPlayer;
        public abstract int getUtility(int player = -1);
        public abstract bool isTerminal();
        public abstract string getInfoState(int player = -1);
        public abstract List<int> legalActions(int player = -1);
        public abstract void applyAction(int action, int player = -1);
    }
}
