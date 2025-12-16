
using DartsScoreboardGames.Services.Models;

namespace DartsScoreboardGames.Services.Games {
    public class MathsGame : IGameState {
        public static string Name => "Sum Darts";
        public static string Description => "Select a Game";
        public static string Logo => "assets/dartboard-face-sums.png";
        public static string Background => "assets/darts-background-sums.png";
        public static int PlayerRequirement => 2;

        public event EventHandler<EventArgs>? OnChange;
        public event EventHandler<EventArgs>? OnTurnEnd;

        public List<Expression> Expressions { get; private set; }
        public Expression CurrentExpression() => Expressions.Last();

        private Dictionary<Player, int> playerScores = [];
        private GameStateProvider GameStateProvider { get; set; } = default!;

        public static IGameState Create(GameStateProvider gameStateProvider) {
            var game = new MathsGame() {
                GameStateProvider = gameStateProvider,
                playerScores = gameStateProvider.Players.ToDictionary(x => x, x => 0),
                Expressions = [Expression.Generate()],
            };

            return game;
        }

        public async Task OnDart(Dart dart) {
            GameStateProvider.CurrentTurn?.AddNext(dart);

            if (dart.FaceValue == CurrentExpression().Answer) {
                var score = 1;

                if (dart.IsDouble)
                    score *= 2;

                if (dart.IsTreble)
                    score *= 3;

                playerScores[GameStateProvider.ActivePlayer] = Math.Min(playerScores[GameStateProvider.ActivePlayer] + score, 10);

                if (playerScores[GameStateProvider.ActivePlayer] >= 10) {
                    await GameStateProvider.CurrentPlayerWin();
                } else {
                    ResetExpression(true);
                }
            }


            if (GameStateProvider.CurrentTurn?.Complete ?? false) {
                await EndTurn();
            }
        }

        public void ResetExpression(bool solved) {
            Expressions.Last().Solved = solved;
            Expressions.Add(Expression.Generate());
        }

        public int PlayerScore(Player player) =>
           playerScores[player];

        public async Task EndTurn() {
            GameStateProvider.CurrentTurn?.End();
            GameStateProvider.ForceStateChange();
            await Task.Delay(1000);
            GameStateProvider.SetNextPlayer();
            OnTurnEnd?.Invoke(this, EventArgs.Empty);
        }
    }
}

public record class Expression {

    public int Answer { get; init; }
    public char Symbol { get; init; }
    public (int, int) Numbers { get; init; }
    public bool? Solved { get; set; }

    public static Expression Generate() {
        var answer = Random.Shared.Next(1, 21);
        var symbol = RandomSymbol();

        return new Expression() {
            Answer = answer,
            Symbol = symbol,
            Numbers = NumbersFor(answer, symbol),
        };
    }

    private const char Add = '+';
    private const char Subtract = '-';
    private const char Multiply = 'x';
    private const char Divide = '÷';

    private static char RandomSymbol() =>
        (new char[] { Add, Subtract, Multiply, Divide })[Random.Shared.Next(0, 4)];

    private static (int, int) NumbersFor(int answer, char symbol) {

        var num1 = 0;
        var num2 = 0;

        switch (symbol) {
            case Add:
                num1 = Random.Shared.Next(1, answer);
                num2 = answer - num1;
                return (num1, num2);
            case Subtract:
                num1 = Random.Shared.Next(answer + 1, 101);
                num2 = num1 - answer;
                return (num1, num2);
            case Multiply:
                List<(int, int)> multiples = [];
                for (int i = 1; i < answer; i++) {
                    if (answer % i == 0) {
                        multiples.Add((i, answer / i));
                    }
                }
                return multiples[Random.Shared.Next(0, multiples.Count - 1)];
            case Divide:
                num2 = Random.Shared.Next(2, 15);
                return (answer * num2, num2);
            default:
                throw new NotSupportedException();
        }
    }

    public override string ToString() =>
        $"{Numbers.Item1} {Symbol} {Numbers.Item2} = {((Solved ?? false) ? Answer : "??")}";
}
