namespace AccessingLBS.DTO
{
    public class CircularArcAreaDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double InnerRadius { get; set; }
        public double OuterRadius { get; set; }
        public double StartAngle { get; set; }
        public double StopAngle { get; set; }
        public double RequestedTime  { get; set; }
    }
}
