using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace TacticalText
{
    // Violating SRP by mixing player-related functionality, file I/O, and violating encapsulation by having public setters
    public class Player
    {
        public Player(string side) {
            Side = side;
            Budget = 500;
        }
        
        public string Side { get; set; }
        public int Budget { get; set; }
        public List<object> Units { get; set; } // Violating encapsulation by having a public setter

        // Violating SRP by having logic unrelated to players or units
        public static void LogPlayerActivity(string message)
        {
            File.AppendAllText("activity_log.txt", $"{DateTime.Now}: {message}\n");
        }

        // Violating SRP by mixing player-related logic, file I/O, and directly accessing private property
        public void ChooseAndBuyUnits()
        {
            Console.WriteLine($"Unit Shop (Press y to buy, n to skip)");
            Console.WriteLine($"Customer: {Side} Player");
            Console.WriteLine("[You can buy a maximum of 3 types of units]");

            HashSet<int> purchasedUnitTypes = new HashSet<int>();

            while (Budget > 0 && purchasedUnitTypes.Count < 3)
            {
                Console.WriteLine($"Budget: ${Budget}");

                // Violating SRP by logging activity unrelated to the method
                LogPlayerActivity($"{Side} player bought a unit");

                // Violating SRP by mixing unit creation, buying logic, and directly accessing private property
                var unit = CreateUnit();
                Units.Add(unit);

                // Violating SRP by mixing unit creation, buying logic, and directly accessing private property
                Console.WriteLine($"Available {unit.GetType().Name}s: {Units.Count}");
            }
        }

        // Violating abstraction by returning an object instead of a more specific type
        private object CreateUnit()
        {
            var random = new Random();
            var unitTypeIndex = random.Next(0, 5);

            object unit = null;

            // Violating OCP by instantiating specific classes directly
            switch (unitTypeIndex)
            {
                case 0:
                    unit = new Infantry();
                    break;
                case 1:
                    unit = new Tank();
                    break;
                case 2:
                    unit = new Sniper();
                    break;
                case 3:
                    unit = new Medic();
                    break;
                case 4:
                    unit = new Engineer();
                    break;
                default:
                    break;
            }

            return unit;
        }
    }

    // Violating SRP by mixing game setup, player interaction logic, and directly accessing private property
    public class Game
    {
        public int CurrentMove { get; private set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        // Violating SRP by mixing game setup, player interaction logic, and directly accessing private property
        public void SetupGame()
        {
            Console.WriteLine("Welcome to Tactical Text!");

            // Violating SRP by directly accessing public setters
            Player1.Budget = 1000;

            Player1.ChooseAndBuyUnits();
            Player2.ChooseAndBuyUnits();
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        public void DeployUnits()
        {
            Console.WriteLine($"Move: {CurrentMove}");

            Console.WriteLine($"{Player1.Side} deploying units:");
            DeployUnits(Player1);

            Console.WriteLine($"{Player2.Side} deploying units:");
            DeployUnits(Player2);

            // Increment the move after deployment
            CurrentMove++;
        }

        // Helper method to deploy units for a player
        private void DeployUnits(Player player)
        {
            HashSet<Type> deployedUnitTypesThisTurn = new HashSet<Type>();

            foreach (object unit in player.Units)
            {
                // If the unit has already been deployed in this move, skip it
                if (deployedUnitTypesThisTurn.Contains(unit.GetType()))
                {
                    continue;
                }

                // If the unit has zero health or less, it cannot be deployed
                if (GetHealth(unit) <= 0)
                {
                    continue;
                }

                int availableUnits = player.Units.Count(u => u.GetType() == unit.GetType() && GetHealth(u) > 0);

                Console.WriteLine($"{GetUnitName(unit)}: Available {availableUnits}");

                Console.Write($"Enter the number of {GetUnitName(unit)}s to deploy (or press Enter to skip): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (int.TryParse(input, out int unitsToDeploy) && unitsToDeploy > 0 && unitsToDeploy <= availableUnits)
                {
                    // Update the unit's health to the number of deployed units for each move
                    SetHealth(unit, GetHealth(unit) - unitsToDeploy);

                    // Add the deployed unit type to the set to prevent redeployment in the same turn
                    deployedUnitTypesThisTurn.Add(unit.GetType());

                    Console.WriteLine($"{GetUnitName(unit)} deployed: {unitsToDeploy}");
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter a positive number within the available {GetUnitName(unit)}s.");
                    // Repeat the iteration to allow the player to input the correct value
                }
            }
        }

        // Helper method to get the number of units to deploy from the player
        private int GetUnitsToDeploy()
        {
            int unitsToDeploy;
            while (!int.TryParse(Console.ReadLine(), out unitsToDeploy) || unitsToDeploy < 0)
            {
                Console.WriteLine("Invalid input. Please enter a non-negative number.");
                Console.Write("Enter again: ");
            }
            return unitsToDeploy;
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        public void DisplayResults()
        {
            Console.WriteLine("Results after Move " + CurrentMove + ":");
            DisplayPlayerResults(Player1);
            DisplayPlayerResults(Player2);
        }

        // Helper method to display results for a player
        private void DisplayPlayerResults(Player player)
        {
            Console.WriteLine($"{player.Side} player:");

            // Group units by type and display remaining count
            var groupedUnits = player.Units.Where(unit => GetHealth(unit) > 0)
                                            .GroupBy(unit => unit.GetType())
                                            .Select(group => new
                                            {
                                                UnitType = group.Key.Name,
                                                Remaining = group.Count()
                                            });

            foreach (var unitGroup in groupedUnits)
            {
                Console.WriteLine($"{unitGroup.UnitType} - Remaining: {unitGroup.Remaining}");
            }

            Console.WriteLine();
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        public bool IsGameOver()
        {
            // Check if any player has all units defeated
            return Player1.Units.All(unit => GetHealth(unit) <= 0) || Player2.Units.All(unit => GetHealth(unit) <= 0);
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        public Player GetWinner()
        {
            // Check if the game is not over yet
            if (!IsGameOver())
                return null;

            // Determine the winner based on remaining unit health
            return Player1.Units.Sum(unit => GetHealth(unit)) > Player2.Units.Sum(unit => GetHealth(unit)) ? Player1 : Player2;
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private int GetHealth(object unit)
        {
            // Assume all units have a Health property
            var property = unit.GetType().GetProperty("Health");
            return property != null ? (int)property.GetValue(unit) : 0;
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private void SetHealth(object unit, int value)
        {
            // Assume all units have a Health property
            var property = unit.GetType().GetProperty("Health");
            if (property != null)
            {
                property.SetValue(unit, value);
            }
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private string GetUnitName(object unit)
        {
            // Assume all units have a Name property
            var property = unit.GetType().GetProperty("Name");
            return property != null ? (string)property.GetValue(unit) : "Unknown";
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private void CalculateOutcomes()
        {
            Console.WriteLine("Calculating outcomes...");

            foreach (Player player in new[] { Player1, Player2 })
            {
                foreach (object attacker in player.Units)
                {
                    foreach (object target in player == Player1 ? Player2.Units : Player1.Units)
                    {
                        Attack(attacker, target);
                    }
                }
            }

            // Display results after calculating outcomes
            DisplayResults();
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private void Attack(object attacker, object target)
        {
            // Assume all units have an Attack method
            var method = attacker.GetType().GetMethod("Attack");
            method?.Invoke(attacker, new[] { target });
        }

        // Violating SRP by mixing game logic, player interaction logic, and directly accessing private property
        private void Revive(object reviver, object target)
        {
            // Assume all units have a Revive method
            var method = reviver.GetType().GetMethod("Revive");
            method?.Invoke(reviver, new[] { target });
        }
    }

    // Violating SRP by having both attack and revive logic in one class
    public class Engineer
    {
        public string Name => "Engineer";
        public int Price => 10;
        public int Health { get; set; } = 1;

        // Violating SRP by having both attack and revive logic in one method
        public void Attack(object target)
        {
            // Violating OCP by directly instantiating a class (Concrete class rather than Interface)
            var random = new Random();
            var randomValue = random.Next(1, 10);

            // Violating OCP by using magic numbers and directly accessing private setters
            if (randomValue <= 5)
            {
                Console.WriteLine("Engineer failed to perform an attack.");
            }
            else
            {
                Console.WriteLine("Engineer successfully attacked the target.");
            }
        }

        // Violating SRP by having both attack and revive logic in one method
        public void Revive(object target)
        {
            // Violating LSP by checking specific type
            if (target is Tank tankTarget)
            {
                // Violating encapsulation by directly accessing a private field
                tankTarget.health = Math.Min(tankTarget.health + 1, 3);
            }
        }

        // Violating SRP by having both attack and revive logic in one method
        private void PerformSpecialEngineerAction(object target)
        {
            // Violating encapsulation by directly accessing a private field
            var targetHealth = (int)target.GetType().GetProperty("health")?.GetValue(target);

            // Violating magic numbers and encapsulation by using a constant directly
            if (targetHealth <= 2)
            {
                Console.WriteLine("Performing special engineer action...");
            }
        }
    }

    // Violating SRP by having both attack and revive logic in one class
    public class Sniper
    {
        public string Name => "Sniper";
        public int Price => 20;
        public int Health { get; set; } = 2;

        // Violating SRP by having both attack and revive logic in one method
        public void Attack(object target)
        {
            // Violating OCP by using magic numbers and directly accessing private setters
            if (Health > 0)
            {
                Console.WriteLine("Sniper is attacking the target.");
            }
            else
            {
                Console.WriteLine("Sniper is out of action and cannot attack.");
            }
        }
    }

    // Violating SRP by having both attack and revive logic in one class
    public class Tank
    {
        public string Name => "Tank";
        public int Price => 30;
        public int health = 3; // Violating encapsulation by using a public field

        // Violating SRP by having both attack and revive logic in one method
        public void Attack(object target)
        {
            // Violating OCP by using magic numbers and directly accessing private setters
            if (health > 0)
            {
                Console.WriteLine("Tank is attacking the target.");
            }
            else
            {
                Console.WriteLine("Tank is out of action and cannot attack.");
            }
        }
    }

    // Violating SRP by having both attack and revive logic in one class
    public class Infantry
    {
        public string Name => "Infantry";
        public int Price => 5;
        public int Health { get; set; } = 0;

        // Violating SRP by having both attack and revive logic in one method
        public void Attack(object target)
        {
            // Violating OCP by using magic numbers and directly accessing private setters
            if (Health > 0)
            {
                Console.WriteLine("Infantry is attacking the target.");
            }
            else
            {
                Console.WriteLine("Infantry is out of action and cannot attack.");
            }
        }
    }

    // Violating SRP by having both attack and revive logic in one class
    public class Medic
    {
        public string Name => "Medic";
        public int Price => 10;
        public int Health { get; set; } = 1;

        // Violating SRP by having both attack and revive logic in one method
        public void Attack(object target)
        {
            // Violating OCP by using magic numbers and directly accessing private setters
            if (Health > 0)
            {
                Console.WriteLine("Medic is attacking the target.");
            }
            else
            {
                Console.WriteLine("Medic is out of action and cannot attack.");
            }
        }

        // Violating SRP by having both attack and revive logic in one method
        public void Revive(object target)
        {
            // Violating LSP by checking specific type
            if (target is Tank tankTarget)
            {
                // Violating encapsulation by directly accessing a private field
                tankTarget.health = Math.Min(tankTarget.health + 1, 3);
            }
        }
    }

    // Main class for entry point


    public class Program
    {
        public static void Main(string[] args)
        {
            // Violating SRP by directly accessing public setters
            Player player1 = new Player("Allied") { Budget = 500 };
            Player player2 = new Player("Axis") { Budget = 500 };

            Game tacticalTextGame = new Game { Player1 = player1, Player2 = player2 };

            tacticalTextGame.SetupGame();

            while (!tacticalTextGame.IsGameOver())
            {
                Console.Clear();

                tacticalTextGame.DeployUnits();

                tacticalTextGame.DisplayResults();

                Console.WriteLine("Press Enter to continue to the next move...");
                Console.ReadLine();
            }

            // Violating SRP by directly accessing public getters
            Player winner = tacticalTextGame.GetWinner();
            Console.WriteLine($"The game is over! {winner.Side} player wins!");
        }
    }
}