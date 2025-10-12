using System;
using System.Linq;
using System.Reflection;

namespace DartsScoreboardGames.Services.Models {
    public readonly record struct Dart {

        public int FaceValue { get; init; }
        public bool IsDouble { get; init; }
        public bool IsTreble { get; init; }
        public string? Desc { get; init; }

        public readonly int Value =>
            FaceValue * (IsDouble ? 2 : IsTreble ? 3 : 1);

        public override readonly string ToString() =>
            string.IsNullOrEmpty(Desc) ?
            $"{(IsDouble ? "D" : IsTreble ? "T" : "")}{FaceValue}" :
            Desc;

        // Singles
        public static Dart S1 => new() { FaceValue = 1 };
        public static Dart S2 => new() { FaceValue = 2 };
        public static Dart S3 => new() { FaceValue = 3 };
        public static Dart S4 => new() { FaceValue = 4 };
        public static Dart S5 => new() { FaceValue = 5 };
        public static Dart S6 => new() { FaceValue = 6 };
        public static Dart S7 => new() { FaceValue = 7 };
        public static Dart S8 => new() { FaceValue = 8 };
        public static Dart S9 => new() { FaceValue = 9 };
        public static Dart S10 => new() { FaceValue = 10 };
        public static Dart S11 => new() { FaceValue = 11 };
        public static Dart S12 => new() { FaceValue = 12 };
        public static Dart S13 => new() { FaceValue = 13 };
        public static Dart S14 => new() { FaceValue = 14 };
        public static Dart S15 => new() { FaceValue = 15 };
        public static Dart S16 => new() { FaceValue = 16 };
        public static Dart S17 => new() { FaceValue = 17 };
        public static Dart S18 => new() { FaceValue = 18 };
        public static Dart S19 => new() { FaceValue = 19 };
        public static Dart S20 => new() { FaceValue = 20 };

        // Doubles
        public static Dart D1 => new() { FaceValue = 1, IsDouble = true };
        public static Dart D2 => new() { FaceValue = 2, IsDouble = true };
        public static Dart D3 => new() { FaceValue = 3, IsDouble = true };
        public static Dart D4 => new() { FaceValue = 4, IsDouble = true };
        public static Dart D5 => new() { FaceValue = 5, IsDouble = true };
        public static Dart D6 => new() { FaceValue = 6, IsDouble = true };
        public static Dart D7 => new() { FaceValue = 7, IsDouble = true };
        public static Dart D8 => new() { FaceValue = 8, IsDouble = true };
        public static Dart D9 => new() { FaceValue = 9, IsDouble = true };
        public static Dart D10 => new() { FaceValue = 10, IsDouble = true };
        public static Dart D11 => new() { FaceValue = 11, IsDouble = true };
        public static Dart D12 => new() { FaceValue = 12, IsDouble = true };
        public static Dart D13 => new() { FaceValue = 13, IsDouble = true };
        public static Dart D14 => new() { FaceValue = 14, IsDouble = true };
        public static Dart D15 => new() { FaceValue = 15, IsDouble = true };
        public static Dart D16 => new() { FaceValue = 16, IsDouble = true };
        public static Dart D17 => new() { FaceValue = 17, IsDouble = true };
        public static Dart D18 => new() { FaceValue = 18, IsDouble = true };
        public static Dart D19 => new() { FaceValue = 19, IsDouble = true };
        public static Dart D20 => new() { FaceValue = 20, IsDouble = true };

        // Trebles
        public static Dart T1 => new() { FaceValue = 1, IsTreble = true };
        public static Dart T2 => new() { FaceValue = 2, IsTreble = true };
        public static Dart T3 => new() { FaceValue = 3, IsTreble = true };
        public static Dart T4 => new() { FaceValue = 4, IsTreble = true };
        public static Dart T5 => new() { FaceValue = 5, IsTreble = true };
        public static Dart T6 => new() { FaceValue = 6, IsTreble = true };
        public static Dart T7 => new() { FaceValue = 7, IsTreble = true };
        public static Dart T8 => new() { FaceValue = 8, IsTreble = true };
        public static Dart T9 => new() { FaceValue = 9, IsTreble = true };
        public static Dart T10 => new() { FaceValue = 10, IsTreble = true };
        public static Dart T11 => new() { FaceValue = 11, IsTreble = true };
        public static Dart T12 => new() { FaceValue = 12, IsTreble = true };
        public static Dart T13 => new() { FaceValue = 13, IsTreble = true };
        public static Dart T14 => new() { FaceValue = 14, IsTreble = true };
        public static Dart T15 => new() { FaceValue = 15, IsTreble = true };
        public static Dart T16 => new() { FaceValue = 16, IsTreble = true };
        public static Dart T17 => new() { FaceValue = 17, IsTreble = true };
        public static Dart T18 => new() { FaceValue = 18, IsTreble = true };
        public static Dart T19 => new() { FaceValue = 19, IsTreble = true };
        public static Dart T20 => new() { FaceValue = 20, IsTreble = true };

        //Specials
        public static Dart OuterBull => new() { FaceValue = 25, Desc = nameof(OuterBull) };
        public static Dart Bull => new() { FaceValue = 50, Desc = nameof(Bull) };
        public static Dart NoScore => new() { FaceValue = 0 };

        /// <summary>
        /// Returns a random static Dart member (excluding NoScore).
        /// </summary>
        public static Dart GetRandomDart() {
            var dartType = typeof(Dart);

            // Get all public static properties of type Dart, excluding NoScore
            var dartProps = dartType
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == dartType)
                .ToArray();

            if (dartProps.Length == 0)
                throw new InvalidOperationException("No static Dart members found.");

            var selected = dartProps[Random.Shared.Next(dartProps.Length)];
            return (Dart)selected.GetValue(null)!;
        }
    }
}
