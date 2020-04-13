using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DePeuter.Shared
{
    public static class Analyzer
    {
        public static void BuildParameters(AnalyzeResult analyze, ref Dictionary<string, object> parameters)
        {
            if(analyze == null) return;

            analyze.BuildParameters(ref parameters);

            BuildParameters(analyze.Left, ref parameters);
            BuildParameters(analyze.Right, ref parameters);
        }

        public static string AnalyzeSelect(MemberExpression expression)
        {
            var analyzed = Analyze(null, null, expression);

            return analyzed.ParameterName;
        }

        public static AnalyzeResult AnalyzeWhere(IDatabaseProvider provider, Expression expression)
        {
            return Analyze(provider, null, expression);
        }

        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Add:
                    break;
                //case ExpressionType.AddAssign:
                //    break;
                //case ExpressionType.AddAssignChecked:
                //    break;
                case ExpressionType.AddChecked:
                    break;
                //case ExpressionType.AndAssign:
                //    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.ArrayLength:
                    break;
                //case ExpressionType.Assign:
                //    break;
                //case ExpressionType.Block:
                //    break;
                case ExpressionType.Call:
                    return Analyze(provider, previousAnalyze, (MethodCallExpression)expression);
                case ExpressionType.Coalesce:
                    break;
                case ExpressionType.Conditional:
                    break;
                case ExpressionType.Constant:
                    return Analyze(provider, previousAnalyze, (ConstantExpression)expression);
                case ExpressionType.Convert:
                    break;
                case ExpressionType.ConvertChecked:
                    break;
                //case ExpressionType.DebugInfo:
                //    break;
                //case ExpressionType.Decrement:
                //    break;
                //case ExpressionType.Default:
                //    break;
                case ExpressionType.Divide:
                    break;
                //case ExpressionType.DivideAssign:
                //    break;
                //case ExpressionType.Dynamic:
                //    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return Analyze(provider, previousAnalyze, (BinaryExpression)expression);
                case ExpressionType.ExclusiveOr:
                    break;
                //case ExpressionType.ExclusiveOrAssign:
                //    break;
                //case ExpressionType.Extension:
                //    break;
                //case ExpressionType.Goto:
                //    break;
                //case ExpressionType.Increment:
                //    break;
                //case ExpressionType.Index:
                //    break;
                case ExpressionType.Invoke:
                    break;
                //case ExpressionType.IsFalse:
                //    break;
                //case ExpressionType.IsTrue:
                //    break;
                //case ExpressionType.Label:
                //    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                //case ExpressionType.LeftShiftAssign:
                //    break;
                case ExpressionType.ListInit:
                    break;
                //case ExpressionType.Loop:
                //    break;
                case ExpressionType.MemberAccess:
                    return Analyze(provider, previousAnalyze, (MemberExpression)expression);
                case ExpressionType.MemberInit:
                    break;
                case ExpressionType.Modulo:
                    break;
                //case ExpressionType.ModuloAssign:
                //    break;
                case ExpressionType.Multiply:
                    break;
                //case ExpressionType.MultiplyAssign:
                //    break;
                //case ExpressionType.MultiplyAssignChecked:
                //    break;
                case ExpressionType.MultiplyChecked:
                    break;
                case ExpressionType.Negate:
                    break;
                case ExpressionType.NegateChecked:
                    break;
                case ExpressionType.New:
                    break;
                case ExpressionType.NewArrayBounds:
                    break;
                case ExpressionType.NewArrayInit:
                    break;
                case ExpressionType.Not:
                    break;
                //case ExpressionType.OnesComplement:
                //    break;
                //case ExpressionType.OrAssign:
                //    break;
                case ExpressionType.Parameter:
                    break;
                //case ExpressionType.PostDecrementAssign:
                //    break;
                //case ExpressionType.PostIncrementAssign:
                //    break;
                case ExpressionType.Power:
                    break;
                //case ExpressionType.PowerAssign:
                //    break;
                //case ExpressionType.PreDecrementAssign:
                //    break;
                //case ExpressionType.PreIncrementAssign:
                //    break;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.RightShift:
                    break;
                //case ExpressionType.RightShiftAssign:
                //    break;
                //case ExpressionType.RuntimeVariables:
                //    break;
                case ExpressionType.Subtract:
                    break;
                //case ExpressionType.SubtractAssign:
                //    break;
                //case ExpressionType.SubtractAssignChecked:
                //    break;
                case ExpressionType.SubtractChecked:
                    break;
                //case ExpressionType.Switch:
                //    break;
                //case ExpressionType.Throw:
                //    break;
                //case ExpressionType.Try:
                //    break;
                case ExpressionType.TypeAs:
                    break;
                //case ExpressionType.TypeEqual:
                //    break;
                case ExpressionType.TypeIs:
                    break;
                case ExpressionType.UnaryPlus:
                    break;
                //case ExpressionType.Unbox:
                //    break;
                default:
                    break;
            }
            if(expression is UnaryExpression)
                return Analyze(provider, previousAnalyze, (UnaryExpression)expression);

            var type = expression.GetType();
            return null;
        }
        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, MethodCallExpression expression)
        {
            switch(expression.Method.Name)
            {
                case "CompareString":
                    {
                        var analyzeResult = new AnalyzeResult(Analyze(provider, previousAnalyze, expression.Arguments[0]).ParameterName, Analyze(provider, previousAnalyze, expression.Arguments[1]).ParameterValue);
                        analyzeResult.SetParameterCombination(analyzeResult.ParameterName, analyzeResult.ParameterValue);
                        //analyzeResult.SetSql(analyzeResult.ParameterName + " = " + provider.CreateParameterName(analyzeResult.ParameterName));
                        return analyzeResult;
                    }
            }

            /*var arguments = new object[expression.Arguments.Count];
            for(int i = 0; i< expression.Arguments.Count; i++)
            {
                var argumentAnalyzeResult = Analyze(provider, previousAnalyze, expression.Arguments[i]);
                arguments[i] = argumentAnalyzeResult.ParameterName != null ? argumentAnalyzeResult.ParameterName:  argumentAnalyzeResult.ParameterValue;
            }
            var result = expression.Method.Invoke(expression.Object, arguments);*/

            //var analyzeResult = Analyze(provider, previousAnalyze, expression.Object);

            throw new NotImplementedException("Method '" + expression.Method.Name + "' in Analyze Call is not implemented");
        }
        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, UnaryExpression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Convert:
                    return Analyze(provider, previousAnalyze, expression.Operand);
                case ExpressionType.ConvertChecked:
                    return Analyze(provider, previousAnalyze, expression.Operand);
                case ExpressionType.Not:
                    var analyzeResult = Analyze(provider, previousAnalyze, expression.Operand);
                    analyzeResult.SetSql("(" + analyzeResult.GetSql() + ") = false");
                    return analyzeResult;
            }

            return null;
        }
        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, BinaryExpression expression)
        {
            var analyzeLeft = Analyze(provider, previousAnalyze, expression.Left);
            var analyzeRight = Analyze(provider, analyzeLeft, expression.Right);

            var leftSql = analyzeLeft.GetSql();
            if(leftSql == null) leftSql = analyzeLeft.ParameterName;
            var rightSql = analyzeRight.GetSql();
            if(rightSql == null) rightSql = provider.CreateParameterName(analyzeLeft.ParameterName);

            var analyzeResult = new AnalyzeResult(analyzeLeft, analyzeRight);
            if(!analyzeLeft.HasParameterCombination && !analyzeRight.HasParameterCombination)
                analyzeResult.SetParameterCombination(analyzeLeft.ParameterName, analyzeRight.ParameterValue);

            switch(expression.NodeType)
            {
                case ExpressionType.Equal:
                    if(analyzeRight.ParameterValue == null)
                    {
                        analyzeResult.SetSql(leftSql + " is null");
                    }
                    else
                    {
                        analyzeResult.SetSql(leftSql + " = " + rightSql);
                    }
                    break;
                case ExpressionType.NotEqual:
                    if(analyzeRight.ParameterValue == null)
                    {
                        analyzeResult.SetSql(leftSql + " is not null");
                    }
                    else
                    {
                        analyzeResult.SetSql(leftSql + " <> " + rightSql);
                    }
                    break;
                case ExpressionType.GreaterThan:
                    analyzeResult.SetSql(leftSql + " > " + rightSql);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    analyzeResult.SetSql(leftSql + " >= " + rightSql);
                    break;
                case ExpressionType.LessThan:
                    analyzeResult.SetSql(leftSql + " < " + rightSql);
                    break;
                case ExpressionType.LessThanOrEqual:
                    analyzeResult.SetSql(leftSql + " <= " + rightSql);
                    break;
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    analyzeResult.SetSql("(" + leftSql + ") and (" + rightSql + ")");
                    break;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    analyzeResult.SetSql("(" + leftSql + ") or (" + rightSql + ")");
                    break;
                default:
                    return null;
            }

            return analyzeResult;
        }
        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, MemberExpression expression, MemberExpression parentMemberExpression = null)
        {
            if(expression.Expression is MemberExpression)
            {
                return Analyze(provider, previousAnalyze, (MemberExpression)expression.Expression, expression);
                /*var memberExpression = (MemberExpression)expression.Expression;
                if (memberExpression.Expression is ConstantExpression)
                {
                    return Analyze(provider, previousAnalyze, (ConstantExpression)memberExpression.Expression, memberExpression);
                }*/
            }

            if(expression.Expression is ConstantExpression)
            {
                return Analyze(provider, previousAnalyze, (ConstantExpression)expression.Expression, expression, parentMemberExpression);
            }

            var pi = expression.Member.DeclaringType.GetProperty(expression.Member.Name);
            var attr = pi.GetCustomAttributes(typeof(SqlFieldAttribute), false).SingleOrDefault() as SqlFieldAttribute;
            return new AnalyzeResult(attr != null ? attr.GetName(pi) : expression.Member.Name, null);
        }
        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, ConstantExpression expression)
        {
            return new AnalyzeResult(null, expression.Value);
        }

        private static AnalyzeResult Analyze(IDatabaseProvider provider, AnalyzeResult previousAnalyze, ConstantExpression expression, MemberExpression parentExpression, MemberExpression parentMemberExpression = null)
        {
            var valueObject = expression.Value.GetType().GetField(parentExpression.Member.Name).GetValue(expression.Value);

            if(valueObject == null) return new AnalyzeResult(null, null);

            //keep going deeper until valueObject is a system type
            if(valueObject.GetType().IsSystemType())
            {
                return new AnalyzeResult(null, valueObject);
            }

            var value = valueObject.GetType().GetProperty(parentMemberExpression.Member.Name).GetValue(valueObject, null);

            return new AnalyzeResult(null, value);
        }
    }

    public class AnalyzeResult
    {
        private AnalyzeResult _left;
        private AnalyzeResult _right;
        private string _parameterName;
        private object _parameterValue;
        private string _sql;
        private bool _hasParameterCombination = false;

        public AnalyzeResult Left { get { return _left; } }
        public AnalyzeResult Right { get { return _right; } }
        public string ParameterName { get { return _parameterName; } }
        public object ParameterValue { get { return _parameterValue; } }
        public bool HasParameterCombination { get { return _hasParameterCombination; } }

        public string GetSql()
        {
            return _sql;
        }

        public AnalyzeResult(AnalyzeResult left, AnalyzeResult right)
        {
            _left = left;
            _right = right;
        }
        public AnalyzeResult(string parameterName, object parameterValue)
        {
            _parameterName = parameterName;
            _parameterValue = parameterValue;
        }
        public AnalyzeResult(AnalyzeResult left, AnalyzeResult right, string parameterName, object parameterValue)
        {
            _left = left;
            _right = right;
            _parameterName = parameterName;
            _parameterValue = parameterValue;
        }

        public void SetSql(string sql)
        {
            _sql = sql;
        }
        public void SetParameterCombination(string parameterName, object parameterValue)
        {
            if(parameterName == null) return;

            _parameterName = parameterName.Replace("\"", "");
            _parameterValue = parameterValue;
            _hasParameterCombination = true;
        }

        public void BuildParameters(ref Dictionary<string, object> parameters)
        {
            if(_hasParameterCombination && _parameterValue != null)
            {
                parameters.Add(_parameterName, _parameterValue);
            }
        }
    }
}
