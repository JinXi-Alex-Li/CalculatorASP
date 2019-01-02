using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalculatorASP.Models
{
    // Exception class
    public class MathException : Exception
    {
        public MathException(string message) : base(message) { }
    }

    // Enum class
    public enum Optor { Nul = 0, Add, Sub, Mult, Div, Neg, Inv, Sqr, Sqrt, Per };
    // operators: No operator given, addition, subtraction, multiplication, division,
    //            negation, multiplicative inverse, square, square root, percent
    // Of which the unary expressions are calculated directly.


    // Helper Operator class
    public class MathOperator
    {
        public Optor Operation { get; }

        public MathOperator()
        {
            Operation = Optor.Nul;
        }

        public MathOperator(char c) // pass in a character representation of the optor
        {
            switch (c)
            {
                case 'a': // Addition
                    Operation = Optor.Add;
                    break;

                case 's': // subtraction
                    Operation = Optor.Sub;
                    break;

                case 'm': // multiplication
                    Operation = Optor.Mult;
                    break;

                case 'd': // division
                    Operation = Optor.Div;
                    break;

                case 'n': // negation
                    Operation = Optor.Neg;
                    break;

                case 'i': // multiplicative (i)nverse
                    Operation = Optor.Inv;
                    break;

                case 'q': //s(q)uare
                    Operation = Optor.Sqr;
                    break;

                case 'r': //square (r)oot
                    Operation = Optor.Sqrt;
                    break;

                case 'p': // percent
                    Operation = Optor.Per;
                    break;

                default:
                    Operation = Optor.Nul;
                    break;
            }
        }

        public bool IsUnary()
        {
            if (Operation == Optor.Neg || Operation == Optor.Inv || Operation == Optor.Sqr ||
                Operation == Optor.Sqrt || Operation == Optor.Per)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            string RetVal = "";
            switch (Operation)
            {
                case Optor.Add:
                    RetVal = "+";
                    break;
                case Optor.Sub:
                    RetVal = "-";
                    break;
                case Optor.Mult:
                    RetVal = "×";
                    break;
                case Optor.Div:
                    RetVal = "÷";
                    break;
                default:
                    break;
            }

            return RetVal;
        }

    }

    // Main calculator class

    public class Calculator
    {
        static char[] AllowedChar = new char[] { 'a', 's', 'm', 'd', 'n', 'i', 'q', 'r', 'p' };
        // allows inputs nased on the operators (Optor)

        private Queue<string> Inputs;
        private Queue<MathOperator> Operations;

        // variables for keeping track while reading inputs
        bool DecimalPtExists = false; // A check if a decimal point exists in the current Input

        bool IsBinary = true;
        string SecondLast = ""; // To calculate Percent

        MathOperator LastOp;
        string LastInput = "";
        string CurrInput = ""; // LastInput and CurrInput are used to model the following special case:
                               //   If a binary operator is given to the calculator, but no second
                               //   argument exists, then Microsoft calculator will repeat the
                               //   previous number as the second argument

        // string representation of the expression
        string Calculation = "";
        string Tail = "";
        bool UnaryDelete = false; // Deal with the case of deleting an unary expression

        public Calculator()
        {
            Inputs = new Queue<string>();
            Operations = new Queue<MathOperator>();
            LastOp = new MathOperator();
        }

        // Helpers to format Unary Expressions
        public void Format(string Number, MathOperator Op, ref string Modified, ref string Result)
        // result is the evaluated result, some short circuits are used here
        {
            if (Inputs.Count == 0)
            {
                Number = LastInput;
            }

            string Prefix = "", Suffix = "";

            decimal ParseRes = decimal.Parse(Number);

            switch (Op.Operation)
            {
                case Optor.Neg:
                    Prefix = "-";

                    ParseRes *= -1;

                    break;
                case Optor.Inv:
                    Prefix = "1/(";
                    Suffix = ")";

                    ParseRes = 1 / ParseRes;

                    break;
                case Optor.Sqr:
                    Prefix = "sqr(";
                    Suffix = ")";

                    ParseRes *= ParseRes;

                    break;
                case Optor.Sqrt:
                    Prefix = "√(";
                    Suffix = ")";

                    ParseRes = (decimal)Math.Sqrt((double)ParseRes);

                    break;
                case Optor.Per:

                    bool Parsed = decimal.TryParse(SecondLast, out ParseRes);
                    decimal percent = decimal.Parse(Number);

                    ParseRes *= (percent / 100);

                    break;

                default:
                    break;
            }

            ParseRes = decimal.Round(ParseRes, 16, MidpointRounding.AwayFromZero);
            Result = ParseRes.ToString();
            Modified = Prefix + Number + Suffix;
        }

        // Evaluates all binary operations
        public string BinEval()
        {
            // Special case 1, when no second argument is given to a binary expression:
            if (CurrInput == "" && IsBinary)
            {
                Inputs.Enqueue(LastInput);
            }

            //Special case 2, evaluating an expression directly after a sum, this will only happen
            //  if the previous calculation was binary
            else if (Operations.Count == 0)
            {
                Inputs.Enqueue(LastInput);
                Inputs.Enqueue(CurrInput);
                Operations.Enqueue(LastOp);
            }

            else
            {
                Inputs.Enqueue(CurrInput);
                Calculation += CurrInput;
            }


            string FirstArgStr = Inputs.Dequeue();

            bool ParseRes = decimal.TryParse(FirstArgStr, out decimal FirstArg);
            decimal Result = FirstArg;

            while (Inputs.Count != 0) // since all unary expressions are evaluated on the spot
                                      // there will only be binary operations
            {
                string ArgStr = Inputs.Dequeue();
                MathOperator Curr = Operations.Dequeue();
                ParseRes |= decimal.TryParse(ArgStr, out decimal SecondArg);
                Optor op = Curr.Operation;

                switch (op)
                {
                    case Optor.Add:
                        Result += SecondArg;
                        break;

                    case Optor.Sub:
                        Result -= SecondArg;
                        break;

                    case Optor.Mult:
                        Result *= SecondArg;
                        break;

                    case Optor.Div:
                        Result /= SecondArg;
                        break;

                    case Optor.Nul:
                        return Result.ToString();

                    default:
                        throw new MathException("Unexpected error");
                }

            }

            Calculation = "";
            CurrInput = "";
            IsBinary = true;
            DecimalPtExists = false;

            LastInput = Result.ToString(); // Special case
            return LastInput;
        }


        public void readInput(char c)
        {
            if (c >= '0' && c <= '9') // c is a digit
            {
                CurrInput += c;
            }

            else if (c == '.' && !DecimalPtExists) // c is a decimal, and no decimal points exists
            {
                CurrInput += c;
                DecimalPtExists = true;
            }

            else if (AllowedChar.Contains(c))
            {
                if (CurrInput != "")
                {
                    SecondLast = LastInput;
                    LastInput = CurrInput;
                    CurrInput = "";
                    DecimalPtExists = false;
                }

                MathOperator op = new MathOperator(c);

                if (op.IsUnary())
                {
                    // Special Handler for Percent, since it requires a previous value
                    // If SecondLast is empty, then there is no number to take the percentage of
                    if (op.Operation == Optor.Per && SecondLast == "") return;

                    IsBinary = false;
                    string Modified = "", Result = "";
                    Format(LastInput, op, ref Modified, ref Result);

                    Calculation += Tail;
                    Tail = Modified;
                    CurrInput = Result;
                }
                else
                {
                    if (UnaryDelete)
                    {
                        Calculation += LastInput;
                        UnaryDelete = false;
                    }
                    else
                    {
                        Calculation += (IsBinary ? LastInput : "");
                    }

                    IsBinary = true;
                    LastOp = op;
                    Inputs.Enqueue(LastInput);
                    Operations.Enqueue(op);
                }

                Calculation += op.ToString();

            }
        }

        public void Delete(char c)
        {
            if (IsBinary && c == '1')
            {
                CurrInput = CurrInput.Substring(0,
                                (CurrInput.Length - 1 >= 0 ? CurrInput.Length - 1 : 0));
                Calculation = Calculation.Substring(0,
                                (Calculation.Length - 1 >= 0 ? Calculation.Length - 1 : 0));
            }
            else if (c == '1')
            {
                Tail = "";
                CurrInput = "";
                UnaryDelete = true;
            }
            else if (c == 'a')
            {
                CurrInput = "";
            }
        }

        public void Clear()
        {
            Inputs.Clear();
            Operations.Clear();
            CurrInput = "";
            LastInput = "";
            SecondLast = "";

            DecimalPtExists = false;
            Calculation = "";
        }

        public override string ToString()
        {
            return Calculation + Tail;
        }

        public string GetCur()
        {
            return (CurrInput == ""? "0" : CurrInput);
        }
    }
}
