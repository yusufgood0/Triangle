using System;
using System.Globalization;
using Microsoft.Xna.Framework;

public static class SkyColorGenerator
{
    // Anchor points for the day, from 0-1 representing time of day
    private static readonly (float time, Color color)[] skyAnchors = new (float, Color)[]
    {
        (0.00f, new Color(10, 10, 35)),      // Midnight
        (0.05f, new Color(20, 20, 50)),      // Pre-dawn
        (0.10f, new Color(60, 60, 100)),     // Early morning twilight
        (0.20f, new Color(255, 150, 80)),    // Sunrise
        (0.35f, new Color(135, 206, 235)),   // Mid-morning
        (0.50f, new Color(135, 206, 250)),   // Noon (bright sky blue)
        (0.65f, new Color(135, 206, 235)),   // Mid-afternoon
        (0.75f, new Color(255, 120, 70)),    // Sunset
        (0.85f, new Color(70, 50, 100)),     // Twilight
        (0.95f, new Color(20, 20, 50)),      // Pre-midnight
        (1.00f, new Color(10, 10, 35))       // Midnight
    };
    public static Vector3 GetSkyDirection(float timeOfDay)
    {
        timeOfDay %= 1f;
        float angle = (float)timeOfDay * MathF.Tau;
        Vector3 vector = new Vector3(
            MathF.Sin(angle),
            -MathF.Cos(angle),
            MathF.Sin(angle)
            );
        vector.Normalize();
        return vector;
    }
    public static Color GetSkyColor(float timeOfDay)
    {
        // Clamp input to [0,1]
        timeOfDay %= 1f;

        // Find the two anchor points we are between
        for (int i = 0; i < skyAnchors.Length - 1; i++)
        {
            float t1 = skyAnchors[i].time;
            float t2 = skyAnchors[i + 1].time;

            if (timeOfDay >= t1 && timeOfDay <= t2)
            {
                float t = (timeOfDay - t1) / (t2 - t1); // Normalize between 0 and 1
                return Color.Lerp(skyAnchors[i].color, skyAnchors[i + 1].color, t);
            }
        }

        // Fallback (shouldn't happen)
        return skyAnchors[0].color;
    }
}