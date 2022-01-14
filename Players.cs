using System;
using System.Collections.Generic;
using System.Linq;

namespace VanillaCFR {
    class HonestPlayer : Player //Denna spelare bluffar aldrig, och tror aldrig att moståndaren bluffar
        {
            Card kortJagSpelade; //Håller reda på vilket kort som spelaren lagt.
            public HonestPlayer()
            {
                Name = "HonestPlayer";
            }

            public override bool BluffStopp(int cardValue, Suit cardSuit, int cardValueToBeat)
            {
                return false; //Säger aldrig bluffstopp!
            }

            public override Card LäggEttKort(int cardValue, Suit cardSuit)
            {
                Hand = Game.SortHandByValue(Hand); //Lägger de lägsta korten först i handen.
                if (cardSuit == Suit.Wild) //Om valfritt kort får spelas
                {
                    Game.StateReason("Jag kan lägga vad jag vill, så jag lägger mitt lägsta kort");
                    kortJagSpelade = Hand[0]; //Sparar kortet som skall spelas 
                    return Hand[0]; //Spelar ut det första kortet på handen
                }
                for (int i = 0; i < Hand.Count; i++) //Går igenom alla korten på handen
                {
                    if (Hand[i].Suit == cardSuit && Hand[i].Value > cardValue) //Om den hittar ett kort på handen som är högre och i samma suit som det som ligger
                    {
                        Game.StateReason("Jag lägger det lägsta kortet jag kan");
                        kortJagSpelade = Hand[i]; //Sparar kortet som skall spelas 
                        return Hand[i]; //Spelar ut det första kort den hittar som är högre och i samma suit
                    }
                }
                Game.StateReason("Jag kan inte lägga något kort och tänker inte bluffa");
                return null; //Om inget kort hittats säger spelaren pass
            }

            public override Card SägEttKort(int cardValue, Suit cardSuit)
            {
                Game.StateReason("Jag bluffar aldrig");
                return kortJagSpelade; //Talar alltid sanning!
            }


            public override void SpelSlut(int cardsLeft, int opponentCardsLeft)
            {
                //Behöver inte användas.
            }
        }

        class MyPlayer : Player //Denna spelare skall du göra till din egen! Nu har den exakt samma strategi som HonestPlayer.
        {
            Card kortJagSpelade; //Håller reda på vilket kort som spelaren lagt.
            public MyPlayer()
            {
                Name = "MyPlayer";
            }

            public override bool BluffStopp(int cardValue, Suit cardSuit, int cardValueToBeat)
            {
                return false; //Säger aldrig bluffstopp!
            }

            public override Card LäggEttKort(int cardValue, Suit cardSuit)
            {
                Hand = Game.SortHandByValue(Hand); //Lägger de lägsta korten först i handen.
                if (cardSuit == Suit.Wild) //Om valfritt kort får spelas
                {
                    Game.StateReason("Jag kan lägga vad jag vill, så jag lägger mitt lägsta kort");
                    kortJagSpelade = Hand[0]; //Sparar kortet som skall spelas 
                    return Hand[0]; //Spelar ut det första kortet på handen
                }
                for (int i = 0; i < Hand.Count; i++) //Går igenom alla korten på handen
                {
                    if (Hand[i].Suit == cardSuit && Hand[i].Value > cardValue) //Om den hittar ett kort på handen som är högre och i samma suit som det som ligger
                    {
                        Game.StateReason("Jag lägger det lägsta kortet jag kan");
                        kortJagSpelade = Hand[i]; //Sparar kortet som skall spelas 
                        return Hand[i]; //Spelar ut det första kort den hittar som är högre och i samma suit
                    }
                }
                Game.StateReason("Jag kan inte lägga något kort och tänker inte bluffa");
                return null; //Om inget kort hittats säger spelaren pass
            }

            public override Card SägEttKort(int cardValue, Suit cardSuit)
            {
                Game.StateReason("Jag bluffar aldrig");
                return kortJagSpelade; //Talar alltid sanning!
            }


            public override void SpelSlut(int cardsLeft, int opponentCardsLeft)
            {
                //Behöver inte användas.
            }
        }

        class RandomPlayer : Player
        //Denna spelare bluffar ibland och synar(säger bluffstopp) ibland beroende på vilka värden som skrivs in i konstruktorn
        {
            Card kortJagSpelade; //Håller reda på vilket kort som spelaren lagt.
            int BluffProcent;  //Sannolikheten för bluff(i %) om spelaren inte har ett ok kort att spela
            int SynProcent;  //Sannolikheten för Bluffstopp (i %)
            bool Bluff;  //Håller koll på om spelaren bluffat
            Random RNG;

            public RandomPlayer(int bluffProcent, int synProcent)  //Konstruktor
            {
                Name = "Random " + bluffProcent + " " + synProcent;
                BluffProcent = bluffProcent;
                SynProcent = synProcent;
                RNG = new Random();
            }

            public override bool BluffStopp(int cardValue, Suit cardSuit, int cardValueToBeat)
            {
                if (SynProcent > RNG.Next(100)) //Slumpar fram om den skall säga bluffstopp
                {
                    Game.StateReason("Jag slumpar fram ett bluffstopp. Sannolikhet " + SynProcent + " %");
                    return true;
                }
                else
                {
                    //Man behövet inte ange en anledning till att inte säga bluffstopp
                    return false;
                }
            }

            public override Card LäggEttKort(int cardValue, Suit cardSuit)
            {
                Bluff = false;
                Hand = Game.SortHandByValue(Hand); //Lägger de lägsta korten först i handen.
                if (cardSuit == Suit.Wild) //Om valfritt kort får spelas
                {
                    Game.StateReason("Jag kan lägga vad jag vill, så jag lägger mitt lägsta kort");
                    kortJagSpelade = Hand[0]; //Sparar kortet som skall spelas 
                    return Hand[0]; //Spelar ut det första kortet på handen
                }
                for (int i = 0; i < Hand.Count; i++) //Går igenom alla korten på handen
                {
                    if (Hand[i].Suit == cardSuit && Hand[i].Value > cardValue) //Om den hittar ett kort på handen som är högre och i smma suit som det som ligger
                    {
                        Game.StateReason("Jag lägger det lägsta kortet jag kan.");
                        kortJagSpelade = Hand[i]; //Sparar kortet som skall spelas 
                        return Hand[i]; //Spelar ut det första kort den hittar som är högre och i samma suit
                    }
                }
                if (BluffProcent > RNG.Next(100)) //Om inget Ok kort hittats slumpar spelaren om den tänker bluffa 
                {
                    Game.StateReason("Jag slumpar fram en bluff, sannolikhet " + BluffProcent + " %");
                    Bluff = true;
                    return Hand[0]; //Här tänker spelaren bluffa och spelar ut sitt lägsta kort
                }
                else
                {
                    Game.StateReason("Jag kan inte lägga något kort, och säger pass");
                    Bluff = false;
                    return null;  //Spelaren väljer att inte bluffa och säger pass
                }
            }

            public override Card SägEttKort(int cardValue, Suit cardSuit)
            {
                if (Bluff == true) //Om spelaren bestämt sig för att bluffa
                {
                    int fakeCardValue = cardValue + 1;  //Sätter värdet på fake-kortet till ett högre än det som ligger
                    Game.StateReason("Jag säger alltid att kortet är 1 högre");
                    return new Card(fakeCardValue, cardSuit);  //Skapar fake-kortet och returnerar det
                }
                else
                {
                    Game.StateReason("Jag bluffar inte");
                    return kortJagSpelade; //Om spelaren valt att inte bluffa, säger samma kort som den spelat.
                }
            }

            public override void SpelSlut(int cardsLeft, int opponentCardsLeft)
            {
                //Behöver inte användas.
            }
        }
    }   

}

    