namespace HP.GFriend.Utils.Charter
{
    internal class PointSD
    {
        private string x;
        private double y;

        public string X
        {
            get { return x; }
            set { x = value; }
        }
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public PointSD(string x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
