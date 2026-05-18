using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OfficeSuite.Spreadsheet.Analysis
{
    public class FormulaEngine
    {
        private readonly Dictionary<string, Func<CellReference, object[], string, object>> _functions;
        private readonly Dictionary<string, string> _variables;
        private readonly Dictionary<string, List<CellRange>> _namedRanges;

        public FormulaEngine()
        {
            _functions = new Dictionary<string, Func<CellReference, object[], string, object>>();
            _variables = new Dictionary<string, string>();
            _namedRanges = new Dictionary<string, List<CellRange>>();
            InitializeBuiltInFunctions();
        }

        private void InitializeBuiltInFunctions()
        {
            _functions["SUM"] = (ref, args, ctx) => Sum(args);
            _functions["AVERAGE"] = (ref, args, ctx) => Average(args);
            _functions["COUNT"] = (ref, args, ctx) => Count(args);
            _functions["COUNTA"] = (ref, args, ctx) => CountA(args);
            _functions["MAX"] = (ref, args, ctx) => Max(args);
            _functions["MIN"] = (ref, args, ctx) => Min(args);
            _functions["IF"] = (ref, args, ctx) => If(args);
            _functions["AND"] = (ref, args, ctx) => And(args);
            _functions["OR"] = (ref, args, ctx) => Or(args);
            _functions["NOT"] = (ref, args, ctx) => Not(args);
            _functions["TRUE"] = (ref, args, ctx) => true;
            _functions["FALSE"] = (ref, args, ctx) => false;
            _functions["ABS"] = (ref, args, ctx) => Abs(args);
            _functions["ROUND"] = (ref, args, ctx) => Round(args);
            _functions["ROUNDUP"] = (ref, args, ctx) => RoundUp(args);
            _functions["ROUNDDOWN"] = (ref, args, ctx) => RoundDown(args);
            _functions["SQRT"] = (ref, args, ctx) => Sqrt(args);
            _functions["POWER"] = (ref, args, ctx) => Power(args);
            _functions["MOD"] = (ref, args, ctx) => Mod(args);
            _functions["INT"] = (ref, args, ctx) => Int(args);
            _functions["TRUNC"] = (ref, args, ctx) => Trunc(args);
            _functions["EXP"] = (ref, args, ctx) => Exp(args);
            _functions["LN"] = (ref, args, ctx) => Ln(args);
            _functions["LOG"] = (ref, args, ctx) => Log(args);
            _functions["LOG10"] = (ref, args, ctx) => Log10(args);
            _functions["SIN"] = (ref, args, ctx) => Sin(args);
            _functions["COS"] = (ref, args, ctx) => Cos(args);
            _functions["TAN"] = (ref, args, ctx) => Tan(args);
            _functions["ASIN"] = (ref, args, ctx) => Asin(args);
            _functions["ACOS"] = (ref, args, ctx) => Acos(args);
            _functions["ATAN"] = (ref, args, ctx) => Atan(args);
            _functions["ATAN2"] = (ref, args, ctx) => Atan2(args);
            _functions["PI"] = (ref, args, ctx) => Math.PI;
            _functions["RADIANS"] = (ref, args, ctx) => Radians(args);
            _functions["DEGREES"] = (ref, args, ctx) => Degrees(args);
            _functions["CEILING"] = (ref, args, ctx) => Ceiling(args);
            _functions["FLOOR"] = (ref, args, ctx) => Floor(args);
            _functions["SIGN"] = (ref, args, ctx) => Sign(args);
            _functions["PRODUCT"] = (ref, args, ctx) => Product(args);
            _functions["QUOTIENT"] = (ref, args, ctx) => Quotient(args);
            _functions["MDETERM"] = (ref, args, ctx) => MDeterm(args);
            _functions["MINVERSE"] = (ref, args, ctx) => MInverse(args);
            _functions["MMULT"] = (ref, args, ctx) => MMult(args);
            _functions["SUMIF"] = (ref, args, ctx) => SumIf(args);
            _functions["COUNTIF"] = (ref, args, ctx) => CountIf(args);
            _functions["AVERAGEIF"] = (ref, args, ctx) => AverageIf(args);
            _functions["SUMIFS"] = (ref, args, ctx) => SumIfs(args);
            _functions["COUNTIFS"] = (ref, args, ctx) => CountIfs(args);
            _functions["AVERAGEIFS"] = (ref, args, ctx) => AverageIfs(args);
            _functions["VLOOKUP"] = (ref, args, ctx) => VLookup(args);
            _functions["HLOOKUP"] = (ref, args, ctx) => HLookup(args);
            _functions["LOOKUP"] = (ref, args, ctx) => Lookup(args);
            _functions["INDEX"] = (ref, args, ctx) => Index(args);
            _functions["MATCH"] = (ref, args, ctx) => Match(args);
            _functions["CHOOSE"] = (ref, args, ctx) => Choose(args);
            _functions["TRANSPOSE"] = (ref, args, ctx) => Transpose(args);
            _functions["CONCATENATE"] = (ref, args, ctx) => Concatenate(args);
            _functions["LEFT"] = (ref, args, ctx) => Left(args);
            _functions["RIGHT"] = (ref, args, ctx) => Right(args);
            _functions["MID"] = (ref, args, ctx) => Mid(args);
            _functions["LEN"] = (ref, args, ctx) => Len(args);
            _functions["UPPER"] = (ref, args, ctx) => Upper(args);
            _functions["LOWER"] = (ref, args, ctx) => Lower(args);
            _functions["PROPER"] = (ref, args, ctx) => Proper(args);
            _functions["TRIM"] = (ref, args, ctx) => Trim(args);
            _functions["SUBSTITUTE"] = (ref, args, ctx) => Substitute(args);
            _functions["REPLACE"] = (ref, args, ctx) => Replace(args);
            _functions["FIND"] = (ref, args, ctx) => Find(args);
            _functions["SEARCH"] = (ref, args, ctx) => Search(args);
            _functions["TEXT"] = (ref, args, ctx) => Text(args);
            _functions["VALUE"] = (ref, args, ctx) => Value(args);
            _functions["CONCAT"] = (ref, args, ctx) => Concat(args);
            _functions["TEXTJOIN"] = (ref, args, ctx) => TextJoin(args);
            _functions["LEFTB"] = (ref, args, ctx) => LeftB(args);
            _functions["RIGHTB"] = (ref, args, ctx) => RightB(args);
            _functions["MIDB"] = (ref, args, ctx) => MidB(args);
            _functions["LENB"] = (ref, args, ctx) => LenB(args);
            _functions["CODE"] = (ref, args, ctx) => Code(args);
            _functions["CHAR"] = (ref, args, ctx) => Char(args);
            _functions["REPT"] = (ref, args, ctx) => Rept(args);
            _functions["EXACT"] = (ref, args, ctx) => Exact(args);
            _functions["CLEAN"] = (ref, args, ctx) => Clean(args);
            _functions["DOLLAR"] = (ref, args, ctx) => Dollar(args);
            _functions["FIXED"] = (ref, args, ctx) => Fixed(args);
            _functions["T"] = (ref, args, ctx) => T(args);
            _functions["N"] = (ref, args, ctx) => N(args);
            _functions["ISBLANK"] = (ref, args, ctx) => IsBlank(args);
            _functions["ISERROR"] = (ref, args, ctx) => IsError(args);
            _functions["ISNA"] = (ref, args, ctx) => IsNA(args);
            _functions["ISNUMBER"] = (ref, args, ctx) => IsNumber(args);
            _functions["ISTEXT"] = (ref, args, ctx) => IsText(args);
            _functions["ISNONTEXT"] = (ref, args, ctx) => IsNonText(args);
            _functions["ISLOGICAL"] = (ref, args, ctx) => IsLogical(args);
            _functions["IFERROR"] = (ref, args, ctx) => IfError(args);
            _functions["IFNA"] = (ref, args, ctx) => IfNA(args);
            _functions["ERROR.TYPE"] = (ref, args, ctx) => ErrorType(args);
            _functions["TYPE"] = (ref, args, ctx) => Type(args);
            _functions["NA"] = (ref, args, ctx) => NA();
            _functions["NOW"] = (ref, args, ctx) => Now();
            _functions["TODAY"] = (ref, args, ctx) => Today();
            _functions["DATE"] = (ref, args, ctx) => Date(args);
            _functions["TIME"] = (ref, args, ctx) => Time(args);
            _functions["YEAR"] = (ref, args, ctx) => Year(args);
            _functions["MONTH"] = (ref, args, ctx) => Month(args);
            _functions["DAY"] = (ref, args, ctx) => Day(args);
            _functions["HOUR"] = (ref, args, ctx) => Hour(args);
            _functions["MINUTE"] = (ref, args, ctx) => Minute(args);
            _functions["SECOND"] = (ref, args, ctx) => Second(args);
            _functions["WEEKDAY"] = (ref, args, ctx) => Weekday(args);
            _functions["WEEKNUM"] = (ref, args, ctx) => WeekNum(args);
            _functions["EDATE"] = (ref, args, ctx) => EDate(args);
            _functions["EOMONTH"] = (ref, args, ctx) => EOMonth(args);
            _functions["DATEDIF"] = (ref, args, ctx) => DateDif(args);
            _functions["DATEVALUE"] = (ref, args, ctx) => DateValue(args);
            _functions["TIMEVALUE"] = (ref, args, ctx) => TimeValue(args);
            _functions["NETWORKDAYS"] = (ref, args, ctx) => NetworkDays(args);
            _functions["WORKDAY"] = (ref, args, ctx) => Workday(args);
            _functions["MAXA"] = (ref, args, ctx) => MaxA(args);
            _functions["MINA"] = (ref, args, ctx) => MinA(args);
            _functions["STDEV"] = (ref, args, ctx) => StDev(args);
            _functions["STDEVP"] = (ref, args, ctx) => StDevP(args);
            _functions["VAR"] = (ref, args, ctx) => Var(args);
            _functions["VARP"] = (ref, args, ctx) => VarP(args);
            _functions["AVEDEV"] = (ref, args, ctx) => AveDev(args);
            _functions["DEVSQ"] = (ref, args, ctx) => DevSq(args);
            _functions["GEOMEAN"] = (ref, args, ctx) => GeoMean(args);
            _functions["HARMEAN"] = (ref, args, ctx) => HarMean(args);
            _functions["MEDIAN"] = (ref, args, ctx) => Median(args);
            _functions["MODE"] = (ref, args, ctx) => Mode(args);
            _functions["PERMUT"] = (ref, args, ctx) => Permut(args);
            _functions["COMBIN"] = (ref, args, ctx) => Combin(args);
            _functions["FACT"] = (ref, args, ctx) => Fact(args);
            _functions["FACTDOUBLE"] = (ref, args, ctx) => FactDouble(args);
            _functions["GCD"] = (ref, args, ctx) => GCD(args);
            _functions["LCM"] = (ref, args, ctx) => LCM(args);
            _functions["MULTINOMIAL"] = (ref, args, ctx) => Multinomial(args);
            _functions["SERIESSUM"] = (ref, args, ctx) => SeriesSum(args);
            _functions["SQRTPI"] = (ref, args, ctx) => SqrtPi(args);
            _functions["SUMSQ"] = (ref, args, ctx) => SumSq(args);
            _functions["SUMPRODUCT"] = (ref, args, ctx) => SumProduct(args);
            _functions["SUMX2MY2"] = (ref, args, ctx) => SumX2MY2(args);
            _functions["SUMX2PY2"] = (ref, args, ctx) => SumX2PY2(args);
            _functions["SUMXMY2"] = (ref, args, ctx) => SumXMY2(args);
            _functions["CORREL"] = (ref, args, ctx) => Correl(args);
            _functions["COVAR"] = (ref, args, ctx) => Covar(args);
            _functions["FORECAST"] = (ref, args, ctx) => Forecast(args);
            _functions["INTERCEPT"] = (ref, args, ctx) => Intercept(args);
            _functions["LINEST"] = (ref, args, ctx) => LinEst(args);
            _functions["LOGEST"] = (ref, args, ctx) => LogEst(args);
            _functions["SLOPE"] = (ref, args, ctx) => Slope(args);
            _functions["STEYX"] = (ref, args, ctx) => SteYX(args);
            _functions["TREND"] = (ref, args, ctx) => Trend(args);
            _functions["GROWTH"] = (ref, args, ctx) => Growth(args);
            _functions["PERCENTILE"] = (ref, args, ctx) => Percentile(args);
            _functions["PERCENTRANK"] = (ref, args, ctx) => PercentRank(args);
            _functions["QUARTILE"] = (ref, args, ctx) => Quartile(args);
            _functions["RANK"] = (ref, args, ctx) => Rank(args);
            _functions["LARGE"] = (ref, args, ctx) => Large(args);
            _functions["SMALL"] = (ref, args, ctx) => Small(args);
            _functions["SUBTOTAL"] = (ref, args, ctx) => Subtotal(args);
            _functions["AGGREGATE"] = (ref, args, ctx) => Aggregate(args);
            _functions["ABSREF"] = (ref, args, ctx) => AbsRef(args);
            _functions["AREAS"] = (ref, args, ctx) => Areas(args);
            _functions["COLUMN"] = (ref, args, ctx) => Column(args);
            _functions["COLUMNS"] = (ref, args, ctx) => Columns(args);
            _functions["INDIRECT"] = (ref, args, ctx) => Indirect(args);
            _functions["OFFSET"] = (ref, args, ctx) => Offset(args);
            _functions["ROW"] = (ref, args, ctx) => Row(args);
            _functions["ROWS"] = (ref, args, ctx) => Rows(args);
            _functions["SHEET"] = (ref, args, ctx) => Sheet(args);
            _functions["SHEETS"] = (ref, args, ctx) => Sheets(args);
            _functions["HYPERLINK"] = (ref, args, ctx) => Hyperlink(args);
            _functions["INFO"] = (ref, args, ctx) => Info(args);
            _functions["CELL"] = (ref, args, ctx) => Cell(args);
            _functions["DB"] = (ref, args, ctx) => DB(args);
            _functions["DDB"] = (ref, args, ctx) => DDB(args);
            _functions["FV"] = (ref, args, ctx) => FV(args);
            _functions["FVSCHEDULE"] = (ref, args, ctx) => FVSchedule(args);
            _functions["IPMT"] = (ref, args, ctx) => IPMT(args);
            _functions["IRR"] = (ref, args, ctx) => IRR(args);
            _functions["MIRR"] = (ref, args, ctx) => MIRR(args);
            _functions["NPER"] = (ref, args, ctx) => NPer(args);
            _functions["NPV"] = (ref, args, ctx) => NPV(args);
            _functions["PMT"] = (ref, args, ctx) => PMT(args);
            _functions["PPMT"] = (ref, args, ctx) => PPMT(args);
            _functions["PV"] = (ref, args, ctx) => PV(args);
            _functions["RATE"] = (ref, args, ctx) => Rate(args);
            _functions["SLN"] = (ref, args, ctx) => SLN(args);
            _functions["SYD"] = (ref, args, ctx) => SYD(args);
            _functions["VDB"] = (ref, args, ctx) => VDB(args);
            _functions["XIRR"] = (ref, args, ctx) => XIRR(args);
            _functions["XNPV"] = (ref, args, ctx) => XNPV(args);
            _functions["BINOM.DIST"] = (ref, args, ctx) => BinomDist(args);
            _functions["BINOM.INV"] = (ref, args, ctx) => BinomInv(args);
            _functions["CHISQ.DIST"] = (ref, args, ctx) => ChiSqDist(args);
            _functions["CHISQ.INV"] = (ref, args, ctx) => ChiSqInv(args);
            _functions["EXPON.DIST"] = (ref, args, ctx) => ExponDist(args);
            _functions["F.DIST"] = (ref, args, ctx) => FDist(args);
            _functions["F.INV"] = (ref, args, ctx) => FInv(args);
            _functions["GAMMA.DIST"] = (ref, args, ctx) => GammaDist(args);
            _functions["GAMMA.INV"] = (ref, args, ctx) => GammaInv(args);
            _functions["HYPGEOM.DIST"] = (ref, args, ctx) => HypGeomDist(args);
            _functions["LOGNORM.DIST"] = (ref, args, ctx) => LogNormDist(args);
            _functions["LOGNORM.INV"] = (ref, args, ctx) => LogNormInv(args);
            _functions["NEGBINOM.DIST"] = (ref, args, ctx) => NegBinomDist(args);
            _functions["NORM.DIST"] = (ref, args, ctx) => NormDist(args);
            _functions["NORM.INV"] = (ref, args, ctx) => NormInv(args);
            _functions["NORM.S.DIST"] = (ref, args, ctx) => NormSDist(args);
            _functions["NORM.S.INV"] = (ref, args, ctx) => NormSInv(args);
            _functions["POISSON.DIST"] = (ref, args, ctx) => PoissonDist(args);
            _functions["T.DIST"] = (ref, args, ctx) => TDist(args);
            _functions["T.INV"] = (ref, args, ctx) => TInv(args);
            _functions["WEIBULL.DIST"] = (ref, args, ctx) => WeibullDist(args);
            _functions["Z.TEST"] = (ref, args, ctx) => ZTest(args);
        }

        private double Sum(object[] args)
        {
            var values = FlattenValues(args);
            return values.Sum(GetNumericValue);
        }

        private double Average(object[] args)
        {
            var values = FlattenValues(args).Where(v => !IsError(v)).ToList();
            if (values.Count == 0) return 0;
            return values.Average(GetNumericValue);
        }

        private int Count(object[] args)
        {
            var values = FlattenValues(args);
            return values.Count(IsNumeric);
        }

        private int CountA(object[] args)
        {
            var values = FlattenValues(args);
            return values.Count(v => v != null && v.ToString() != "");
        }

        private double Max(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue);
            return values.Any() ? values.Max() : 0;
        }

        private double Min(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue);
            return values.Any() ? values.Min() : 0;
        }

        private object If(object[] args)
        {
            if (args.Length < 2) return "#VALUE!";
            var condition = args[0];
            var isTrue = ToBoolean(condition);
            return isTrue ? args[1] : (args.Length > 2 ? args[2] : false);
        }

        private bool And(object[] args)
        {
            return args.All(a => ToBoolean(a));
        }

        private bool Or(object[] args)
        {
            return args.Any(a => ToBoolean(a));
        }

        private bool Not(object[] args)
        {
            if (args.Length < 1) return true;
            return !ToBoolean(args[0]);
        }

        private double Abs(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Abs(GetNumericValue(args[0]));
        }

        private double Round(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var digits = args.Length > 1 ? (int)GetNumericValue(args[1]) : 0;
            return Math.Round(value, digits);
        }

        private double RoundUp(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var digits = args.Length > 1 ? (int)GetNumericValue(args[1]) : 0;
            var factor = Math.Pow(10, digits);
            return Math.Ceiling(value * factor) / factor;
        }

        private double RoundDown(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var digits = args.Length > 1 ? (int)GetNumericValue(args[1]) : 0;
            var factor = Math.Pow(10, digits);
            return Math.Floor(value * factor) / factor;
        }

        private double Sqrt(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Sqrt(GetNumericValue(args[0]));
        }

        private double Power(object[] args)
        {
            if (args.Length < 2) return 0;
            return Math.Pow(GetNumericValue(args[0]), GetNumericValue(args[1]));
        }

        private double Mod(object[] args)
        {
            if (args.Length < 2) return 0;
            var divisor = GetNumericValue(args[1]);
            if (divisor == 0) return "#DIV/0!";
            return GetNumericValue(args[0]) % divisor;
        }

        private double Int(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Floor(GetNumericValue(args[0]));
        }

        private double Trunc(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var digits = args.Length > 1 ? (int)GetNumericValue(args[1]) : 0;
            var factor = Math.Pow(10, digits);
            return Math.Truncate(value * factor) / factor;
        }

        private double Exp(object[] args)
        {
            if (args.Length < 1) return 1;
            return Math.Exp(GetNumericValue(args[0]));
        }

        private double Ln(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Log(GetNumericValue(args[0]));
        }

        private double Log(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args.Length > 1)
                return Math.Log(GetNumericValue(args[0]), GetNumericValue(args[1]));
            return Math.Log10(GetNumericValue(args[0]));
        }

        private double Log10(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Log10(GetNumericValue(args[0]));
        }

        private double Sin(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Sin(GetNumericValue(args[0]));
        }

        private double Cos(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Cos(GetNumericValue(args[0]));
        }

        private double Tan(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Tan(GetNumericValue(args[0]));
        }

        private double Asin(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Asin(GetNumericValue(args[0]));
        }

        private double Acos(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Acos(GetNumericValue(args[0]));
        }

        private double Atan(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Atan(GetNumericValue(args[0]));
        }

        private double Atan2(object[] args)
        {
            if (args.Length < 2) return 0;
            return Math.Atan2(GetNumericValue(args[0]), GetNumericValue(args[1]));
        }

        private double Radians(object[] args)
        {
            if (args.Length < 1) return 0;
            return GetNumericValue(args[0]) * Math.PI / 180;
        }

        private double Degrees(object[] args)
        {
            if (args.Length < 1) return 0;
            return GetNumericValue(args[0]) * 180 / Math.PI;
        }

        private double Ceiling(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var significance = args.Length > 1 ? GetNumericValue(args[1]) : 1;
            if (significance == 0) return 0;
            return Math.Ceiling(value / significance) * significance;
        }

        private double Floor(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            var significance = args.Length > 1 ? GetNumericValue(args[1]) : 1;
            if (significance == 0) return 0;
            return Math.Floor(value / significance) * significance;
        }

        private int Sign(object[] args)
        {
            if (args.Length < 1) return 0;
            var value = GetNumericValue(args[0]);
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        private double Product(object[] args)
        {
            var values = FlattenValues(args);
            var result = 1.0;
            foreach (var v in values)
            {
                if (IsNumeric(v) && !IsError(v))
                    result *= GetNumericValue(v);
            }
            return result;
        }

        private double Quotient(object[] args)
        {
            if (args.Length < 2) return 0;
            var divisor = GetNumericValue(args[1]);
            if (divisor == 0) return "#DIV/0!";
            return Math.Floor(GetNumericValue(args[0]) / divisor);
        }

        private double MDeterm(object[] args)
        {
            return 0;
        }

        private object MInverse(object[] args)
        {
            return null;
        }

        private object MMult(object[] args)
        {
            return null;
        }

        private double SumIf(object[] args)
        {
            return 0;
        }

        private int CountIf(object[] args)
        {
            return 0;
        }

        private double AverageIf(object[] args)
        {
            return 0;
        }

        private double SumIfs(object[] args)
        {
            return 0;
        }

        private int CountIfs(object[] args)
        {
            return 0;
        }

        private double AverageIfs(object[] args)
        {
            return 0;
        }

        private object VLookup(object[] args)
        {
            return null;
        }

        private object HLookup(object[] args)
        {
            return null;
        }

        private object Lookup(object[] args)
        {
            return null;
        }

        private object Index(object[] args)
        {
            return null;
        }

        private object Match(object[] args)
        {
            return null;
        }

        private object Choose(object[] args)
        {
            if (args.Length < 2) return "#VALUE!";
            var index = (int)GetNumericValue(args[0]);
            if (index < 1 || index >= args.Length) return "#VALUE!";
            return args[index];
        }

        private object Transpose(object[] args)
        {
            return null;
        }

        private string Concatenate(object[] args)
        {
            return string.Join("", args.Select(a => a?.ToString() ?? ""));
        }

        private string Left(object[] args)
        {
            if (args.Length < 1) return "";
            var text = args[0]?.ToString() ?? "";
            var numChars = args.Length > 1 ? (int)GetNumericValue(args[1]) : 1;
            return text.Substring(0, Math.Min(numChars, text.Length));
        }

        private string Right(object[] args)
        {
            if (args.Length < 1) return "";
            var text = args[0]?.ToString() ?? "";
            var numChars = args.Length > 1 ? (int)GetNumericValue(args[1]) : 1;
            return text.Length <= numChars ? text : text.Substring(text.Length - numChars);
        }

        private string Mid(object[] args)
        {
            if (args.Length < 3) return "";
            var text = args[0]?.ToString() ?? "";
            var start = (int)GetNumericValue(args[1]) - 1;
            var numChars = (int)GetNumericValue(args[2]);
            if (start < 0 || start >= text.Length) return "";
            return text.Substring(start, Math.Min(numChars, text.Length - start));
        }

        private int Len(object[] args)
        {
            if (args.Length < 1) return 0;
            return args[0]?.ToString().Length ?? 0;
        }

        private string Upper(object[] args)
        {
            return args.Length > 0 ? (args[0]?.ToString().ToUpper() ?? "") : "";
        }

        private string Lower(object[] args)
        {
            return args.Length > 0 ? (args[0]?.ToString().ToLower() ?? "") : "";
        }

        private string Proper(object[] args)
        {
            if (args.Length < 1) return "";
            var text = args[0]?.ToString() ?? "";
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        private string Trim(object[] args)
        {
            if (args.Length < 1) return "";
            return Regex.Replace(args[0]?.ToString() ?? "", @"\s+", " ").Trim();
        }

        private string Substitute(object[] args)
        {
            if (args.Length < 3) return "";
            var text = args[0]?.ToString() ?? "";
            var oldText = args[1]?.ToString() ?? "";
            var newText = args[2]?.ToString() ?? "";
            if (args.Length > 3)
            {
                var instanceNum = (int)GetNumericValue(args[3]);
                var index = -1;
                for (int i = 0; i < instanceNum; i++)
                {
                    index = text.IndexOf(oldText, index + 1);
                    if (index < 0) break;
                }
                if (index >= 0)
                    return text.Remove(index, oldText.Length).Insert(index, newText);
                return text;
            }
            return text.Replace(oldText, newText);
        }

        private string Replace(object[] args)
        {
            if (args.Length < 4) return "";
            var text = args[0]?.ToString() ?? "";
            var start = (int)GetNumericValue(args[1]) - 1;
            var numChars = (int)GetNumericValue(args[2]);
            var newText = args[3]?.ToString() ?? "";
            if (start < 0 || start >= text.Length) return text;
            return text.Remove(start, Math.Min(numChars, text.Length - start)).Insert(start, newText);
        }

        private int Find(object[] args)
        {
            if (args.Length < 2) return 0;
            var findText = args[0]?.ToString() ?? "";
            var withinText = args[1]?.ToString() ?? "";
            var startNum = args.Length > 2 ? (int)GetNumericValue(args[2]) : 0;
            var index = withinText.IndexOf(findText, startNum);
            return index >= 0 ? index + 1 : 0;
        }

        private int Search(object[] args)
        {
            return Find(args);
        }

        private string Text(object[] args)
        {
            if (args.Length < 2) return "";
            var value = args[0];
            var format = args[1]?.ToString() ?? "";
            return string.Format($"{{0:{format}}}", value);
        }

        private double Value(object[] args)
        {
            if (args.Length < 1) return 0;
            double.TryParse(args[0]?.ToString(), out double result);
            return result;
        }

        private string Concat(object[] args)
        {
            return Concatenate(args);
        }

        private string TextJoin(object[] args)
        {
            if (args.Length < 3) return "";
            var delimiter = args[0]?.ToString() ?? "";
            var ignoreEmpty = args.Length > 1 && ToBoolean(args[1]);
            var values = args.Skip(2).SelectMany(a => FlattenValues(new[] { a }));
            if (ignoreEmpty)
                values = values.Where(v => v != null && v.ToString() != "");
            return string.Join(delimiter, values.Select(v => v?.ToString() ?? ""));
        }

        private string LeftB(object[] args)
        {
            return Left(args);
        }

        private string RightB(object[] args)
        {
            return Right(args);
        }

        private string MidB(object[] args)
        {
            return Mid(args);
        }

        private int LenB(object[] args)
        {
            return Len(args);
        }

        private int Code(object[] args)
        {
            if (args.Length < 1 || string.IsNullOrEmpty(args[0]?.ToString())) return 0;
            return (int)args[0].ToString()[0];
        }

        private string Char(object[] args)
        {
            if (args.Length < 1) return "";
            var code = (int)GetNumericValue(args[0]);
            return ((char)code).ToString();
        }

        private string Rept(object[] args)
        {
            if (args.Length < 2) return "";
            var text = args[0]?.ToString() ?? "";
            var count = (int)GetNumericValue(args[1]);
            return string.Concat(Enumerable.Repeat(text, Math.Max(0, count)));
        }

        private bool Exact(object[] args)
        {
            if (args.Length < 2) return false;
            return args[0]?.ToString() == args[1]?.ToString();
        }

        private string Clean(object[] args)
        {
            if (args.Length < 1) return "";
            var text = args[0]?.ToString() ?? "";
            return new string(text.Where(c => !char.IsControl(c)).ToArray());
        }

        private string Dollar(object[] args)
        {
            if (args.Length < 1) return "$0.00";
            var value = GetNumericValue(args[0]);
            var decimals = args.Length > 1 ? (int)GetNumericValue(args[1]) : 2;
            return "$" + value.ToString($"N{decimals}");
        }

        private string Fixed(object[] args)
        {
            if (args.Length < 1) return "0";
            var value = GetNumericValue(args[0]);
            var decimals = args.Length > 1 ? (int)GetNumericValue(args[1]) : 2;
            var noCommas = args.Length > 2 && ToBoolean(args[2]);
            var format = noCommas ? $"F{decimals}" : $"N{decimals}";
            return value.ToString(format);
        }

        private string T(object[] args)
        {
            return args.Length > 0 && args[0] is string ? args[0].ToString() : "";
        }

        private double N(object[] args)
        {
            if (args.Length < 1) return 0;
            return IsNumeric(args[0]) ? GetNumericValue(args[0]) : 0;
        }

        private bool IsBlank(object[] args)
        {
            return args.Length < 1 || args[0] == null || args[0].ToString() == "";
        }

        private bool IsError(object[] args)
        {
            if (args.Length < 1) return false;
            var value = args[0]?.ToString() ?? "";
            return value.StartsWith("#") && value.EndsWith("!");
        }

        private bool IsNA(object[] args)
        {
            return args.Length > 0 && args[0]?.ToString() == "#N/A";
        }

        private bool IsNumber(object[] args)
        {
            return args.Length > 0 && IsNumeric(args[0]);
        }

        private bool IsText(object[] args)
        {
            return args.Length > 0 && args[0] is string;
        }

        private bool IsNonText(object[] args)
        {
            return !IsText(args);
        }

        private bool IsLogical(object[] args)
        {
            if (args.Length < 1) return false;
            var value = args[0]?.ToString()?.ToUpper() ?? "";
            return value == "TRUE" || value == "FALSE";
        }

        private object IfError(object[] args)
        {
            if (args.Length < 2) return "#VALUE!";
            try
            {
                return args[0];
            }
            catch
            {
                return args[1];
            }
        }

        private object IfNA(object[] args)
        {
            if (args.Length < 2) return "#VALUE!";
            return args[0]?.ToString() == "#N/A" ? args[1] : args[0];
        }

        private int ErrorType(object[] args)
        {
            if (args.Length < 1) return 1;
            var error = args[0]?.ToString() ?? "";
            return error switch
            {
                "#NULL!" => 1,
                "#DIV/0!" => 2,
                "#VALUE!" => 3,
                "#REF!" => 4,
                "#NAME?" => 5,
                "#NUM!" => 6,
                "#N/A" => 7,
                "#GETTING_DATA" => 8,
                _ => 1
            };
        }

        private int Type(object[] args)
        {
            if (args.Length < 1) return 1;
            if (IsNumeric(args[0])) return 1;
            if (args[0] is string) return 2;
            if (IsLogical(args[0])) return 4;
            if (args[0] is System.IO.MemoryStream) return 16;
            return 64;
        }

        private object NA()
        {
            return "#N/A";
        }

        private DateTime Now()
        {
            return DateTime.Now;
        }

        private DateTime Today()
        {
            return DateTime.Today;
        }

        private DateTime Date(object[] args)
        {
            if (args.Length < 3) return DateTime.MinValue;
            return new DateTime((int)GetNumericValue(args[0]), (int)GetNumericValue(args[1]), (int)GetNumericValue(args[2]));
        }

        private DateTime Time(object[] args)
        {
            if (args.Length < 3) return DateTime.MinValue;
            var hours = (int)GetNumericValue(args[0]);
            var minutes = (int)GetNumericValue(args[1]);
            var seconds = (int)GetNumericValue(args[2]);
            return new DateTime(1900, 1, 1, hours, minutes, seconds);
        }

        private int Year(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Year;
            return 0;
        }

        private int Month(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Month;
            return 0;
        }

        private int Day(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Day;
            return 0;
        }

        private int Hour(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Hour;
            return 0;
        }

        private int Minute(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Minute;
            return 0;
        }

        private int Second(object[] args)
        {
            if (args.Length < 1) return 0;
            if (args[0] is DateTime dt) return dt.Second;
            return 0;
        }

        private int Weekday(object[] args)
        {
            if (args.Length < 1) return 1;
            if (args[0] is DateTime dt) return (int)dt.DayOfWeek + 1;
            return 1;
        }

        private int WeekNum(object[] args)
        {
            if (args.Length < 1) return 1;
            if (args[0] is DateTime dt)
            {
                var jan1 = new DateTime(dt.Year, 1, 1);
                var days = (dt - jan1).Days;
                return (days + (int)jan1.DayOfWeek) / 7 + 1;
            }
            return 1;
        }

        private DateTime EDate(object[] args)
        {
            if (args.Length < 2) return DateTime.MinValue;
            if (args[0] is DateTime start)
            {
                var months = (int)GetNumericValue(args[1]);
                return start.AddMonths(months);
            }
            return DateTime.MinValue;
        }

        private DateTime EOMonth(object[] args)
        {
            if (args.Length < 2) return DateTime.MinValue;
            if (args[0] is DateTime start)
            {
                var months = (int)GetNumericValue(args[1]);
                return new DateTime(start.Year, start.Month, 1).AddMonths(months + 1).AddDays(-1);
            }
            return DateTime.MinValue;
        }

        private double DateDif(object[] args)
        {
            if (args.Length < 3) return 0;
            if (args[0] is DateTime start && args[1] is DateTime end)
            {
                var unit = args[2]?.ToString().ToUpper() ?? "D";
                var diff = end - start;
                return unit switch
                {
                    "Y" => (end.Year - start.Year - (end.DayOfYear < start.DayOfYear ? 1 : 0)),
                    "M" => (end.Month - start.Month + (end.Year - start.Year) * 12),
                    "D" => diff.Days,
                    "MD" => diff.Days % 30,
                    "YM" => (end.Month - start.Month + 12 * (end.Year - start.Year)) % 12,
                    "YD" => (end - start.AddYears(end.Year - start.Year)).Days,
                    _ => diff.Days
                };
            }
            return 0;
        }

        private DateTime DateValue(object[] args)
        {
            if (args.Length < 1) return DateTime.MinValue;
            DateTime.TryParse(args[0]?.ToString(), out DateTime result);
            return result;
        }

        private DateTime TimeValue(object[] args)
        {
            return DateValue(args);
        }

        private double NetworkDays(object[] args)
        {
            return 0;
        }

        private DateTime Workday(object[] args)
        {
            return DateTime.MinValue;
        }

        private double MaxA(object[] args)
        {
            var values = FlattenValues(args);
            return values.Count > 0 ? values.Max(v => Convert.ToDouble(v)) : 0;
        }

        private double MinA(object[] args)
        {
            var values = FlattenValues(args);
            return values.Count > 0 ? values.Min(v => Convert.ToDouble(v)) : 0;
        }

        private double StDev(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).ToList();
            if (values.Count < 2) return 0;
            var avg = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        private double StDevP(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).ToList();
            if (values.Count < 1) return 0;
            var avg = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }

        private double Var(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).ToList();
            if (values.Count < 2) return 0;
            var avg = values.Average();
            return values.Sum(v => Math.Pow(v - avg, 2)) / (values.Count - 1);
        }

        private double VarP(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).ToList();
            if (values.Count < 1) return 0;
            var avg = values.Average();
            return values.Sum(v => Math.Pow(v - avg, 2)) / values.Count;
        }

        private double AveDev(object[] args)
        {
            return 0;
        }

        private double DevSq(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).ToList();
            if (values.Count < 1) return 0;
            var avg = values.Average();
            return values.Sum(v => Math.Pow(v - avg, 2));
        }

        private double GeoMean(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v) && (double)v > 0).Select(GetNumericValue).ToList();
            if (values.Count < 1) return 0;
            var product = values.Aggregate(1.0, (a, b) => a * b);
            return Math.Pow(product, 1.0 / values.Count);
        }

        private double HarMean(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v) && (double)v > 0).Select(GetNumericValue).ToList();
            if (values.Count < 1 || values.Sum() == 0) return 0;
            return values.Count / values.Sum(v => 1 / v);
        }

        private double Median(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue).OrderBy(v => v).ToList();
            if (values.Count == 0) return 0;
            var mid = values.Count / 2;
            return values.Count % 2 != 0 ? values[mid] : (values[mid - 1] + values[mid]) / 2;
        }

        private double Mode(object[] args)
        {
            var values = FlattenValues(args);
            var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count());
            return groups.FirstOrDefault()?.Key is double d ? d : 0;
        }

        private double Permut(object[] args)
        {
            if (args.Length < 2) return 0;
            var n = (int)GetNumericValue(args[0]);
            var k = (int)GetNumericValue(args[1]);
            if (n < k) return 0;
            return Factorial(n) / Factorial(n - k);
        }

        private double Combin(object[] args)
        {
            if (args.Length < 2) return 0;
            var n = (int)GetNumericValue(args[0]);
            var k = (int)GetNumericValue(args[1]);
            if (n < k) return 0;
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        private double Fact(object[] args)
        {
            if (args.Length < 1) return 0;
            return Factorial((int)GetNumericValue(args[0]));
        }

        private double FactDouble(object[] args)
        {
            if (args.Length < 1) return 0;
            var n = (int)GetNumericValue(args[0]);
            var result = 1.0;
            for (int i = n; i > 1; i -= 2) result *= i;
            return result;
        }

        private double GCD(object[] args)
        {
            var values = args.Select(a => (int)GetNumericValue(a)).ToList();
            return values.Aggregate(GCD);
        }

        private double LCM(object[] args)
        {
            var values = args.Select(a => (int)GetNumericValue(a)).ToList();
            return values.Aggregate(LCM);
        }

        private double Multinomial(object[] args)
        {
            return 0;
        }

        private double SeriesSum(object[] args)
        {
            return 0;
        }

        private double SqrtPi(object[] args)
        {
            if (args.Length < 1) return 0;
            return Math.Sqrt(Math.PI * GetNumericValue(args[0]));
        }

        private double SumSq(object[] args)
        {
            var values = FlattenValues(args).Where(v => IsNumeric(v) && !IsError(v)).Select(GetNumericValue);
            return values.Sum(v => v * v);
        }

        private double SumProduct(object[] args)
        {
            if (args.Length < 2) return 0;
            var arrays = args.Select(a => FlattenValues(new[] { a }).Where(v => IsNumeric(v)).Select(GetNumericValue).ToArray()).ToArray();
            var minLength = arrays.Min(a => a.Length);
            var result = 0.0;
            for (int i = 0; i < minLength; i++)
            {
                var product = arrays.Aggregate(1.0, (a, arr) => a * arr[i]);
                result += product;
            }
            return result;
        }

        private double SumX2MY2(object[] args)
        {
            return 0;
        }

        private double SumX2PY2(object[] args)
        {
            return 0;
        }

        private double SumXMY2(object[] args)
        {
            return 0;
        }

        private double Correl(object[] args)
        {
            return 0;
        }

        private double Covar(object[] args)
        {
            return 0;
        }

        private double Forecast(object[] args)
        {
            return 0;
        }

        private double Intercept(object[] args)
        {
            return 0;
        }

        private object LinEst(object[] args)
        {
            return null;
        }

        private object LogEst(object[] args)
        {
            return null;
        }

        private double Slope(object[] args)
        {
            return 0;
        }

        private double SteYX(object[] args)
        {
            return 0;
        }

        private object Trend(object[] args)
        {
            return null;
        }

        private object Growth(object[] args)
        {
            return null;
        }

        private double Percentile(object[] args)
        {
            return 0;
        }

        private double PercentRank(object[] args)
        {
            return 0;
        }

        private double Quartile(object[] args)
        {
            return 0;
        }

        private double Rank(object[] args)
        {
            return 0;
        }

        private double Large(object[] args)
        {
            return 0;
        }

        private double Small(object[] args)
        {
            return 0;
        }

        private double Subtotal(object[] args)
        {
            return 0;
        }

        private double Aggregate(object[] args)
        {
            return 0;
        }

        private object AbsRef(object[] args)
        {
            return null;
        }

        private int Areas(object[] args)
        {
            return 1;
        }

        private int Column(object[] args)
        {
            return 1;
        }

        private int Columns(object[] args)
        {
            return 1;
        }

        private object Indirect(object[] args)
        {
            return null;
        }

        private object Offset(object[] args)
        {
            return null;
        }

        private int Row(object[] args)
        {
            return 1;
        }

        private int Rows(object[] args)
        {
            return 1;
        }

        private int Sheet(object[] args)
        {
            return 1;
        }

        private int Sheets(object[] args)
        {
            return 1;
        }

        private object Hyperlink(object[] args)
        {
            return null;
        }

        private object Info(object[] args)
        {
            return null;
        }

        private object Cell(object[] args)
        {
            return null;
        }

        private double DB(object[] args)
        {
            return 0;
        }

        private double DDB(object[] args)
        {
            return 0;
        }

        private double FV(object[] args)
        {
            if (args.Length < 3) return 0;
            var rate = GetNumericValue(args[0]);
            var nper = GetNumericValue(args[1]);
            var pmt = GetNumericValue(args[2]);
            var pv = args.Length > 3 ? GetNumericValue(args[3]) : 0;
            var type = args.Length > 4 && ToBoolean(args[4]) ? 1 : 0;
            return -pv * Math.Pow(1 + rate, nper) - pmt * ((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type);
        }

        private double FVSchedule(object[] args)
        {
            return 0;
        }

        private double IPMT(object[] args)
        {
            return 0;
        }

        private double IRR(object[] args)
        {
            return 0;
        }

        private double MIRR(object[] args)
        {
            return 0;
        }

        private double NPer(object[] args)
        {
            return 0;
        }

        private double NPV(object[] args)
        {
            if (args.Length < 2) return 0;
            var rate = GetNumericValue(args[0]);
            var values = args.Skip(1).SelectMany(a => FlattenValues(new[] { a })).Select(GetNumericValue);
            return values.Sum(v => v / Math.Pow(1 + rate, Array.IndexOf(args.Skip(1).ToArray(), args[1]) + 1));
        }

        private double PMT(object[] args)
        {
            if (args.Length < 3) return 0;
            var rate = GetNumericValue(args[0]);
            var nper = GetNumericValue(args[1]);
            var pv = GetNumericValue(args[2]);
            var fv = args.Length > 3 ? GetNumericValue(args[3]) : 0;
            var type = args.Length > 4 && ToBoolean(args[4]) ? 1 : 0;
            if (rate == 0) return -(pv + fv) / nper;
            return -(pv * Math.Pow(1 + rate, nper) + fv) / ((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type);
        }

        private double PPMT(object[] args)
        {
            return 0;
        }

        private double PV(object[] args)
        {
            if (args.Length < 3) return 0;
            var rate = GetNumericValue(args[0]);
            var nper = GetNumericValue(args[1]);
            var pmt = GetNumericValue(args[2]);
            var fv = args.Length > 3 ? GetNumericValue(args[3]) : 0;
            var type = args.Length > 4 && ToBoolean(args[4]) ? 1 : 0;
            if (rate == 0) return -(fv + pmt * nper);
            return -(fv + pmt * ((Math.Pow(1 + rate, nper) - 1) / rate) * (1 + rate * type)) / Math.Pow(1 + rate, nper);
        }

        private double Rate(object[] args)
        {
            return 0;
        }

        private double SLN(object[] args)
        {
            if (args.Length < 3) return 0;
            return (GetNumericValue(args[1]) - GetNumericValue(args[2])) / GetNumericValue(args[0]);
        }

        private double SYD(object[] args)
        {
            if (args.Length < 4) return 0;
            var cost = GetNumericValue(args[0]);
            var salvage = GetNumericValue(args[1]);
            var life = GetNumericValue(args[2]);
            var period = GetNumericValue(args[3]);
            return (cost - salvage) * (life - period + 1) * 2 / (life * (life + 1));
        }

        private double VDB(object[] args)
        {
            return 0;
        }

        private double XIRR(object[] args)
        {
            return 0;
        }

        private double XNPV(object[] args)
        {
            return 0;
        }

        private double BinomDist(object[] args)
        {
            return 0;
        }

        private double BinomInv(object[] args)
        {
            return 0;
        }

        private double ChiSqDist(object[] args)
        {
            return 0;
        }

        private double ChiSqInv(object[] args)
        {
            return 0;
        }

        private double ExponDist(object[] args)
        {
            return 0;
        }

        private double FDist(object[] args)
        {
            return 0;
        }

        private double FInv(object[] args)
        {
            return 0;
        }

        private double GammaDist(object[] args)
        {
            return 0;
        }

        private double GammaInv(object[] args)
        {
            return 0;
        }

        private double HypGeomDist(object[] args)
        {
            return 0;
        }

        private double LogNormDist(object[] args)
        {
            return 0;
        }

        private double LogNormInv(object[] args)
        {
            return 0;
        }

        private double NegBinomDist(object[] args)
        {
            return 0;
        }

        private double NormDist(object[] args)
        {
            return 0;
        }

        private double NormInv(object[] args)
        {
            return 0;
        }

        private double NormSDist(object[] args)
        {
            return 0;
        }

        private double NormSInv(object[] args)
        {
            return 0;
        }

        private double PoissonDist(object[] args)
        {
            return 0;
        }

        private double TDist(object[] args)
        {
            return 0;
        }

        private double TInv(object[] args)
        {
            return 0;
        }

        private double WeibullDist(object[] args)
        {
            return 0;
        }

        private double ZTest(object[] args)
        {
            return 0;
        }

        public object Evaluate(string formula)
        {
            if (string.IsNullOrEmpty(formula) || !formula.StartsWith("="))
                return formula;

            formula = formula.Substring(1);

            var funcMatch = Regex.Match(formula, @"^(\w+)\s*\((.*)\)$", RegexOptions.IgnoreCase);
            if (funcMatch.Success)
            {
                var funcName = funcMatch.Groups[1].Value.ToUpper();
                var argsStr = funcMatch.Groups[2].Value;
                var args = ParseArguments(argsStr);

                if (_functions.ContainsKey(funcName))
                {
                    var cellRef = new CellReference();
                    return _functions[funcName](cellRef, args, "");
                }
            }

            return EvaluateExpression(formula);
        }

        private object EvaluateExpression(string expression)
        {
            expression = expression.Trim();

            foreach (var variable in _variables)
            {
                expression = expression.Replace(variable.Key, variable.Value);
            }

            double.TryParse(expression, out double numResult);
            return numResult;
        }

        private object[] ParseArguments(string argsStr)
        {
            var args = new List<object>();
            var current = "";
            var depth = 0;
            var inQuotes = false;

            for (int i = 0; i < argsStr.Length; i++)
            {
                var c = argsStr[i];
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == '(' && !inQuotes)
                    depth++;
                else if (c == ')' && !inQuotes)
                    depth--;
                else if (c == ',' && depth == 0 && !inQuotes)
                {
                    args.Add(current.Trim());
                    current = "";
                    continue;
                }
                current += c;
            }

            if (!string.IsNullOrEmpty(current))
                args.Add(current.Trim());

            return args.ToArray();
        }

        private List<object> FlattenValues(object[] args)
        {
            var values = new List<object>();
            foreach (var arg in args)
            {
                if (arg == null)
                    continue;
                if (arg is object[] arr)
                    values.AddRange(FlattenValues(arr));
                else if (arg is IEnumerable<object> enumerable)
                    values.AddRange(enumerable);
                else
                    values.Add(arg);
            }
            return values;
        }

        private bool IsNumeric(object value)
        {
            if (value == null) return false;
            return double.TryParse(value.ToString(), out _);
        }

        private double GetNumericValue(object value)
        {
            if (value == null) return 0;
            if (double.TryParse(value.ToString(), out double result))
                return result;
            return 0;
        }

        private bool IsError(object value)
        {
            return value?.ToString()?.StartsWith("#") == true;
        }

        private bool ToBoolean(object value)
        {
            if (value == null) return false;
            var str = value.ToString()?.ToUpper() ?? "";
            return str == "TRUE" || str == "1" || str == "-1" || str == "YES";
        }

        private long Factorial(int n)
        {
            if (n <= 1) return 1;
            long result = 1;
            for (int i = 2; i <= n; i++) result *= i;
            return result;
        }

        private int GCD(int a, int b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        private int LCM(int a, int b)
        {
            return (a * b) / GCD(a, b);
        }

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
        }

        public string GetVariable(string name)
        {
            return _variables.ContainsKey(name) ? _variables[name] : null;
        }

        public void AddNamedRange(string name, List<CellRange> ranges)
        {
            _namedRanges[name] = ranges;
        }

        public List<CellRange> GetNamedRange(string name)
        {
            return _namedRanges.ContainsKey(name) ? _namedRanges[name] : null;
        }

        public void RemoveNamedRange(string name)
        {
            _namedRanges.Remove(name);
        }
    }

    public class CellReference
    {
        public string SheetName { get; set; }
        public string Column { get; set; }
        public int Row { get; set; }
        public string AbsoluteColumn { get; set; }
        public string AbsoluteRow { get; set; }

        public CellReference()
        {
            SheetName = "";
            Column = "";
            Row = 0;
            AbsoluteColumn = "";
            AbsoluteRow = "";
        }
    }

    public class CellRange
    {
        public string SheetName { get; set; }
        public string StartCell { get; set; }
        public string EndCell { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public CellRange()
        {
            SheetName = "";
            StartCell = "";
            EndCell = "";
            StartRow = 0;
            EndRow = 0;
            StartColumn = 0;
            EndColumn = 0;
        }
    }
}
