/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    [TestClass]
    public class 
        ObjectToArrayOfPropertiesConverterTests
    {
        public enum PersonType
        {
            One,
            Two
        }

        public class ClassWithParamData
        {
            public string Name;
            public double Age;
            public int Size;
            public PersonType Type;
            public bool Adjusted;
        }

        [TestMethod]
        public void CreateObjectTest()
        {
            var fields = new List<object> {"Samuel Clemmens", 50, 1223, PersonType.One, false};
            var paramData = ObjectToArrayOfPropertiesConverter.CreateObject<ClassWithParamData>(fields, out var error);
            Assert.IsFalse(error);
            Assert.AreEqual(paramData.Name, "Samuel Clemmens");
            Assert.AreEqual(paramData.Age, 50);
            Assert.AreEqual(paramData.Size, 1223);
            Assert.AreEqual(paramData.Type, PersonType.One);
            Assert.AreEqual(paramData.Adjusted, false);
            var fields2 = new List<object> {null, 50, 1223, PersonType.Two, null};
            var paramData2 = ObjectToArrayOfPropertiesConverter.CreateObject<ClassWithParamData>(fields2, out error);
            Assert.IsFalse(error);
            Assert.IsNull(paramData2.Name);//default value
            Assert.AreEqual(paramData2.Age, 50);
            Assert.AreEqual(paramData2.Size, 1223);
            Assert.AreEqual(paramData2.Type, PersonType.Two);
            Assert.AreEqual(paramData2.Adjusted, false);//default value 

        }

        [TestMethod]
        public void CreateObjectSimpleTypes()
        {
            var stringFields = new List<object>(new object[] { "fieldsString" });
            var stringValue = ObjectToArrayOfPropertiesConverter.CreateObject<string>(stringFields, out var error);
            Assert.IsFalse(error);
            Assert.AreEqual("fieldsString", stringValue);
            var dateTimeFields = new List<object>(new object[] { DateTime.Today.Date });
            var dateTimeValue = ObjectToArrayOfPropertiesConverter.CreateObject<DateTime>(dateTimeFields, out error);
            Assert.IsFalse(error);
            Assert.AreEqual(DateTime.Today.Date, dateTimeValue);
            var intFields = new List<object>(new object[]{1});
            var intValue = ObjectToArrayOfPropertiesConverter.CreateObject<int>(intFields, out error);
            Assert.IsFalse(error);
            Assert.AreEqual(1, intValue);
            var doubleFields = new List<object>(new object[]{1.1});
            var doubleValue = ObjectToArrayOfPropertiesConverter.CreateObject<double>(doubleFields, out error);
            Assert.IsFalse(error);
            Assert.AreEqual(1.1, doubleValue);
            var decimalFields = new List<object>(new object[]{1.2m});
            var decimalValue = ObjectToArrayOfPropertiesConverter.CreateObject<decimal>(decimalFields, out error);
            Assert.IsFalse(error);
            Assert.AreEqual(1.2m, decimalValue);
        }
      

        [TestMethod]
        public void CreateObjectPropertiesTest()
        {
            var fields = new List<object> {"18", "M", true, "ID-1"};
            var paramData = ObjectToArrayOfPropertiesConverter.CreateObject<Period>(fields, out _);
            Assert.AreEqual(paramData.periodMultiplier, "18");
            Assert.AreEqual(paramData.period, PeriodEnum.M);
            Assert.AreEqual(paramData.id, "ID-1");
        }

        [TestMethod]
        public void CreateListOfObjectsTest()
        {
            var fields = new List<object> {"Samuel Clemmens", 50, 1223, PersonType.One, false};
            var fields2 = new List<object> {null, 50, 1223, PersonType.Two, null};
            var listOfFields = new List<List<object>> {fields, fields2};
            List<ClassWithParamData> listParamData = ObjectToArrayOfPropertiesConverter.CreateListOfObjectsFromListOfListOfFields<ClassWithParamData>(listOfFields);
            Assert.AreEqual(listParamData[0].Name, "Samuel Clemmens");
            Assert.AreEqual(listParamData[0].Age, 50);
            Assert.AreEqual(listParamData[0].Size, 1223);
            Assert.AreEqual(listParamData[0].Type, PersonType.One);
            Assert.AreEqual(listParamData[0].Adjusted, false);
            Assert.IsNull(listParamData[1].Name);//default value
            Assert.AreEqual(listParamData[1].Age, 50);
            Assert.AreEqual(listParamData[1].Size, 1223);
            Assert.AreEqual(listParamData[1].Type, PersonType.Two);
            Assert.AreEqual(listParamData[1].Adjusted, false);//default value 
        }


        [TestMethod]
        public void ConvertClassTo2DArraySingleObject()
        {
            var classWithParamData = new ClassWithParamData
                                         {
                                             Name = "Jonh Doe",
                                             Age = 100,
                                             Size = 40,
                                             Type = PersonType.One,
                                             Adjusted = false
                                         };
            object[,] array = ObjectToArrayOfPropertiesConverter.ConvertObjectToVerticalArrayRange(classWithParamData);
            Assert.AreEqual(array.GetLength(0), 5);
            Assert.AreEqual(array.GetLength(1), 1);
            Assert.AreEqual(array[0, 0], "Jonh Doe");
            Assert.AreEqual(array[1, 0], 100d);
            Assert.AreEqual(array[2, 0], 40);
            Assert.AreEqual(array[3, 0], PersonType.One);
            Assert.AreEqual(array[4, 0], false);
        }

        [TestMethod]
        public void CreateListFromHorizontalArrayRange()
        {
            var sourceArrayWithExtraData = new object[3,10];
            sourceArrayWithExtraData[0, 0] = "Jonh Doe";
            sourceArrayWithExtraData[0, 1] = 100;
            sourceArrayWithExtraData[0, 2] = 40;
            sourceArrayWithExtraData[0, 3] = PersonType.One;
            sourceArrayWithExtraData[0, 4] = false;
            sourceArrayWithExtraData[0, 5] = null;
            sourceArrayWithExtraData[0, 6] = "#N/A";
            sourceArrayWithExtraData[0, 7] = null;
            sourceArrayWithExtraData[0, 8] = "#N/A";
            sourceArrayWithExtraData[0, 9] = null;
            sourceArrayWithExtraData[1, 0] = "Jonh Doe ";
            sourceArrayWithExtraData[1, 1] = 200;
            sourceArrayWithExtraData[1, 2] = 50;
            sourceArrayWithExtraData[1, 3] = PersonType.Two;
            sourceArrayWithExtraData[1, 4] = true;
            sourceArrayWithExtraData[1, 5] = null;
            sourceArrayWithExtraData[1, 6] = "#N/A";
            sourceArrayWithExtraData[1, 7] = null;
            sourceArrayWithExtraData[1, 8] = "#N/A";
            sourceArrayWithExtraData[1, 9] = null;
            // Third row is empty and should be not converted to object.
            //
            sourceArrayWithExtraData[2, 0] = null;
            sourceArrayWithExtraData[2, 1] = null;
            sourceArrayWithExtraData[2, 2] = null;
            sourceArrayWithExtraData[2, 3] = null;
            sourceArrayWithExtraData[2, 4] = null;
            sourceArrayWithExtraData[2, 5] = null;
            sourceArrayWithExtraData[2, 6] = null;
            sourceArrayWithExtraData[2, 7] = null;
            sourceArrayWithExtraData[2, 8] = null;
            sourceArrayWithExtraData[2, 9] = null;
            List<ClassWithParamData> list = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ClassWithParamData>(sourceArrayWithExtraData);
            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void CreateListFromHorizontalArrayRangeSimpleTypes()
        {
            var dateTimeArray = new object[3,1];
            dateTimeArray[0, 0] = DateTime.Today.Date.AddDays(1);
            dateTimeArray[1, 0] = null;
            dateTimeArray[2, 0] = DateTime.Today.Date.AddDays(2);
            List<DateTime> dateTimesList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DateTime>(dateTimeArray);
            Assert.AreEqual(2, dateTimesList.Count);
            Assert.AreEqual(DateTime.Today.Date.AddDays(1), dateTimesList[0]);
            Assert.AreEqual(DateTime.Today.Date.AddDays(2), dateTimesList[1]);
            var stringArray = new object[3,1];
            stringArray[0, 0] = "One";
            stringArray[1, 0] = null;
            stringArray[2, 0] = "Three";
            List<string> stringList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<string>(stringArray);
            Assert.AreEqual(2, stringList.Count);
            Assert.AreEqual("One", stringList[0]);
            Assert.AreEqual("Three", stringList[1]);
            var intArray = new object[3,1];
            intArray[0, 0] = 1;
            intArray[1, 0] = null;
            intArray[2, 0] = 3;
            List<int> intList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<int>(intArray);
            Assert.AreEqual(2, intList.Count);
            Assert.AreEqual(1, intList[0]);
            Assert.AreEqual(3, intList[1]);
            var doubleArray = new object[3,1];
            doubleArray[0, 0] = 1;
            doubleArray[1, 0] = null;
            doubleArray[2, 0] = 3;
            List<double> doubleList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<double>(doubleArray);
            Assert.AreEqual(2, doubleList.Count);
            Assert.AreEqual(1, doubleList[0]);
            Assert.AreEqual(3, doubleList[1]);
            var decimalArray = new object[3,1];
            decimalArray[0, 0] = 1.1m;
            decimalArray[1, 0] = null;
            decimalArray[2, 0] = 3.1m;
            List<decimal> decimalList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<decimal>(decimalArray);
            Assert.AreEqual(2, decimalList.Count);
            Assert.AreEqual(1.1m, decimalList[0]);
            Assert.AreEqual(3.1m, decimalList[1]);
        }

        [TestMethod]
        public void ConvertListToHorizontalArrayRangeSimpleTypes()
        {
            var dateTimeArray = new object[3, 1];
            dateTimeArray[0, 0] = DateTime.Today.Date.AddDays(1);
            dateTimeArray[1, 0] = null;
            dateTimeArray[2, 0] = DateTime.Today.Date.AddDays(2);
            var dateTimesList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DateTime>(dateTimeArray);
            Assert.AreEqual(2, dateTimesList.Count);
            Assert.AreEqual(DateTime.Today.Date.AddDays(1), dateTimesList[0]);
            Assert.AreEqual(DateTime.Today.Date.AddDays(2), dateTimesList[1]);
            var stringArray = new object[3, 1];
            stringArray[0, 0] = "One";
            stringArray[1, 0] = null;
            stringArray[2, 0] = "Three";
            List<string> stringList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<string>(stringArray);
            Assert.AreEqual(2, stringList.Count);
            Assert.AreEqual("One", stringList[0]);
            Assert.AreEqual("Three", stringList[1]);
            var intArray = new object[3, 1];
            intArray[0, 0] = 1;
            intArray[1, 0] = null;
            intArray[2, 0] = 3;
            List<int> intList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<int>(intArray);
            Assert.AreEqual(2, intList.Count);
            Assert.AreEqual(1, intList[0]);
            Assert.AreEqual(3, intList[1]);
            var doubleArray = new object[3, 1];
            doubleArray[0, 0] = 1;
            doubleArray[1, 0] = null;
            doubleArray[2, 0] = 3;
            List<double> doubleList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<double>(doubleArray);
            Assert.AreEqual(2, doubleList.Count);
            Assert.AreEqual(1, doubleList[0]);
            Assert.AreEqual(3, doubleList[1]);
            var decimalArray = new object[3, 1];
            decimalArray[0, 0] = 1.1m;
            decimalArray[1, 0] = null;
            decimalArray[2, 0] = 3.1m;
            List<decimal> decimalList = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<decimal>(decimalArray);
            Assert.AreEqual(2, decimalList.Count);
            Assert.AreEqual(1.1m, decimalList[0]);
            Assert.AreEqual(3.1m, decimalList[1]);
        }
        
        [TestMethod]
        public void CreateListFromVerticalArrayRangeSimpleTypes()
        {
            var dateTimeArray = new object[1,3];
            dateTimeArray[0, 0] = DateTime.Today.Date.AddDays(1);
            dateTimeArray[0, 1] = null;
            dateTimeArray[0, 2] = DateTime.Today.Date.AddDays(2);
            List<DateTime> dateTimesList = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<DateTime>(dateTimeArray);
            Assert.AreEqual(2, dateTimesList.Count);
            Assert.AreEqual(DateTime.Today.Date.AddDays(1), dateTimesList[0]);
            Assert.AreEqual(DateTime.Today.Date.AddDays(2), dateTimesList[1]);
            var stringArray = new object[1,3];
            stringArray[0, 0] = "One";
            stringArray[0, 1] = null;
            stringArray[0, 2] = "Three";
            List<string> stringList = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<string>(stringArray);
            Assert.AreEqual(2, stringList.Count);
            Assert.AreEqual("One", stringList[0]);
            Assert.AreEqual("Three", stringList[1]);
            var intArray = new object[1, 3];
            intArray[0, 0] = 1;
            intArray[0, 1] = null;
            intArray[0, 2] = 3;
            List<int> intList = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<int>(intArray);
            Assert.AreEqual(2, intList.Count);
            Assert.AreEqual(1, intList[0]);
            Assert.AreEqual(3, intList[1]);
            var doubleArray = new object[1, 3];
            doubleArray[0, 0] = 1;
            doubleArray[0, 1] = null;
            doubleArray[0, 2] = 3;
            var doubleList = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<double>(doubleArray);
            Assert.AreEqual(2, doubleList.Count);
            Assert.AreEqual(1, doubleList[0]);
            Assert.AreEqual(3, doubleList[1]);
            var decimalArray = new object[1, 3];
            decimalArray[0, 0] = 1.1m;
            decimalArray[0, 1] = null;
            decimalArray[0, 2] = 3.1m;
            var decimalList = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<decimal>(decimalArray);
            Assert.AreEqual(2, decimalList.Count);
            Assert.AreEqual(1.1m, decimalList[0]);
            Assert.AreEqual(3.1m, decimalList[1]);
        }
                              
        [ExpectedException(typeof(System.Exception))]
        [TestMethod]
        [Ignore]
        public void CreateListFromHorizontalArrayRangeMattsQuestion()
        {
            var sourceArrayWithExtraData = new object[4, 4];
            // first two rows is almost empty (except 0-s)
            //
            sourceArrayWithExtraData[0, 0] = null;
            sourceArrayWithExtraData[0, 1] = null;
            sourceArrayWithExtraData[0, 2] = 0;
            sourceArrayWithExtraData[0, 3] = 0;
            sourceArrayWithExtraData[1, 0] = null;
            sourceArrayWithExtraData[1, 1] = null;
            sourceArrayWithExtraData[1, 2] = 0;
            sourceArrayWithExtraData[1, 3] = 0;
            sourceArrayWithExtraData[2, 0] = DateTime.Today;
            sourceArrayWithExtraData[2, 1] = 1000;
            sourceArrayWithExtraData[2, 2] = 0.0;
            sourceArrayWithExtraData[2, 3] = 0.0;
            sourceArrayWithExtraData[3, 0] = DateTime.Today.AddYears(10);
            sourceArrayWithExtraData[3, 1] = -1000;
            sourceArrayWithExtraData[3, 2] = 0.0;
            sourceArrayWithExtraData[3, 3] = 0.0;
            List<PrincipalExchangeCashflowRangeItem> list = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<PrincipalExchangeCashflowRangeItem>(sourceArrayWithExtraData);
            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void CreateListFromHorizontalArrayRangeMattsRequest()
        {
            var sourceArrayWithExtraData = new object[4, 4];
            // first two rows is almost empty (except 0-s)
            //
            sourceArrayWithExtraData[0, 0] = "";
            sourceArrayWithExtraData[0, 1] = "";
            sourceArrayWithExtraData[0, 2] = "";
            sourceArrayWithExtraData[0, 3] = "";
            sourceArrayWithExtraData[1, 0] = "";
            sourceArrayWithExtraData[1, 1] = "";
            sourceArrayWithExtraData[1, 2] = "";
            sourceArrayWithExtraData[1, 3] = "";
            sourceArrayWithExtraData[2, 0] = DateTime.Today;
            sourceArrayWithExtraData[2, 1] = 1000;
            sourceArrayWithExtraData[2, 2] = 0.0;
            sourceArrayWithExtraData[2, 3] = 0.0;
            sourceArrayWithExtraData[3, 0] = DateTime.Today.AddYears(10);
            sourceArrayWithExtraData[3, 1] = -1000;
            sourceArrayWithExtraData[3, 2] = 0.0;
            sourceArrayWithExtraData[3, 3] = 0.0;
            List<InputPrincipalExchangeCashflowRangeItem> list = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(sourceArrayWithExtraData);
            Assert.AreEqual(list.Count, 2);
        }


        [TestMethod]
        public void CreateListFromVerticalArrayRange()
        {
            var sourceArrayWithExtraData = new object[10,3];
            sourceArrayWithExtraData[0, 0] = "Jonh Doe";
            sourceArrayWithExtraData[1, 0] = 100;
            sourceArrayWithExtraData[2, 0] = 40;
            sourceArrayWithExtraData[3, 0] = PersonType.One;
            sourceArrayWithExtraData[4, 0] = false;
            sourceArrayWithExtraData[5, 0] = null;
            sourceArrayWithExtraData[6, 0] = "#N/A";
            sourceArrayWithExtraData[7, 0] = null;
            sourceArrayWithExtraData[8, 0] = "#N/A";
            sourceArrayWithExtraData[9, 0] = null;
            sourceArrayWithExtraData[0, 1] = "Jonh Doe ";
            sourceArrayWithExtraData[1, 1] = 200;
            sourceArrayWithExtraData[2, 1] = 50;
            sourceArrayWithExtraData[3, 1] = PersonType.Two;
            sourceArrayWithExtraData[4, 1] = true;
            sourceArrayWithExtraData[5, 1] = null;
            sourceArrayWithExtraData[6, 1] = "#N/A";
            sourceArrayWithExtraData[7, 1] = null;
            sourceArrayWithExtraData[8, 1] = "#N/A";
            sourceArrayWithExtraData[9, 1] = null;
            // Third row is empty and should be not converted to object.
            //
            sourceArrayWithExtraData[0, 2] = null;
            sourceArrayWithExtraData[1, 2] = null;
            sourceArrayWithExtraData[2, 2] = null;
            sourceArrayWithExtraData[3, 2] = null;
            sourceArrayWithExtraData[4, 2] = null;
            sourceArrayWithExtraData[5, 2] = null;
            sourceArrayWithExtraData[6, 2] = null;
            sourceArrayWithExtraData[7, 2] = null;
            sourceArrayWithExtraData[8, 2] = null;
            sourceArrayWithExtraData[9, 2] = null;
            List<ClassWithParamData> list = ObjectToArrayOfPropertiesConverter.CreateListFromVerticalArrayRange<ClassWithParamData>(sourceArrayWithExtraData);
            Assert.AreEqual(list.Count, 2);
        }


        [TestMethod]
        public void CreateListFromHorizontalArrayRangeNonZeroBasedArray()
        {
            var sourceArrayWithExtraData = (object[,])Array.CreateInstance(typeof(object), new[] { 3, 10 }, new[] { 2, 2 });
            sourceArrayWithExtraData[2, 2] = "Jonh Doe";
            sourceArrayWithExtraData[2, 3] = 100;
            sourceArrayWithExtraData[2, 4] = 40;
            sourceArrayWithExtraData[2, 5] = PersonType.One;
            sourceArrayWithExtraData[2, 6] = false;
            sourceArrayWithExtraData[2, 7] = null;
            sourceArrayWithExtraData[2, 8] = "#N/A";
            sourceArrayWithExtraData[2, 9] = null;
            sourceArrayWithExtraData[2, 10] = "#N/A";
            sourceArrayWithExtraData[2, 11] = null;
            sourceArrayWithExtraData[3, 2] = "Jonh Doe ";
            sourceArrayWithExtraData[3, 3] = 200;
            sourceArrayWithExtraData[3, 4] = 50;
            sourceArrayWithExtraData[3, 5] = PersonType.Two;
            sourceArrayWithExtraData[3, 6] = true;
            sourceArrayWithExtraData[3, 7] = null;
            sourceArrayWithExtraData[3, 8] = "#N/A";
            sourceArrayWithExtraData[3, 9] = null;
            sourceArrayWithExtraData[3, 10] = "#N/A";
            sourceArrayWithExtraData[3, 11] = null;
            // Third row is empty and should be not converted to object.
            //
            sourceArrayWithExtraData[4, 2] = null;
            sourceArrayWithExtraData[4, 3] = null;
            sourceArrayWithExtraData[4, 4] = null;
            sourceArrayWithExtraData[4, 5] = null;
            sourceArrayWithExtraData[4, 6] = null;
            sourceArrayWithExtraData[4, 7] = null;
            sourceArrayWithExtraData[4, 8] = null;
            sourceArrayWithExtraData[4, 9] = null;
            sourceArrayWithExtraData[4, 10] = null;
            sourceArrayWithExtraData[4, 11] = null;
            List<ClassWithParamData> list = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ClassWithParamData>(sourceArrayWithExtraData);
            Assert.AreEqual(list.Count, 2);
        }
     
        [TestMethod]
        public void ConvertObjectToHorizontalArraySingleObject()
        {
            var classWithParamData = new ClassWithParamData
                                                        {
                                                            Name = "Name 21",
                                                            Age = 100,
                                                            Size = 40,
                                                            Type = PersonType.One,
                                                            Adjusted = false
                                                        };
            object[,] lists = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(classWithParamData);
            Assert.AreEqual(lists[0, 0], "Name 21");
            Assert.AreEqual(lists[0, 1], 100d);
            Assert.AreEqual(lists[0, 2], 40);
            Assert.AreEqual(lists[0, 3], PersonType.One);
            Assert.AreEqual(lists[0, 4], false);
        }    
        
        [TestMethod]
        public void ConvertObjectToVerticalArraySingleObject()
        {
            var classWithParamData = new ClassWithParamData
                                         {
                                             Name = "Name 21",
                                             Age = 100,
                                             Size = 40,
                                             Type = PersonType.One,
                                             Adjusted = false
                                         };
            object[,] lists = ObjectToArrayOfPropertiesConverter.ConvertObjectToVerticalArrayRange(classWithParamData);
            Assert.AreEqual(lists[0, 0], "Name 21");
            Assert.AreEqual(lists[1, 0], 100d);
            Assert.AreEqual(lists[2, 0], 40);
            Assert.AreEqual(lists[3, 0], PersonType.One);
            Assert.AreEqual(lists[4, 0], false);
        }

        [TestMethod]
        public void ConvertObjectToHorizontalArrayArrayOfTwoObjects()
        {
            var classWithParamData1 = new ClassWithParamData
                                          {
                                              Name = "Name 21",
                                              Age = 100,
                                              Size = 40,
                                              Type = PersonType.One,
                                              Adjusted = false
                                          };
            var classWithParamData2 = new ClassWithParamData
                                          {
                                              Name = "Name 22",
                                              Age = 200,
                                              Size = 40,
                                              Type = PersonType.Two,
                                              Adjusted = true
                                          };
            var classWithParamDatas = new[]{classWithParamData1, classWithParamData2 };
            object[,] lists = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(classWithParamDatas);   
            Assert.AreEqual(lists[0,0], "Name 21");
            Assert.AreEqual(lists[0,1], 100d);
            Assert.AreEqual(lists[0,2], 40);
            Assert.AreEqual(lists[0, 3], PersonType.One);
            Assert.AreEqual(lists[0,4], false);
            Assert.AreEqual(lists[1,0], "Name 22");
            Assert.AreEqual(lists[1,1], 200d);
            Assert.AreEqual(lists[1,2], 40);
            Assert.AreEqual(lists[1, 3], PersonType.Two);
            Assert.AreEqual(lists[1,4], true);
        }

        [TestMethod]
        public void ConvertObjectToVerticalArrayArrayOfTwoObjects()
        {
            var classWithParamData1 = new ClassWithParamData
                                          {
                                              Name = "Name 21",
                                              Age = 100,
                                              Size = 40,
                                              Type = PersonType.One,
                                              Adjusted = false
                                          };
            var classWithParamData2 = new ClassWithParamData
                                          {
                                              Name = "Name 22",
                                              Age = 200,
                                              Size = 40,
                                              Type = PersonType.Two,
                                              Adjusted = true
                                          };
            var classWithParamDatas = new[]{classWithParamData1, classWithParamData2 };
            object[,] lists = ObjectToArrayOfPropertiesConverter.ConvertObjectToVerticalArrayRange(classWithParamDatas);     
            Assert.AreEqual(lists[0,0], "Name 21");
            Assert.AreEqual(lists[1,0], 100d);
            Assert.AreEqual(lists[2,0], 40);
            Assert.AreEqual(lists[3, 0], PersonType.One);
            Assert.AreEqual(lists[4,0], false);                                  
            Assert.AreEqual(lists[0,1], "Name 22");
            Assert.AreEqual(lists[1,1], 200d);
            Assert.AreEqual(lists[2,1], 40);
            Assert.AreEqual(lists[3, 1], PersonType.Two);
            Assert.AreEqual(lists[4,1], true);
        }
    }
}