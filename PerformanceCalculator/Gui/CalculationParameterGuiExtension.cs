using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Containers;
using osuTK;

using osu.Game.Rulesets.Difficulty;

using PerformanceCalculator.Gui.Components;

namespace PerformanceCalculator.Gui
{
    public static class CalculationParameterGuiExtension
    {
        public static Drawable CreateControlSection(this CalculationParameters obj, String label)
        {
            Drawable[] labelDrawable = new Drawable[] 
            {
                new SpriteText
                {
                    Text = label,
                    Font = FrameworkFont.Regular.With(weight: "Bold"),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Padding = new MarginPadding
                    {
                        Top = 10,
                        Bottom = 10
                    }
                }
            };
            Drawable[] controls = CreateControls(obj);

            return new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(0, 5),
                Children = labelDrawable.Concat(controls).ToArray()
            };
        }

        public static Drawable[] CreateControls(this CalculationParameters obj)
        {
            // Get all field with CalculationParameterAttribute in obj, and put it into a map of field to attribute
            Dictionary<FieldInfo, CalculationParameterAttribute> fields = obj.GetType().GetFields()
                .Where(f => f.GetCustomAttributes(typeof(CalculationParameterAttribute), true).Any())
                .ToDictionary(
                    f => f,
                    f => f.GetCustomAttributes(typeof(CalculationParameterAttribute), true)
                        .First() as CalculationParameterAttribute);

            // Create controls for each field
            Drawable[] controls = new Drawable[fields.Count];

            for (int i = 0; i < fields.Count; ++i)
            {
                var pair = fields.ElementAt(i);
                Drawable control;
                switch (pair.Key.GetValue(obj))
                {
                    case float number:
                        control = new CalculationParameterControl<float>(obj, pair.Key, pair.Value);
                        break;
                    case double number:
                        control = new CalculationParameterControl<double>(obj, pair.Key, pair.Value);
                        break;
                    case int number:
                        control = new CalculationParameterControl<int>(obj, pair.Key, pair.Value);
                        break;
                    default:
                        throw new InvalidOperationException($"{pair.Value.Name} was attached to an unsupported type ({pair.Key.GetType()})");
                }
                controls[i] = control;
            }

            return controls;
        }

        private static BasicSliderBar<T> CreateSliderBar<T>(
            CalculationParameters obj,
            FieldInfo field,
            CalculationParameterAttribute attribute)
            where T : struct, IComparable<T>, IConvertible, IEquatable<T>
        {
            BindableNumber<T> bindable = new BindableNumber<T>
            {
                MinValue = (T)attribute.Min,
                MaxValue = (T)attribute.Max,
                Precision = (T)attribute.Step,
                Value = (T)field.GetValue(obj),
                Default = (T)attribute.DefaultValue
            };
            bindable.ValueChanged += (newValue) => field.SetValue(obj, newValue);

            return new BasicSliderBar<T>
            {
                Current = bindable,
                RelativeSizeAxes = Axes.X,
                Height = 20,
                Width = 0.8f,
                Margin = new MarginPadding { Top = 5, Bottom = 5 }
            };
        }
    }
}