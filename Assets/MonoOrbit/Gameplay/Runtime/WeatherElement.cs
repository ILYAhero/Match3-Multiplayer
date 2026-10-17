namespace MonoOrbit.Gameplay
{
    // Stable numeric IDs: these values will later be transmitted over the network.
    public enum WeatherElement : byte
    {
        Empty = 0,
        Sun = 1,
        Rain = 2,
        Snow = 3,
        Wind = 4,
        Thunder = 5,
        Cloud = 6
    }
}
