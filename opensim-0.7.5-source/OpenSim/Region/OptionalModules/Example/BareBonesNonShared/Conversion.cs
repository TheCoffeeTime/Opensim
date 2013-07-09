using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;

using System.Collections;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    public class Conversion
    {
        static IGraph g;
        static string query;
        static string abbreviation;
        static string code;
        static string conversionMultiplier;
        static string conversionOffset;
        static string quantityKind;
        static string schema;

        public Conversion()
        {

        }

        public static void Initialize()
        {
            g = new Graph();
            g.LoadFromFile("C:/Users/mona/Downloads/qudt-(l1-v1_0_0)/qudt-(l1-v1_0_0)/unit.owl");
            g.LoadFromFile("C:/Users/mona/Downloads/qudt-(l1-v1_0_0)/qudt-(l1-v1_0_0)/quantity.owl");
            abbreviation = "<http://data.nasa.gov/qudt/owl/qudt#abbreviation>";
            code = "<http://data.nasa.gov/qudt/owl/qudt#code>";
            conversionMultiplier = "<http://data.nasa.gov/qudt/owl/qudt#conversionMultiplier>";
            conversionOffset = "<http://data.nasa.gov/qudt/owl/qudt#conversionOffset>";
            quantityKind = "<http://data.nasa.gov/qudt/owl/qudt#quantityKind>";
            schema = "<http://www.w3.org/2001/XMLSchema#string>";

            // workOnUnit("1000", "KHz");
        }
        public static string workOnUnit(string value, string unit)
        {
            string getUnitClass = "";
            double UserValue = double.Parse(value);
            if (UserValue < 1 || UserValue > 999)
            {
                getUnitClass = Get_Unit_Class(value, unit);
            }
            else
                getUnitClass = value + "_" + unit;

            if (getUnitClass == "")
            {
                getUnitClass = value + "_" + unit;
            }
            return getUnitClass;

        }
        public static string Get_Unit_Class(string value, string unit)
        {
            string unitClass = "";
            string multiplier = "";
            double valueIs = 0;
            try
            {

                query = "";
                query += " SELECT * ";
                query += "WHERE ";
                query += "{";
                query += "?x " + abbreviation + "'" + unit + "'^^" + schema;
                query += ".?x " + conversionMultiplier + "?y";
                query += ".?x " + code + "?z";
                query += "} ";

                Object results = g.ExecuteQuery(query);
                //Get_Unit_Multiplier(results);
                if (results is SparqlResultSet)
                {

                    SparqlResultSet rset = (SparqlResultSet)results;

                    foreach (SparqlResult r in rset)
                    {
                        unitClass = r[0].ToString();
                        multiplier = r[1].ToString();


                    }
                    //Console.WriteLine(unitClass);
                    return suitable_Convesion(unitClass, value, "");
                }

            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            ///////////////Now I got the unit with the multiplier///////////////////////
            ////////////////////NOW I need to find the best suitable unit/////////////
            return unitClass;
        }

        public static string suitable_Convesion(string unitClass, string value, string unit)
        {
            ArrayList allUnits = new ArrayList();
            string unitQuantityKind = "";
            string multiplier = "";
            double valueIs = 0;
            string unitFrom = unitClass;
            unitClass = "<" + unitClass + ">";
            Object results;
            try
            {

                query = "";
                query += " SELECT * ";
                query += "WHERE ";
                query += "{";
                query += unitClass;
                query += quantityKind;
                query += "?m";
                query += "} ";

                results = g.ExecuteQuery(query);
                //Get_Unit_Multiplier(results);
                if (results is SparqlResultSet)
                {
                    //Console.WriteLine("resultsSearched");
                    //SELECT/ASK queries give a SparqlResultSet
                    SparqlResultSet rset = (SparqlResultSet)results;

                    //  Console.WriteLine(rset.Count);
                    foreach (SparqlResult r in rset)
                    {
                        unitQuantityKind = r[0].ToString();
                        //multiplier = r[1].ToString();
                        //   Console.WriteLine(r.ToString());

                    }

                }

                ////valueIs = Double.Parse(value.Split('^')[0]) / Double.Parse(multiplier.Split('^')[0]);
                // Console.WriteLine(unitQuantityKind.ToString());
                unitQuantityKind = "<" + unitQuantityKind + ">";
            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }

            query = "";
            query += " SELECT * ";
            query += "WHERE ";
            query += "{";
            query += "?a";
            query += quantityKind;
            query += unitQuantityKind;
            query += "} ";

            results = g.ExecuteQuery(query);
            //Get_Unit_Multiplier(results);
            if (results is SparqlResultSet)
            {
                //Console.WriteLine("resultsSearched");
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    // unitQuantityKind = r[0].ToString();
                    if (r.ToString().Contains("Hertz") || r.ToString().Contains("meter") || r.ToString().Contains("Meter") || r.ToString().Contains("watt") || r.ToString().Contains("Decibel"))
                    {
                        allUnits.Add(r[0].ToString());
                    }

                }

            }
            ////////////////////////Work on Unit////////////////////
            string finalResult = "";
            double returnedConversion = 0;
            bool flag = false;
            for (int length = 0; length < allUnits.Count; length++)
            {
                returnedConversion = Double.Parse(Conversion_Between_units(unitFrom.ToString(), (string)allUnits[length], value.ToString()).Split('^')[0]);
                if (returnedConversion >= 1 && returnedConversion <= 9999)
                {

                    // Console.WriteLine(finalResult);
                    //Need some work to get the unit//

                    query = "";
                    query += " SELECT * ";
                    query += "WHERE ";
                    query += "{";
                    query += "<" + allUnits[length] + ">" + abbreviation + "?x ";
                    query += "} ";

                    results = g.ExecuteQuery(query);
                    //Get_Unit_Multiplier(results);
                    if (results is SparqlResultSet)
                    {
                        //Console.WriteLine("resultsSearched");
                        //SELECT/ASK queries give a SparqlResultSet
                        SparqlResultSet rset = (SparqlResultSet)results;


                        foreach (SparqlResult r in rset)
                        {


                            finalResult = returnedConversion.ToString() + "_" + r[0].ToString().Split('^')[0];
                        }
                    }

                    break;

                }


            }
            return finalResult;
        }
        public static string Get_Unit_Class1(string value, string unit)
        {
            string unitClass = "";
            string multiplier = "";
            double valueIs = 0;
            try
            {

                query = "";
                query += " SELECT * ";
                query += "WHERE ";
                query += "{";
                query += "?x " + abbreviation + "'" + unit + "'^^" + schema;
                query += ".?x " + conversionMultiplier + "?y";
                query += ".?x " + code + "?z";
                query += "} ";

                Object results = g.ExecuteQuery(query);
                //Get_Unit_Multiplier(results);
                if (results is SparqlResultSet)
                {

                    SparqlResultSet rset = (SparqlResultSet)results;

                    foreach (SparqlResult r in rset)
                    {
                        unitClass = r[0].ToString();
                        multiplier = r[1].ToString();


                    }
                    //Console.WriteLine(unitClass);
                    // return suitable_Convesion(unitClass, value, "");
                }

            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            ///////////////Now I got the unit with the multiplier///////////////////////
            ////////////////////NOW I need to find the best suitable unit/////////////
            return unitClass;
        }
        public static string Conversion_Between_units(string from, string to, string value)
        {

            string fromUnit = "<" + from + ">";

            string toUnit = "<" + to + ">";

            string result = "";
            try
            {
                query = "";
                query += " SELECT ((((( ";
                query += value;
                query += "* ?M1) + ?O1) - ?O2) / ?M2) AS ?value)";
                query += "WHERE ";
                query += "{";
                query += fromUnit;
                query += conversionMultiplier;
                query += "?M1 ";
                query += ";";
                query += conversionOffset;
                query += "?O1 ";
                query += ".";
                query += toUnit;
                query += conversionMultiplier;
                query += "?M2 ";
                query += ";";
                query += conversionOffset;
                query += "?O2 ";
                query += ".";
                query += "}";
                Object results = g.ExecuteQuery(query);
                //Console.WriteLine("Executed");
                if (results is SparqlResultSet)
                {
                    //Console.WriteLine("resultsSearched");
                    //SELECT/ASK queries give a SparqlResultSet
                    SparqlResultSet rset = (SparqlResultSet)results;
                    //Console.WriteLine(rset.Count);
                    foreach (SparqlResult r in rset)
                    {
                        result = r[0].ToString();
                        //Console.WriteLine(r.ToString());

                    }

                }
                else if (results is IGraph)
                {
                    //CONSTRUCT/DESCRIBE queries give a IGraph
                    IGraph resGraph = (IGraph)results;
                    foreach (Triple t in resGraph.Triples)
                    {
                        //Do whatever you want with each Triple
                    }

                }
                else
                {
                    //If you don't get a SparqlResutlSet or IGraph something went wrong 
                    //but didn't throw an exception so you should handle it here
                    //Console.WriteLine("ERROR");

                }
            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                // Console.WriteLine(queryEx.Message);
            }
            //  Console.WriteLine("Result of it all");
            //  Console.WriteLine(result);
            //   Console.WriteLine("Result of it all");
            //return result + "_" + to;
            return result.ToString();
        }
    }

}

