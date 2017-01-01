namespace CodeRedundancyCheck
{
    public struct CodeBlockLinkKey
    {
        public readonly long Key1;
        public readonly long Key2;

        private readonly int hashCode;

        public CodeBlockLinkKey(long key1, long key2)
        {
            this.Key1 = key1;
            this.Key2 = key2;

            this.hashCode = (int)(key1 + key2);
        }

        public override int GetHashCode() => this.hashCode;

        public override bool Equals(object obj)
        {
            var other = (CodeBlockLinkKey)obj;
            return (other.Key1 == this.Key1 && other.Key2 == this.Key2) ||
                   (other.Key1 == this.Key2 && other.Key2 == this.Key1);
        }
    }
}