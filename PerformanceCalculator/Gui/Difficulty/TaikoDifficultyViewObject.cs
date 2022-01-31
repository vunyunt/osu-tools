using osu.Framework.Bindables;

namespace PerformanceCalculator.Gui.Difficulty.Taiko {
    public class TaikoDifficultyViewObject : Bindable<TaikoDifficultyViewObject> {
        public string BeatmapId;
        public BindableDouble StarRating { get; } = new BindableDouble(0);
        public BindableDouble ColourDifficulty { get; } = new BindableDouble(0);
        public BindableDouble StaminaDifficulty { get; } = new BindableDouble(0);
        public BindableDouble RhythmDifficulty { get; } = new BindableDouble(0);

        private void onValueChanged() {
            this.TriggerChange();
        }

        public TaikoDifficultyViewObject() {
            this.StarRating.ValueChanged += (e) => this.onValueChanged();
            this.ColourDifficulty.ValueChanged += (e) => this.onValueChanged();
            this.StaminaDifficulty.ValueChanged += (e) => this.onValueChanged();
            this.RhythmDifficulty.ValueChanged += (e) => this.onValueChanged();
        }
    }
}