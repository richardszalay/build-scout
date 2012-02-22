using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace RichardSzalay.PocketCiTray.Extensions.Extensions
{
    public static class PropertyChangeExtensions
    {
        public static IObservable<TProperty> GetPropertyValues<TSource, TProperty>(this TSource source, 
            Expression<Func<TSource, TProperty>> propertyAccessor) where TSource : INotifyPropertyChanged
        {
            var expression = (MemberExpression)propertyAccessor.Body;

            Func<TSource, TProperty> accesor = propertyAccessor.Compile();

            return Observable.Defer(() => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => new PropertyChangedEventHandler(h),
                h => source.PropertyChanged += h,
                h => source.PropertyChanged -= h
                )
                .Where(e => e.EventArgs.PropertyName == expression.Member.Name)
                .Select(x => accesor(source))
                .StartWith(accesor(source)));
        }
    }
}
