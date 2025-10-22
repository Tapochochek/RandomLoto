namespace RandomTrust.Core.Utilities;

public static class MathHelper
{
    public static double NormalCdf(double x)
    {
        // Упрощенная реализация функции нормального распределения
        return 0.5 * (1 + Erf(x / Math.Sqrt(2)));
    }
    
    public static double Erf(double x)
    {
        // Approximation of error function
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);

        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return sign * y;
    }
    
    public static double Erfc(double x) => 1 - Erf(x);
}
