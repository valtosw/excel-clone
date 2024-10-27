using LAB1.Parser;

namespace LAB1.UnitTests
{
    public class EvaluatorTests
    {
        [Fact]
        public void Evaluate_ValidExpression_ReturnsValidResult()
        {
            var evaluator = new Evaluator(new Dictionary<string, string>());

            var result1 = evaluator.Evaluate("(2 + -3 * (9 mod 7)) - mmin(9, 23, 1 - 1.5)");

            Assert.Equal(-3.5, result1);
        }

        [Fact]
        public void Evaluate_InvalidExpression_ThrowsDivideByZeroException()
        {
            var evaluator = new Evaluator(new Dictionary<string, string>());

            var exception = Assert.Throws<DivideByZeroException>(() => evaluator.Evaluate("3 / 0"));

            Assert.Equal("Операція ділення: правий операнд не може дорівнювати нулю.", exception.Message);
        }

        [Fact]
        public void Evaluate_InvalidExpression_ThrowsInvalidDataException()
        {
            var evaluator = new Evaluator(new Dictionary<string, string>());

            var exception = Assert.Throws<InvalidDataException>(() => evaluator.Evaluate("9 + helloworld 9 -2..0 + 2 mod 0 mod 2"));

            Assert.Equal("Введено невідомий символ", exception.Message);
        }
    }
}