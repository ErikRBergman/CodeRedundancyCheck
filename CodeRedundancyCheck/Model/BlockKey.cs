namespace CodeRedundancyCheck.Model
{
    public struct BlockKey
    {
        public readonly int FileUniqueId;

        public readonly int LineIndex;

        public readonly int NumberOfLines;

        public readonly long FullHash;

        private readonly int hashCode;

        public BlockKey(int fileUniqueId, int lineIndex, int numberOfLines)
        {
            this.FileUniqueId = fileUniqueId;
            this.LineIndex = lineIndex;
            this.NumberOfLines = numberOfLines;
            this.FullHash = (((long)fileUniqueId) << 32) + (lineIndex << 16) + numberOfLines;
            this.hashCode = (int)this.FullHash;
        }

        public override string ToString()
        {
            return "FileId: " + this.FileUniqueId + ", LineIndex:" + this.LineIndex + ", NumberOfLines: " + this.NumberOfLines;
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            var other = (BlockKey)obj;
            return other.FullHash == this.FullHash;
        }
    }
}