namespace CodeRedundancyCheck.Model
{
    public class CodeLine
    {
        public CodeLine(string originalLineText, int originalLineNumber, int originalLinePosition)
        {
            this.OriginalLineText = originalLineText;
            this.OriginalLineNumber = originalLineNumber;
            this.OriginalLinePosition = originalLinePosition;
        }
        
        public string OriginalLineText { get; set; }

        public int OriginalLineNumber { get; set; }

        public int OriginalLinePosition { get; set; }

        public int CodeFileLineIndex { get; set; }

        public string WashedLineText { get; set; }

        public CodeLine ParentCodeLine { get; set; }

        public CodeLineMeaning CodeLineMeaning { get; set; }

        public override string ToString()
        {
            return this.OriginalLineNumber + ":" + this.WashedLineText;
        }
    }
}