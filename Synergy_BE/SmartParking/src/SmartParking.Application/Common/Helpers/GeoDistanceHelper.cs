namespace SmartParking.Application.Common.Helpers;

/// <summary>
/// Helper class for calculating geographic distances using Haversine formula
/// </summary>
public static class GeoDistanceHelper
{
    /// <summary>
    /// Earth's radius in meters
    /// </summary>
    private const double EarthRadiusMeters = 6371000;

    /// <summary>
    /// Calculate distance between two geographic points using Haversine formula
    /// </summary>
    /// <param name="lat1">Latitude of first point (in degrees)</param>
    /// <param name="lon1">Longitude of first point (in degrees)</param>
    /// <param name="lat2">Latitude of second point (in degrees)</param>
    /// <param name="lon2">Longitude of second point (in degrees)</param>
    /// <returns>Distance in meters</returns>
    public static double CalculateDistanceInMeters(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        // Convert degrees to radians
        var lat1Rad = DegreesToRadians(lat1);
        var lon1Rad = DegreesToRadians(lon1);
        var lat2Rad = DegreesToRadians(lat2);
        var lon2Rad = DegreesToRadians(lon2);

        // Haversine formula
        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Distance in meters
        var distance = EarthRadiusMeters * c;

        return distance;
    }

    /// <summary>
    /// Calculate distance between two geographic points (decimal overload)
    /// </summary>
    public static double CalculateDistanceInMeters(
        decimal lat1,
        decimal lon1,
        decimal lat2,
        decimal lon2)
    {
        return CalculateDistanceInMeters(
            (double)lat1,
            (double)lon1,
            (double)lat2,
            (double)lon2);
    }

    /// <summary>
    /// Check if a point is within a bounding box (for pre-filtering before Haversine)
    /// </summary>
    /// <param name="centerLat">Center latitude</param>
    /// <param name="centerLon">Center longitude</param>
    /// <param name="pointLat">Point latitude</param>
    /// <param name="pointLon">Point longitude</param>
    /// <param name="radiusMeters">Radius in meters</param>
    /// <returns>True if point is within bounding box</returns>
    public static bool IsWithinBoundingBox(
        double centerLat,
        double centerLon,
        double pointLat,
        double pointLon,
        double radiusMeters)
    {
        // Approximate degrees per meter (at equator)
        const double metersPerDegreeLat = 111320.0;
        var latRad = DegreesToRadians(centerLat);
        var metersPerDegreeLon = 111320.0 * Math.Cos(latRad);

        var latDelta = radiusMeters / metersPerDegreeLat;
        var lonDelta = radiusMeters / metersPerDegreeLon;

        return pointLat >= centerLat - latDelta &&
               pointLat <= centerLat + latDelta &&
               pointLon >= centerLon - lonDelta &&
               pointLon <= centerLon + lonDelta;
    }

    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
