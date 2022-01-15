namespace bluffstopCFR {
    class RandomPlayer : Player {
        public RandomPlayer(){}
        
        public override int getAction(BluffStop game, List<string> moves) {
            Random rnd = new Random();
            int action = rnd.Next(moves.Count);
            return action;
        }
        
        public override Player clone() {
            return new RandomPlayer();
        }
    }
} // end namespace bluffstopCFR