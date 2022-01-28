using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osuTK;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;

using PerformanceCalculator.Gui;
using PerformanceCalculator.Gui.Api.Tres;

namespace PerformanceCalculator.Gui.Difficulty.Taiko
{
    public class TaikoBeatmapViewObject
    {
        public ProcessorWorkingBeatmap Beatmap;
        public TaikoDifficultyViewObject OnlineDifficulty;
        public TaikoDifficultyViewObject TresDifficulty;
        public TaikoDifficultyViewObject CalculatedDifficulty;

        public TaikoBeatmapViewObject(ProcessorWorkingBeatmap beatmap)
        {
            this.Beatmap = beatmap;
            this.OnlineDifficulty = new TaikoDifficultyViewObject
            {
                BeatmapId = beatmap.BeatmapInfo.ID.ToString()
            };
        }
    };

    public class TaikoDifficultyCalculatorScreen : Screen
    {
        private TaikoDiffficultyCalculationParameters parameters;
        private BindableDouble repetitionPenaltyDecayMultiplier;
        private BindableDouble skillMultiplier;
        private BindableDouble strainDecayBase;
        private Bindable<string> beatmapIdInputValue = new Bindable<string>(string.Empty);
        private BindableList<TaikoBeatmapViewObject> beatmaps = new BindableList<TaikoBeatmapViewObject>();
        private FillFlowContainer beatmapsContainer;
        private Button loadFromTresButton;
        private TresApi tresApi;

        public TaikoDifficultyCalculatorScreen()
        {
            tresApi = new TresApi();
        }

        public TaikoDifficultyCalculatorScreen(string tresApiUrl)
        {
            tresApi = new TresApi(tresApiUrl);
        }

        private void InitializeBindableParameters()
        {
            repetitionPenaltyDecayMultiplier = new BindableDouble(
                this.parameters.ColourParameters.RepetitionPenaltyDecayMultiplier)
            {
                MinValue = 0,
                MaxValue = 0.1,
                Precision = 0.001
            };
            repetitionPenaltyDecayMultiplier.ValueChanged += (e) =>
            {
                this.parameters.ColourParameters.RepetitionPenaltyDecayMultiplier = e.NewValue;
            };
            skillMultiplier = new BindableDouble(
                this.parameters.ColourParameters.SkillMultiplier)
            {
                MinValue = 0,
                MaxValue = 2,
                Precision = 0.01
            };
            skillMultiplier.ValueChanged += (e) =>
            {
                this.parameters.ColourParameters.SkillMultiplier = e.NewValue;
            };
            strainDecayBase = new BindableDouble(
                this.parameters.ColourParameters.StrainDecayBase)
            {
                MinValue = 0,
                MaxValue = 1,
                Precision = 0.01
            };
            strainDecayBase.ValueChanged += (e) =>
            {
                this.parameters.ColourParameters.StrainDecayBase = e.NewValue;
            };
        }

        private void RecomputeDifficulty()
        {
            for (int i = 0; i < this.beatmaps.Count; ++i)
            {
                TaikoDifficultyCalculator calculator = new TaikoDifficultyCalculator(
                new TaikoRuleset().RulesetInfo,
                this.beatmaps[i].Beatmap,
                this.parameters);
                TaikoDifficultyAttributes result = calculator.Calculate() as TaikoDifficultyAttributes;
                beatmaps[i].CalculatedDifficulty = new TaikoDifficultyViewObject
                {
                    StarRating = result.StarRating,
                    ColourDifficulty = result.ColourDifficulty,
                    StaminaDifficulty = result.StaminaDifficulty,
                    RhythmDifficulty = result.RhythmDifficulty
                };
            }

            this.Schedule(() =>
            {
                this.OnBeatmapsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        private void LoadBeatmapId(string beatmapId)
        {
            ProcessorWorkingBeatmap beatmap = ProcessorWorkingBeatmap.FromFileOrId(beatmapId);
            this.Schedule(() =>
            {
                beatmaps.Add(new TaikoBeatmapViewObject(beatmap));
            });
        }

        private async void LoadBeatmapsFromTres()
        {
            this.Schedule(() =>
            {
                loadFromTresButton.Enabled.Value = false;
            });

            try
            {
                string[] beatmapIds = await this.tresApi.getBeatmapIds(limit: 10);
                foreach (string beatmapId in beatmapIds)
                {
                    ProcessorWorkingBeatmap beatmap = ProcessorWorkingBeatmap.FromFileOrId(beatmapId);
                    this.Schedule(() =>
                    {
                        beatmaps.Add(new TaikoBeatmapViewObject(beatmap));
                    });
                }
                this.Schedule(() =>
                {
                    loadFromTresButton.Enabled.Value = true;
                });
            }
            catch (System.Exception e)
            {
                this.Schedule(() =>
                {
                    loadFromTresButton.Enabled.Value = true;
                });

                // TODO: Show error message
            }
        }

        private Drawable RenderBeatmap(TaikoBeatmapViewObject beatmap)
        {
            return new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                        new SpriteText
                        {
                            Text = beatmap.Beatmap.Metadata.Artist + " - " + beatmap.Beatmap.Metadata.Title,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText
                        {
                            Text = "[" + beatmap.Beatmap.BeatmapInfo.DifficultyName + "]",
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText
                        {
                            Text = "Calculated Star Rating: " + beatmap.CalculatedDifficulty.StarRating.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText
                        {
                            Text = "Calculated Colour Difficulty: " + beatmap.CalculatedDifficulty.ColourDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText
                        {
                            Text = "Calculated Stamina Difficulty: " + beatmap.CalculatedDifficulty.StaminaDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        },
                        new SpriteText
                        {
                            Text = "Calculated Rhythm Difficulty: " + beatmap.CalculatedDifficulty.RhythmDifficulty.ToString(),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White
                        }
                }
            };
        }

        /// <Summary>
        ///    Event handler for when the beatmap list changes.
        /// </Summary>
        private void OnBeatmapsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (beatmapsContainer == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TaikoBeatmapViewObject beatmap in e.NewItems)
                        beatmapsContainer.Add(RenderBeatmap(beatmap));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TaikoBeatmapViewObject beatmap in e.OldItems)
                        beatmapsContainer.Remove(beatmapsContainer[e.OldStartingIndex]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    beatmapsContainer.Clear();
                    foreach (TaikoBeatmapViewObject beatmap in beatmaps)
                        beatmapsContainer.Add(RenderBeatmap(beatmap));
                    break;
            }
        }

        private Drawable CreateBeatmapIdInput()
        {
            this.loadFromTresButton = new BasicButton
            {
                Text = "Load from TRES",
                Size = new Vector2(200, 30),
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            };

            this.loadFromTresButton.Action += () =>
            {
                Task.Run(LoadBeatmapsFromTres);
            };

            return new BasicScrollContainer(scrollDirection: Direction.Horizontal)
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Name = "BeatmapIdInputContainer",
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new BasicTextBox {
                                PlaceholderText = "Beatmap ID",
                                Width = 200,
                                Height = 30,
                                Current = this.beatmapIdInputValue
                            },
                            new BasicButton {
                                Text = "Load",
                                Size = new Vector2(100, 30),
                                Action = () =>
                                {
                                    LoadBeatmapId(this.beatmapIdInputValue.Value);
                                }
                            },
                            this.loadFromTresButton
                        }
                    }
                }
            };
        }

        /// <Summary>
        ///     Create the content of beatmap list, including controls
        /// </Summary>
        private Drawable CreateBeatmapsContainer()
        {
            this.beatmapsContainer = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 50),
                Children = this.beatmaps.Select(b => RenderBeatmap(b)).ToArray(),
            };

            this.beatmaps.CollectionChanged += this.OnBeatmapsChanged;

            return new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Margin = new MarginPadding(25),
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Distributed),
                },
                RowDimensions = new[] {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Distributed)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        this.CreateBeatmapIdInput(),
                    },
                    new Drawable[] {
                        // this.beatmapsContainer
                        new BasicScrollContainer(scrollDirection: Direction.Vertical)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                this.beatmapsContainer
                            }
                        }
                    }
                }
            };
        }

        private Drawable CreateParameterInputs()
        {
            return new BasicScrollContainer(scrollDirection: Direction.Vertical)
            {
                Name = "ParameterInputScrollable",
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding(25),
                    Children = new Drawable[]
                    {
                        new BasicButton {
                            Text = "Compute Difficulty",
                            Size = new Vector2(200, 30),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Action = () =>
                            {
                                Task.Run(RecomputeDifficulty);
                            }
                        },
                        this.parameters.ColourParameters.CreateControlSection(label: "Colour"),
                    }
                }
            };
        }

        private Drawable CreateTopLevelContainer()
        {
            GridContainer container = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Distributed),
                    new Dimension(GridSizeMode.Distributed),
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Distributed),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        this.CreateParameterInputs(),
                        this.CreateBeatmapsContainer(),
                    },
                }
            };

            return container;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            this.parameters = new TaikoDiffficultyCalculationParameters();

            // this.InitializeBindableParameters();
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                this.CreateTopLevelContainer()
            };
        }
    }
}