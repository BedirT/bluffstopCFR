namespace bluffstopCFR
{
    // Blueprint for a player
    public class HumanPlayer : Player
    {
        public HumanPlayer(Dictionary<string, Node> strategy = null) : base(strategy)
        {
        }

        public override int getAction(BluffStop game, List<string> moves)
        {
            // Print some information
            Console.WriteLine("Current player: " + game.currentPlayer);
            // The card in the middle
            Console.WriteLine("Last claimed card: " + game.getLastClaimedCard());
            System.Console.WriteLine("Last face up card: " + game.cardHistory[game.cardHistory.Count - 1]);
            // The cards in the hand
            Console.WriteLine("Cards in hand: ");
            System.Console.WriteLine(String.Join(", ", game.playerHands[game.currentPlayer]));
            // get the action
            Console.WriteLine("Your moves are: ");
            for (int i = 0; i < moves.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, moves[i]);
            }
            Console.WriteLine("Enter your move: ");
            string move_str = Console.ReadLine();
            int move_int = int.Parse(move_str);
            return move_int;
        }

        public override Player clone()
        {
            // clone the player
            return new HumanPlayer(this.strategy);
        }
    }
} // end namespace bluffstopCFR