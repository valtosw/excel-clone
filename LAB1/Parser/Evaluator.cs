using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB1.Parser
{
    public class Evaluator : ParserBaseVisitor<double>
    {
        private readonly Dictionary<string, string> cell_expressions;

        public Evaluator(Dictionary<string, string> cell_expressions)
        {
            this.cell_expressions = cell_expressions;
        }

        public double Evaluate(string expression)
        {
            var lexer = new ParserLexer(new AntlrInputStream(expression));
            var tokens = new CommonTokenStream(lexer);
            var parser = new ParserParser(tokens);

            var tree = parser.expression();

            ValidateTree(tree);

            return Visit(tree);
        }

        public override double VisitMultiplicativeExpression(ParserParser.MultiplicativeExpressionContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return context.GetChild(1).GetText() switch
            {
                "*" => left * right,
                "/" => DivideOrModuloLeftByRight(left, right, "/"),
                "div" => Math.Floor(DivideOrModuloLeftByRight(left, right, "/")),
                "mod" => DivideOrModuloLeftByRight(left, right, "mod"),
                _ => throw new InvalidOperationException("Invalid multiplicative operator")
            };
        }

        public override double VisitAdditiveExpression(ParserParser.AdditiveExpressionContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return context.GetChild(1).GetText() switch
            {
                "+" => left + right,
                "-" => left - right,
                _ => throw new InvalidOperationException("Invalid additive operator")
            };
        }

        public override double VisitUnaryExpression(ParserParser.UnaryExpressionContext context)
        {
            var right = Visit(context.right);

            if (context.op.Text == "-")
            {
                return -right;
            }

            if (context.op.Text == "+")
            {
                return right;
            }

            throw new InvalidOperationException("Invalid unary operator");
        }

        public override double VisitMmaxFunction(ParserParser.MmaxFunctionContext context)
        {
            if (context.exprList().expression().Length < 1)
            {
                throw new InvalidDataException("Недостатня кількість аргументів для функції mmax()");
            }

            var values = context.exprList()
                                .expression()
                                .Select(expr => Visit(expr))
                                .ToList();

            return values.Max();
        }

        public override double VisitMminFunction(ParserParser.MminFunctionContext context)
        {
            if (context.exprList().expression().Length < 1)
            {
                throw new InvalidDataException("Недостатня кількість аргументів для функції mmin()");
            }

            var values = context.exprList()
                                .expression()
                                .Select(expr => Visit(expr))
                                .ToList();

            return values.Min();
        }

        public override double VisitNumber(ParserParser.NumberContext context)
        {
            if (double.TryParse(context.GetText(), System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0;
        }
        
        public override double VisitParenthesizedExpression(ParserParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitCell(ParserParser.CellContext context)
        {
            string cell_name = context.GetText()[1..];

            if (cell_expressions.ContainsKey(cell_name))
            {
                string expr = cell_expressions[cell_name];
                return Evaluate(expr);
            }
            else
            {
                throw new InvalidDataException($"Не існує клітинки {cell_name}");
            }
        }

        public static double DivideOrModuloLeftByRight(double left, double right, string operation)
        {
            if (operation == "mod")
            {
                return right == 0 ? throw new DivideByZeroException("Операція mod: правий операнд не може дорівнювати нулю.") : (left % right);
            }

            if (operation == "/")
            {
                return right == 0 ? throw new DivideByZeroException("Операція ділення: правий операнд не може дорівнювати нулю.") : (left / right);
            }

            throw new InvalidOperationException("Невідомий оператор ділення");
        }

        public void ValidateTree(IParseTree tree)
        {
            if (tree is TerminalNodeImpl terminal_node && terminal_node.Symbol.Type == ParserLexer.INVALID)
            {
                throw new InvalidDataException("Введено невідомий символ");
            }

            for (int i = 0; i < tree.ChildCount; i++)
            {
                ValidateTree(tree.GetChild(i));
            }
        }

        public static void HandleEmptyExpression(string expr)
        {
            if (string.IsNullOrEmpty(expr) || string.IsNullOrWhiteSpace(expr))
            {
                throw new ArgumentNullException(nameof(expr), "Введений вираз пустий.");
            }
        }
    }
}
