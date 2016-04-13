using System;
using System.Collections.Generic;

namespace myWindAPI
{

    /// <summary>
    /// 根据BS公式按期权价格计算隐含波动率以及按隐含波动率计算期权理论价格。
    /// 提供了两个静态方法，分别用来计算隐含波动率和期权理论价格
    /// </summary>
    class Impv
    {

        /// <summary>
        /// 计算每一份期权的开仓保证金。
        /// </summary>
        /// <param name="etfClose">etf收盘价</param>
        /// <param name="optionSettle">期权结算价</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="optionType">期权类型</param>
        /// <returns>返回保证金</returns>
        public static double Margin(double etfClose,double optionSettle,double strike,string optionType)
        {
            double myMargin = 0;
            if (optionType == "认购")
            {
                myMargin = optionSettle + Math.Max(0.12 * etfClose - Math.Max(strike - etfClose, 0), 0.07 * etfClose) ;
            }
            else
            {
                myMargin = Math.Min(optionSettle + Math.Max(0.12 * etfClose - Math.Max(etfClose - strike, 0), 0.07 * strike), strike) ;
            }
            return myMargin;
        }

        /// <summary>
        /// 利用期权价格等参数计算隐含波动率
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="optionPrice">期权价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到日期</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型区分看涨还是看跌</param>
        /// <returns>返回隐含波动率</returns>
        public static double sigma(double etfPrice, double optionPrice, double strike, double duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return sigmaOfCall(optionPrice, etfPrice, strike, duration / 252.0, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return sigmaOfPut(optionPrice, etfPrice, strike, duration / 252.0, r);
            }
            return 0;
        }
        /// <summary>
        /// 根据隐含波动率计算期权价格
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="sigma">隐含波动率</param>
        /// <param name="strike">期权行权价格</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型看涨还是看跌</param>
        /// <returns>返回期权理论价格</returns>
        public static double optionPrice(double etfPrice, double sigma, double strike, double duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return callPrice(etfPrice, strike, sigma, duration / 252.0, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return putPrice(etfPrice, strike, sigma, duration / 252.0, r);
            }
            return 0.0;
        }
        /// <summary>
        /// 计算希腊值delta
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="sigma">隐含波动率</param>
        /// <param name="strike">期权行权价格</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型看涨还是看跌</param>
        /// <returns></returns>
        public static double optionDelta(double etfPrice, double sigma, double strike, double duration, double r, string optionType)
        {
            if (duration <= 0)
            {
                Console.WriteLine("duration Wrong!");
                return 0;
            }
            if (sigma == 0)
            {
                return (optionType == "认购") ? 1 : -1;
            }
            double delta = 0;
            double durationByYear = duration / 252;
            double d1 = (Math.Log(etfPrice / strike) + (r + sigma * sigma / 2) * durationByYear) / (sigma * Math.Sqrt(durationByYear));
            if (optionType == "认购")
            {
                delta = normcdf(d1);
            }
            else
            {
                delta = normcdf(d1) - 1;
            }
            return Math.Round(delta,4);
        }
        /// <summary>
        /// 计算期权的希腊值Gamma
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="sigma">隐含波动率</param>
        /// <param name="strike">期权行权价格</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns></returns>
        public static double optionGamma(double etfPrice, double sigma, double strike, double duration, double r)
        {
            double gamma = 0;
            if (duration <= 0)
            {
                Console.WriteLine("duration Wrong!");
                return 0;
            }
            double durationByYear = duration / 252;
            double d1 = (Math.Log(etfPrice / strike) + (r + sigma * sigma / 2) * durationByYear) / (sigma * Math.Sqrt(durationByYear));
            gamma = 1 / (Math.Sqrt(2 * Math.PI) * sigma * Math.Sqrt(durationByYear) * etfPrice) * Math.Exp(-d1 * d1 / 2);
            return gamma;
        }
        /// <summary>
        /// 辅助函数erf(x),利用近似的方法进行计算
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回etf(x)</returns>
        private static double erf(double x)
        {
            double tau = 0;
            double t = 1 / (1 + 0.5 * Math.Abs(x));
            tau = t * Math.Exp(-Math.Pow(x, 2) - 1.26551223 + 1.00002368 * t + 0.37409196 * Math.Pow(t, 2) + 0.09678418 * Math.Pow(t, 3) - 0.18628806 * Math.Pow(t, 4) + 0.27886807 * Math.Pow(t, 5) - 1.13520398 * Math.Pow(t, 6) + 1.48851587 * Math.Pow(t, 7) - 0.82215223 * Math.Pow(t, 8) + 0.17087277 * Math.Pow(t, 9));
            if (x >= 0)
            {
                return 1 - tau;
            }
            else
            {
                return tau - 1;
            }
        }
        /// <summary>
        /// 辅助函数normcdf(x)
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回normcdf(x)</returns>
        private static double normcdf(double x)
        {
            return 0.5 + 0.5 * erf(x / Math.Sqrt(2));
        }
        /// <summary>
        /// 计算看涨期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看涨期权理论价格</returns>
        private static double callPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((spotPrice - strike) > 0) ? (spotPrice - strike) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
        }
        /// <summary>
        /// 计算看跌期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看跌期权理论价格</returns>
        private static double putPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((strike - spotPrice) > 0) ? (strike - spotPrice) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return -normcdf(-d1) * spotPrice + normcdf(-d2) * strike * Math.Exp(-r * duration);
        }
        /// <summary>
        /// 计算看涨期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        private static double sigmaOfCall(double callPrice, double spotPrice, double strike, double duration, double r)
        {
            double sigma = 1, sigmaold = 1;
            if (callPrice==0)
            {
                return 0;
            }
            if (callPrice < spotPrice - strike * Math.Exp(-r * duration))
            {
                return 0;
            }
            for (int num = 0; num < 10; num++)
            {
                sigmaold = sigma;
                double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
                double d2 = d1 - sigma * Math.Sqrt(duration);
                double f_sigma = normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
                double df_sigma = spotPrice * Math.Sqrt(duration) * Math.Exp(-d1 * d1 / 2) / (Math.Sqrt(2 * Math.PI));
                sigma = sigma + (callPrice - f_sigma) / df_sigma;
                if (Math.Abs(sigma - sigmaold) < 0.0001)
                {
                    break;
                }
                if (Math.Abs(sigma)>100 && duration<1)
                {
                    return 0;
                }
            }
            if (sigma > 3)
            {
                sigma = 0;
            }
            return Math.Round(sigma,4);
        }
        /// <summary>
        /// 计算看跌期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        private static double sigmaOfPut(double putPrice, double spotPrice, double strike, double duration, double r)
        {
            double sigma = 1, sigmaold = 1;
            if (putPrice == 0)
            {
                return 0;
            }
            if (strike * Math.Exp(-r * duration) - spotPrice > putPrice)
            {
                return 0;
            }
            for (int num = 0; num < 10; num++)
            {
                sigmaold = sigma;
                double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
                double d2 = d1 - sigma * Math.Sqrt(duration);
                double f_sigma = -normcdf(-d1) * spotPrice + normcdf(-d2) * strike * Math.Exp(-r * duration);
                double df_sigma = spotPrice * Math.Sqrt(duration) * Math.Exp(-d1 * d1 / 2) / (Math.Sqrt(2 * Math.PI));
                sigma = sigma + (putPrice - f_sigma) / df_sigma;
                if (Math.Abs(sigma - sigmaold) < 0.0001)
                {
                    break;
                }
                if (Math.Abs(sigma) > 100)
                {
                    return 0;
                }
            }
            if (sigma > 3 || sigma < 0)
            {
                sigma = 0;
            }
            return Math.Round(sigma,4);
        }
    }
}
