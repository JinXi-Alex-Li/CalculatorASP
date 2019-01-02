using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalculatorASP.Models;

namespace CalculatorASP.Controllers
{
    public class CalculatorController : Controller
    {
        static Calculator Calc = new Calculator(); // model
        static Dictionary<string, char> ReadTranslator = new Dictionary<string, char>();

        private void InputMapping() // construct the input mapping used by the model
        {
            var Sequence = Enumerable.Range(0, 10);
            string[] Inputs = { "%", "√", "x²", "⅟x", "÷", "×", "-", "+", "±", "." }; // all commands that calls ReadInput
            char[] InputsCorr = { 'p', 'r', 'q', 'i', 'd', 'm', 's', 'a', 'n', '.' };

            foreach (int i in Enumerable.Range(0, 10))
            {
                try
                {
                ReadTranslator.Add(i.ToString(), i.ToString()[0]);
                }
                catch (Exception) { } // Do nothing
            }

            foreach (int i in Enumerable.Range(0, Inputs.Length))
            {
                try
                {
                ReadTranslator.Add(Inputs[i], InputsCorr[i]);
                }
                catch (Exception) { }
            }
        }

        private bool TryIndex(string key, ref char value)
        {
            try
            {
                value = ReadTranslator[key];
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // GET: Calculator
        public ActionResult Calculator()
        {
            InputMapping();

            ViewBag.UpperText = Calc.ToString();
            ViewBag.LowerText = Calc.GetCur();

            return View();
        }

        [HttpPost]
        public ActionResult Calculator(string command)
        {
            char CommandMap = '0';

            if (TryIndex(command, ref CommandMap))
            {
                Calc.readInput(CommandMap);
            } // command can be one of: CE, C, delete, or solve
            else if (command == "CE")
            {
                Calc.Clear();
            }
            else if (command == "C")
            {
                Calc.Delete('a');
            }
            else if (command == "=")
            {
                try
                {
                    ViewBag.LowerText = Calc.BinEval();
                    ViewBag.UpperText = Calc.ToString();
                    return View();
                }
                catch (Exception)
                {
                    return Redirect("~/Shared/DataErrorInfoModelValidatorProvider.cshtml");
                }
            }
            else
            {
                Calc.Delete('1');
            }

            ViewBag.UpperText = Calc.ToString();
            ViewBag.LowerText = Calc.GetCur();

            return View();
        }
    }
}