namespace Protocol
{
    public struct BoundingBox
    {
        public readonly float Width, Height;

        public BoundingBox(float width, float height)
        {
            Width = width; Height = height;
        }

        public bool TestCollision(BoundingBox boundingBox)
        {
            throw new System.NotImplementedException();
        }

    }

}
