using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IThemeCssGenerator
    {
        string Generate();
    }

    public class PhoneThemeCssGenerator : IThemeCssGenerator
    {
        private IApplicationResourceFacade applicationResourceFacade;

        private Dictionary<string, string> selectorMappings = new Dictionary<string, string>
        {
            //{ "body", "PhoneTextBlockBase" },            
            { "p", "PhoneTextSmallStyle" },
            { "li span", "PhoneTextSmallStyle" },
            { "li", "PhoneForegroundBrush" },
            { "em", "PhoneAccentBrush" },
            { "h1", "PhoneTextTitle1Style" },
            { "h2", "PhoneTextTitle2Style" },
            { "h3", "PhoneTextTitle3Style" }
        };

        public PhoneThemeCssGenerator(IApplicationResourceFacade applicationResourceFacade)
        {
            this.applicationResourceFacade = applicationResourceFacade;
        }

        public string Generate()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateBackgroundClass("body",
                applicationResourceFacade.GetResource<Brush>("PhoneBackgroundBrush")));

            foreach (var kvp in selectorMappings)
            {
                string cls = GenerateClass(kvp.Key, 
                    applicationResourceFacade.GetResource<object>(kvp.Value));

                sb.AppendLine(cls);
            }

            return sb.ToString();
        }

        private static string GenerateClass(string selector, object value)
        {
            if (value is Style)
            {
                return GenerateStyleClass(selector, (Style)value);
            }

            if (value is SolidColorBrush)
            {
                return GenerateForegroundClass(selector, (SolidColorBrush)value);
            }

            throw new NotSupportedException("Resource type not supported: " + value.GetType().FullName);
        }

        public static string GenerateStyleClass(string selector, Style style)
        {
            StringBuilder sb = new StringBuilder();

            var propertyConversions = new Dictionary<DependencyProperty, Func<object, string>>
            {
                { Control.ForegroundProperty, value => MapForeground((Brush)value) },                
                { Control.FontFamilyProperty, value => MapFontFamily((FontFamily)value) },                
                { Control.FontSizeProperty, value => MapFontSize((double)value) },                
                { Control.PaddingProperty, value => MapPadding((Thickness)value) },                
                { Control.MarginProperty, value => MapMargin((Thickness)value) },
                { TextBlock.ForegroundProperty, value => MapForeground((Brush)value) },
                { TextBlock.FontFamilyProperty, value => MapFontFamily((FontFamily)value) },
                { TextBlock.FontSizeProperty, value => MapFontSize((double)value) },
                //{ TextBlock.PaddingProperty, value => MapFontSize((double)value) },
                //{ TextBlock.MarginProperty, value => MapFontSize((double)value) },
            };

            AppendStartClass(sb, selector);

            var setters = new Dictionary<DependencyProperty, object>();

            GenerateStylePropertyDictionary(setters, style);

            foreach (var setter in setters)
            {
                if (propertyConversions.ContainsKey(setter.Key))
                {
                    string attribute = propertyConversions[setter.Key](setter.Value);

                    sb.Append(" ");
                    sb.Append(attribute);

                    continue;
                }
            }

            AppendEndClass(sb, selector);

            return sb.ToString();
        }

        private static void GenerateStylePropertyDictionary(IDictionary<DependencyProperty, object> setters, Style style)
        {
            foreach (var setter in style.Setters.OfType<Setter>())
            {
                if (!setters.ContainsKey(setter.Property))
                {
                    setters.Add(setter.Property, setter.Value);
                }
            }

            if (style.BasedOn != null)
            {
                GenerateStylePropertyDictionary(setters, style.BasedOn);
            }
        }

        public static string GenerateBackgroundClass(string selector, Brush brush)
        {
            StringBuilder sb = new StringBuilder();

            AppendStartClass(sb, selector);
            sb.Append(MapBrush("background-color", brush));
            AppendEndClass(sb, selector);

            return sb.ToString();
        }

        public static string GenerateForegroundClass(string selector, Brush brush)
        {
            StringBuilder sb = new StringBuilder();

            AppendStartClass(sb, selector);
            sb.Append(MapBrush("color", brush));
            AppendEndClass(sb, selector);

            return sb.ToString();
        }

        private static void AppendStartClass(StringBuilder sb, string selector)
        {
            sb.AppendFormat("{0} {{", selector);
        }

        private static void AppendEndClass(StringBuilder sb, string selector)
        {
            sb.Append(" }");
        }

        private static string MapPadding(Thickness thickness)
        {
            return String.Format("padding: {0}px {0}px {0}px {0}px;", 
                thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }

        private static string MapMargin(Thickness thickness)
        {
            return String.Format("margin: {0}px {0}px {0}px {0}px;",
                thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }

        private static string MapFontSize(double p)
        {
            return String.Format("font-size: {0}pt;",
                (int)(p * 0.6));
        }

        private static string MapFontFamily(FontFamily fontFamily)
        {
            return String.Format("font-family: \"{0}\";", fontFamily.Source);
        }

        private static string MapForeground(Brush brush)
        {
            return MapBrush("color", brush);
        }

        private static string MapBrush(string attribute, Brush brush)
        {
            var scb = brush as SolidColorBrush;

            if (scb == null)
            {
                throw new NotSupportedException("Brush not supported:" + brush.GetType().FullName);
            }

            var color = scb.Color;

            return MapColor(attribute, color);
        }

        private static string MapColor(string attribute, Color color)
        {
            return String.Format("{0}: #{1:X2}{2:X2}{3:X2};", attribute, color.R, color.G, color.B);
        }
    }
}
