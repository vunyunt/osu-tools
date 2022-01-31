using System.Linq;
using System.Collections.Generic;
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

using PerformanceCalculator.Gui.Difficulty.Taiko.Components;
using PerformanceCalculator.Gui.Api.Tres;

namespace PerformanceCalculator.Gui.Difficulty.Taiko
{
    public class TaikoDifficultyCalculatorScreen : Screen
    {
        private TaikoDiffficultyCalculationParameters parameters;
        private Bindable<string> beatmapIdInputValue = new Bindable<string>(string.Empty);
        private BindableList<TaikoBeatmapViewObject> beatmaps = new BindableList<TaikoBeatmapViewObject>();
        private Button loadFromTresButton;
        private Button loadBeatmapIDButton;
        private TresApi tresApi;
        private Bindable<bool> canLoadNewBeatmap = new Bindable<bool>(true);

        public TaikoDifficultyCalculatorScreen()
        {
            tresApi = new TresApi();
        }

        public TaikoDifficultyCalculatorScreen(string tresApiUrl)
        {
            tresApi = new TresApi(tresApiUrl);
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

                this.updateBeatmapDifficultyOnUIThread(i, result);
            }
        }

        private void updateBeatmapDifficultyOnUIThread(int i, TaikoDifficultyAttributes result)
        {
            this.Schedule(() =>
            {
                beatmaps[i].CalculatedDifficulty.StarRating.Value = result.StarRating;
                beatmaps[i].CalculatedDifficulty.ColourDifficulty.Value = result.ColourDifficulty;
                beatmaps[i].CalculatedDifficulty.StaminaDifficulty.Value = result.StaminaDifficulty;
                beatmaps[i].CalculatedDifficulty.RhythmDifficulty.Value = result.RhythmDifficulty;
            });
        }

        private void LoadBeatmapId(string beatmapId)
        {
            this.Schedule(() => this.canLoadNewBeatmap.Value = false);
            try
            {
                ProcessorWorkingBeatmap beatmap = ProcessorWorkingBeatmap.FromFileOrId(beatmapId);
                this.Schedule(() =>
                {
                    beatmaps.Add(new TaikoBeatmapViewObject(beatmap));
                });
            }
            catch (System.Exception e)
            {
                this.Schedule(() => this.canLoadNewBeatmap.Value = true);
            }


            this.Schedule(() => this.canLoadNewBeatmap.Value = true);
        }

        private async void LoadBeatmapsFromTres()
        {
            this.Schedule(() =>
            {
                this.canLoadNewBeatmap.Value = false;
            });

            try
            {
                TresBeatmap[] tresBeatmaps = await this.tresApi.getBeatmaps(limit: 10);
                foreach (TresBeatmap tresBeatmap in tresBeatmaps)
                {
                    ProcessorWorkingBeatmap beatmap = ProcessorWorkingBeatmap.FromFileOrId(tresBeatmap.beatmap_id);
                    this.Schedule(() =>
                    {
                        TaikoBeatmapViewObject beatmapViewObject = new TaikoBeatmapViewObject(beatmap);
                        TaikoDifficultyViewObject tresDifficulty = new TaikoDifficultyViewObject
                        {
                            BeatmapId = tresBeatmap.beatmap_id,
                        };
                        tresDifficulty.StarRating.Value = tresBeatmap.new_rating;
                        beatmapViewObject.ComparisonDifficulty = tresDifficulty;

                        beatmaps.Add(beatmapViewObject);
                    });
                }
                this.Schedule(() =>
                {
                    this.canLoadNewBeatmap.Value = true;
                });
            }
            catch (System.Exception e)
            {
                this.Schedule(() =>
                {
                    this.canLoadNewBeatmap.Value = true;
                });

                // TODO: Show error message
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
            this.loadFromTresButton.Enabled.BindTo(this.canLoadNewBeatmap);
            this.loadFromTresButton.Action += () =>
            {
                Task.Run(LoadBeatmapsFromTres);
            };

            this.loadBeatmapIDButton = new BasicButton
            {
                Text = "Load",
                Size = new Vector2(100, 30),
                Action = () =>
                {
                    Task.Run(() => LoadBeatmapId(this.beatmapIdInputValue.Value));
                }
            };
            this.loadBeatmapIDButton.Enabled.BindTo(this.canLoadNewBeatmap);


            return new BasicScrollContainer(scrollDirection: Direction.Horizontal)
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                ScrollbarVisible = false,
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
                                PlaceholderText = "Beatmap ID/Path",
                                Width = 200,
                                Height = 30,
                                Current = this.beatmapIdInputValue
                            },
                            this.loadBeatmapIDButton,
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
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(25),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
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
                            new Container {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding {Top = 25, Bottom = 25},
                                Child = new TaikoBeatmapListDisplay {
                                    Beatmaps = this.beatmaps
                                }
                            }
                        }
                    }
                }
            };
        }

        private Drawable CreateParameterInputs()
        {
            var parameterInput = new BasicScrollContainer(scrollDirection: Direction.Vertical)
            {
                Padding = new MarginPadding { Top = 25, Bottom = 25 },
                Name = "ParameterInputScrollable",
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Right = 25 },
                    Children = new Drawable[]
                    {
                        this.parameters.CreateControlSection(label: "General"),
                        this.parameters.ColourParameters.CreateControlSection(label: "Colour"),
                        this.parameters.RhythmParameters.CreateControlSection(label: "Rhythm"),
                        this.parameters.StaminaParameters.CreateControlSection(label: "Stamina"),
                    }
                }
            };

            var gridContainer = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
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
                        new BasicButton
                        {
                            Text = "Compute Difficulty",
                            Size = new Vector2(200, 30),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Action = () =>
                            {
                                Task.Run(RecomputeDifficulty);
                            }
                        },
                    },
                    new Drawable[] {
                        parameterInput
                    }
                }
            };

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(25),
                Children = new Drawable[]
                {
                    gridContainer
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