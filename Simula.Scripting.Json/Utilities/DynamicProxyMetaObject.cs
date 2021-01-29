
#if HAVE_DYNAMIC
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Simula.Scripting.Json.Utilities
{
    internal sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
    {
        private readonly DynamicProxy<T> _proxy;

        internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _proxy = proxy;
        }

        private bool IsOverridden(string method)
        {
            return ReflectionUtils.IsMethodOverridden(_proxy.GetType(), typeof(DynamicProxy<T>), method);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryGetMember))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryGetMember), binder, NoArgs, e => binder.FallbackGetMember(this, e))
                : base.BindGetMember(binder);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TrySetMember))
                ? CallMethodReturnLast(nameof(DynamicProxy<T>.TrySetMember), binder, GetArgs(value), e => binder.FallbackSetMember(this, value, e))
                : base.BindSetMember(binder, value);
        }

        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryDeleteMember))
                ? CallMethodNoResult(nameof(DynamicProxy<T>.TryDeleteMember), binder, NoArgs, e => binder.FallbackDeleteMember(this, e))
                : base.BindDeleteMember(binder);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryConvert))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryConvert), binder, NoArgs, e => binder.FallbackConvert(this, e))
                : base.BindConvert(binder);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (!IsOverridden(nameof(DynamicProxy<T>.TryInvokeMember))) {
                return base.BindInvokeMember(binder, args);
            }
            Fallback fallback = e => binder.FallbackInvokeMember(this, args, e);

            return BuildCallMethodWithResult(
                nameof(DynamicProxy<T>.TryInvokeMember),
                binder,
                GetArgArray(args),
                BuildCallMethodWithResult(
                    nameof(DynamicProxy<T>.TryGetMember),
                    new GetBinderAdapter(binder),
                    NoArgs,
                    fallback(null),
                    e => binder.FallbackInvoke(e, args, null)
                    ),
                null
                );
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryCreateInstance))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryCreateInstance), binder, GetArgArray(args), e => binder.FallbackCreateInstance(this, args, e))
                : base.BindCreateInstance(binder, args);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryInvoke))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryInvoke), binder, GetArgArray(args), e => binder.FallbackInvoke(this, args, e))
                : base.BindInvoke(binder, args);
        }

        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryBinaryOperation))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryBinaryOperation), binder, GetArgs(arg), e => binder.FallbackBinaryOperation(this, arg, e))
                : base.BindBinaryOperation(binder, arg);
        }

        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryUnaryOperation))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryUnaryOperation), binder, NoArgs, e => binder.FallbackUnaryOperation(this, e))
                : base.BindUnaryOperation(binder);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryGetIndex))
                ? CallMethodWithResult(nameof(DynamicProxy<T>.TryGetIndex), binder, GetArgArray(indexes), e => binder.FallbackGetIndex(this, indexes, e))
                : base.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TrySetIndex))
                ? CallMethodReturnLast(nameof(DynamicProxy<T>.TrySetIndex), binder, GetArgArray(indexes, value), e => binder.FallbackSetIndex(this, indexes, value, e))
                : base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return IsOverridden(nameof(DynamicProxy<T>.TryDeleteIndex))
                ? CallMethodNoResult(nameof(DynamicProxy<T>.TryDeleteIndex), binder, GetArgArray(indexes), e => binder.FallbackDeleteIndex(this, indexes, e))
                : base.BindDeleteIndex(binder, indexes);
        }

        private delegate DynamicMetaObject Fallback(DynamicMetaObject? errorSuggestion);

        private static Expression[] NoArgs => CollectionUtils.ArrayEmpty<Expression>();

        private static IEnumerable<Expression> GetArgs(params DynamicMetaObject[] args)
        {
            return args.Select(arg => {
                Expression exp = arg.Expression;
                return exp.Type.IsValueType() ? Expression.Convert(exp, typeof(object)) : exp;
            });
        }

        private static Expression[] GetArgArray(DynamicMetaObject[] args)
        {
            return new[] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
        }

        private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
        {
            Expression exp = value.Expression;
            return new[]
            {
                Expression.NewArrayInit(typeof(object), GetArgs(args)),
                exp.Type.IsValueType() ? Expression.Convert(exp, typeof(object)) : exp
            };
        }

        private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
        {
            Type t = binder.GetType();
            while (!t.IsVisible()) {
                t = t.BaseType();
            }
            return Expression.Constant(binder, t);
        }
        private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback, Fallback? fallbackInvoke = null)
        {
            DynamicMetaObject fallbackResult = fallback(null);

            return BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
        }

        private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, DynamicMetaObject fallbackResult, Fallback? fallbackInvoke)
        {
            ParameterExpression result = Expression.Parameter(typeof(object), null);

            IList<Expression> callArgs = new List<Expression>();
            callArgs.Add(Expression.Convert(Expression, typeof(T)));
            callArgs.Add(Constant(binder));
            callArgs.AddRange(args);
            callArgs.Add(result);

            DynamicMetaObject resultMetaObject = new DynamicMetaObject(result, BindingRestrictions.Empty);
            if (binder.ReturnType != typeof(object)) {
                UnaryExpression convert = Expression.Convert(resultMetaObject.Expression, binder.ReturnType);

                resultMetaObject = new DynamicMetaObject(convert, resultMetaObject.Restrictions);
            }

            if (fallbackInvoke != null) {
                resultMetaObject = fallbackInvoke(resultMetaObject);
            }

            DynamicMetaObject callDynamic = new DynamicMetaObject(
                Expression.Block(
                    new[] { result },
                    Expression.Condition(
                        Expression.Call(
                            Expression.Constant(_proxy),
                            typeof(DynamicProxy<T>).GetMethod(methodName),
                            callArgs
                            ),
                        resultMetaObject.Expression,
                        fallbackResult.Expression,
                        binder.ReturnType
                        )
                    ),
                GetRestrictions().Merge(resultMetaObject.Restrictions).Merge(fallbackResult.Restrictions)
                );

            return callDynamic;
        }
        private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback)
        {
            DynamicMetaObject fallbackResult = fallback(null);
            ParameterExpression result = Expression.Parameter(typeof(object), null);

            IList<Expression> callArgs = new List<Expression>();
            callArgs.Add(Expression.Convert(Expression, typeof(T)));
            callArgs.Add(Constant(binder));
            callArgs.AddRange(args);
            callArgs[callArgs.Count - 1] = Expression.Assign(result, callArgs[callArgs.Count - 1]);

            return new DynamicMetaObject(
                Expression.Block(
                    new[] { result },
                    Expression.Condition(
                        Expression.Call(
                            Expression.Constant(_proxy),
                            typeof(DynamicProxy<T>).GetMethod(methodName),
                            callArgs
                            ),
                        result,
                        fallbackResult.Expression,
                        typeof(object)
                        )
                    ),
                GetRestrictions().Merge(fallbackResult.Restrictions)
                );
        }
        private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
        {
            DynamicMetaObject fallbackResult = fallback(null);

            IList<Expression> callArgs = new List<Expression>();
            callArgs.Add(Expression.Convert(Expression, typeof(T)));
            callArgs.Add(Constant(binder));
            callArgs.AddRange(args);
            return new DynamicMetaObject(
                Expression.Condition(
                    Expression.Call(
                        Expression.Constant(_proxy),
                        typeof(DynamicProxy<T>).GetMethod(methodName),
                        callArgs
                        ),
                    Expression.Empty(),
                    fallbackResult.Expression,
                    typeof(void)
                    ),
                GetRestrictions().Merge(fallbackResult.Restrictions)
                );
        }
        private BindingRestrictions GetRestrictions()
        {
            return (Value == null && HasValue)
                ? BindingRestrictions.GetInstanceRestriction(Expression, null)
                : BindingRestrictions.GetTypeRestriction(Expression, LimitType);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _proxy.GetDynamicMemberNames((T)Value);
        }
        private sealed class GetBinderAdapter : GetMemberBinder
        {
            internal GetBinderAdapter(InvokeMemberBinder binder) :
                base(binder.Name, binder.IgnoreCase)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotSupportedException();
            }
        }
    }
}

#endif