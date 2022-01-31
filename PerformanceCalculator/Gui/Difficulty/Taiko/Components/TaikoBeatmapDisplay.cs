using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osuTK;

#nullable enable

namespace PerformanceCalculator.Gui.Difficulty.Taiko.Components
{
    public class TaikoBeatmapViewObject : Bindable<TaikoBeatmapViewObject>
    {
        public ProcessorWorkingBeatmap Beatmap;
        public TaikoDifficultyViewObject CalculatedDifficulty;
        private TaikoDifficultyViewObject? comparisonDifficulty;
        public TaikoDifficultyViewObject? ComparisonDifficulty
        {
            get {
                return this.comparisonDifficulty;
            }
            set {
                this.comparisonDifficulty = value;
                if(value is TaikoDifficultyViewObject val) {
                    val.ValueChanged += (e) => 
                    {   
                        this.calculateDelta();
                        this.TriggerChange();
                    };
                }
                this.calculateDelta();
                this.TriggerChange();
            }
        }
        public BindableDouble ComparisonDelta = new BindableDouble(0);

        public TaikoBeatmapViewObject(ProcessorWorkingBeatmap beatmap) :
            this(
                beatmap,
                null,
                new Bindable<string>())
        {
        }

        public TaikoBeatmapViewObject(
            ProcessorWorkingBeatmap beatmap,
            TaikoDifficultyViewObject? comparisonDifficulty,
            Bindable<string> comparisonKey)
        {
            this.Beatmap = beatmap;
            this.CalculatedDifficulty = new TaikoDifficultyViewObject
            {
                BeatmapId = beatmap.BeatmapInfo.OnlineID.ToString()
            };
            this.ComparisonDifficulty = comparisonDifficulty;

            this.CalculatedDifficulty.ValueChanged += (e) => 
            {
                this.calculateDelta();
                this.TriggerChange();
            };
        }

        private void calculateDelta()
        {
            if (this.ComparisonDifficulty is TaikoDifficultyViewObject comparisonDifficulty)
            {
                ComparisonDelta.Value = this.CalculatedDifficulty.StarRating.Value - comparisonDifficulty.StarRating.Value;

            }
            else
            {
                this.ComparisonDelta.Value = 0;
            }
        }
    }

    public class TaikoBeatmapDisplay : FillFlowContainer
    {
        public TaikoBeatmapViewObject BeatmapViewObject { get; }

        public Bindable<string> ComparisonLabel { get; } = new Bindable<string>("Comparison SR");

        public TaikoBeatmapDisplay(TaikoBeatmapViewObject beatmapViewObject) : base()
        {
            this.AutoSizeAxes = Axes.Y;
            this.RelativeSizeAxes = Axes.X;
            this.Direction = FillDirection.Vertical;
            this.BeatmapViewObject = beatmapViewObject;
            this.Spacing = new Vector2(0, 5);

            this.BeatmapViewObject.ValueChanged += (e) => this.Refresh();

            this.CreateChildren();
        }

        private Drawable CreateTitle()
        {
            string title = String.Join(String.Empty, new String[]
            {
                BeatmapViewObject.Beatmap.Metadata.Artist,
                " - ",
                BeatmapViewObject.Beatmap.Metadata.Title,
                " [",
                BeatmapViewObject.Beatmap.BeatmapInfo.DifficultyName,
                "]",
            });

            return new SpriteText
            {
                Text = title,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Colour = Color4.White,
                Font = FrameworkFont.Regular.With(weight: "Bold", size: 24),
            };
        }

        private Drawable CreateDifficultyComponents()
        {
            return new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Distributed),
                    new Dimension(GridSizeMode.Distributed),
                    new Dimension(GridSizeMode.Distributed),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText()
                        {
                            Text = "Colour",
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText()
                        {
                            Text = "Stamina",
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText()
                        {
                            Text = "Rhythm",
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                    },
                    new Drawable[]
                    {
                        new SpriteText()
                        {
                            Text = BeatmapViewObject.CalculatedDifficulty.ColourDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText()
                        {
                            Text = BeatmapViewObject.CalculatedDifficulty.StaminaDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText()
                        {
                            Text = BeatmapViewObject.CalculatedDifficulty.RhythmDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                    },
                }
            };
        }

        private Drawable CreateSRTable()
        {
            double calculatedSR = BeatmapViewObject.CalculatedDifficulty.StarRating.Value;
            List<Drawable> row1 = new List<Drawable>(
                new Drawable[]
                {
                    new SpriteText()
                    {
                        Text = "Calculated SR",
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Colour = Color4.White
                    }
                }
            );

            List<Drawable> row2 = new List<Drawable>(
                new Drawable[]
                {
                    new SpriteText()
                    {
                        Text = BeatmapViewObject.CalculatedDifficulty.StarRating.ToString(),
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Colour = Color4.White
                    }
                }
            );

            if (this.BeatmapViewObject.ComparisonDifficulty is TaikoDifficultyViewObject comparisonDifficulty)
            {
                double comparisonSR = comparisonDifficulty.StarRating.Value;

                row1.Add(new SpriteText()
                {
                    Text = this.ComparisonLabel.Value,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Colour = Color4.White
                });
                row1.Add(new SpriteText()
                {
                    Text = "Delta",
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Colour = Color4.White
                });

                row2.Add(new SpriteText()
                {
                    Text = comparisonSR.ToString(),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Colour = Color4.White
                });
                row2.Add(new SpriteText()
                {
                    Text = (comparisonSR - calculatedSR).ToString(),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Colour = Color4.White
                });
            }

            return new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Distributed),
                    new Dimension(GridSizeMode.Distributed),
                },
                Content = new[]
                {
                    row1.ToArray(),
                    row2.ToArray()
                }
            };
        }

        private Drawable CreateSRDisplay()
        {
            return new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    this.CreateSRTable()
                }
            };
        }

        private void CreateChildren()
        {
            this.Children = new Drawable[]
            {
                this.CreateTitle(),
                this.CreateDifficultyComponents(),
                this.CreateSRDisplay()
            };
        }

        private void Refresh()
        {
            this.Clear();
            this.CreateChildren();
        }
    }
}