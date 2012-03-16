using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public class TextBlockHighlightBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.Register("HighlightText", typeof(string), typeof(TextBlockHighlightBehavior), new PropertyMetadata(OnHighlightTextChanged));
        
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBlockHighlightBehavior), new PropertyMetadata(OnTextChanged));

        public TextBlockHighlightBehavior()
        {
            this.HighlightBrush = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        }

        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush), typeof(TextBlockHighlightBehavior), new PropertyMetadata(null));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string HighlightText
        {
            get { return (string)GetValue(HighlightTextProperty); }
            set { SetValue(HighlightTextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void ApplyHighlight()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            string highlightText = HighlightText;

            AssociatedObject.Inlines.Clear();

            if (String.IsNullOrEmpty(Text) || String.IsNullOrEmpty(highlightText))
            {
                AssociatedObject.Text = Text;
            }
            else
            {
                foreach (var inline in CreateHighlightInlines(Text, highlightText))
                {
                    AssociatedObject.Inlines.Add(inline);
                }
            }
        }

        private IEnumerable<Inline> CreateHighlightInlines(string text, string highlight)
        {
            List<Inline> inlines = new List<Inline>();

            int cursor = 0;

            while(true)
            {
                int index = text.IndexOf(highlight, cursor, StringComparison.InvariantCultureIgnoreCase);

                if (index == -1)
                {
                    inlines.Add(new Run
                    {
                        Text = text.Substring(cursor)
                    });

                    break;
                }
                else
                {
                    if (index > cursor)
                    {
                        inlines.Add(new Run
                        {
                            Text = text.Substring(cursor, index - cursor)
                        });
                    }

                    inlines.Add(new Run
                    {
                        Text = text.Substring(index, highlight.Length),
                        Foreground = HighlightBrush
                    });
                }

                cursor += (index - cursor) + highlight.Length;

                if (cursor >= text.Length)
                {
                    break;
                }
            }

            return inlines;
        }

        private static void OnHighlightTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlockHighlightBehavior behavior = (TextBlockHighlightBehavior)sender;

            behavior.ApplyHighlight();
        }

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlockHighlightBehavior behavior = (TextBlockHighlightBehavior)sender;

            behavior.ApplyHighlight();
        }
    }
}
