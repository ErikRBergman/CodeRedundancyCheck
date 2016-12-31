namespace CodeRedundancyCheck
{
    public struct CodeFileMatchKey
    {
        private readonly CodeFile codeFile;

        private readonly int firstLineNumber;

        private readonly int numberOfLines;

        private readonly int hashCode;

        public CodeFileMatchKey(CodeFile codeFile, int firstLineNumber, int numberOfLines)
        {
            this.codeFile = codeFile;
            this.firstLineNumber = firstLineNumber;
            this.numberOfLines = numberOfLines;
            this.hashCode = codeFile.GetHashCode() + firstLineNumber + numberOfLines;
        }

        public override int GetHashCode() => this.hashCode;

        public override bool Equals(object obj)
        {
            var other = (CodeFileMatchKey)obj;
            return other.codeFile == this.codeFile && other.firstLineNumber == this.firstLineNumber && other.numberOfLines == this.numberOfLines;
        }
    }
}