﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pleaseing
{
    public static class Tweening
    {
        private static List<TweenTimeline> tweens;

        static Tweening()
        {
            tweens = new List<TweenTimeline>();
        }

        public static TweenTimeline Tween(object obj, Func<float, float> easingFunction, float endTime, float startTime = 0)
        {
            var tween = new Tween(obj, startTime, endTime, easingFunction);
            var timeline = new TweenTimeline(endTime);
            timeline.Add(tween);
            return timeline;
        }

        public static TweenTimeline NewTimeline(float duration)
        {
            var timeline = new TweenTimeline(duration);
            tweens.Add(timeline);
            return timeline;
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var tween in tweens)
            {
                tween.Update(gameTime);
            }
        }
    }

    public enum TweenState
    {
        Running,
        Paused,
        Stopped
    }

    public class TweenTimeline
    {
        public List<Tween> tweens;
        public float elapsedMilliseconds;
        public bool Loop;
        public float duration;
        public TweenState State;

        public TweenTimeline(float duration)
        {
            this.duration = duration;
            tweens = new List<Tween>();
            State = TweenState.Running;
        }

        public void Start()
        {
            State = TweenState.Running;
        }
        public void Stop()
        {
            elapsedMilliseconds = 0;
            State = TweenState.Stopped;
        }
        public void Pause()
        {
            State = TweenState.Paused;
        }
        public void Restart()
        {
            elapsedMilliseconds = 0;
            State = TweenState.Running;
        }

        public Tween Add(Tween tween)
        {
            tweens.Add(tween);
            return tween;
        }
        public Tween Add(object obj, Func<float, float> easingFunction, float startTime, float endTime)
        {
            var tween = new Tween(obj, startTime, endTime, easingFunction);
            tweens.Add(tween);

            if (endTime > duration)
                duration = endTime;

            return tween;
        }

        public void Update(GameTime gameTime)
        {
            if (State == TweenState.Running)
            {
                elapsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (elapsedMilliseconds >= duration)
                {
                    if (Loop)
                    {
                        elapsedMilliseconds = elapsedMilliseconds - duration;
                    }
                    else
                    {
                        Stop();
                    }
                }

                if (State == TweenState.Running)
                {
                    foreach (var tween in tweens)
                        tween.Update(elapsedMilliseconds);
                }
            }
        }
    }

    public class Tween
    {
        private object targetObject;
        private PropertyInfo[] properties;
        private FieldInfo[] fields;
        private List<TweenableProperty> tweeningProperties;
        private float startTime;
        private float endTime;
        private float elapsedMilliseconds;
        private Func<float, float> easingFunction;

        public float Progress
        {
            get
            {
                return easingFunction.Invoke(MathHelper.Clamp((elapsedMilliseconds - startTime) / (endTime - startTime), 0, 1));
            }
        }

        public Tween(object obj, float startTime, float endTime, Func<float, float> easingFunction)
        {
            targetObject = obj;
            this.easingFunction = easingFunction;
            properties = obj.GetType().GetProperties();
            fields = obj.GetType().GetFields();
            tweeningProperties = new List<TweenableProperty>();
            this.startTime = startTime;
            this.endTime = endTime;
        }

        public Tween Add<T>(string propertyName, T value, LerpFunction<T> lerpFunction)
        {
            var property = properties.FirstOrDefault(x => x.Name == propertyName);
            if (property != null)
            {
                if (property.PropertyType == typeof(T))
                {
                    var tweenableProperty = new TweenProperty<T>(targetObject, property, value, lerpFunction);
                    tweeningProperties.Add(tweenableProperty);
                }
            }
            else
            {
                var field = fields.FirstOrDefault(x => x.Name == propertyName);
                if (field != null)
                {
                    if (field.FieldType == typeof(T))
                    {
                        var tweenableProperty = new TweenField<T>(targetObject, field, value, lerpFunction);
                        tweeningProperties.Add(tweenableProperty);
                    }
                }
            }

            return this;
        }
        public Tween Add(string propertyName, float value)
        {
            Add<float>(propertyName, value, LerpFunctions.Float);
            return this;
        }
        public Tween Add(string propertyName, Vector2 value)
        {
            Add(propertyName, value, LerpFunctions.Vector2);
            return this;
        }
        public Tween Add(string propertyName, Vector3 value)
        {
            Add(propertyName, value, LerpFunctions.Vector3);
            return this;
        }
        public Tween Add(string propertyName, Vector4 value)
        {
            Add(propertyName, value, LerpFunctions.Vector4);
            return this;
        }
        public Tween Add(string propertyName, Color value)
        {
            Add(propertyName, value, LerpFunctions.Color);
            return this;
        }
        public Tween Add(string propertyName, Quaternion value)
        {
            Add(propertyName, value, LerpFunctions.Quaternion);
            return this;
        }
        public Tween Add(string propertyName, Rectangle value)
        {
            Add(propertyName, value, LerpFunctions.Rectangle);
            return this;
        }

        public Tween Add<T>(T startValue, T endValue, Action<T> setter, LerpFunction<T> lerpFunction)
        {
            var tweenableProperty = new TweenSetter<T>(startValue, endValue, setter, lerpFunction);
            tweeningProperties.Add(tweenableProperty);
            return this;
        }
        public Tween Add(float startValue, float endValue, Action<float> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Float);
        }
        public Tween Add(Vector2 startValue, Vector2 endValue, Action<Vector2> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Vector2);
        }
        public Tween Add(Vector3 startValue, Vector3 endValue, Action<Vector3> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Vector3);
        }
        public Tween Add(Vector4 startValue, Vector4 endValue, Action<Vector4> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Vector4);
        }
        public Tween Add(Color startValue, Color endValue, Action<Color> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Color);
        }
        public Tween Add(Quaternion startValue, Quaternion endValue, Action<Quaternion> setter)
        {
            return Add(startValue, endValue, setter, LerpFunctions.Quaternion);
        }

        public void Update(float timelineElapsedMilliseconds)
        {
            elapsedMilliseconds = timelineElapsedMilliseconds;

            foreach (var property in tweeningProperties)
                property.Tween(Progress);

        }
    }

    public interface TweenableProperty
    {
        void Tween(float progress);
    }

    class TweenProperty<T> : TweenableProperty
    {
        public object target;
        public PropertyInfo property;
        public T startValue;
        public T value;
        LerpFunction<T> lerpFunction;

        public TweenProperty(object target, PropertyInfo property, T value, LerpFunction<T> lerpFunction)
        {
            this.target = target;
            this.property = property;
            this.value = value;
            this.lerpFunction = lerpFunction;
            startValue = (T)property.GetValue(target);
        }

        public void Tween(float progress)
        {
            var lerpValue = lerpFunction(startValue, value, progress);
            property.SetValue(target, lerpValue);
        }
    }
    class TweenField<T> : TweenableProperty
    {
        public object target;
        public FieldInfo field;
        public T startValue;
        public T value;
        LerpFunction<T> lerpFunction;

        public TweenField(object target, FieldInfo field, T value, LerpFunction<T> lerpFunction)
        {
            this.target = target;
            this.field = field;
            this.value = value;
            this.lerpFunction = lerpFunction;
            startValue = (T)field.GetValue(target);
        }

        public void Tween(float progress)
        {
            var lerpValue = lerpFunction(startValue, value, progress);
            field.SetValue(target, lerpValue);
        }
    }
    class TweenSetter<T> : TweenableProperty
    {
        public T startValue;
        public T endValue;
        public LerpFunction<T> lerpFunction;
        public Action<T> setter;

        public TweenSetter(T startValue, T endValue, Action<T> setter, LerpFunction<T> lerpFunction)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.setter = setter;
            this.lerpFunction = lerpFunction;
        }

        public void Tween(float progress)
        {
            var currentValue = lerpFunction(startValue, endValue, progress);
            setter.Invoke(currentValue);
        }
    }


    public delegate T LerpFunction<T>(T start, T end, float progress);

    static class LerpFunctions
    {
        public static LerpFunction<float> Float = (s, e, p) => s + (e - s) * p;
        public static LerpFunction<Vector2> Vector2 = (s, e, p) => { return Microsoft.Xna.Framework.Vector2.Lerp(s, e, p); };
        public static LerpFunction<Vector3> Vector3 = (s, e, p) => { return Microsoft.Xna.Framework.Vector3.Lerp(s, e, p); };
        public static LerpFunction<Vector4> Vector4 = (s, e, p) => { return Microsoft.Xna.Framework.Vector4.Lerp(s, e, p); };
        public static LerpFunction<Color> Color = (s, e, p) => { return Microsoft.Xna.Framework.Color.Lerp(s, e, p); };
        public static LerpFunction<Quaternion> Quaternion = (s, e, p) => { return Microsoft.Xna.Framework.Quaternion.Lerp(s, e, p); };
        public static LerpFunction<Rectangle> Rectangle = (s, e, p) =>
            {
                var pX = s.X + (e.X - s.X) * p;
                var pY = s.Y + (e.Y - s.Y) * p;
                var width = s.Width + (e.Width - s.Width) * p;
                var height = s.Height + (e.Height - s.Height) * p;
                return new Microsoft.Xna.Framework.Rectangle((int)pX, (int)pY, (int)width, (int)height);
            };
    }


}
