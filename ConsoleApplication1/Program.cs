using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // ToDo's:
            // 1. Better validation
            // 2. Exception handling

            String dob;
            String gender;
            String postCode;
            String smokerYN;
            Int32  exerciseHrs;
            String childrenYN;

            Int32 age;
            Decimal price;

            // Pull and validate user data 
            dob = getDateofBirth();

            gender = getGender();

            postCode = getPostcode();

            smokerYN = getSmoker();

            exerciseHrs = getExercise();

            childrenYN = getChildren();

            // Calculate applicant's age in years 
            age = calculateAgeinYears(dob);

            // Use age and gender to lookup base price 
            price = getBasePrice(age, gender);

            // Use 'postcodes.io' web service to derive customer's country of residence
            price = applyRegionalHealthIndex(price, postCode);

            // Apply loading factor if applicant has children 
            price = applyChildrenFactor(price, childrenYN);

            // Apply loading factor if applicant smokes
            price = applySmokerFactor(price, smokerYN);

            // Apply loading factor according to the number of hours exercise done per week 
            price = applyLifestyleFactor(price, exerciseHrs);

            // Write calculated premium to console
            Console.WriteLine("\n\nYour calculated premium is " + price.ToString("C2"));

            // Pause until keypress
            showPrompt("\n\nPress a key to exit");
            Console.ReadKey();
        }

        //***************************************************************************************************
        //* Get user date of birth
        //***************************************************************************************************
        static String getDateofBirth()
        {
            String dob;

            do {
                Console.WriteLine("Please enter your date of birth (in dd/mm/yyyy format)");
                dob = Console.ReadLine();
                if (validateDoB(dob) == false)
                    {
                        dob = String.Empty;
                        showError("dd/mm/yyyy");
                    }
            
            } while (dob == String.Empty);

            return dob;
        }

        //***************************************************************************************************
        //* Get user gender
        //***************************************************************************************************
        static String getGender()
        {
            String gender;
            do
            {
                Console.WriteLine("\nAre you male or female? (enter M or F)");
                gender = Console.ReadLine().ToUpper();
                if (gender != "M" && gender != "F")
                {
                    gender = String.Empty;
                    showError("M or F");
                }
            } while (gender != "M" && gender != "F");

            return gender;
        }

        //***************************************************************************************************
        //* Get postcode
        //***************************************************************************************************
        static String getPostcode()
        {
            String postCode;
            do
            {
                Console.WriteLine("\nPlease enter your postcode");
                postCode = Console.ReadLine().ToUpper();

                if (validatePostcode(postCode) == false) {
                    postCode = String.Empty;
                    showError("in the usual UK postcode style");
                }

            } while (postCode == String.Empty);

            return postCode;

        }

        //***************************************************************************************************
        //* Get customer's filthy habit value
        //***************************************************************************************************
        static string getSmoker()
        {
            String smokerYN;

            do
            {
                Console.WriteLine("\nDo you smoke? (enter Y or N)");
                smokerYN = Console.ReadLine().ToUpper();

                if (smokerYN != "Y" && smokerYN != "N")
                {
                    smokerYN = String.Empty;
                    showError("Y or N");
                }

            } while (smokerYN != "Y" && smokerYN != "N");

            return smokerYN;
        }

        //***************************************************************************************************
        //* Get hours exercise/week
        //***************************************************************************************************
        static Int32 getExercise()
        {
            int exerciseHrs;
            do
            {
                Console.WriteLine("\nHow many hours exercise do you do per week? (0 to 99 hours)");
                exerciseHrs = Convert.ToInt32(Console.ReadLine());
                
            } while (exerciseHrs < 0 || exerciseHrs > 99);
            
            return exerciseHrs;
        }

        //***************************************************************************************************
        //* See if customer has kiddy-winks
        //***************************************************************************************************
        static String getChildren()
        {
            String childrenYN;
            do
            {
                Console.WriteLine("\nDo you have any children? (enter Y or N)");

                childrenYN = Console.ReadLine().ToUpper();

                if (childrenYN != "Y" && childrenYN != "N")
                {
                    childrenYN = String.Empty;
                    showError("Y or N");
                }
            }
            while (childrenYN != "Y" && childrenYN != "N");

            return childrenYN;
        }

        //***************************************************************************************************
        //* Use regex to validate postcode format
        //***************************************************************************************************
        static bool validatePostcode(String postCode)
        {
            Regex regex = new Regex("^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]|[ABEHMNPRV-Y]))|[0-9][A-HJKPS-UW]) {0,1}[0-9][ABD-HJLNP-UW-Z]{2})$");
            return regex.IsMatch(postCode.ToUpper().Trim());
        }

        //***************************************************************************************************
        //* Use regex to validate DoB (dd/mm/yyyy format)
        //***************************************************************************************************
        static bool validateDoB(String DoB)
        {
            Regex regex = new Regex("^(((0[1-9]|[12][0-9]|30)[-/]?(0[13-9]|1[012])|31[-/]?(0[13578]|1[02])|(0[1-9]|1[0-9]|2[0-8])[-/]?02)[-/]?[0-9]{4}|29[-/]?02[-/]?([0-9]{2}(([2468][048]|[02468][48])|[13579][26])|([13579][26]|[02468][048]|0[0-9]|1[0-6])00))$");
            return regex.IsMatch(DoB.Trim());
        }

        //***************************************************************************************************
        //* Show generic error message to console
        //***************************************************************************************************
        static void showError(String errString)
        {
            Console.WriteLine("Invalid input; please enter " + errString);
        }

        //***************************************************************************************************
        // Show generic prompt message to console
        //***************************************************************************************************
        static void showPrompt(String promptString)
        {
            Console.WriteLine(promptString);
        }

        //***************************************************************************************************
        //* Use customer's date of birth to derive their age in years
        //***************************************************************************************************
        static Int32 calculateAgeinYears(String dob)
        {
            Int32 age = 0;
            DateTime dateOfBirth;

            dateOfBirth = DateTime.Parse(dob);

            age = (DateTime.Now.Year - dateOfBirth.Year);
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear) age = age - 1;

            return age;
        }

        //***************************************************************************************************
        //* Use customer's age and gender to lookup the base price
        //***************************************************************************************************
        static Decimal getBasePrice(Int32 age, String gender)
        {
            Int32[] ageGroup = { 18, 24, 35, 45, 60, 999 };
            Decimal[] basePriceMale = { 150, 180, 200, 250, 320, 500 };
            Decimal[] basePriceFemale = { 100, 165, 180, 225, 315, 485 };

            // Must be a better way of looking up in an array!!
            for (int i=0; i <= ageGroup.GetUpperBound(0); i++)
            {
                if (age < ageGroup[i])
                {
                    if (gender == "M")
                    {
                        return basePriceMale[i];
                    }
                    else
                    {
                        return basePriceFemale[i];
                    }
                }
            }

            return -1; 
            
        }

        //***************************************************************************************************
        //* Use country (obtained from postcode lookup) to apply a weighting to price
        //***************************************************************************************************
        static Decimal applyRegionalHealthIndex(Decimal price, String postCode)
        {
            String[] countries = { "England", "Wales", "Scotland", "Ireland", "Northern Ireland" };
            Decimal[] RHI = { 0.0m, -100.00m, 200.00m, 50.00m, 75.00m};
            Decimal defaultRHI = 100.00m;

            // Can't seem to test Ireland 
            String country = getCountryFromPostcode(postCode);

            for (int i = 0; i <= countries.GetUpperBound(0); i++)
            {
                if (country == countries[i])
                {
                    price = price + RHI[i];
                    return applyFloorLimit(price);
                }
            }

            return applyFloorLimit(price + defaultRHI);   
        }

        //***************************************************************************************************
        //* Apply loading if customer has children
        //***************************************************************************************************
        static Decimal applyChildrenFactor(Decimal price, String childrenYN)
        {
            if (childrenYN == "Y")
            {
                return applyFloorLimit(price * 1.5m);
            }
            else return applyFloorLimit(price);
        }

        //***************************************************************************************************
        //* Apply loading to price based on number of hours exercise/week customer does
        //***************************************************************************************************
        static Decimal applyLifestyleFactor(Decimal price, Int32 hrsExercise)
        {
            Int32[] exercise = { 9, 6, 3, 1 };
            Decimal[] factor = { 1.5m, 0.5m, 0.7m, 1.0m };
            Decimal defaultFactor = 1.2m;

            for(int i = 0; i <= exercise.GetUpperBound(0); i++)
            {
                if (hrsExercise >= exercise[i])
                {
                    return applyFloorLimit(price * factor[i]);
                }
            }

            return applyFloorLimit(price * defaultFactor);
        }

        //***************************************************************************************************
        //* Apply loading to price if customer sticks burning vegetation in their mouth regularly
        //***************************************************************************************************
        static Decimal applySmokerFactor(Decimal price, String smokerYN)
        {
            if (smokerYN == "Y")
            {
                return applyFloorLimit(price * 3.0m);
            }
            else return applyFloorLimit(price);
        }

        //***************************************************************************************************
        //* Use webservice to get country of residence from customer's postcode
        //***************************************************************************************************
        static String getCountryFromPostcode(String postCode)
        {
            // Note - this process is a bit crude, could do with some better error handling etc

            var requestUri = String.Format("http://api.postcodes.io/postcodes/{0}", Uri.EscapeDataString(postCode));

            HttpWebRequest GETrequest;
            GETrequest = WebRequest.Create(requestUri) as HttpWebRequest;

            WebProxy pegProxy = new WebProxy("peg-proxy01", 3128);
            pegProxy.BypassProxyOnLocal = true;

            GETrequest.Proxy = pegProxy;

            Stream objStream;
            objStream = GETrequest.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            String json = objReader.ReadToEnd();

            var jObj = JObject.Parse(json);
            String country = jObj["result"]["country"].ToString();

            return country;
        }

        //***************************************************************************************************
        //* Apply floor limit (if necessary)
        //***************************************************************************************************
        static Decimal applyFloorLimit(Decimal price)
        {
            Decimal FLOOR_LIMIT = 50.00m;

            if (price < FLOOR_LIMIT)
                return FLOOR_LIMIT;
            else
                return price;
        }

    }
    
}
