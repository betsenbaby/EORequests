using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using EORequests.Web.Forms;


namespace EORequests.Tests
{
    public class ExpressionEvalTests
    {
        private static JsonElement Model(object o)
            => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(o));

        [Theory]
        [InlineData("budget > 10000", 5000, false)]
        [InlineData("budget > 10000", 15000, true)]
        [InlineData("needApproval == true", 0, true)] // 'needApproval' true in model below
        public void Numeric_And_Bool_Comparisons_Work(string expr, double number, bool expected)
        {
            var model = Model(new { budget = number, needApproval = true });
            var visible = ExpressionEval.IsVisible(expr, model);
            Assert.Equal(expected, visible);
        }

        [Theory]
        [InlineData("category == hardware", "hardware", true)]
        [InlineData("category == hardware", "software", false)]
        [InlineData("category != hardware", "software", true)]
        public void String_Comparisons_Work(string expr, string category, bool expected)
        {
            var model = Model(new { category });
            var visible = ExpressionEval.IsVisible(expr, model);
            Assert.Equal(expected, visible);
        }

        [Fact]
        public void Combined_And_Or_Works()
        {
            var model = Model(new { budget = 12000, needApproval = false });
            var expr = "budget > 10000 && needApproval == true";
            var visible = ExpressionEval.IsVisible(expr, model);
            Assert.False(visible);

            expr = "budget > 10000 || needApproval == true";
            visible = ExpressionEval.IsVisible(expr, model);
            Assert.True(visible);
        }

        [Fact]
        public void Invalid_Expression_Defaults_To_Visible()
        {
            var model = Model(new { a = 1 });
            var visible = ExpressionEval.IsVisible("a > > 5", model);
            Assert.True(visible);
        }
    }
}
