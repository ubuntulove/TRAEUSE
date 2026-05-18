using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OfficeSuite.Spreadsheet.Functions
{
    public class FunctionContext
    {
        public List<object> Parameters { get; set; } = new List<object>();
    }

    public enum CellError
    {
        Null = 0,
        Div0 = 1,
        Value = 2,
        Ref = 3,
        Name = 4,
        Num = 5,
        NA = 6,
        GettingData = 7
    }

    public class AdvancedSpreadsheetFunctions
    {
        private readonly Dictionary<string, Func<FunctionContext, object>> _functions;

        public AdvancedSpreadsheetFunctions()
        {
            _functions = new Dictionary<string, Func<FunctionContext, object>>(StringComparer.OrdinalIgnoreCase);
            RegisterDatabaseFunctions();
            RegisterDateTimeFunctions();
            RegisterEngineeringFunctions();
            RegisterFinancialFunctions();
            RegisterInformationFunctions();
            RegisterLogicalFunctions();
            RegisterLookupFunctions();
            RegisterMathFunctions();
            RegisterStatisticalFunctions();
            RegisterTextFunctions();
            RegisterWebFunctions();
        }

        public object Execute(string functionName, FunctionContext context)
        {
            if (_functions.TryGetValue(functionName, out var function))
            {
                return function(context);
            }
            throw new ArgumentException($"Function '{functionName}' not found");
        }

        private void RegisterDatabaseFunctions()
        {
            _functions["DAVERAGE"] = context => CalculateAverage(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DCOUNT"] = context => CalculateCount(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DCOUNTA"] = context => CalculateCountA(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DGET"] = context => GetValue(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DMAX"] = context => CalculateMax(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DMIN"] = context => CalculateMin(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DPRODUCT"] = context => CalculateProduct(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DSTDEV"] = context => CalculateStDev(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DSTDEVP"] = context => CalculateStDevP(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DSUM"] = context => CalculateSum(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DVAR"] = context => CalculateVar(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
            _functions["DVARP"] = context => CalculateVarP(GetArrayParameter(context, 0), GetStringOrNumberParameter(context, 1), GetArrayParameter(context, 2));
        }

        private void RegisterDateTimeFunctions()
        {
            _functions["DATE"] = context => new DateTime(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]));
            _functions["DATEVALUE"] = context => DateTime.TryParse(context.Parameters[0].ToString(), out DateTime result) ? result : 0;
            _functions["DAY"] = context => GetDateParameter(context, 0).Day;
            _functions["DAYS"] = context => (GetDateParameter(context, 0) - GetDateParameter(context, 1)).Days;
            _functions["DAYS360"] = context => CalculateDays360(GetDateParameter(context, 0), GetDateParameter(context, 1), context.Parameters.Count > 2 && Convert.ToBoolean(context.Parameters[2]));
            _functions["EDATE"] = context => GetDateParameter(context, 0).AddMonths(Convert.ToInt32(context.Parameters[1]));
            _functions["EOMONTH"] = context => { var result = GetDateParameter(context, 0).AddMonths(Convert.ToInt32(context.Parameters[1]) + 1); return new DateTime(result.Year, result.Month, 1).AddDays(-1); };
            _functions["HOUR"] = context => GetDateTimeParameter(context, 0).Hour;
            _functions["ISOWEEKNUM"] = context => System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(GetDateParameter(context, 0), System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            _functions["MINUTE"] = context => GetDateTimeParameter(context, 0).Minute;
            _functions["MONTH"] = context => GetDateParameter(context, 0).Month;
            _functions["NETWORKDAYS"] = context => CalculateNetworkDays(GetDateParameter(context, 0), GetDateParameter(context, 1), context.Parameters.Count > 2 ? GetArrayParameter(context, 2) : null);
            _functions["NETWORKDAYS.INTL"] = context => CalculateNetworkDaysIntl(GetDateParameter(context, 0), GetDateParameter(context, 1), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1, context.Parameters.Count > 3 ? GetArrayParameter(context, 3) : null);
            _functions["NOW"] = context => DateTime.Now;
            _functions["SECOND"] = context => GetDateTimeParameter(context, 0).Second;
            _functions["TIME"] = context => new TimeSpan(0, Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2])).TotalDays;
            _functions["TIMEVALUE"] = context => TimeSpan.TryParse(context.Parameters[0].ToString(), out TimeSpan result) ? result.TotalDays : 0;
            _functions["TODAY"] = context => DateTime.Today;
            _functions["WEEKDAY"] = context => CalculateWeekday(GetDateParameter(context, 0), context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 1);
            _functions["WEEKNUM"] = context => CalculateWeekNum(GetDateParameter(context, 0), context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 1);
            _functions["WORKDAY"] = context => CalculateWorkday(GetDateParameter(context, 0), Convert.ToInt32(context.Parameters[1]), context.Parameters.Count > 2 ? GetArrayParameter(context, 2) : null);
            _functions["WORKDAY.INTL"] = context => CalculateWorkdayIntl(GetDateParameter(context, 0), Convert.ToInt32(context.Parameters[1]), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1, context.Parameters.Count > 3 ? GetArrayParameter(context, 3) : null);
            _functions["YEAR"] = context => GetDateParameter(context, 0).Year;
            _functions["YEARFRAC"] = context => CalculateYearFrac(GetDateParameter(context, 0), GetDateParameter(context, 1), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0);
        }

        private void RegisterEngineeringFunctions()
        {
            _functions["BESSELI"] = context => CalculateBesselI(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["BESSELJ"] = context => CalculateBesselJ(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["BESSELK"] = context => CalculateBesselK(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["BESSELY"] = context => CalculateBesselY(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["BIN2DEC"] = context => Convert.ToInt64(context.Parameters[0].ToString(), 2);
            _functions["BIN2HEX"] = context => { string hex = Convert.ToString(Convert.ToInt64(context.Parameters[0].ToString(), 2), 16).ToUpper(); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; return places > 0 && hex.Length < places ? hex.PadLeft(places, '0') : hex; };
            _functions["BIN2OCT"] = context => { string octal = Convert.ToString(Convert.ToInt64(context.Parameters[0].ToString(), 2), 8); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; return places > 0 && octal.Length < places ? octal.PadLeft(places, '0') : octal; };
            _functions["BITAND"] = context => Convert.ToInt64(context.Parameters[0]) & Convert.ToInt64(context.Parameters[1]);
            _functions["BITLSHIFT"] = context => Convert.ToInt64(context.Parameters[0]) << Convert.ToInt32(context.Parameters[1]);
            _functions["BITOR"] = context => Convert.ToInt64(context.Parameters[0]) | Convert.ToInt64(context.Parameters[1]);
            _functions["BITRSHIFT"] = context => Convert.ToInt64(context.Parameters[0]) >> Convert.ToInt32(context.Parameters[1]);
            _functions["BITXOR"] = context => Convert.ToInt64(context.Parameters[0]) ^ Convert.ToInt64(context.Parameters[1]);
            _functions["COMPLEX"] = context => $"{Convert.ToDouble(context.Parameters[0])}+{Convert.ToDouble(context.Parameters[1])}{(context.Parameters.Count > 2 ? context.Parameters[2].ToString() : "i")}";
            _functions["CONVERT"] = context => ConvertUnit(Convert.ToDouble(context.Parameters[0]), context.Parameters[1].ToString(), context.Parameters[2].ToString());
            _functions["DEC2BIN"] = context => { long number = Convert.ToInt64(context.Parameters[0]); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string binary = Convert.ToString(number, 2); return places > 0 && binary.Length < places ? binary.PadLeft(places, '0') : binary; };
            _functions["DEC2HEX"] = context => { long number = Convert.ToInt64(context.Parameters[0]); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string hex = Convert.ToString(number, 16).ToUpper(); return places > 0 && hex.Length < places ? hex.PadLeft(places, '0') : hex; };
            _functions["DEC2OCT"] = context => { long number = Convert.ToInt64(context.Parameters[0]); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string octal = Convert.ToString(number, 8); return places > 0 && octal.Length < places ? octal.PadLeft(places, '0') : octal; };
            _functions["DELTA"] = context => Math.Abs(Convert.ToDouble(context.Parameters[0]) - (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 0)) < 1e-9 ? 1 : 0;
            _functions["ERF"] = context => CalculateErf(Convert.ToDouble(context.Parameters[0]), context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : Convert.ToDouble(context.Parameters[0]));
            _functions["ERF.PRECISE"] = context => CalculateErf(0, Convert.ToDouble(context.Parameters[0]));
            _functions["ERFC"] = context => 1 - CalculateErf(0, Convert.ToDouble(context.Parameters[0]));
            _functions["ERFC.PRECISE"] = context => 1 - CalculateErf(0, Convert.ToDouble(context.Parameters[0]));
            _functions["GESTEP"] = context => Convert.ToDouble(context.Parameters[0]) >= (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 0) ? 1 : 0;
            _functions["HEX2BIN"] = context => { string hex = context.Parameters[0].ToString(); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string binary = Convert.ToString(Convert.ToInt64(hex, 16), 2); return places > 0 && binary.Length < places ? binary.PadLeft(places, '0') : binary; };
            _functions["HEX2DEC"] = context => Convert.ToInt64(context.Parameters[0].ToString(), 16);
            _functions["HEX2OCT"] = context => { string hex = context.Parameters[0].ToString(); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string octal = Convert.ToString(Convert.ToInt64(hex, 16), 8); return places > 0 && octal.Length < places ? octal.PadLeft(places, '0') : octal; };
            _functions["IMABS"] = context => CalculateIMAbs(context.Parameters[0].ToString());
            _functions["IMAGINARY"] = context => ExtractImaginaryPart(context.Parameters[0].ToString());
            _functions["IMARGUMENT"] = context => CalculateIMArgument(context.Parameters[0].ToString());
            _functions["IMCONJUGATE"] = context => CalculateIMConjugate(context.Parameters[0].ToString());
            _functions["IMCOS"] = context => CalculateIMCos(context.Parameters[0].ToString());
            _functions["IMCOSH"] = context => CalculateIMCosh(context.Parameters[0].ToString());
            _functions["IMDIV"] = context => CalculateIMDiv(context.Parameters[0].ToString(), context.Parameters[1].ToString());
            _functions["IMEXP"] = context => CalculateIMExp(context.Parameters[0].ToString());
            _functions["IMLN"] = context => CalculateIMLn(context.Parameters[0].ToString());
            _functions["IMLOG2"] = context => CalculateIMLog2(context.Parameters[0].ToString());
            _functions["IMLOG10"] = context => CalculateIMLog10(context.Parameters[0].ToString());
            _functions["IMPOWER"] = context => CalculateIMPower(context.Parameters[0].ToString(), Convert.ToDouble(context.Parameters[1]));
            _functions["IMPRODUCT"] = context => { double realResult = 1, imagResult = 0; for (int i = 0; i < context.Parameters.Count; i++) { var (real, imag) = ParseComplex(context.Parameters[i].ToString()); double newReal = realResult * real - imagResult * imag; double newImag = realResult * imag + imagResult * real; realResult = newReal; imagResult = newImag; } return FormatComplex(realResult, imagResult); };
            _functions["IMREAL"] = context => ExtractRealPart(context.Parameters[0].ToString());
            _functions["IMSIN"] = context => CalculateIMSin(context.Parameters[0].ToString());
            _functions["IMSINH"] = context => CalculateIMSinh(context.Parameters[0].ToString());
            _functions["IMSQRT"] = context => CalculateIMSqrt(context.Parameters[0].ToString());
            _functions["IMSUB"] = context => { var (real1, imag1) = ParseComplex(context.Parameters[0].ToString()); var (real2, imag2) = ParseComplex(context.Parameters[1].ToString()); return FormatComplex(real1 - real2, imag1 - imag2); };
            _functions["IMSUM"] = context => { double realSum = 0, imagSum = 0; for (int i = 0; i < context.Parameters.Count; i++) { var (real, imag) = ParseComplex(context.Parameters[i].ToString()); realSum += real; imagSum += imag; } return FormatComplex(realSum, imagSum); };
            _functions["OCT2BIN"] = context => { string octal = context.Parameters[0].ToString(); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string binary = Convert.ToString(Convert.ToInt64(octal, 8), 2); return places > 0 && binary.Length < places ? binary.PadLeft(places, '0') : binary; };
            _functions["OCT2DEC"] = context => Convert.ToInt64(context.Parameters[0].ToString(), 8);
            _functions["OCT2HEX"] = context => { string octal = context.Parameters[0].ToString(); int places = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; string hex = Convert.ToString(Convert.ToInt64(octal, 8), 16).ToUpper(); return places > 0 && hex.Length < places ? hex.PadLeft(places, '0') : hex; };
        }

        private void RegisterFinancialFunctions()
        {
            _functions["ACCRINT"] = context => CalculateAccrint(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToInt32(context.Parameters[5]), context.Parameters.Count > 6 ? Convert.ToInt32(context.Parameters[6]) : 0);
            _functions["ACCRINTM"] = context => CalculateAccrintM(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["AMORDEGRC"] = context => CalculateAmorDegrc(Convert.ToDouble(context.Parameters[0]), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), context.Parameters.Count > 6 ? Convert.ToInt32(context.Parameters[6]) : 0);
            _functions["AMORLINC"] = context => CalculateAmorLinc(Convert.ToDouble(context.Parameters[0]), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), context.Parameters.Count > 6 ? Convert.ToInt32(context.Parameters[6]) : 0);
            _functions["COUPDAYBS"] = context => CalculateCoupDayBs(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["COUPDAYS"] = context => CalculateCoupDays(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["COUPDAYSNC"] = context => CalculateCoupDaysNc(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["COUPNCD"] = context => CalculateCoupNcd(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["COUPNUM"] = context => CalculateCoupNum(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["COUPPCD"] = context => CalculateCoupPcd(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0);
            _functions["CUMIPMT"] = context => CalculateCumIpmt(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]), Convert.ToInt32(context.Parameters[4]), Convert.ToInt32(context.Parameters[5]));
            _functions["CUMPRINC"] = context => CalculateCumPrinc(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]), Convert.ToInt32(context.Parameters[4]), Convert.ToInt32(context.Parameters[5]));
            _functions["DB"] = context => CalculateDb(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 12);
            _functions["DDB"] = context => CalculateDdb(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToDouble(context.Parameters[4]) : 2);
            _functions["DISC"] = context => CalculateDisc(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["DOLLARDE"] = context => CalculateDollarDe(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["DOLLARFR"] = context => CalculateDollarFr(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["DURATION"] = context => CalculateDuration(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), Convert.ToInt32(context.Parameters[4]), context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
            _functions["EFFECT"] = context => CalculateEffect(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["FV"] = context => CalculateFv(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["FVSCHEDULE"] = context => { double result = Convert.ToDouble(context.Parameters[0]); foreach (var rate in GetArrayParameter(context, 1)) { result *= (1 + Convert.ToDouble(rate)); } return result; };
            _functions["INTRATE"] = context => CalculateIntrate(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["IPMT"] = context => CalculateIpmt(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToDouble(context.Parameters[4]) : 0, context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
            _functions["IRR"] = context => CalculateIrr(GetArrayParameter(context, 0), context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 0.1);
            _functions["ISPMT"] = context => CalculateIsPmt(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]));
            _functions["MDURATION"] = context => CalculateMduration(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), Convert.ToInt32(context.Parameters[4]), context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
            _functions["MIRR"] = context => CalculateMirr(GetArrayParameter(context, 0), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]));
            _functions["NOMINAL"] = context => CalculateNominal(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["NPER"] = context => CalculateNper(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["NPV"] = context => { double rate = Convert.ToDouble(context.Parameters[0]); double npv = 0; for (int i = 1; i < context.Parameters.Count; i++) { var values = GetArrayParameter(context, i); for (int j = 0; j < values.Count; j++) { npv += Convert.ToDouble(values[j]) / Math.Pow(1 + rate, j + 1); } } return npv; };
            _functions["ODDFPRICE"] = context => CalculateOddfPrice(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), GetDateParameter(context, 3), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), Convert.ToDouble(context.Parameters[6]), Convert.ToInt32(context.Parameters[7]), context.Parameters.Count > 8 ? Convert.ToInt32(context.Parameters[8]) : 0);
            _functions["ODDFYIELD"] = context => CalculateOddfYield(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), GetDateParameter(context, 3), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), Convert.ToDouble(context.Parameters[6]), Convert.ToInt32(context.Parameters[7]), context.Parameters.Count > 8 ? Convert.ToInt32(context.Parameters[8]) : 0);
            _functions["ODDLPRICE"] = context => CalculateOddlPrice(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), Convert.ToInt32(context.Parameters[6]), context.Parameters.Count > 7 ? Convert.ToInt32(context.Parameters[7]) : 0);
            _functions["ODDLYIELD"] = context => CalculateOddlYield(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToDouble(context.Parameters[5]), Convert.ToInt32(context.Parameters[6]), context.Parameters.Count > 7 ? Convert.ToInt32(context.Parameters[7]) : 0);
            _functions["PDURATION"] = context => Math.Log(Convert.ToDouble(context.Parameters[2]) / Convert.ToDouble(context.Parameters[1])) / Math.Log(1 + Convert.ToDouble(context.Parameters[0]));
            _functions["PMT"] = context => CalculatePmt(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["PPMT"] = context => CalculatePpmt(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToDouble(context.Parameters[4]) : 0, context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
            _functions["PRICE"] = context => CalculatePrice(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToInt32(context.Parameters[5]), context.Parameters.Count > 6 ? Convert.ToInt32(context.Parameters[6]) : 0);
            _functions["PRICEDISC"] = context => CalculatePriceDisc(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["PRICEMAT"] = context => CalculatePriceMat(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
            _functions["PV"] = context => CalculatePv(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["RATE"] = context => CalculateRate(Convert.ToInt32(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0, context.Parameters.Count > 5 ? Convert.ToDouble(context.Parameters[5]) : 0.1);
            _functions["RECEIVED"] = context => CalculateReceived(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["RRI"] = context => Math.Pow(Convert.ToDouble(context.Parameters[2]) / Convert.ToDouble(context.Parameters[1]), 1.0 / Convert.ToInt32(context.Parameters[0])) - 1;
            _functions["SLN"] = context => (Convert.ToDouble(context.Parameters[0]) - Convert.ToDouble(context.Parameters[1])) / Convert.ToInt32(context.Parameters[2]);
            _functions["SYD"] = context => { int life = Convert.ToInt32(context.Parameters[2]); int period = Convert.ToInt32(context.Parameters[3]); return (Convert.ToDouble(context.Parameters[0]) - Convert.ToDouble(context.Parameters[1])) * (life - period + 1) * 2 / (life * (life + 1)); };
            _functions["TBILLEQ"] = context => CalculateTbilleq(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]));
            _functions["TBILLPRICE"] = context => CalculateTbillPrice(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]));
            _functions["TBILLYIELD"] = context => CalculateTbillYield(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]));
            _functions["VDB"] = context => CalculateVdb(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]), Convert.ToInt32(context.Parameters[4]), context.Parameters.Count > 5 ? Convert.ToDouble(context.Parameters[5]) : 2, context.Parameters.Count > 6 && Convert.ToBoolean(context.Parameters[6]));
            _functions["XIRR"] = context => { var values = GetArrayParameter(context, 0); var dates = new List<DateTime>(); for (int i = 1; i < context.Parameters.Count; i++) { dates.Add(GetDateParameter(context, i)); } return CalculateXIrr(values, dates, context.Parameters.Count > values.Count + 1 ? Convert.ToDouble(context.Parameters[values.Count + 1]) : 0.1); };
            _functions["XNPV"] = context => { double rate = Convert.ToDouble(context.Parameters[0]); var values = GetArrayParameter(context, 1); var dates = new List<DateTime>(); for (int i = 2; i < context.Parameters.Count; i++) { dates.Add(GetDateParameter(context, i)); } return CalculateXnpv(rate, values, dates); };
            _functions["YIELD"] = context => CalculateYield(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), Convert.ToInt32(context.Parameters[5]), context.Parameters.Count > 6 ? Convert.ToInt32(context.Parameters[6]) : 0);
            _functions["YIELDDISC"] = context => CalculateYieldDisc(GetDateParameter(context, 0), GetDateParameter(context, 1), Convert.ToDouble(context.Parameters[2]), Convert.ToDouble(context.Parameters[3]), context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 0);
            _functions["YIELDMAT"] = context => CalculateYieldMat(GetDateParameter(context, 0), GetDateParameter(context, 1), GetDateParameter(context, 2), Convert.ToDouble(context.Parameters[3]), Convert.ToDouble(context.Parameters[4]), context.Parameters.Count > 5 ? Convert.ToInt32(context.Parameters[5]) : 0);
        }

        private void RegisterInformationFunctions()
        {
            _functions["CELL"] = context => GetCellInfo(context.Parameters[0].ToString(), context.Parameters.Count > 1 ? context.Parameters[1] : null);
            _functions["ERROR.TYPE"] = context => GetErrorType(context.Parameters[0]);
            _functions["INFO"] = context => GetInfo(context.Parameters[0].ToString());
            _functions["ISBLANK"] = context => context.Parameters[0] == null || context.Parameters[0].ToString() == "";
            _functions["ISERR"] = context => { object value = context.Parameters[0]; if (value is CellError error) return error != CellError.NA; if (value is string str) return str == "#NULL!" || str == "#DIV/0!" || str == "#VALUE!" || str == "#REF!" || str == "#NAME?" || str == "#NUM!" || str == "#N/A"; return false; };
            _functions["ISERROR"] = context => { object value = context.Parameters[0]; if (value is CellError) return true; if (value is string str) return str.StartsWith("#") && (str.EndsWith("!") || str == "#N/A"); return false; };
            _functions["ISEVEN"] = context => Convert.ToDouble(context.Parameters[0]) % 2 == 0;
            _functions["ISFORMULA"] = context => context.Parameters[0] != null && context.Parameters[0].ToString().StartsWith("=");
            _functions["ISLOGICAL"] = context => context.Parameters[0] is bool;
            _functions["ISNA"] = context => { object value = context.Parameters[0]; if (value is CellError error) return error == CellError.NA; if (value is string str) return str == "#N/A"; return false; };
            _functions["ISNONTEXT"] = context => !(context.Parameters[0] is string);
            _functions["ISNUMBER"] = context => context.Parameters[0] != null && double.TryParse(context.Parameters[0].ToString(), out _);
            _functions["ISODD"] = context => Convert.ToDouble(context.Parameters[0]) % 2 != 0;
            _functions["ISREF"] = context => Regex.IsMatch(context.Parameters[0]?.ToString() ?? "", @"^[A-Z]+\d+$|^[$]?[A-Z]+[$]?\d+$");
            _functions["ISTEXT"] = context => context.Parameters[0] is string;
            _functions["N"] = context => { object value = context.Parameters[0]; if (value is double d) return d; if (value is int i) return i; if (value is bool b) return b ? 1 : 0; if (value is DateTime dt) return dt.Ticks / TimeSpan.TicksPerDay; if (value is CellError) return 0; return 0; };
            _functions["NA"] = context => CellError.NA;
            _functions["TYPE"] = context => { object value = context.Parameters[0]; if (value is double || value is int) return 1; if (value is string) return 2; if (value is bool) return 4; if (value == null) return 1; return 16; };
        }

        private void RegisterLogicalFunctions()
        {
            _functions["AND"] = context => { foreach (var param in context.Parameters) { if (param is bool b && !b) return false; if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (item is bool ab && !ab) return false; } } } return true; };
            _functions["FALSE"] = context => false;
            _functions["IF"] = context => Convert.ToBoolean(context.Parameters[0]) ? context.Parameters[1] : (context.Parameters.Count > 2 ? context.Parameters[2] : false);
            _functions["IFERROR"] = context => { try { return context.Parameters[0]; } catch { return context.Parameters.Count > 1 ? context.Parameters[1] : ""; } };
            _functions["IFNA"] = context => { try { object value = context.Parameters[0]; if (value is CellError error && error == CellError.NA) return context.Parameters.Count > 1 ? context.Parameters[1] : ""; if (value is string str && str == "#N/A") return context.Parameters.Count > 1 ? context.Parameters[1] : ""; return value; } catch { return context.Parameters.Count > 1 ? context.Parameters[1] : ""; } };
            _functions["IFS"] = context => { for (int i = 0; i < context.Parameters.Count; i += 2) { if (i + 1 >= context.Parameters.Count) break; if (Convert.ToBoolean(context.Parameters[i])) return context.Parameters[i + 1]; } return CellError.NA; };
            _functions["NOT"] = context => !Convert.ToBoolean(context.Parameters[0]);
            _functions["OR"] = context => { foreach (var param in context.Parameters) { if (param is bool b && b) return true; if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (item is bool ab && ab) return true; } } } return false; };
            _functions["SWITCH"] = context => { object expression = context.Parameters[0]; for (int i = 1; i < context.Parameters.Count; i += 2) { if (i + 1 >= context.Parameters.Count) break; if (Equals(expression, context.Parameters[i])) return context.Parameters[i + 1]; } return context.Parameters.Count % 2 == 0 ? context.Parameters[context.Parameters.Count - 1] : CellError.NA; };
            _functions["TRUE"] = context => true;
            _functions["XOR"] = context => { int trueCount = 0; foreach (var param in context.Parameters) { if (param is bool b && b) trueCount++; if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (item is bool ab && ab) trueCount++; } } } return trueCount % 2 == 1; };
        }

        private void RegisterLookupFunctions()
        {
            _functions["ADDRESS"] = context => FormatAddress(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1, context.Parameters.Count > 3 ? Convert.ToBoolean(context.Parameters[3]) : true, context.Parameters.Count > 4 ? context.Parameters[4].ToString() : "");
            _functions["AREAS"] = context => 1;
            _functions["CHOOSE"] = context => { int index = Convert.ToInt32(context.Parameters[0]); return index < 1 || index >= context.Parameters.Count ? CellError.Value : context.Parameters[index]; };
            _functions["COLUMN"] = context => context.Parameters.Count == 0 ? 1 : ExtractColumnNumber(context.Parameters[0].ToString());
            _functions["COLUMNS"] = context => { var array = GetArrayParameter(context, 0); return array.Count > 0 ? array[0].Count : 0; };
            _functions["FORMULATEXT"] = context => context.Parameters[0]?.ToString() ?? "";
            _functions["HLOOKUP"] = context => HLookup(context.Parameters[0], GetArrayParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
            _functions["HYPERLINK"] = context => $"HYPERLINK({context.Parameters[0].ToString()}, {(context.Parameters.Count > 1 ? context.Parameters[1].ToString() : context.Parameters[0].ToString())})";
            _functions["INDEX"] = context => GetIndex(GetArrayParameter(context, 0), Convert.ToInt32(context.Parameters[1]), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1);
            _functions["INDIRECT"] = context => $"INDIRECT({context.Parameters[0].ToString()})";
            _functions["LOOKUP"] = context => Lookup(context.Parameters[0], GetArrayParameter(context, 1), context.Parameters.Count > 2 ? GetArrayParameter(context, 2) : null);
            _functions["MATCH"] = context => Match(context.Parameters[0], GetArrayParameter(context, 1), context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1);
            _functions["OFFSET"] = context => $"OFFSET(..., {Convert.ToInt32(context.Parameters[1])}, {Convert.ToInt32(context.Parameters[2])}, {(context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 1)}, {(context.Parameters.Count > 4 ? Convert.ToInt32(context.Parameters[4]) : 1)})";
            _functions["ROW"] = context => context.Parameters.Count == 0 ? 1 : ExtractRowNumber(context.Parameters[0].ToString());
            _functions["ROWS"] = context => GetArrayParameter(context, 0).Count;
            _functions["RTD"] = context => "RTD()";
            _functions["TRANSPOSE"] = context => TransposeArray(GetArrayParameter(context, 0));
            _functions["VLOOKUP"] = context => VLookup(context.Parameters[0], GetArrayParameter(context, 1), Convert.ToInt32(context.Parameters[2]), context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
        }

        private void RegisterMathFunctions()
        {
            _functions["ABS"] = context => Math.Abs(Convert.ToDouble(context.Parameters[0]));
            _functions["ACOS"] = context => Math.Acos(Convert.ToDouble(context.Parameters[0]));
            _functions["ACOSH"] = context => Math.Acosh(Convert.ToDouble(context.Parameters[0]));
            _functions["ACOT"] = context => Math.PI / 2 - Math.Atan(Convert.ToDouble(context.Parameters[0]));
            _functions["ACOTH"] = context => 0.5 * Math.Log((Convert.ToDouble(context.Parameters[0]) + 1) / (Convert.ToDouble(context.Parameters[0]) - 1));
            _functions["AGGREGATE"] = context => CalculateAggregate(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), GetArrayParameter(context, 2));
            _functions["ASIN"] = context => Math.Asin(Convert.ToDouble(context.Parameters[0]));
            _functions["ASINH"] = context => Math.Asinh(Convert.ToDouble(context.Parameters[0]));
            _functions["ATAN"] = context => Math.Atan(Convert.ToDouble(context.Parameters[0]));
            _functions["ATAN2"] = context => Math.Atan2(Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[0]));
            _functions["ATANH"] = context => Math.Atanh(Convert.ToDouble(context.Parameters[0]));
            _functions["BASE"] = context => { long number = Convert.ToInt64(context.Parameters[0]); int radix = Convert.ToInt32(context.Parameters[1]); int minLength = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; string result = Convert.ToString(number, radix).ToUpper(); return minLength > 0 && result.Length < minLength ? result.PadLeft(minLength, '0') : result; };
            _functions["CEILING"] = context => { double number = Convert.ToDouble(context.Parameters[0]); double significance = context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1; double mode = context.Parameters.Count > 2 ? Convert.ToDouble(context.Parameters[2]) : 0; return number >= 0 ? Math.Ceiling(number / significance) * significance : (mode == 0 ? -Math.Floor(Math.Abs(number) / significance) * significance : -Math.Ceiling(Math.Abs(number) / significance) * significance); };
            _functions["CEILING.MATH"] = context => { double number = Convert.ToDouble(context.Parameters[0]); double significance = context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1; int mode = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; return number >= 0 ? Math.Ceiling(number / significance) * significance : (mode == 0 ? -Math.Floor(Math.Abs(number) / significance) * significance : -Math.Ceiling(Math.Abs(number) / significance) * significance); };
            _functions["CEILING.PRECISE"] = context => Math.Ceiling(Convert.ToDouble(context.Parameters[0]) / (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1)) * (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1);
            _functions["COMBIN"] = context => CalculateCombinations(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["COMBINA"] = context => CalculateCombinationsWithRepetition(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["COS"] = context => Math.Cos(Convert.ToDouble(context.Parameters[0]));
            _functions["COSH"] = context => Math.Cosh(Convert.ToDouble(context.Parameters[0]));
            _functions["COT"] = context => 1 / Math.Tan(Convert.ToDouble(context.Parameters[0]));
            _functions["COTH"] = context => { double x = Convert.ToDouble(context.Parameters[0]); return (Math.Exp(2 * x) + 1) / (Math.Exp(2 * x) - 1); };
            _functions["CSC"] = context => 1 / Math.Sin(Convert.ToDouble(context.Parameters[0]));
            _functions["CSCH"] = context => 1 / Math.Sinh(Convert.ToDouble(context.Parameters[0]));
            _functions["DECIMAL"] = context => Convert.ToInt64(context.Parameters[0].ToString(), Convert.ToInt32(context.Parameters[1]));
            _functions["DEGREES"] = context => Convert.ToDouble(context.Parameters[0]) * 180 / Math.PI;
            _functions["EVEN"] = context => { double number = Convert.ToDouble(context.Parameters[0]); return number >= 0 ? Math.Ceiling(number / 2) * 2 : -Math.Floor(Math.Abs(number) / 2) * 2; };
            _functions["EXP"] = context => Math.Exp(Convert.ToDouble(context.Parameters[0]));
            _functions["FACT"] = context => CalculateFactorial(Convert.ToInt32(context.Parameters[0]));
            _functions["FACTDOUBLE"] = context => CalculateDoubleFactorial(Convert.ToInt32(context.Parameters[0]));
            _functions["FLOOR"] = context => { double number = Convert.ToDouble(context.Parameters[0]); double significance = context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1; double mode = context.Parameters.Count > 2 ? Convert.ToDouble(context.Parameters[2]) : 0; return number >= 0 ? Math.Floor(number / significance) * significance : (mode == 0 ? -Math.Ceiling(Math.Abs(number) / significance) * significance : -Math.Floor(Math.Abs(number) / significance) * significance); };
            _functions["FLOOR.MATH"] = context => { double number = Convert.ToDouble(context.Parameters[0]); double significance = context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1; int mode = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; return number >= 0 ? Math.Floor(number / significance) * significance : (mode == 0 ? -Math.Ceiling(Math.Abs(number) / significance) * significance : -Math.Floor(Math.Abs(number) / significance) * significance); };
            _functions["FLOOR.PRECISE"] = context => Math.Floor(Convert.ToDouble(context.Parameters[0]) / (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1)) * (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1);
            _functions["GCD"] = context => CalculateGcd(context.Parameters.Select(p => Convert.ToInt64(p)).ToArray());
            _functions["INT"] = context => Math.Floor(Convert.ToDouble(context.Parameters[0]));
            _functions["ISO.CEILING"] = context => Math.Ceiling(Convert.ToDouble(context.Parameters[0]) / (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1)) * (context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 1);
            _functions["LCM"] = context => CalculateLcm(context.Parameters.Select(p => Convert.ToInt64(p)).ToArray());
            _functions["LN"] = context => Math.Log(Convert.ToDouble(context.Parameters[0]));
            _functions["LOG"] = context => Math.Log(Convert.ToDouble(context.Parameters[0]), context.Parameters.Count > 1 ? Convert.ToDouble(context.Parameters[1]) : 10);
            _functions["LOG10"] = context => Math.Log10(Convert.ToDouble(context.Parameters[0]));
            _functions["MDETERM"] = context => CalculateMatrixDeterminant(GetArrayParameter(context, 0));
            _functions["MID"] = context => context.Parameters[0].ToString().Substring(Convert.ToInt32(context.Parameters[1]) - 1, Convert.ToInt32(context.Parameters[2]));
            _functions["MINVERSE"] = context => CalculateMatrixInverse(GetArrayParameter(context, 0));
            _functions["MMULT"] = context => MultiplyMatrices(GetArrayParameter(context, 0), GetArrayParameter(context, 1));
            _functions["MOD"] = context => Mod(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]));
            _functions["MROUND"] = context => { double number = Convert.ToDouble(context.Parameters[0]); double multiple = Convert.ToDouble(context.Parameters[1]); if (number == 0) return 0; if (number < 0 && multiple > 0 || number > 0 && multiple < 0) return CellError.Num; return Math.Round(number / multiple) * multiple; };
            _functions["MULTINOMIAL"] = context => { double sum = context.Parameters.Sum(p => Convert.ToDouble(p)); double product = 1; for (int i = 0; i < context.Parameters.Count; i++) { product *= CalculateFactorial(Convert.ToInt32(context.Parameters[i])); } return CalculateFactorial(Convert.ToInt32(sum)) / product; };
            _functions["MUNIT"] = context => { int dimension = Convert.ToInt32(context.Parameters[0]); var matrix = new List<List<double>>(); for (int i = 0; i < dimension; i++) { var row = new List<double>(); for (int j = 0; j < dimension; j++) { row.Add(i == j ? 1 : 0); } matrix.Add(row); } return matrix; };
            _functions["ODD"] = context => { double number = Convert.ToDouble(context.Parameters[0]); return number >= 0 ? Math.Ceiling(number / 2) * 2 + 1 : -Math.Floor(Math.Abs(number) / 2) * 2 - 1; };
            _functions["PI"] = context => Math.PI;
            _functions["POWER"] = context => Math.Pow(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]));
            _functions["PRODUCT"] = context => { double result = 1; foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (IsNumeric(item)) result *= Convert.ToDouble(item); } } else if (IsNumeric(param)) { result *= Convert.ToDouble(param); } } return result; };
            _functions["QUOTIENT"] = context => Math.Floor(Convert.ToDouble(context.Parameters[0]) / Convert.ToDouble(context.Parameters[1]));
            _functions["RADIANS"] = context => Convert.ToDouble(context.Parameters[0]) * Math.PI / 180;
            _functions["RAND"] = context => new Random().NextDouble();
            _functions["RANDBETWEEN"] = context => new Random().Next(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]) + 1);
            _functions["ROMAN"] = context => ConvertToRoman(Convert.ToInt32(context.Parameters[0]), context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0);
            _functions["ROUND"] = context => { double number = Convert.ToDouble(context.Parameters[0]); int digits = Convert.ToInt32(context.Parameters[1]); double factor = Math.Pow(10, digits); return Math.Round(number * factor) / factor; };
            _functions["ROUNDDOWN"] = context => { double number = Convert.ToDouble(context.Parameters[0]); int digits = Convert.ToInt32(context.Parameters[1]); double factor = Math.Pow(10, digits); return Math.Floor(number * factor) / factor; };
            _functions["ROUNDUP"] = context => { double number = Convert.ToDouble(context.Parameters[0]); int digits = Convert.ToInt32(context.Parameters[1]); double factor = Math.Pow(10, digits); return Math.Ceiling(number * factor) / factor; };
            _functions["SEC"] = context => 1 / Math.Cos(Convert.ToDouble(context.Parameters[0]));
            _functions["SECH"] = context => 1 / Math.Cosh(Convert.ToDouble(context.Parameters[0]));
            _functions["SERIES"] = context => "SERIES()";
            _functions["SIGN"] = context => { double number = Convert.ToDouble(context.Parameters[0]); return number > 0 ? 1 : (number < 0 ? -1 : 0); };
            _functions["SIN"] = context => Math.Sin(Convert.ToDouble(context.Parameters[0]));
            _functions["SINH"] = context => Math.Sinh(Convert.ToDouble(context.Parameters[0]));
            _functions["SQRT"] = context => Convert.ToDouble(context.Parameters[0]) < 0 ? CellError.Num : Math.Sqrt(Convert.ToDouble(context.Parameters[0]));
            _functions["SQRTPI"] = context => Math.Sqrt(Math.PI * Convert.ToDouble(context.Parameters[0]));
            _functions["SUBTOTAL"] = context => { int functionNum = Convert.ToInt32(context.Parameters[0]); var values = new List<double>(); for (int i = 1; i < context.Parameters.Count; i++) { if (context.Parameters[i] is IEnumerable<object> arr) { foreach (var item in arr) { if (IsNumeric(item)) values.Add(Convert.ToDouble(item)); } } else if (IsNumeric(context.Parameters[i])) { values.Add(Convert.ToDouble(context.Parameters[i])); } } return CalculateSubtotal(functionNum, values); };
            _functions["SUM"] = context => { double sum = 0; foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (IsNumeric(item)) sum += Convert.ToDouble(item); } } else if (IsNumeric(param)) { sum += Convert.ToDouble(param); } } return sum; };
            _functions["SUMIF"] = context => SumIf(GetArrayParameter(context, 0), context.Parameters[1], context.Parameters.Count > 2 ? GetArrayParameter(context, 2) : GetArrayParameter(context, 0));
            _functions["SUMIFS"] = context => { var sumRange = GetArrayParameter(context, 0); var ranges = new List<List<object>>(); var criterias = new List<object>(); for (int i = 1; i < context.Parameters.Count; i += 2) { ranges.Add(GetArrayParameter(context, i)); criterias.Add(context.Parameters[i + 1]); } return SumIfs(sumRange, ranges, criterias); };
            _functions["SUMPRODUCT"] = context => { var arrays = context.Parameters.Select(p => GetArrayParameter(context, context.Parameters.IndexOf(p))).ToList(); double result = 0; for (int i = 0; i < arrays[0].Count; i++) { double product = 1; for (int j = 0; j < arrays.Count; j++) { if (arrays[j].Count > i && IsNumeric(arrays[j][i])) product *= Convert.ToDouble(arrays[j][i]); } result += product; } return result; };
            _functions["SUMSQ"] = context => { double sum = 0; foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (IsNumeric(item)) sum += Math.Pow(Convert.ToDouble(item), 2); } } else if (IsNumeric(param)) { sum += Math.Pow(Convert.ToDouble(param), 2); } } return sum; };
            _functions["SUMX2MY2"] = context => { var array1 = GetArrayParameter(context, 0); var array2 = GetArrayParameter(context, 1); double sum = 0; for (int i = 0; i < Math.Min(array1.Count, array2.Count); i++) { if (IsNumeric(array1[i]) && IsNumeric(array2[i])) sum += Math.Pow(Convert.ToDouble(array1[i]), 2) - Math.Pow(Convert.ToDouble(array2[i]), 2); } return sum; };
            _functions["SUMX2PY2"] = context => { var array1 = GetArrayParameter(context, 0); var array2 = GetArrayParameter(context, 1); double sum = 0; for (int i = 0; i < Math.Min(array1.Count, array2.Count); i++) { if (IsNumeric(array1[i]) && IsNumeric(array2[i])) sum += Math.Pow(Convert.ToDouble(array1[i]), 2) + Math.Pow(Convert.ToDouble(array2[i]), 2); } return sum; };
            _functions["SUMXMY2"] = context => { var array1 = GetArrayParameter(context, 0); var array2 = GetArrayParameter(context, 1); double sum = 0; for (int i = 0; i < Math.Min(array1.Count, array2.Count); i++) { if (IsNumeric(array1[i]) && IsNumeric(array2[i])) sum += Math.Pow(Convert.ToDouble(array1[i]) - Convert.ToDouble(array2[i]), 2); } return sum; };
            _functions["TAN"] = context => Math.Tan(Convert.ToDouble(context.Parameters[0]));
            _functions["TANH"] = context => Math.Tanh(Convert.ToDouble(context.Parameters[0]));
            _functions["TRUNC"] = context => { double number = Convert.ToDouble(context.Parameters[0]); int digits = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 0; double factor = Math.Pow(10, digits); return Math.Truncate(number * factor) / factor; };
        }

        private void RegisterStatisticalFunctions()
        {
            _functions["AVEDEV"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Abs(v - mean)) / values.Count; };
            _functions["AVERAGE"] = context => { var values = GetNumericValues(context); return values.Count > 0 ? values.Average() : 0; };
            _functions["AVERAGEA"] = context => { var values = GetAllNumericValues(context); return values.Count > 0 ? values.Average() : 0; };
            _functions["AVERAGEIF"] = context => AverageIf(GetArrayParameter(context, 0), context.Parameters[1], context.Parameters.Count > 2 ? GetArrayParameter(context, 2) : GetArrayParameter(context, 0));
            _functions["AVERAGEIFS"] = context => { var averageRange = GetArrayParameter(context, 0); var ranges = new List<List<object>>(); var criterias = new List<object>(); for (int i = 1; i < context.Parameters.Count; i += 2) { ranges.Add(GetArrayParameter(context, i)); criterias.Add(context.Parameters[i + 1]); } return AverageIfs(averageRange, ranges, criterias); };
            _functions["BETADIST"] = context => CalculateBetaDist(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToDouble(context.Parameters[4]) : 1);
            _functions["BETAINV"] = context => CalculateBetaInv(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : 0, context.Parameters.Count > 4 ? Convert.ToDouble(context.Parameters[4]) : 1);
            _functions["BINOMDIST"] = context => CalculateBinomDist(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToBoolean(context.Parameters[3]));
            _functions["CHIDIST"] = context => CalculateChiDist(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["CHIINV"] = context => CalculateChiInv(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["CHITEST"] = context => CalculateChiTest(GetArrayParameter(context, 0), GetArrayParameter(context, 1));
            _functions["CONFIDENCE"] = context => CalculateConfidence(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]));
            _functions["CORREL"] = context => CalculateCorrel(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["COUNT"] = context => { int count = 0; foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { foreach (var item in arr) { if (IsNumeric(item)) count++; } } else if (IsNumeric(param)) { count++; } } return count; };
            _functions["COUNTA"] = context => { int count = 0; foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { count += arr.Count(); } else if (param != null) { count++; } } return count; };
            _functions["COUNTBLANK"] = context => GetArrayParameter(context, 0).Count(r => r == null || r.ToString() == "");
            _functions["COUNTIF"] = context => CountIf(GetArrayParameter(context, 0), context.Parameters[1]);
            _functions["COUNTIFS"] = context => { var ranges = new List<List<object>>(); var criterias = new List<object>(); for (int i = 0; i < context.Parameters.Count; i += 2) { ranges.Add(GetArrayParameter(context, i)); criterias.Add(context.Parameters[i + 1]); } return CountIfs(ranges, criterias); };
            _functions["COVAR"] = context => CalculateCovar(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["CRITBINOM"] = context => CalculateCritBinom(Convert.ToInt32(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]));
            _functions["DEVSQ"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)); };
            _functions["EXPONDIST"] = context => CalculateExponDist(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToBoolean(context.Parameters[2]));
            _functions["FDIST"] = context => CalculateFDist(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]));
            _functions["FINV"] = context => CalculateFInv(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]));
            _functions["FISHER"] = context => 0.5 * Math.Log((1 + Convert.ToDouble(context.Parameters[0])) / (1 - Convert.ToDouble(context.Parameters[0])));
            _functions["FISHERINV"] = context => (Math.Exp(2 * Convert.ToDouble(context.Parameters[0])) - 1) / (Math.Exp(2 * Convert.ToDouble(context.Parameters[0])) + 1);
            _functions["FORECAST"] = context => CalculateForecast(Convert.ToDouble(context.Parameters[0]), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[2] as IEnumerable<object> ?? new List<object>()));
            _functions["FREQUENCY"] = context => CalculateFrequency(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["FTEST"] = context => CalculateFTest(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["GAMMADIST"] = context => CalculateGammaDist(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToBoolean(context.Parameters[3]));
            _functions["GAMMAINV"] = context => CalculateGammaInv(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]));
            _functions["GAMMALN"] = context => GammaLn(Convert.ToDouble(context.Parameters[0]));
            _functions["GEOMEAN"] = context => { var values = GetNumericValues(context); return Math.Pow(values.Aggregate(1.0, (a, b) => a * b), 1.0 / values.Count); };
            _functions["GROWTH"] = context => CalculateGrowth(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), context.Parameters.Count > 1 ? GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 2 ? GetNumericValuesFromArray(context.Parameters[2] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
            _functions["HARMEAN"] = context => { var values = GetNumericValues(context); return values.Count / values.Sum(v => 1 / v); };
            _functions["HYPGEOMDIST"] = context => CalculateHypgeomDist(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]));
            _functions["INTERCEPT"] = context => CalculateIntercept(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["KURT"] = context => { var values = GetNumericValues(context); double mean = values.Average(); double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count); return values.Sum(v => Math.Pow((v - mean) / stdDev, 4)) * values.Count * (values.Count - 1) / ((values.Count - 2) * (values.Count - 3)) - 3; };
            _functions["LARGE"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); int k = Convert.ToInt32(context.Parameters[1]); var sorted = values.OrderByDescending(v => v).ToList(); return k >= 1 && k <= sorted.Count ? sorted[k - 1] : CellError.Num; };
            _functions["LINEST"] = context => CalculateLinest(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), context.Parameters.Count > 1 ? GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 2 && Convert.ToBoolean(context.Parameters[2]), context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
            _functions["LOGEST"] = context => CalculateLogest(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), context.Parameters.Count > 1 ? GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 2 && Convert.ToBoolean(context.Parameters[2]), context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
            _functions["LOGINV"] = context => Math.Exp(Convert.ToDouble(context.Parameters[1])) * Math.Pow(Convert.ToDouble(context.Parameters[2]), NormInv(Convert.ToDouble(context.Parameters[0])));
            _functions["LOGNORMDIST"] = context => NormDist(Math.Log(Convert.ToDouble(context.Parameters[0])), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), true);
            _functions["MAX"] = context => { var values = GetNumericValues(context); return values.Count > 0 ? values.Max() : 0; };
            _functions["MAXA"] = context => { var values = GetAllNumericValues(context); return values.Count > 0 ? values.Max() : 0; };
            _functions["MEDIAN"] = context => { var values = GetNumericValues(context); values.Sort(); int n = values.Count; return n % 2 == 0 ? (values[n / 2 - 1] + values[n / 2]) / 2 : values[n / 2]; };
            _functions["MIN"] = context => { var values = GetNumericValues(context); return values.Count > 0 ? values.Min() : 0; };
            _functions["MINA"] = context => { var values = GetAllNumericValues(context); return values.Count > 0 ? values.Min() : 0; };
            _functions["MODE"] = context => { var values = GetNumericValues(context); return values.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key; };
            _functions["NEGBINOMDIST"] = context => CalculateNegbinomDist(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]));
            _functions["NORMDIST"] = context => NormDist(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToBoolean(context.Parameters[3]));
            _functions["NORMINV"] = context => Convert.ToDouble(context.Parameters[1]) + Convert.ToDouble(context.Parameters[2]) * NormInv(Convert.ToDouble(context.Parameters[0]));
            _functions["NORMSDIST"] = context => NormDist(Convert.ToDouble(context.Parameters[0]), 0, 1, true);
            _functions["NORMSINV"] = context => NormInv(Convert.ToDouble(context.Parameters[0]));
            _functions["PEARSON"] = context => CalculateCorrel(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["PERCENTILE"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double k = Convert.ToDouble(context.Parameters[1]); values.Sort(); double index = k * (values.Count - 1); int lower = (int)Math.Floor(index); int upper = (int)Math.Ceiling(index); return lower == upper ? values[lower] : values[lower] + (values[upper] - values[lower]) * (index - lower); };
            _functions["PERCENTILE.EXC"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double k = Convert.ToDouble(context.Parameters[1]); values.Sort(); int n = values.Count; double index = (n + 1) * k; int lower = (int)Math.Floor(index); int upper = (int)Math.Ceiling(index); return lower == upper ? values[lower - 1] : values[lower - 1] + (values[upper - 1] - values[lower - 1]) * (index - lower); };
            _functions["PERCENTILE.INC"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double k = Convert.ToDouble(context.Parameters[1]); values.Sort(); double index = k * (values.Count - 1); int lower = (int)Math.Floor(index); int upper = (int)Math.Ceiling(index); return lower == upper ? values[lower] : values[lower] + (values[upper] - values[lower]) * (index - lower); };
            _functions["PERCENTRANK"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double x = Convert.ToDouble(context.Parameters[1]); int significance = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 3; values.Sort(); int count = values.Count; for (int i = 0; i < count; i++) { if (values[i] >= x) { if (i == 0) return 0; double rank = i; double lower = values[i - 1]; double upper = values[i]; return Math.Round((rank - (x - lower) / (upper - lower)) / (count - 1), significance); } } return 1; };
            _functions["PERMUT"] = context => CalculatePermutations(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["PERMUTATIONA"] = context => Math.Pow(Convert.ToInt32(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["PHI"] = context => Math.Exp(-0.5 * Math.Pow(Convert.ToDouble(context.Parameters[0]), 2)) / Math.Sqrt(2 * Math.PI);
            _functions["POISSON"] = context => CalculatePoisson(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToBoolean(context.Parameters[2]));
            _functions["PROB"] = context => { var xRange = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); var probRange = GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()); double lower = Convert.ToDouble(context.Parameters[2]); double upper = context.Parameters.Count > 3 ? Convert.ToDouble(context.Parameters[3]) : lower; double sum = 0; for (int i = 0; i < xRange.Count && i < probRange.Count; i++) { if (xRange[i] >= lower && xRange[i] <= upper) sum += probRange[i]; } return sum; };
            _functions["QUARTILE"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); int quart = Convert.ToInt32(context.Parameters[1]); values.Sort(); double k = quart / 4.0; double index = k * (values.Count - 1); int lower = (int)Math.Floor(index); int upper = (int)Math.Ceiling(index); return lower == upper ? values[lower] : values[lower] + (values[upper] - values[lower]) * (index - lower); };
            _functions["RANK"] = context => { double number = Convert.ToDouble(context.Parameters[0]); var refArray = GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()); int order = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; int rank = 1; foreach (var val in refArray) { if (order == 0 && val > number) rank++; else if (order != 0 && val < number) rank++; } return rank; };
            _functions["RANK.AVG"] = context => { double number = Convert.ToDouble(context.Parameters[0]); var refArray = GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()); int order = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; var equalValues = refArray.Where(v => v == number).ToList(); int rank = 1; foreach (var val in refArray) { if (order == 0 && val > number) rank++; else if (order != 0 && val < number) rank++; } return rank + (equalValues.Count - 1) / 2.0; };
            _functions["RANK.EQ"] = context => { double number = Convert.ToDouble(context.Parameters[0]); var refArray = GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()); int order = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 0; int rank = 1; foreach (var val in refArray) { if (order == 0 && val > number) rank++; else if (order != 0 && val < number) rank++; } return rank; };
            _functions["RSQ"] = context => { var array1 = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); var array2 = GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()); double correl = CalculateCorrel(array1, array2); return correl * correl; };
            _functions["SKEW"] = context => { var values = GetNumericValues(context); double mean = values.Average(); double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count); return values.Sum(v => Math.Pow((v - mean) / stdDev, 3)) / values.Count; };
            _functions["SKEW.P"] = context => { var values = GetNumericValues(context); double mean = values.Average(); double n = values.Count; double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / n); return n / (n - 1) / (n - 2) * values.Sum(v => Math.Pow((v - mean) / stdDev, 3)); };
            _functions["SLOPE"] = context => CalculateSlope(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["SMALL"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); int k = Convert.ToInt32(context.Parameters[1]); var sorted = values.OrderBy(v => v).ToList(); return k >= 1 && k <= sorted.Count ? sorted[k - 1] : CellError.Num; };
            _functions["STANDARDIZE"] = context => (Convert.ToDouble(context.Parameters[0]) - Convert.ToDouble(context.Parameters[1])) / Convert.ToDouble(context.Parameters[2]);
            _functions["STDEV"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1)); };
            _functions["STDEV.P"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count); };
            _functions["STDEV.S"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1)); };
            _functions["STDEVA"] = context => { var values = GetAllNumericValues(context); double mean = values.Average(); return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1)); };
            _functions["STDEVPA"] = context => { var values = GetAllNumericValues(context); double mean = values.Average(); return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count); };
            _functions["STEYX"] = context => CalculateSteyx(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()));
            _functions["T"] = context => context.Parameters[0] is string ? context.Parameters[0] : "";
            _functions["TDIST"] = context => CalculateTDist(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]), Convert.ToInt32(context.Parameters[2]));
            _functions["TINV"] = context => CalculateTInv(Convert.ToDouble(context.Parameters[0]), Convert.ToInt32(context.Parameters[1]));
            _functions["TREND"] = context => CalculateTrend(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), context.Parameters.Count > 1 ? GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 2 ? GetNumericValuesFromArray(context.Parameters[2] as IEnumerable<object> ?? new List<object>()) : null, context.Parameters.Count > 3 && Convert.ToBoolean(context.Parameters[3]));
            _functions["TRIMMEAN"] = context => { var values = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double percent = Convert.ToDouble(context.Parameters[1]); values.Sort(); int trimCount = (int)(values.Count * percent / 2); return values.Skip(trimCount).Take(values.Count - 2 * trimCount).Average(); };
            _functions["TTEST"] = context => CalculateTTest(GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()), GetNumericValuesFromArray(context.Parameters[1] as IEnumerable<object> ?? new List<object>()), Convert.ToInt32(context.Parameters[2]), Convert.ToInt32(context.Parameters[3]));
            _functions["VAR"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1); };
            _functions["VAR.P"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)) / values.Count; };
            _functions["VAR.S"] = context => { var values = GetNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1); };
            _functions["VARA"] = context => { var values = GetAllNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1); };
            _functions["VARPA"] = context => { var values = GetAllNumericValues(context); double mean = values.Average(); return values.Sum(v => Math.Pow(v - mean, 2)) / values.Count; };
            _functions["WEIBULL"] = context => CalculateWeibull(Convert.ToDouble(context.Parameters[0]), Convert.ToDouble(context.Parameters[1]), Convert.ToDouble(context.Parameters[2]), Convert.ToBoolean(context.Parameters[3]));
            _functions["ZTEST"] = context => { var array = GetNumericValuesFromArray(context.Parameters[0] as IEnumerable<object> ?? new List<object>()); double x = Convert.ToDouble(context.Parameters[1]); double sigma = context.Parameters.Count > 2 ? Convert.ToDouble(context.Parameters[2]) : 0; double mean = array.Average(); if (sigma == 0) sigma = Math.Sqrt(array.Sum(v => Math.Pow(v - mean, 2)) / array.Count); double z = (mean - x) / (sigma / Math.Sqrt(array.Count)); return 1 - NormDist(z, 0, 1, true); };
        }

        private void RegisterTextFunctions()
        {
            _functions["ASC"] = context => ToHalfWidth(context.Parameters[0].ToString());
            _functions["BAHTTEXT"] = context => ConvertToThaiBaht(Convert.ToDouble(context.Parameters[0]));
            _functions["CHAR"] = context => ((char)Convert.ToInt32(context.Parameters[0])).ToString();
            _functions["CLEAN"] = context => new string(context.Parameters[0].ToString().Where(c => c >= 32 && c != 127).ToArray());
            _functions["CODE"] = context => context.Parameters[0].ToString().Length > 0 ? (int)context.Parameters[0].ToString()[0] : 0;
            _functions["CONCAT"] = context => { var parts = new List<string>(); foreach (var param in context.Parameters) { if (param is IEnumerable<object> arr) { parts.AddRange(arr.Select(o => o?.ToString() ?? "")); } else { parts.Add(param?.ToString() ?? ""); } } return string.Join("", parts); };
            _functions["CONCATENATE"] = context => string.Join("", context.Parameters.Select(p => p?.ToString() ?? ""));
            _functions["DBCS"] = context => ToFullWidth(context.Parameters[0].ToString());
            _functions["DOLLAR"] = context => "$" + Convert.ToDouble(context.Parameters[0]).ToString($"N{(context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 2)}");
            _functions["EXACT"] = context => context.Parameters[0].ToString() == context.Parameters[1].ToString();
            _functions["FIND"] = context => { string findText = context.Parameters[0].ToString(); string withinText = context.Parameters[1].ToString(); int startNum = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1; int index = withinText.IndexOf(findText, startNum - 1, StringComparison.Ordinal); return index >= 0 ? index + 1 : CellError.Value; };
            _functions["FINDB"] = context => { byte[] bytes = Encoding.UTF8.GetBytes(context.Parameters[1].ToString()); byte[] searchBytes = Encoding.UTF8.GetBytes(context.Parameters[0].ToString()); int index = IndexOfBytes(bytes, searchBytes, (context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1) - 1); return index >= 0 ? index + 1 : CellError.Value; };
            _functions["FIXED"] = context => { double number = Convert.ToDouble(context.Parameters[0]); int decimals = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 2; bool noCommas = context.Parameters.Count > 2 && Convert.ToBoolean(context.Parameters[2]); string format = noCommas ? $"F{decimals}" : $"N{decimals}"; return number.ToString(format); };
            _functions["LEFT"] = context => context.Parameters[0].ToString().Substring(0, Math.Min(context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 1, context.Parameters[0].ToString().Length));
            _functions["LEFTB"] = context => { string text = context.Parameters[0].ToString(); int numBytes = context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 1; byte[] bytes = Encoding.UTF8.GetBytes(text); int charCount = 0; int byteCount = 0; while (byteCount < numBytes && byteCount < bytes.Length) { byteCount += (bytes[byteCount] & 0x80) == 0 ? 1 : ((bytes[byteCount] & 0xE0) == 0xC0 ? 2 : ((bytes[byteCount] & 0xF0) == 0xE0 ? 3 : 4)); if (byteCount <= numBytes) charCount++; } return text.Substring(0, charCount); };
            _functions["LEN"] = context => context.Parameters[0].ToString().Length;
            _functions["LENB"] = context => Encoding.UTF8.GetByteCount(context.Parameters[0].ToString());
            _functions["LOWER"] = context => context.Parameters[0].ToString().ToLower();
            _functions["MID"] = context => { string text = context.Parameters[0].ToString(); int startNum = Convert.ToInt32(context.Parameters[1]); int numChars = Convert.ToInt32(context.Parameters[2]); if (startNum < 1 || startNum > text.Length) return ""; return text.Substring(startNum - 1, Math.Min(numChars, text.Length - startNum + 1)); };
            _functions["MIDB"] = context => { string text = context.Parameters[0].ToString(); int startNum = Convert.ToInt32(context.Parameters[1]); int numBytes = Convert.ToInt32(context.Parameters[2]); byte[] bytes = Encoding.UTF8.GetBytes(text); int byteIndex = 0; int charIndex = 1; while (byteIndex < bytes.Length && charIndex < startNum) { byteIndex += (bytes[byteIndex] & 0x80) == 0 ? 1 : ((bytes[byteIndex] & 0xE0) == 0xC0 ? 2 : ((bytes[byteIndex] & 0xF0) == 0xE0 ? 3 : 4)); charIndex++; } int startByteIndex = byteIndex; byteIndex = 0; charIndex = 1; while (byteIndex < bytes.Length && charIndex < numBytes) { byteIndex += (bytes[byteIndex] & 0x80) == 0 ? 1 : ((bytes[byteIndex] & 0xE0) == 0xC0 ? 2 : ((bytes[byteIndex] & 0xF0) == 0xE0 ? 3 : 4)); charIndex++; } return Encoding.UTF8.GetString(bytes, startByteIndex, byteIndex - startByteIndex); };
            _functions["NUMBERVALUE"] = context => { string text = context.Parameters[0].ToString(); text = text.Replace(context.Parameters.Count > 2 ? context.Parameters[2].ToString() : ",", "").Replace(context.Parameters.Count > 1 ? context.Parameters[1].ToString() : ".", "."); return double.Parse(text); };
            _functions["PROPER"] = context => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(context.Parameters[0].ToString().ToLower());
            _functions["REPLACE"] = context => { string oldText = context.Parameters[0].ToString(); int startNum = Convert.ToInt32(context.Parameters[1]); int numChars = Convert.ToInt32(context.Parameters[2]); if (startNum < 1 || startNum > oldText.Length) return oldText; return oldText.Substring(0, startNum - 1) + context.Parameters[3].ToString() + oldText.Substring(startNum - 1 + numChars); };
            _functions["REPLACEB"] = context => "REPLACEB()";
            _functions["REPT"] = context => string.Concat(Enumerable.Repeat(context.Parameters[0].ToString(), Convert.ToInt32(context.Parameters[1])));
            _functions["RIGHT"] = context => context.Parameters[0].ToString().Substring(Math.Max(0, context.Parameters[0].ToString().Length - (context.Parameters.Count > 1 ? Convert.ToInt32(context.Parameters[1]) : 1)));
            _functions["RIGHTB"] = context => "RIGHTB()";
            _functions["SEARCH"] = context => { string findText = context.Parameters[0].ToString(); string withinText = context.Parameters[1].ToString(); int startNum = context.Parameters.Count > 2 ? Convert.ToInt32(context.Parameters[2]) : 1; int index = withinText.IndexOf(findText, startNum - 1, StringComparison.OrdinalIgnoreCase); return index >= 0 ? index + 1 : CellError.Value; };
            _functions["SEARCHB"] = context => "SEARCHB()";
            _functions["SUBSTITUTE"] = context => { string text = context.Parameters[0].ToString(); int instanceNum = context.Parameters.Count > 3 ? Convert.ToInt32(context.Parameters[3]) : 0; if (instanceNum == 0) return text.Replace(context.Parameters[1].ToString(), context.Parameters[2].ToString()); int count = 0; int index = 0; while ((index = text.IndexOf(context.Parameters[1].ToString(), index, StringComparison.Ordinal)) != -1) { count++; if (count == instanceNum) { return text.Substring(0, index) + context.Parameters[2].ToString() + text.Substring(index + context.Parameters[1].ToString().Length); } index += context.Parameters[1].ToString().Length; } return text; };
            _functions["T"] = context => context.Parameters[0] is string ? context.Parameters[0].ToString() : "";
            _functions["TEXT"] = context => FormatNumber(Convert.ToDouble(context.Parameters[0]), context.Parameters[1].ToString());
            _functions["TEXTJOIN"] = context => { string delimiter = context.Parameters[0].ToString(); bool ignoreEmpty = Convert.ToBoolean(context.Parameters[1]); var values = new List<string>(); for (int i = 2; i < context.Parameters.Count; i++) { if (context.Parameters[i] is IEnumerable<object> arr) { foreach (var item in arr) { string s = item?.ToString() ?? ""; if (!ignoreEmpty || !string.IsNullOrEmpty(s)) values.Add(s); } } else { string s = context.Parameters[i]?.ToString() ?? ""; if (!ignoreEmpty || !string.IsNullOrEmpty(s)) values.Add(s); } } return string.Join(delimiter, values); };
            _functions["TRIM"] = context => string.Join(" ", context.Parameters[0].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            _functions["UNICHAR"] = context => char.ConvertFromUtf32(Convert.ToInt32(context.Parameters[0]));
            _functions["UNICODE"] = context => context.Parameters[0].ToString().Length == 0 ? CellError.Value : char.ConvertToUtf32(context.Parameters[0].ToString(), 0);
            _functions["UPPER"] = context => context.Parameters[0].ToString().ToUpper();
            _functions["VALUE"] = context => double.TryParse(context.Parameters[0].ToString().Replace(",", ""), out double result) ? result : CellError.Value;
            _functions["VALUETOTEXT"] = context => context.Parameters[0] is double d ? (context.Parameters.Count > 1 && Convert.ToInt32(context.Parameters[1]) == 0 ? d.ToString() : d.ToString("G17")) : context.Parameters[0]?.ToString() ?? "";
        }

        private void RegisterWebFunctions()
        {
            _functions["ENCODEURL"] = context => Uri.EscapeDataString(context.Parameters[0].ToString());
            _functions["FILTERXML"] = context => $"FILTERXML({context.Parameters[0].ToString()}, {context.Parameters[1].ToString()})";
            _functions["WEBSERVICE"] = context => $"WEBSERVICE({context.Parameters[0].ToString()})";
        }

        private object GetArrayParameter(FunctionContext context, int index)
        {
            if (context.Parameters[index] is IEnumerable<object> array)
            {
                return array;
            }
            return new List<object> { context.Parameters[index] };
        }

        private DateTime GetDateParameter(FunctionContext context, int index)
        {
            if (context.Parameters[index] is DateTime dt)
                return dt;
            if (double.TryParse(context.Parameters[index]?.ToString(), out double serial))
            {
                return DateTime.FromOADate(serial);
            }
            if (DateTime.TryParse(context.Parameters[index]?.ToString(), out DateTime parsed))
                return parsed;
            return DateTime.MinValue;
        }

        private DateTime GetDateTimeParameter(FunctionContext context, int index)
        {
            if (context.Parameters[index] is DateTime dt)
                return dt;
            if (double.TryParse(context.Parameters[index]?.ToString(), out double serial))
            {
                return DateTime.FromOADate(serial);
            }
            return DateTime.Now;
        }

        private string GetStringOrNumberParameter(FunctionContext context, int index)
        {
            return context.Parameters[index]?.ToString() ?? "";
        }

        private List<double> GetNumericValues(FunctionContext context)
        {
            var values = new List<double>();
            foreach (var param in context.Parameters)
            {
                if (param is IEnumerable<object> arr)
                {
                    foreach (var item in arr)
                    {
                        if (IsNumeric(item))
                            values.Add(Convert.ToDouble(item));
                    }
                }
                else if (IsNumeric(param))
                {
                    values.Add(Convert.ToDouble(param));
                }
            }
            return values;
        }

        private List<double> GetAllNumericValues(FunctionContext context)
        {
            var values = new List<double>();
            foreach (var param in context.Parameters)
            {
                if (param is IEnumerable<object> arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is bool b)
                            values.Add(b ? 1 : 0);
                        else if (IsNumeric(item))
                            values.Add(Convert.ToDouble(item));
                    }
                }
                else if (param is bool b)
                {
                    values.Add(b ? 1 : 0);
                }
                else if (IsNumeric(param))
                {
                    values.Add(Convert.ToDouble(param));
                }
            }
            return values;
        }

        private List<double> GetNumericValuesFromArray(IEnumerable<object> array)
        {
            var values = new List<double>();
            foreach (var item in array)
            {
                if (IsNumeric(item))
                    values.Add(Convert.ToDouble(item));
            }
            return values;
        }

        private bool IsNumeric(object value)
        {
            if (value == null) return false;
            if (value is double || value is int || value is float || value is decimal)
                return true;
            return double.TryParse(value.ToString(), out _);
        }

        private object CalculateAverage(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateCount(List<object> database, object field, List<object> criteria) => 0;
        private object CalculateCountA(List<object> database, object field, List<object> criteria) => 0;
        private object GetValue(List<object> database, object field, List<object> criteria) => null;
        private object CalculateMax(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateMin(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateProduct(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateStDev(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateStDevP(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateSum(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateVar(List<object> database, object field, List<object> criteria) => 0.0;
        private object CalculateVarP(List<object> database, object field, List<object> criteria) => 0.0;

        private int CalculateDays360(DateTime startDate, DateTime endDate, bool useUSMethod) => (endDate.Year - startDate.Year) * 360 + (endDate.Month - startDate.Month) * 30 + (endDate.Day - startDate.Day);

        private int CalculateNetworkDays(DateTime startDate, DateTime endDate, List<object> holidays)
        {
            int days = 0;
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    days++;
            }
            return days;
        }

        private int CalculateNetworkDaysIntl(DateTime startDate, DateTime endDate, int weekend, List<object> holidays) => CalculateNetworkDays(startDate, endDate, holidays);
        private DateTime CalculateWorkday(DateTime startDate, int days, List<object> holidays) => startDate.AddDays(days);
        private DateTime CalculateWorkdayIntl(DateTime startDate, int days, int weekend, List<object> holidays) => startDate.AddDays(days);

        private int CalculateWeekday(DateTime date, int returnType)
        {
            switch (returnType)
            {
                case 1: return (int)date.DayOfWeek + 1;
                case 2: return (int)date.DayOfWeek;
                case 3: return (int)date.DayOfWeek == 6 ? 5 : (int)date.DayOfWeek;
                default: return (int)date.DayOfWeek + 1;
            }
        }

        private int CalculateWeekNum(DateTime date, int returnType) => System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        private double CalculateYearFrac(DateTime startDate, DateTime endDate, int basis) => (endDate - startDate).TotalDays / 365.25;

        private double CalculateBesselI(double x, int n) => 0.0;
        private double CalculateBesselJ(double x, int n) => 0.0;
        private double CalculateBesselK(double x, int n) => 0.0;
        private double CalculateBesselY(double x, int n) => 0.0;
        private double ConvertUnit(double value, string fromUnit, string toUnit) => value;
        private double CalculateErf(double lower, double upper) => 0.0;

        private double CalculateIMAbs(string complexNum) { var (real, imag) = ParseComplex(complexNum); return Math.Sqrt(real * real + imag * imag); }
        private double ExtractImaginaryPart(string complexNum) { var (real, imag) = ParseComplex(complexNum); return imag; }
        private double CalculateIMArgument(string complexNum) { var (real, imag) = ParseComplex(complexNum); return Math.Atan2(imag, real); }
        private string CalculateIMConjugate(string complexNum) { var (real, imag) = ParseComplex(complexNum); return FormatComplex(real, -imag); }
        private string CalculateIMCos(string complexNum) => "0";
        private string CalculateIMCosh(string complexNum) => "0";
        private string CalculateIMDiv(string complex1, string complex2) { var (a, b) = ParseComplex(complex1); var (c, d) = ParseComplex(complex2); return FormatComplex((a * c + b * d) / (c * c + d * d), (b * c - a * d) / (c * c + d * d)); }
        private string CalculateIMExp(string complexNum) => "0";
        private string CalculateIMLn(string complexNum) => "0";
        private string CalculateIMLog2(string complexNum) => "0";
        private string CalculateIMLog10(string complexNum) => "0";
        private string CalculateIMPower(string complexNum, double power) => "0";
        private string CalculateIMSin(string complexNum) => "0";
        private string CalculateIMSinh(string complexNum) => "0";
        private string CalculateIMSqrt(string complexNum) => "0";
        private double ExtractRealPart(string complexNum) { var (real, imag) = ParseComplex(complexNum); return real; }

        private (double, double) ParseComplex(string complexNum)
        {
            double real = 0, imag = 0;
            var match = Regex.Match(complexNum, @"([+-]?\d*\.?\d*)?([+-]\d*\.?\d*)i?");
            if (match.Success)
            {
                double.TryParse(match.Groups[1].Value, out real);
                double.TryParse(match.Groups[2].Value.Replace("+", "").Replace("i", ""), out imag);
            }
            return (real, imag);
        }

        private string FormatComplex(double real, double imag) => $"{real}{(imag >= 0 ? "+" : "-")}{Math.Abs(imag)}i";

        private double CalculateAccrint(DateTime issue, DateTime firstCoupon, DateTime settlement, double rate, double par, int frequency, int basis) => 0.0;
        private double CalculateAccrintM(DateTime issue, DateTime settlement, double rate, double par, int basis) => 0.0;
        private double CalculateAmorDegrc(double cost, DateTime purchaseDate, DateTime firstPeriodEnd, double salvage, double period, double rate, int basis) => 0.0;
        private double CalculateAmorLinc(double cost, DateTime purchaseDate, DateTime firstPeriodEnd, double salvage, double period, double rate, int basis) => 0.0;
        private double CalculateCoupDayBs(DateTime settlement, DateTime maturity, int frequency, int basis) => 0.0;
        private double CalculateCoupDays(DateTime settlement, DateTime maturity, int frequency, int basis) => 0.0;
        private double CalculateCoupDaysNc(DateTime settlement, DateTime maturity, int frequency, int basis) => 0.0;
        private DateTime CalculateCoupNcd(DateTime settlement, DateTime maturity, int frequency, int basis) => DateTime.MinValue;
        private double CalculateCoupNum(DateTime settlement, DateTime maturity, int frequency, int basis) => 0.0;
        private DateTime CalculateCoupPcd(DateTime settlement, DateTime maturity, int frequency, int basis) => DateTime.MinValue;
        private double CalculateCumIpmt(double rate, int nper, double pv, int startPeriod, int endPeriod, int type) => 0.0;
        private double CalculateCumPrinc(double rate, int nper, double pv, int startPeriod, int endPeriod, int type) => 0.0;
        private double CalculateDb(double cost, double salvage, int life, int period, int month) => 0.0;
        private double CalculateDdb(double cost, double salvage, int life, int period, double factor) => 0.0;
        private double CalculateDisc(DateTime settlement, DateTime maturity, double pr, double redemption, int basis) => 0.0;
        private double CalculateDollarDe(double dollarFractional, int fraction) => 0.0;
        private double CalculateDollarFr(double dollarDecimal, int fraction) => 0.0;
        private double CalculateDuration(DateTime settlement, DateTime maturity, double coupon, double yld, int frequency, int basis) => 0.0;
        private double CalculateEffect(double nominalRate, int npery) => 0.0;
        private double CalculateFv(double rate, int nper, double pmt, double pv, int type) => 0.0;
        private double CalculateIntrate(DateTime settlement, DateTime maturity, double investment, double redemption, int basis) => 0.0;
        private double CalculateIpmt(double rate, int per, int nper, double pv, double fv, int type) => 0.0;
        private double CalculateIrr(List<object> values, double guess) => 0.0;
        private double CalculateIsPmt(double rate, int per, int nper, double pv) => 0.0;
        private double CalculateMduration(DateTime settlement, DateTime maturity, double coupon, double yld, int frequency, int basis) => 0.0;
        private double CalculateMirr(List<object> values, double financeRate, double reinvestRate) => 0.0;
        private double CalculateNominal(double effectRate, int npery) => 0.0;
        private double CalculateNper(double rate, double pmt, double pv, double fv, int type) => 0.0;
        private double CalculateOddfPrice(DateTime settlement, DateTime maturity, DateTime issue, DateTime firstCoupon, double rate, double yld, double redemption, int frequency, int basis) => 0.0;
        private double CalculateOddfYield(DateTime settlement, DateTime maturity, DateTime issue, DateTime firstCoupon, double rate, double pr, double redemption, int frequency, int basis) => 0.0;
        private double CalculateOddlPrice(DateTime settlement, DateTime maturity, DateTime lastInterest, double rate, double yld, double redemption, int frequency, int basis) => 0.0;
        private double CalculateOddlYield(DateTime settlement, DateTime maturity, DateTime lastInterest, double rate, double pr, double redemption, int frequency, int basis) => 0.0;
        private double CalculatePmt(double rate, int nper, double pv, double fv, int type) => 0.0;
        private double CalculatePpmt(double rate, int per, int nper, double pv, double fv, int type) => 0.0;
        private double CalculatePrice(DateTime settlement, DateTime maturity, double rate, double yld, double redemption, int frequency, int basis) => 0.0;
        private double CalculatePriceDisc(DateTime settlement, DateTime maturity, double discount, double redemption, int basis) => 0.0;
        private double CalculatePriceMat(DateTime settlement, DateTime maturity, DateTime issue, double rate, double yld, int basis) => 0.0;
        private double CalculatePv(double rate, int nper, double pmt, double fv, int type) => 0.0;
        private double CalculateRate(int nper, double pmt, double pv, double fv, int type, double guess) => 0.0;
        private double CalculateReceived(DateTime settlement, DateTime maturity, double investment, double discount, int basis) => 0.0;
        private double CalculateTbilleq(DateTime settlement, DateTime maturity, double discount) => 0.0;
        private double CalculateTbillPrice(DateTime settlement, DateTime maturity, double discount) => 0.0;
        private double CalculateTbillYield(DateTime settlement, DateTime maturity, double pr) => 0.0;
        private double CalculateVdb(double cost, double salvage, int life, int startPeriod, int endPeriod, double factor, bool noSwitch) => 0.0;
        private double CalculateXIrr(List<object> values, List<DateTime> dates, double guess) => 0.0;
        private double CalculateXnpv(double rate, List<object> values, List<DateTime> dates) => 0.0;
        private double CalculateYield(DateTime settlement, DateTime maturity, double rate, double pr, double redemption, int frequency, int basis) => 0.0;
        private double CalculateYieldDisc(DateTime settlement, DateTime maturity, double pr, double redemption, int basis) => 0.0;
        private double CalculateYieldMat(DateTime settlement, DateTime maturity, DateTime issue, double rate, double pr, int basis) => 0.0;

        private object GetCellInfo(string infoType, object reference) => "";
        private object GetErrorType(object value) => 0;
        private object GetInfo(string infoType) => "";

        private string FormatAddress(int row, int col, int absNum, bool a1, string sheetText) => "";
        private int ExtractColumnNumber(string reference) => 1;
        private int ExtractRowNumber(string reference) => 1;
        private object HLookup(object lookupValue, List<object> tableArray, int rowIndex, bool rangeLookup) => null;
        private object GetIndex(List<object> array, int rowNum, int colNum) => null;
        private object Lookup(object lookupValue, List<object> lookupVector, List<object> resultVector) => null;
        private object Match(object lookupValue, List<object> lookupArray, int matchType) => 0;
        private List<object> TransposeArray(List<object> array) => new List<object>();
        private object VLookup(object lookupValue, List<object> tableArray, int colIndex, bool rangeLookup) => null;

        private double CalculateAggregate(int functionNum, int options, List<object> values) => 0.0;
        private long CalculateCombinations(int n, int k) => 0;
        private long CalculateCombinationsWithRepetition(int n, int k) => 0;
        private long CalculateDoubleFactorial(int number) => 0;
        private long CalculateFactorial(int number) => 0;
        private long CalculateGcd(long[] numbers) => 0;
        private long CalculateLcm(long[] numbers) => 0;
        private double CalculateMatrixDeterminant(List<object> matrix) => 0.0;
        private List<List<double>> CalculateMatrixInverse(List<object> matrix) => new List<List<double>>();
        private List<object> MultiplyMatrices(List<object> matrix1, List<object> matrix2) => new List<object>();
        private double Mod(double number, double divisor) => 0.0;
        private long CalculatePermutations(int n, int k) => 0;
        private string ConvertToRoman(int number, int style) => "";
        private double CalculateSubtotal(int functionNum, List<double> values) => 0.0;
        private object SumIf(List<object> range, object criteria, List<object> sumRange) => 0.0;
        private object SumIfs(List<object> sumRange, List<List<object>> ranges, List<object> criterias) => 0.0;

        private double CalculateBetaDist(double x, double alpha, double beta, double a, double b) => 0.0;
        private double CalculateBetaInv(double probability, double alpha, double beta, double a, double b) => 0.0;
        private double CalculateBinomDist(int numberS, int trials, double probabilityS, bool cumulative) => 0.0;
        private double CalculateChiDist(double x, double degreesFreedom) => 0.0;
        private double CalculateChiInv(double probability, double degreesFreedom) => 0.0;
        private double CalculateChiTest(List<object> actual, List<object> expected) => 0.0;
        private double CalculateConfidence(double alpha, double standardDev, int size) => 0.0;
        private double CalculateCorrel(List<double> array1, List<double> array2) => 0.0;
        private object CountIf(List<object> range, object criteria) => 0;
        private object CountIfs(List<List<object>> ranges, List<object> criterias) => 0;
        private double CalculateCovar(List<double> array1, List<double> array2) => 0.0;
        private double CalculateCritBinom(int trials, double probabilityS, double alpha) => 0.0;
        private double CalculateExponDist(double x, double lambda, bool cumulative) => 0.0;
        private double CalculateFDist(double x, int degreesFreedom1, int degreesFreedom2) => 0.0;
        private double CalculateFInv(double probability, int degreesFreedom1, int degreesFreedom2) => 0.0;
        private double CalculateForecast(double x, List<double> knownYs, List<double> knownXs) => 0.0;
        private List<double> CalculateFrequency(List<double> dataArray, List<double> binsArray) => new List<double>();
        private double CalculateFTest(List<double> array1, List<double> array2) => 0.0;
        private double CalculateGammaDist(double x, double alpha, double beta, bool cumulative) => 0.0;
        private double CalculateGammaInv(double probability, double alpha, double beta) => 0.0;
        private double GammaLn(double x) => 0.0;
        private object CalculateGrowth(List<double> knownYs, List<double> knownXs, List<double> newXs, bool useConst) => 0.0;
        private double CalculateHypgeomDist(int sampleS, int numberSample, int populationS, int numberPop) => 0.0;
        private double CalculateIntercept(List<double> knownYs, List<double> knownXs) => 0.0;
        private object CalculateLinest(List<double> knownYs, List<double> knownXs, bool useConst, bool stats) => 0.0;
        private object CalculateLogest(List<double> knownYs, List<double> knownXs, bool useConst, bool stats) => 0.0;
        private double NormDist(double x, double mean, double stdDev, bool cumulative) => 0.0;
        private double NormInv(double probability) => 0.0;
        private double CalculateNegbinomDist(int numberF, int numberS, double probabilityS) => 0.0;
        private double CalculatePoisson(double x, double mean, bool cumulative) => 0.0;
        private double CalculateSlope(List<double> knownYs, List<double> knownXs) => 0.0;
        private double CalculateSteyx(List<double> knownYs, List<double> knownXs) => 0.0;
        private double CalculateTDist(double x, int degreesFreedom, int tails) => 0.0;
        private double CalculateTInv(double probability, int degreesFreedom) => 0.0;
        private object CalculateTrend(List<double> knownYs, List<double> knownXs, List<double> newXs, bool useConst) => 0.0;
        private double CalculateTTest(List<double> array1, List<double> array2, int tails, int type) => 0.0;
        private double CalculateWeibull(double x, double alpha, double beta, bool cumulative) => 0.0;
        private object AverageIf(List<object> range, object criteria, List<object> averageRange) => 0.0;
        private object AverageIfs(List<object> averageRange, List<List<object>> ranges, List<object> criterias) => 0.0;

        private string ToHalfWidth(string text) => text;
        private string ToFullWidth(string text) => text;
        private string ConvertToThaiBaht(double number) => "";
        private int IndexOfBytes(byte[] bytes, byte[] searchBytes, int startIndex) => -1;
        private string FormatNumber(double number, string formatText) => number.ToString();
    }
}
