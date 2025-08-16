namespace RPGCreator.Core.Parser.PRATT;

public static class PrattBasicFunctions
{
    public static readonly IReadOnlyDictionary<string, Func<double[], double>> Default
        = CreateDefault();

    private static IReadOnlyDictionary<string, Func<double[], double>> CreateDefault()
    {
        var defaultDictionnary = new Dictionary<string, Func<double[], double>>(StringComparer.OrdinalIgnoreCase)
        {
            ["min"]   = a => { ReqAtLeast(a,1); return a.Min(); },
            ["max"]   = a => { ReqAtLeast(a,1); return a.Max(); },
            ["clamp"] = a => { Req(a,3); var x=a[0]; var lo=a[1]; var hi=a[2];
                if (lo>hi) (lo,hi)=(hi,lo);
                return Math.Clamp(x, lo, hi); },
            ["abs"]   = a => { Req(a,1); return Math.Abs(a[0]); },
            ["floor"] = a => { Req(a,1); return Math.Floor(a[0]); },
            ["ceil"]  = a => { Req(a,1); return Math.Ceiling(a[0]); },
            ["round"] = a => { ReqBetween(a,1,2); return a.Length==1 ? Math.Round(a[0]) : Math.Round(a[0], (int)a[1]); },
            ["sqrt"]  = a => { Req(a,1); if (a[0]<0) throw new Exception("sqrt of negative"); return Math.Sqrt(a[0]); },
            ["pow"]   = a => { Req(a,2); return Math.Pow(a[0], a[1]); },
            ["lerp"]  = a => { Req(a,3); return a[0] + (a[1]-a[0]) * a[2]; },
            ["sin"] = a =>
                {
                    Req(a, 1);
                    return Math.Sin(a[0]);
                },
            ["cos"] = a =>
                {
                    Req(a, 1);
                    return Math.Cos(a[0]);
                },
            ["tan"] = a =>
                {
                    Req(a, 1);
                    return Math.Tan(a[0]);
                },
            ["asin"] = a =>
                {
                    Req(a, 1);
                    return Math.Asin(a[0]);
                },
            ["acos"] = a =>
                {
                    Req(a, 1);
                    return Math.Acos(a[0]);
                },
            ["atan"] = a =>
                {
                    Req(a, 1);
                    return Math.Atan(a[0]);
                },
            ["atan2"] = a =>
                {
                    Req(a, 2);
                    return Math.Atan2(a[0], a[1]);
                },
            ["log"] = a =>
                {
                    ReqBetween(a, 1, 2);
                    if (a.Length == 1)
                        return Math.Log(a[0]);
                    else
                        return Math.Log(a[0], a[1]);
                },
            ["mod"] = a =>
            {
                // Modulo function
                Req(a, 2);
                return a[0] % a[1];
            }
        };
        return defaultDictionnary;
    }

    private static void Req(double[] a, int n)
    {
        if (a.Length != n) throw new Exception($"Expected {n} args, got {a.Length}");
    }

    private static void ReqAtLeast(double[] a, int n)
    {
        if (a.Length < n) throw new Exception($"Expected at least {n} args, got {a.Length}");
    }

    private static void ReqBetween(double[] a, int lo, int hi)
    {
        if (a.Length < lo || a.Length > hi) throw new Exception($"Expected {lo}..{hi} args, got {a.Length}");
    }

}