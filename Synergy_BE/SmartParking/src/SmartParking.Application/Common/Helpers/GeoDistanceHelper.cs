namespace SmartParking.Application.Common.Helpers;

/// <summary>
/// Helper class for calculating geographic distances using the Haversine formula.
/// Used for finding nearby parking locations based on latitude/longitude coordinates.
/// </summary>
public static class GeoDistanceHelper
{
    /// <summary>
    /// Earth's radius in meters (mean radius)
    /// </summary>
    private const double EarthRadiusMeters = 6371000;

    /// <summary>
    /// Calculates the great-circle distance between two points on Earth using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of point 1 (in degrees)</param>
    /// <param name="lon1">Longitude of point 1 (in degrees)</param>
    /// <param name="lat2">Latitude of point 2 (in degrees)</param>
    /// <param name="lon2">Longitude of point 2 (in degrees)</param>
    /// <returns>Distance in meters</returns>
    /// <remarks>
    /// Haversine formula:
    /// a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
    /// c = 2 ⋅ atan2( √a, √(1−a) )
    /// d = R ⋅ c
    /// where φ is latitude, λ is longitude, R is earth's radius
    /// </remarks>
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

        // Calculate differences
        var deltaLat = lat2Rad - lat1Rad;
        var deltaLon = lon2Rad - lon1Rad;

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Distance in meters
        var distance = EarthRadiusMeters * c;

        return distance;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Validates if latitude is within valid range [-90, 90].
    /// </summary>
    public static bool IsValidLatitude(double latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    /// <summary>
    /// Validates if longitude is within valid range [-180, 180].
    /// </summary>
    public static bool IsValidLongitude(double longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Calculates an approximate bounding box for efficient database filtering
    /// before applying precise Haversine distance calculation.
    /// PERFORMANCE: Reduces candidate set by ~95% for typical searches.
    /// </summary>
    /// <param name="latitude">Center latitude</param>
    /// <param name="longitude">Center longitude</param>
    /// <param name="radiusInMeters">Search radius</param>
    /// <returns>Tuple of (minLat, maxLat, minLon, maxLon)</returns>
    public static (double minLat, double maxLat, double minLon, double maxLon) GetBoundingBox(
        double latitude,
        double longitude,
        double radiusInMeters)
    {
        // 1 degree of latitude = ~111,320 meters (constant)
        const double metersPerDegreeLat = 111320.0;
        
        // 1 degree of longitude varies by latitude
        var metersPerDegreeLon = metersPerDegreeLat * Math.Cos(DegreesToRadians(latitude));

        var latDelta = radiusInMeters / metersPerDegreeLat;
        var lonDelta = radiusInMeters / metersPerDegreeLon;

        return (
            minLat: latitude - latDelta,
            maxLat: latitude + latDelta,
            minLon: longitude - lonDelta,
            maxLon: longitude + lonDelta
        );
    }
}
