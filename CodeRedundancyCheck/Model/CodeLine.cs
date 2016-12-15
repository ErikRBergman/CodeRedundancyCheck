namespace CodeRedundancyCheck.Model
{
    public class CodeLine
    {
        private string writableLine;

        public CodeLine(string originalLineText, int originalLineNumber, int originalLinePosition)
        {
            this.OriginalLineText = originalLineText;
            this.OriginalLineNumber = originalLineNumber;
            this.OriginalLinePosition = originalLinePosition;
        }

        private CodeLine()
        {
        }

        public static CodeLine CreateTargetLine(string targetLineText)
        {
            var line = new CodeLine
            {
                writableLine = targetLineText
            };

            return line;
        }


        public string OriginalLineText { get; set; }

        public int OriginalLineNumber { get; set; }

        public int OriginalLinePosition { get; set; }

        public int CodeFileLineIndex;

        public string WashedLineText;

        public int WashedLineHashCode;

        public uint Next4MiniHash;

        public string WriteableLine
        {
            get
            {
                return this.writableLine ?? this.OriginalLineText;
            }
            set
            {
                this.writableLine = value;
            }
        }

        public CodeLine ParentCodeLine { get; set; }

        public bool IsFullLine { get; set; } = true;

        public bool IsCodeLine { get; set; } = true;

        public CodeLineMeaning CodeLineMeaning { get; set; }

        public override string ToString()
        {
            return this.OriginalLineNumber + ":" + this.WashedLineText;
        }
    }
}