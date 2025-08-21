namespace EORequests.Web.Forms
{
    public class FormSchema
    {
        public string Title { get; set; } = "Form";
        public List<FormField> Fields { get; set; } = new();
    }

    public class FormField
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Type { get; set; } = "text"; // text, textarea, number, date, select, checkbox, radio, file
        public bool Required { get; set; }
        public string? Placeholder { get; set; }
        public string? Help { get; set; }
        public List<FormOption>? Options { get; set; }  // for select/radio
        public string? VisibleWhen { get; set; }       // expression, e.g. "budget > 10000 && needApproval == true"
    }

    public class FormOption
    {
        public string Value { get; set; } = default!;
        public string Text { get; set; } = default!;
    }
}
