using System;
using System.Collections.Specialized;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace PerformanceCalculator.Gui.Difficulty.Taiko.Components
{
    public class TaikoBeatmapListDisplay : BasicScrollContainer
    {
        private BindableList<TaikoBeatmapViewObject> beatmaps = new BindableList<TaikoBeatmapViewObject>();

        private FillFlowContainer beatmapsContainer;

        /// <summary>
        ///     Beatmap list to show
        /// </summary>
        public BindableList<TaikoBeatmapViewObject> Beatmaps 
        { 
            get => this.beatmaps; 
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                this.beatmaps.Clear();
                this.beatmaps.AddRange(value);
                this.beatmaps.BindTo(value);
            }
        }

        private void OnBeatmapsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (beatmapsContainer == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TaikoBeatmapViewObject beatmap in e.NewItems)
                        beatmapsContainer.Add(new TaikoBeatmapDisplay(beatmap));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TaikoBeatmapViewObject beatmap in e.OldItems)
                        beatmapsContainer.Remove(beatmapsContainer[e.OldStartingIndex]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    beatmapsContainer.Clear();
                    foreach (TaikoBeatmapViewObject beatmap in beatmaps)
                        beatmapsContainer.Add(new TaikoBeatmapDisplay(beatmap));
                    break;
            }
        }

        public TaikoBeatmapListDisplay() : base(Direction.Vertical)
        {
            RelativeSizeAxes = Axes.Both;

            this.beatmaps.CollectionChanged += this.OnBeatmapsChanged;

            this.beatmapsContainer = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 20),
            };
            this.Child = this.beatmapsContainer;
        }
    }
}