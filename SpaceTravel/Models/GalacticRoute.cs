namespace SpaceTravel.Models
{
    public class GalacticRoute
    {
        public string Name { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<string> NavigationPoints { get; set; }
        public string Duration { get; set; }
        public List<string> Dangers { get; set; }
        public string FuelUsage { get; set; }
        public string Description { get; set; }
    }
}
