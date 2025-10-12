namespace DartsScoreboardGames.Services.Models {
    public class Turn {

        public (Dart? First, Dart? Second, Dart? Third) Throws = new();

        public int TotalValue =>
            (Throws.First?.Value ?? 0)
            + (Throws.Second?.Value ?? 0)
            + (Throws.Third?.Value ?? 0);

        public bool Complete =>
            Throws.First.HasValue
            && Throws.Second.HasValue
            && Throws.Third.HasValue;

        public int Remaining =>
            (Throws.First.HasValue ? 0 : 1)
            + (Throws.Second.HasValue ? 0 : 1)
            + (Throws.Third.HasValue ? 0 : 1);

        public bool Bust = false;

        public void GoBust() {
            Throws = new();
            Bust = true;
            End();
        }

        public void End() {
            if(Bust) { 
                return;
            }
            while (!Complete) {
                AddNext(Dart.NoScore);
            }
        }

        public void AddNext(Dart dart) {
            if (Throws.First is null) {
                Throws.First = dart;
            } else if (Throws.Second is null) {
                Throws.Second = dart;
            } else if (Throws.Third is null) {
                Throws.Third = dart;
            }
        }
    }
}
