using System.Text.Json;

namespace EORequests.Web.Forms
{
    public static class ExpressionEval
    {
        // Very small, safe evaluator supporting: ==, !=, >, >=, <, <=, &&, || over numbers/bools/strings
        // Example: "budget > 10000 && needApproval == true"
        public static bool IsVisible(string? expr, JsonElement model)
        {
            if (string.IsNullOrWhiteSpace(expr)) return true;

            try
            {
                // Tokenize super simply
                var tokens = expr.Replace("&&", " && ").Replace("||", " || ")
                                 .Replace(">=", " >=").Replace("<=", " <=")
                                 .Replace("==", " ==").Replace("!=", " !=")
                                 .Replace(">", " > ").Replace("<", " < ")
                                 .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                bool? current = null;
                string? pendingOp = null;

                for (int i = 0; i < tokens.Length;)
                {
                    // parse atomic condition: <field> <op> <literal>
                    var field = tokens[i++];

                    if (field is "&&" or "||")
                    {
                        pendingOp = field;
                        continue;
                    }

                    var op = tokens[i++];
                    var lit = tokens[i++];

                    bool cond = Compare(GetValue(model, field), op, lit);

                    if (current is null) current = cond;
                    else current = pendingOp == "&&" ? (current.Value && cond) : (current.Value || cond);

                    pendingOp = null;
                }

                return current ?? true;
            }
            catch
            {
                // if parsing fails, default to visible to avoid blocking users
                return true;
            }
        }

        private static JsonElement? GetValue(JsonElement model, string field)
        {
            if (model.ValueKind != JsonValueKind.Object) return null;
            if (!model.TryGetProperty(field, out var v)) return null;
            return v;
        }

        private static bool Compare(JsonElement? left, string op, string rightRaw)
        {
            if (left is null) return false;

            // normalize right
            bool rightBool;
            double rightNum;
            string right = rightRaw.Trim().Trim('"');

            // try numeric compare
            if (double.TryParse(right, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rightNum)
                && left.Value.ValueKind is JsonValueKind.Number)
            {
                var leftNum = left.Value.GetDouble();
                return op switch
                {
                    ">" => leftNum > rightNum,
                    "<" => leftNum < rightNum,
                    ">=" => leftNum >= rightNum,
                    "<=" => leftNum <= rightNum,
                    "==" => leftNum == rightNum,
                    "!=" => leftNum != rightNum,
                    _ => false
                };
            }

            // try bool compare
            if ((right.Equals("true", StringComparison.OrdinalIgnoreCase) || right.Equals("false", StringComparison.OrdinalIgnoreCase))
                && left.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                rightBool = bool.Parse(right);
                var leftBool = left.Value.GetBoolean();
                return op switch
                {
                    "==" => leftBool == rightBool,
                    "!=" => leftBool != rightBool,
                    _ => false
                };
            }

            // string compare
            var leftStr = left.Value.ToString();
            return op switch
            {
                "==" => string.Equals(leftStr, right, StringComparison.OrdinalIgnoreCase),
                "!=" => !string.Equals(leftStr, right, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
    }
}
