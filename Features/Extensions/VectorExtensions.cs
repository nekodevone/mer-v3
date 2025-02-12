using System.Globalization;
using UnityEngine;

namespace ProjectMER.Features.Extensions;

public static class VectorExtensions
{
	public static Vector3 ToVector3(this string s)
	{
		s = s.Trim('(', ')').Replace(" ", "");
		string[] split = s.Split(',');

		float x = float.Parse(split[0]);
		float y = float.Parse(split[1]);
		float z = float.Parse(split[2]);

		return new Vector3(x, y, z);
	}

	public static bool TryParseToFloat(this string s, out float result) => float.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);

	public static bool TryGetVector(string x, string y, string z, out Vector3 vector)
    {
        vector = Vector3.zero;

        if (!x.TryParseToFloat(out float xValue) || !y.TryParseToFloat(out float yValue) || !z.TryParseToFloat(out float zValue))
            return false;

        vector = new Vector3(xValue, yValue, zValue);
        return true;
    }
}
